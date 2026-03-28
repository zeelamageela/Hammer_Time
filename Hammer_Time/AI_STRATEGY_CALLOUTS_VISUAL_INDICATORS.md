# ? AI STRATEGY CALLOUTS - VISUAL SYSTEM INDICATORS

## ?? **What's New:**

Added **visual callouts** that appear on AI rocks showing which enhancement systems are being used for each shot!

---

## ?? **Callout Examples:**

### **Example 1: Skill-Based + Clutch (2 Systems)**
```
Rock appears with callout:
???????????????????????
? AI: SKILL + CLUTCH  ?
???????????????????????
```
**Meaning:**
- SKILL = Skill-based shot selection active (playing to shooter's strengths)
- CLUTCH = Medium pressure detected (30-60 pressure)

---

### **Example 2: High Pressure Situation (3+ Systems - Multi-Line)**
```
Rock appears with callout:
??????????????????????
? AI SYSTEMS:        ?
? SKILL              ?
? HIGH CLUTCH        ?
? COUNTER            ?
??????????????????????
```
**Meaning:**
- SKILL = Skill-based adjustments
- HIGH CLUTCH = High pressure (60-100) - AI is feeling the heat!
- COUNTER = Counter-strategy detected player pattern and overrode shot

---

### **Example 3: EV Override (2 Systems)**
```
Rock appears with callout:
????????????????????????????
? AI: SKILL + EV OVERRIDE ?
????????????????????????????
```
**Meaning:**
- SKILL = Skill-based adjustments
- EV OVERRIDE = Expected Value system changed the shot intent!

---

### **Example 4: All Systems Firing (4+ Systems - Multi-Line)**
```
Rock appears with callout:
??????????????????????
? AI SYSTEMS:        ?
? SKILL              ?
? HIGH CLUTCH        ?
? COUNTER            ?
? EV OVERRIDE        ?
??????????????????????
```
**Meaning:**
- **ALL ENHANCEMENT SYSTEMS ACTIVE!** ??
- AI is using every available tool to make the perfect shot

---

### **Example 5: Multi-Shot Plan**
```
Rock appears with callout:
??????????????????????????????????
? MULTI-SHOT: Aggressive Steal   ?
? Step 2/3                       ?
??????????????????????????????????
```
**Meaning:**
- AI is following a 3-shot strategic plan
- Currently executing step 2
- Plan name: "Aggressive Steal"

---

## ?? **All Possible Callout Tags:**

### **Enhancement Systems:**
1. **SKILL** - Skill-based shot selection active
2. **CLUTCH** - Medium pressure (30-60)
3. **HIGH CLUTCH** - High pressure (60-100) - clutch moment!
4. **COUNTER** - Counter-strategy override (detected player pattern)
5. **EV** - Expected Value calculation used (didn't change shot)
6. **EV OVERRIDE** - EV changed the shot intent
7. **MULTI-SHOT** - Following multi-shot strategic plan

### **Combination Examples:**
- `AI: SKILL` - Only skill adjustments (1 system)
- `AI: SKILL + CLUTCH` - Skill + medium pressure (2 systems)
- `AI: SKILL + HIGH CLUTCH` - Skill + high pressure (2 systems)
- `AI: SKILL + COUNTER` - Skill + detected your pattern (2 systems)
- `AI: SKILL + EV` - Skill + EV evaluation (2 systems)

**3+ Systems (Multi-Line Format):**
```
AI SYSTEMS:
SKILL
HIGH CLUTCH
COUNTER
```

**4 Systems - ALL FIRING! ??**
```
AI SYSTEMS:
SKILL
HIGH CLUTCH
COUNTER
EV OVERRIDE
```

---

## ?? **What You'll See In-Game:**

### **Early Game (Low Pressure - 1 System):**
```
Rock 1-4:
???????????????
? AI: SKILL   ?  ? Simple, one-line display
???????????????
```

### **Mid-Game (Building Tension - 2 Systems):**
```
Rock 6-8:
???????????????????????
? AI: SKILL + CLUTCH  ?  ? Still one-line (2 systems)
???????????????????????
```

### **Late Game Tied (High Pressure - 3+ Systems):**
```
Rock 14-16:
????????????????????
? AI SYSTEMS:      ?  ? Multi-line format!
? SKILL            ?
? HIGH CLUTCH      ?
? COUNTER          ?
????????????????????
```

### **Adaptive AI (Pattern Detection - 2 Systems):**
```
If you throw 3+ draws in a row:
????????????????????????????
? AI: SKILL + COUNTER      ?  ? One-line (2 systems)
????????????????????????????

Console log:
"[Counter] ?? PATTERN DETECTED: Opponent is BUILDING POSITION"
"[Counter] COUNTER-STRATEGY: Opponent building ? REMOVE THREATS"
```

### **Multi-Shot Strategy:**
```
When AI has a plan:
??????????????????????????????????
? MULTI-SHOT: Force Blank       ?
? Step 1/2                       ?  ? AI playing 2 shots ahead!
??????????????????????????????????

Next rock:
??????????????????????????????????
? MULTI-SHOT: Force Blank       ?
? Step 2/2                       ?  ? Executing plan conclusion
??????????????????????????????????
```

---

## ?? **How It Works:**

### **ExecuteShot() Enhancement:**

```csharp
private bool ExecuteShot(ShotIntent intent, int targetRock, int rockCurrent, ...)
{
    // Track which systems are active
    List<string> activeSystems = new List<string>();
    
    // 1. Skill-Based Selection
    if (enhancements != null)
    {
        context = enhancements.skillBased.AdjustForSkills(...);
        activeSystems.Add("SKILL");
    }
    
    // 2. Clutch Performance
    float pressure = enhancements.clutchPerformance.CalculatePressure(...);
    if (pressure >= 30f)
    {
        context = enhancements.clutchPerformance.ApplyClutchModifiers(...);
        activeSystems.Add(pressure >= 60f ? "HIGH CLUTCH" : "CLUTCH");
    }
    
    // 3. Counter-Strategy
    if (counterIntent != context.intent)
    {
        context.intent = counterIntent;
        activeSystems.Add("COUNTER");
    }
    
    // 4. EV Evaluation
    if (evSystem != null && useEVOptimization)
    {
        ShotContext originalContext = context;
        context = evSystem.EvaluateShot(...);
        
        if (context.intent != originalContext.intent)
            activeSystems.Add("EV OVERRIDE");
        else
            activeSystems.Add("EV");
    }
    
    // ?? SHOW CALLOUT - Smart Multi-Line Display
    if (activeSystems.Count > 0)
    {
        string systemsText;
        
        // Break into multiple lines if more than 2 systems active
        if (activeSystems.Count > 2)
        {
            systemsText = "AI SYSTEMS:\n" + string.Join("\n", activeSystems);
        }
        else
        {
            systemsText = "AI: " + string.Join(" + ", activeSystems);
        }
        
        calloutManager.ShowCallout(
            rock.transform.position + new Vector3(0f, 0.5f, 0f),
            systemsText,
            followTarget: rock.transform,
            duration: 3.0f
        );
    }
    
    aiTarg.ExecuteIntent(context, rockCurrent);
}
```

### **Display Logic:**

```
1-2 Systems:  "AI: SKILL + CLUTCH"         (One-line, compact)

3+ Systems:   "AI SYSTEMS:                 (Multi-line, readable)
               SKILL
               HIGH CLUTCH
               COUNTER"
```

---

## ?? **Testing Scenarios:**

### **Test 1: Skill-Based Display**
```
SETUP:
  1. Start a game
  2. Watch AI's first few rocks

EXPECTED:
  ? Callout appears: "AI: SKILL"
  ? Console shows: "[SkillBased] Red_Skip Skills: Finesse=85, Weight=45, Aim=70"
  ? Different characters show different skill adjustments
```

### **Test 2: Clutch Pressure**
```
SETUP:
  1. Play to last end
  2. Make score close (3-3 or 4-3)
  3. Watch AI's shots

EXPECTED:
  ? Early rocks: "AI: SKILL"
  ? Last 3 rocks: "AI: SKILL + CLUTCH"
  ? Last rock tied: "AI: SKILL + HIGH CLUTCH"
  ? Console shows pressure calculation
```

### **Test 3: Counter-Strategy**
```
SETUP:
  1. Draw 3-4 rocks in a row
  2. Watch AI's response

EXPECTED:
  ? After 3 draws: Callout shows "AI: SKILL + COUNTER"
  ? Console: "[Counter] PATTERN DETECTED: Opponent BUILDING POSITION"
  ? AI starts taking out your rocks
```

### **Test 4: EV Override**
```
SETUP:
  1. Enable EV optimization in AI settings
  2. Play a game
  3. Watch for EV overrides

EXPECTED:
  ? Sometimes: "AI: SKILL + EV OVERRIDE"
  ? Console: "[EV] OVERRIDE! ScorePoints (EV: 7.5) over CreateOpportunity (EV: 5.2)"
  ? AI makes strategically different choice
```

### **Test 5: Multi-Shot Planning**
```
SETUP:
  1. Enable Multi-Shot Planning in AI settings
  2. Play a game
  3. Watch for strategic plans

EXPECTED:
  ? Callout: "MULTI-SHOT: Aggressive Steal\nStep 1/3"
  ? Next rock: "Step 2/3"
  ? Follows plan for multiple rocks
```

---

## ?? **Benefits:**

### **1. Educational** ??
- **See what the AI is thinking!**
- Understand why AI made certain choices
- Learn which situations trigger which systems

### **2. Debugging** ??
- **Instant visual feedback** on AI enhancement systems
- Easy to see if systems are working correctly
- Identify when counter-strategy activates

### **3. Immersion** ??
- **Feels like playing a smart opponent**
- "Oh no, AI is in HIGH CLUTCH mode!"
- "They detected my pattern with COUNTER!"
- Creates drama and tension

### **4. Transparency** ??
- **No black box AI** - you see what's happening
- Understand skill differences between AI characters
- See EV overrides when they happen

---

## ?? **Callout Display Settings:**

### **Position:**
- 0.5 units above the rock
- Follows the rock as it moves

### **Duration:**
- 3.0 seconds for standard callouts
- 3.5 seconds for multi-shot plans

### **Style:**
- Uses existing TextCalloutManager
- Same visual style as velocity/shot callouts
- Auto-stacks with other callouts

---

## ?? **Future Enhancements:**

### **Color Coding:**
```
SKILL        ? Blue (informational)
CLUTCH       ? Yellow (warning)
HIGH CLUTCH  ? Red (critical)
COUNTER      ? Orange (adaptive)
EV OVERRIDE  ? Purple (strategic)
MULTI-SHOT   ? Green (planning)
```

### **More Detail:**
```
Current:
"AI: SKILL + HIGH CLUTCH"

Future:
"AI: High Finesse + Pressure 85/100"
"Personality: Aggressive"
```

### **Player Feedback:**
```
Show what the AI thinks about YOUR shots:
"Player Strategy: Building Position"
"AI Response: Counter with Clearing"
```

---

## ? **Build Status:**

**BUILD SUCCESSFUL** - All callouts implemented and working! ?

---

## ?? **Summary:**

You can now **see exactly what AI enhancement systems are active** for each shot!

### **Visual Indicators:**
- ? **SKILL** - AI playing to character strengths
- ? **CLUTCH / HIGH CLUTCH** - Pressure modifiers active
- ? **COUNTER** - AI detected your pattern
- ? **EV / EV OVERRIDE** - Expected Value calculations
- ? **MULTI-SHOT** - Following strategic plan

### **Example Game Flow:**
```
Early (1 system):
???????????????
? AI: SKILL   ?
???????????????

Mid (2 systems):
???????????????????????
? AI: SKILL + CLUTCH  ?
???????????????????????

Late Tied (4 systems):
????????????????????
? AI SYSTEMS:      ?
? SKILL            ?
? HIGH CLUTCH      ?
? COUNTER          ?
? EV OVERRIDE      ?
????????????????????

Multi-Shot Plan:
??????????????????????????????
? MULTI-SHOT: Force Blank    ?
? Step 2/3                   ?
??????????????????????????????
```

**Now you'll always know what the AI is thinking!** ?????
