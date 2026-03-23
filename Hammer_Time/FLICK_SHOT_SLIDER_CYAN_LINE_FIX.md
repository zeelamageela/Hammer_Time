# ?? FLICK SHOT SLIDER & CYAN LINE TROUBLESHOOTING

## ?? **Issues Identified:**

### **Issue 1: Slider Not Activating**
**Problem:** The slider doesn't show up at all during power phase.

**Root Cause:** The `UpdateSpeedSlider()` method has a logic error - it's checking if slider is active BEFORE the slider has been initialized!

```csharp
// Line 470 - WRONG ORDER!
private void UpdateSpeedSlider()
{
    // Only update if we're in power phase AND slider is active
    if (currentPhase != FlickShotPhase.PowerPhase || !isSliderActive || speedSlider == null)
    {
        // Not in power phase - ensure slider is hidden
        if (speedSlider != null && speedSlider.gameObject.activeSelf)
        {
            speedSlider.gameObject.SetActive(false);  // ? This HIDES the slider we just enabled!
        }
        return;
    }
```

**The Problem:**
1. `StartPowerPhase()` calls `InitializeSpeedSlider()`
2. `InitializeSpeedSlider()` sets `speedSlider.gameObject.SetActive(true)`
3. `Update()` calls `UpdatePowerPhase()` 
4. `UpdatePowerPhase()` calls `UpdateSpeedSlider()`
5. `UpdateSpeedSlider()` sees slider is active and IMMEDIATELY HIDES IT again! ?

---

### **Issue 2: Cyan Line Not Matching Velocity**
**Problem:** The predicted stop position (cyan line) only matches actual shot location ~25% of the time.

**Root Causes:**

#### **A) Incorrect Velocity Calculation**
```csharp
// Line 238 - GetPredictedVelocity()
return Mathf.Lerp(minVel, maxVel, calculatedSpeed);
```

**Problem:** `calculatedSpeed` is only calculated AFTER release in `ReleaseFlickShot()`, but `GetPredictedVelocity()` is called BEFORE that!

**Result:** `calculatedSpeed` is 0 or stale, so velocity is always `minVel` (5 m/s)!

#### **B) Wrong Drag Time Formula**
```csharp
// Line 310 - CalculateSpeedBand()
float normalizedTime = Mathf.Clamp01((dragTime - minDragTime) / (maxDragTime - minDragTime));
normalizedTime = 1f - normalizedTime; // Invert so faster = higher
```

**Problem:** This formula doesn't match the slider's `idealDragTime` calculation!

**Slider uses:** `idealDragTime = 0.8s` (middle band, 50% normalized)  
**Speed calc uses:** Linear interpolation between `minDragTime` (0.1s) and `maxDragTime` (1.5s)

**Result:** Mismatched timing - slider shows one speed, actual velocity uses different formula!

#### **C) Trajectory Prediction Issues**
```csharp
// Line 246 - CalculatePredictedStopPosition()
Vector2 testVelocity = aimDirection * initialVelocity;
```

**Problems:**
1. Uses `initialVelocity` parameter, but this is calculated from wrong formula
2. Doesn't account for sweeping modifications
3. Doesn't use same physics simulation as actual rock launch
4. `isInTurn` (curl direction) might be incorrect at prediction time

---

## ? **THE FIXES:**

### **Fix 1: Slider Activation Logic**

**Change `UpdateSpeedSlider()` to check phase BEFORE hiding:**

```csharp
private void UpdateSpeedSlider()
{
    // CRITICAL FIX: Check if NOT in power phase FIRST, then hide
    if (currentPhase != FlickShotPhase.PowerPhase)
    {
        // Not in power phase anymore - ensure slider is hidden
        if (speedSlider != null && speedSlider.gameObject.activeSelf)
        {
            speedSlider.gameObject.SetActive(false);
            Debug.Log("[FlickShot] Slider hidden - not in power phase");
        }
        isSliderActive = false;
        return;
    }
    
    // Now check if slider exists and is initialized
    if (!isSliderActive || speedSlider == null)
    {
        Debug.LogWarning("[FlickShot] Slider not active or null - skipping update");
        return;
    }
    
    // Slider is in power phase AND initialized - proceed with update
    // ... rest of method
}
```

**Key Changes:**
- Check phase FIRST (exit if not PowerPhase)
- THEN check if slider is initialized
- Don't hide slider during PowerPhase!

---

### **Fix 2: Velocity Calculation During Drag**

**Add real-time velocity calculation in `UpdatePowerPhase()`:**

```csharp
private void UpdatePowerPhase()
{
    // Update speed slider animation
    UpdateSpeedSlider();
    
    // Wait for mouse down to start dragging
    if (!isPowerDragging)
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPowerDragging = true;
            powerDragStartTime = Time.time;
            swipePoints.Clear();
            
            // Add starting point at launcher
            Vector3 startPos = launcher.transform.position;
            startPos.z = -1f;
            swipePoints.Add(startPos);
            
            Debug.Log("[FlickShot] Power swipe started - draw your path!");
        }
        return;
    }
    
    // Get current mouse position in world space
    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector3 mousePos3D = new Vector3(mouseWorldPos.x, mouseWorldPos.y, -1f);
    
    // CRITICAL FIX: Calculate velocity DURING drag for preview!
    float currentDragTime = Time.time - powerDragStartTime;
    float currentY = Mathf.Clamp(mouseWorldPos.y, powerDragTargetY, powerDragStartY);
    float currentDragDistance = Mathf.Abs(currentY - powerDragStartY);
    
    // Calculate speed continuously (use same formula as final release!)
    CalculateSpeedBand(currentDragTime, currentDragDistance);
    float previewVelocity = GetPredictedVelocity();
    float previewStopY = CalculatePredictedStopPosition(previewVelocity);
    
    // Update cyan line DURING drag to show real-time prediction!
    if (predictedStopLine != null)
    {
        float lineWidth = 3f;
        Vector3 leftPoint = new Vector3(-lineWidth, previewStopY, -1f);
        Vector3 rightPoint = new Vector3(lineWidth, previewStopY, -1f);
        
        predictedStopLine.SetPosition(0, leftPoint);
        predictedStopLine.SetPosition(1, rightPoint);
        
        if (!predictedStopLine.enabled)
        {
            predictedStopLine.enabled = true;
            Debug.Log("[FlickShot] Cyan prediction line enabled during drag");
        }
    }
    
    // Add cursor position to trail with smoothing
    // ... rest of swipe trail code
}
```

**Key Changes:**
- Calculate velocity **continuously** during drag
- Update cyan line **in real-time** as player drags
- Use SAME formula for preview and final velocity
- Shows player what their current speed will produce

---

### **Fix 3: Unified Speed Formula**

**Create a single method for speed calculation:**

```csharp
/// <summary>
/// Calculate speed multiplier (0-1) from drag time
/// Used by both preview and final velocity calculation
/// </summary>
private float CalculateSpeedFromDragTime(float dragTime)
{
    // Match the slider's timing formula!
    // idealDragTime (0.8s) = perfect speed (0.5 normalized = middle band)
    
    // Map drag time to speed multiplier
    // Faster drag (shorter time) = higher speed
    // Slower drag (longer time) = lower speed
    
    float normalizedSpeed;
    
    if (dragTime <= minDragTime)
    {
        // Ultra-fast drag = maximum speed
        normalizedSpeed = 1.0f;
    }
    else if (dragTime >= maxDragTime)
    {
        // Ultra-slow drag = minimum speed
        normalizedSpeed = 0.0f;
    }
    else
    {
        // Linear interpolation: faster drag = higher speed
        // Invert so shorter time = higher speed
        normalizedSpeed = 1.0f - ((dragTime - minDragTime) / (maxDragTime - minDragTime));
    }
    
    // Apply forgiveness factor (optional - makes it easier)
    normalizedSpeed = Mathf.Lerp(0.5f, normalizedSpeed, 1f / forgivenessFactor);
    
    return Mathf.Clamp01(normalizedSpeed);
}
```

**Then use it everywhere:**

```csharp
private void CalculateSpeedBand(float dragTime, float dragDistance)
{
    // Use unified formula
    calculatedSpeed = CalculateSpeedFromDragTime(dragTime);
    
    // Calculate speed band from calculatedSpeed
    speedBand = Mathf.FloorToInt(calculatedSpeed * speedBands);
    speedBand = Mathf.Clamp(speedBand, 0, speedBands - 1);
}
```

---

### **Fix 4: Slider Timing Alignment**

**Update `CalculateIdealDragTime()` to match speed formula:**

```csharp
private float CalculateIdealDragTime()
{
    // Perfect band = middle band (0.5 normalized speed)
    // From CalculateSpeedFromDragTime: 0.5 = midpoint between min and max drag time
    
    float perfectNormalized = 0.5f; // Middle speed band
    
    // Solve for drag time that gives 0.5 normalized speed:
    // 0.5 = 1.0 - ((dragTime - minDragTime) / (maxDragTime - minDragTime))
    // 0.5 = (dragTime - minDragTime) / (maxDragTime - minDragTime)
    // dragTime = minDragTime + 0.5 * (maxDragTime - minDragTime)
    
    float idealTime = minDragTime + (0.5f * (maxDragTime - minDragTime));
    
    Debug.Log($"[FlickShot] Ideal drag time calculated: {idealTime:F2}s (min: {minDragTime:F2}, max: {maxDragTime:F2})");
    
    return idealTime;
}
```

**Key:** Slider ghost speed now EXACTLY matches the velocity formula!

---

### **Fix 5: Trajectory Prediction Accuracy**

**Improve `CalculatePredictedStopPosition()` to use EXACT physics:**

```csharp
private float CalculatePredictedStopPosition(float initialVelocity)
{
    // Use TrajectorySimulator to get REAL predicted stop position
    if (trajLine != null)
    {
        System.Type trajType = trajLine.GetType();
        System.Reflection.FieldInfo simulatorField = trajType.GetField("simulator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (simulatorField != null)
        {
            object simulator = simulatorField.GetValue(trajLine);
            if (simulator != null)
            {
                // CRITICAL: Use CURRENT aim direction (set in SetAimPosition)
                Vector2 testVelocity = aimDirection * initialVelocity;
                
                // CRITICAL: Get turn direction from Rock_Force RIGHT NOW
                bool isInTurn = false;
                Rock_Force rockForce = GetComponent<Rock_Force>();
                if (rockForce != null)
                {
                    // Use reflection to get flipAxis field
                    System.Type forceType = rockForce.GetType();
                    System.Reflection.FieldInfo flipAxisField = forceType.GetField("flipAxis", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (flipAxisField != null)
                    {
                        isInTurn = (bool)flipAxisField.GetValue(rockForce);
                        Debug.Log($"[FlickShot Prediction] Rock turn direction: {(isInTurn ? "IN-TURN" : "OUT-TURN")}");
                    }
                }
                
                // CRITICAL: Simulate from LAUNCHER position (0, -25)
                Vector2 startPos = new Vector2(0f, -25f);
                
                // Call SimulateTrajectory with CORRECT parameters
                System.Type simType = simulator.GetType();
                System.Reflection.MethodInfo simMethod = simType.GetMethod("SimulateTrajectory");
                
                if (simMethod != null)
                {
                    // Parameters: startPos, velocity, isInTurn, maxPoints, rocksInPlay, forPlayerPreview
                    object[] parameters = new object[] { startPos, testVelocity, isInTurn, 300, null, true };
                    object result = simMethod.Invoke(simulator, parameters);
                    
                    if (result is List<Vector2> trajectory && trajectory.Count > 0)
                    {
                        Vector2 finalPos = trajectory[trajectory.Count - 1];
                        
                        Debug.Log($"[FlickShot Prediction] TrajectorySimulator result:");
                        Debug.Log($"  Velocity: {initialVelocity:F1} m/s, Direction: {aimDirection}, Angle: {aimAngle:F1}°");
                        Debug.Log($"  Turn: {(isInTurn ? "IN" : "OUT")}, Points simulated: {trajectory.Count}");
                        Debug.Log($"  Predicted stop: Y = {finalPos.y:F2}");
                        
                        return finalPos.y;
                    }
                    else
                    {
                        Debug.LogWarning($"[FlickShot Prediction] SimulateTrajectory returned invalid result: {result?.GetType().Name ?? "null"}");
                    }
                }
            }
        }
    }
    
    // Fallback: Use simple physics estimate
    Debug.LogWarning("[FlickShot Prediction] TrajectorySimulator not available - using fallback formula");
    
    float hogLineY = -16f;
    float frictionFactor = 1.8f; // Approximate deceleration
    float estimatedDistance = (initialVelocity * initialVelocity) / (2f * frictionFactor);
    float predictedStopY = hogLineY + estimatedDistance;
    predictedStopY = Mathf.Clamp(predictedStopY, -16f, 15f);
    
    return predictedStopY;
}
```

**Key Improvements:**
- Gets turn direction RIGHT BEFORE prediction (not at initialization)
- Uses EXACT TrajectorySimulator (same code as normal shots!)
- Logs all parameters for debugging
- More maxPoints (300 vs 200) for longer shots

---

## ?? **Testing the Fixes:**

### **Test 1: Slider Visibility**

1. Enable Flick Shot mode
2. Aim with pullback (Phase 1)
3. Release aim (Phase 1 ? AimSet)
4. Click launcher (AimSet ? PowerPhase)
5. **WATCH FOR:** Slider should appear immediately at bottom of screen
6. **CHECK CONSOLE:** Should see `[FlickShot] Speed slider initialized`
7. **CHECK CONSOLE:** Should NOT see `Slider hidden` during power phase

**Expected:** Slider visible and animating ghost rock up and down.

---

### **Test 2: Cyan Line Real-Time Update**

1. Start power phase (slider visible)
2. Click and start dragging
3. **WATCH CYAN LINE:** Should appear and move UP/DOWN as you drag faster/slower
4. **SLOW DRAG:** Cyan line near bottom (slow = short distance)
5. **FAST DRAG:** Cyan line near top (fast = long distance)
6. **CHECK CONSOLE:** Should see continuous prediction updates

**Expected:** Cyan line updates smoothly during drag, showing WHERE the rock will stop based on current speed.

---

### **Test 3: Velocity Accuracy**

1. Complete a flick shot with medium speed drag (~0.8s)
2. Note the cyan line position (e.g., Y = 5.0)
3. Watch rock travel and stop
4. **COMPARE:** Rock final Y should be within ±2 units of cyan line
5. Repeat 5 times with different speeds

**Expected:** 
- Cyan line accuracy: 80%+ shots within ±2 units
- If off, check console for prediction logs

---

## ?? **Debug Output Guide:**

### **Slider Initialization:**
```
[FlickShot] Found speed slider: FlickShotSpeedSlider
[FlickShot] Found slider handle image
[FlickShot] Speed slider GameObject re-enabled for new turn
[FlickShot] Speed slider initialized - ideal time: 0.80s
```

### **During Drag (new):**
```
[FlickShot] Power swipe started - draw your path!
[FlickShot Prediction] Rock turn direction: IN-TURN
[FlickShot Prediction] TrajectorySimulator result:
  Velocity: 9.5 m/s, Direction: (0.0, 1.0), Angle: 90.0°
  Turn: IN, Points simulated: 245
  Predicted stop: Y = 6.75
```

### **After Release:**
```
[FlickShot] RELEASED - Time: 0.823s, Speed: 0.52, Band: 2
[FlickShot] Final velocity: 9.6 m/s at angle 90.0°
[FlickShot] Predicted stop line shown at Y=6.80
```

**Compare:** During-drag prediction (6.75) should match final prediction (6.80)!

---

## ?? **Expected Results After Fixes:**

? **Slider shows up** immediately when entering power phase  
? **Ghost rock animates** at correct "ideal" speed  
? **Cyan line appears** as soon as player starts dragging  
? **Cyan line moves** smoothly as drag speed changes  
? **Cyan line accuracy** improves to 80%+ (within ±2 units)  
? **Slider timing matches** velocity formula (ghost speed = actual speed)  

---

## ?? **If Issues Persist:**

### **Slider Still Not Showing:**
1. Check GameObject exists: `GameObject.Find("FlickShotSpeedSlider")`
2. Check initial state: Is it enabled in scene by default?
3. Add breakpoint in `InitializeSpeedSlider()` - does it get called?
4. Check parent Canvas - is it active?

### **Cyan Line Still Inaccurate:**
1. Check console for prediction logs
2. Compare `testVelocity` with actual launch velocity
3. Check if `aimDirection` is correct (should point UP toward house)
4. Verify `isInTurn` matches actual rock turn
5. Check if `TrajectorySimulator` parameters match `TrajectoryLine.ShowTrajectory()`

### **Slider Shows But Ghost Speed Wrong:**
1. Check `idealDragTime` value (should be ~0.8s for medium speed)
2. Verify `minDragTime` (0.1s) and `maxDragTime` (1.5s) are reasonable
3. Compare ghost animation duration with actual drag time that produces same result

---

**Implementation order:**
1. Fix slider activation logic FIRST (most critical!)
2. Add real-time velocity calculation during drag
3. Update cyan line continuously
4. Unify speed formula
5. Test and iterate on trajectory prediction accuracy

Good luck! ??
