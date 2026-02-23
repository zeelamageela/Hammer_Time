# AI Draw Shot Comprehensive Fix

## Problems Identified

### Problem 1: Limited Candidate Generation
**Current behavior:** Only generates ~10-15 candidates (1 direct + radial around guards)
- If no guards exist ? ONLY 1 candidate (direct to target)
- If guards poorly positioned ? Bad radial candidates
- Missing huge swaths of scoring space

**What we need:** FULL RADIAL SWEEP around target position
- Multiple radii: 0.0m, 0.3m, 0.6m, 0.9m, 1.2m from button
- Multiple angles: Every 30° (12 positions per radius)
- Both turn directions tested
- **Total: ~60-70 candidates** for comprehensive evaluation

### Problem 2: Guard Protection Scoring Doesn't Check Team
**Current code (lines 2052-2078):**
```csharp
foreach (GameObject guard in guards)
{
    Vector2 guardPos = guard.transform.position;
    
    // PROBLEM: Doesn't check if guard is FRIENDLY or OPPONENT!
    // Protection scoring should ONLY apply to FRIENDLY guards
    bool inFront = guardPos.y < finalPos.y; // Guard is closer to launcher
    float lateralAlignment = Mathf.Abs(guardPos.x - finalPos.x);
    
    if (inFront && lateralAlignment < 0.6f && depthSeparation > 0.3f)
    {
        // Scores ALL guards equally - WRONG!
        protectionScore = alignmentQuality;
    }
}
```

**The fix:** Check `guard.GetComponent<Rock_Info>().teamName == currentRockInfo.teamName`

### Problem 3: Out-Turn Not Being Tried Enough
**Root cause:** Radial candidates are generated around GUARDS, not around TARGET
- If target is at (0, 6.5) and no center guards exist
- We only test the ONE direct path
- We never explore (0.3, 6.5), (-0.3, 6.5), (0, 6.8), etc.
- **Out-turns could score better at these alternate positions!**

### Problem 4: Acceptance Threshold Too High
**Current code (line 2148):**
```csharp
if (bestScore > float.MinValue && bestScore >= 20f)
```

**The problem:**
- Threshold of 20 is reasonable BUT
- With only 1-2 candidates being tested (no guards case)
- If that one candidate scores 15-19 ? REJECTED
- Falls back to old magic number code

**What happens:** AI gives up on physics and uses terrible fallback

---

## The Complete Fix

### Fix 1: Generate Full Radial Sweep of Candidates

Replace the candidate generation section (lines 1974-2024) with:

```csharp
List<Vector2> candidateTargets = new List<Vector2>();

// CANDIDATE 1: Direct to original target (baseline)
candidateTargets.Add(targetPosition);

// CANDIDATE 2-N: FULL RADIAL SWEEP around button
// Test positions at multiple radii and angles for COMPLETE coverage
// This ensures we explore ALL potential scoring positions, not just guarded lanes

float[] radii = new float[] { 0.0f, 0.3f, 0.6f, 0.9f, 1.2f, 1.5f }; // Button, 4-foot, 8-foot, 12-foot, edges
float[] angles = new float[] { 0f, 30f, 60f, 90f, 120f, 150f, 180f, 210f, 240f, 270f, 300f, 330f }; // Every 30°

foreach (float radius in radii)
{
    if (radius == 0.0f)
    {
        // Button is already added as candidate 1
        continue;
    }
    
    foreach (float angleDeg in angles)
    {
        float angleRad = angleDeg * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
        Vector2 candidatePos = button + offset;
        
        // Must be in playable area (within sheet bounds)
        if (candidatePos.y > 5.0f && candidatePos.y < 9.0f && Mathf.Abs(candidatePos.x) < 2.0f)
        {
            candidateTargets.Add(candidatePos);
        }
    }
}

// CANDIDATE N+1: PROTECTED POSITIONS behind FRIENDLY guards
foreach (GameObject guard in guards)
{
    Vector2 guardPos = guard.transform.position;
    
    Rock_Info guardInfo = guard.GetComponent<Rock_Info>();
    if (guardInfo == null || guardInfo.teamName != currentRockInfo.teamName)
        continue; // Only use OUR guards for protection
    
    // Generate radial positions BEHIND guard (toward house)
    float[] protectedAngles = new float[] { 90f, 75f, 105f, 60f, 120f }; // Behind and beside
    float[] protectedRadii = new float[] { 0.4f, 0.6f, 0.8f };
    
    foreach (float angle in protectedAngles)
    {
        foreach (float radius in protectedRadii)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector2 candidatePos = guardPos + offset;
            
            // Must be in house (Y > 5.0) to score
            if (candidatePos.y > 5.0f && candidatePos.y < 9.0f)
            {
                candidateTargets.Add(candidatePos);
            }
        }
    }
}

Debug.Log($"[Physics Draw] Generated {candidateTargets.Count} candidate positions:");
Debug.Log($"  1 direct target + {radii.Length * angles.Length} radial + {candidateTargets.Count - 1 - (radii.Length * angles.Length)} protected");
```

**Result:** ~70-80 candidates tested instead of 1-10!

### Fix 2: Fix Guard Protection Scoring (Team Check)

Replace the protection scoring section (lines 2052-2078) with:

```csharp
// PART 1: PROTECTED POSITION (50 points max) - HIGHEST PRIORITY!
float protectionScore = 0f;
GameObject protectingGuard = null;

foreach (GameObject guard in guards)
{
    Vector2 guardPos = guard.transform.position;
    
    // CRITICAL FIX: Only OUR guards provide protection!
    Rock_Info guardInfo = guard.GetComponent<Rock_Info>();
    if (guardInfo == null || guardInfo.teamName != currentRockInfo.teamName)
        continue; // Skip opponent guards
    
    // Check if guard is protecting this position
    // Protection = guard is BETWEEN launcher and final position
    bool inFront = guardPos.y < finalPos.y; // Guard is closer to launcher
    float lateralAlignment = Mathf.Abs(guardPos.x - finalPos.x); // How aligned laterally
    float depthSeparation = finalPos.y - guardPos.y; // How far behind guard
    
    // Good protection: Guard in front, good lateral alignment, reasonable depth
    if (inFront && lateralAlignment < 0.6f && depthSeparation > 0.3f && depthSeparation < 3.0f)
    {
        // Score protection quality
        float alignmentQuality = 1.0f - Mathf.Clamp01(lateralAlignment / 0.6f);
        float depthQuality = 1.0f - Mathf.Clamp01(Mathf.Abs(depthSeparation - 1.5f) / 1.5f); // Ideal: 1.5 units behind
        
        float guardProtectionQuality = alignmentQuality * 0.6f + depthQuality * 0.4f;
        
        if (guardProtectionQuality > protectionScore)
        {
            protectionScore = guardProtectionQuality;
            protectingGuard = guard;
        }
    }
}

score += protectionScore * 50f; // Up to 50 points for FRIENDLY guard protection
```

### Fix 3: Lower Acceptance Threshold + Better Fallback

Replace the acceptance check (lines 2145-2154) with:

```csharp
// ACCEPTANCE CRITERIA: Accept ANY reasonable draw attempt
// Threshold lowered to 10.0 (was 20.0) to accept more attempts
// Better to try SOMETHING than fall back to magic numbers!
if (bestScore > float.MinValue && bestScore >= 10f)
{
    pullbackPosition = bestPullback;
    useInTurn = bestInTurn;
    
    Debug.Log($"[Physics Draw] ? SUCCESS! Score: {bestScore:F1}/125\n" +
              $"  Final position: ({bestFinalPos.x:F2}, {bestFinalPos.y:F2})\n" +
              $"  Pullback: ({bestPullback.x:F3}, {bestPullback.y:F3})\n" +
              $"  Turn: {(bestInTurn ? "IN-TURN" : "OUT-TURN")}\n" +
              $"  Tested {candidateTargets.Count} candidates");
    return true;
}

// IMPROVED FALLBACK: Try direct physics to button as last resort
Debug.LogWarning($"[Physics Draw] All candidates scored low (best: {bestScore:F1}), trying direct button shot");

Vector2 directButtonVelocity = trajectorySimulator.CalculateVelocityToTarget(
    launcherPos,
    button,
    false // Try out-turn first
);

if (directButtonVelocity.magnitude > 3f && directButtonVelocity.magnitude < 20f)
{
    pullbackPosition = CalculatePullbackFromVelocity(directButtonVelocity, launcherPos, false);
    useInTurn = false;
    
    Debug.Log($"[Physics Draw] ? FALLBACK: Direct button shot (out-turn), pullback: {pullbackPosition}");
    return true;
}

// Last resort: In-turn direct to button
directButtonVelocity = trajectorySimulator.CalculateVelocityToTarget(
    launcherPos,
    button,
    true
);

if (directButtonVelocity.magnitude > 3f && directButtonVelocity.magnitude < 20f)
{
    pullbackPosition = CalculatePullbackFromVelocity(directButtonVelocity, launcherPos, true);
    useInTurn = true;
    
    Debug.Log($"[Physics Draw] ? FALLBACK: Direct button shot (in-turn), pullback: {pullbackPosition}");
    return true;
}

Debug.LogError($"[Physics Draw] COMPLETE FAILURE - even direct button shot failed!");
pullbackPosition = launcherPos + new Vector2(0f, -2f);
useInTurn = false;
return false;
```

---

## Expected Results

### Before Fix
```
[Physics Draw] Generated 2 candidate positions (no guards)
[Physics Draw] Testing IN-TURN:
  Candidate (0, 6.5) ? Score: 15.2 (below threshold)
[Physics Draw] Testing OUT-TURN:
  Candidate (0, 6.5) ? Score: 14.8 (below threshold)
[Physics Draw] FAILED - bestScore 15.2 too low (need >= 20.0)
? Falls back to magic numbers ?
```

### After Fix
```
[Physics Draw] Generated 72 candidate positions:
  1 direct + 66 radial + 5 protected
[Physics Draw] Testing IN-TURN:
  (0, 6.5) ? Score: 15.2
  (0.3, 6.5) ? Score: 22.5 ?
  (0.6, 6.5) ? Score: 18.3
  (-0.3, 6.5) ? Score: 21.8 ?
  ...
[Physics Draw] Testing OUT-TURN:
  (0, 6.5) ? Score: 14.8
  (0.3, 6.5) ? Score: 28.1 ?? BEST!
  (0.6, 6.5) ? Score: 19.2
  ...
[Physics Draw] ? SUCCESS! Score: 28.1/125
  Final position: (0.3, 6.5)
  Turn: OUT-TURN
  Tested 72 candidates
? Uses physics! ?
```

---

## Why This Fixes Out-Turn Bias

**The ROOT cause of out-turn preference was:**
1. Limited candidates (only direct to target)
2. Direct path to (0, 6.5) might favor in-turn due to ice conditions
3. But OUT-TURN might score better at (0.3, 6.5) or (-0.3, 6.5)!
4. **We never tested those positions**, so out-turn never got a fair shot

**With full radial sweep:**
- We test (0, 6.5), (0.3, 6.5), (-0.3, 6.5), (0, 6.8), etc.
- OUT-TURN gets to compete at ALL positions
- **Best position wins**, regardless of turn direction
- **True physics-based selection** instead of positional bias!

---

## Implementation Priority

1. **CRITICAL:** Add full radial sweep (Fix 1) - Lines 1974-2024
2. **HIGH:** Fix guard team check (Fix 2) - Lines 2052-2078  
3. **MEDIUM:** Lower threshold + better fallback (Fix 3) - Lines 2145-2154

**Estimated LOC:** ~150 lines changed in `CalculatePhysicsBasedDrawShot()`

---

## Testing Verification

### Test Case 1: Empty House, No Guards
**Expected:** AI tests ~66 radial positions, finds best scoring position for each turn

### Test Case 2: Empty House, One Friendly Center Guard
**Expected:** AI tests 66 radial + 15 protected = ~81 positions, prefers protected draws

### Test Case 3: Opponent Rock at Button
**Expected:** AI tests all positions, finds one that beats opponent or gets close

### Test Case 4: Multiple Guards (Mixed Teams)
**Expected:** Only FRIENDLY guards give protection bonus, opponent guards are obstacles

---

## Debug Output Enhancement

Add to each candidate evaluation:

```csharp
if (score > bestScore)
{
    Debug.Log($"  ? NEW BEST: ({candidateTarget.x:F2}, {candidateTarget.y:F2}) ? " +
              $"Final: ({finalPos.x:F2}, {finalPos.y:F2}), " +
              $"Turn: {(tryInTurn ? "IN" : "OUT")}, " +
              $"Score: {score:F1}/125 " +
              $"(Prot: {protectionScore * 50f:F1}, Scoring: {scoringPositionScore * 30f:F1}, " +
              $"Collision: {collisionPenalty:F1}, Prox: {proximityScore:F1}, House: {houseBonus:F1})");
}
```

This will show **exactly why** each shot is chosen, making it clear if out-turns are being fairly evaluated!

---

**Status:** ? **FIX DESIGNED** - Ready for implementation
