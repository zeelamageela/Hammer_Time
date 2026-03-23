# ?? AI EV OPTIMIZATION - TROUBLESHOOTING GUIDE

## ? **Common Issues & Solutions**

---

### **Issue 1: "Toggles don't update inspector values!"**

**STATUS: ? This is EXPECTED BEHAVIOR!**

#### **What's Happening:**
- UI toggles update **runtime** values (in memory)
- Inspector shows **default** values (serialized to disk)
- When you play, runtime values START at inspector defaults, then UI can change them

#### **How to Verify It's Working:**

1. **Select AISettingsPanel** in Hierarchy
2. **Inspector** ? Set `Debug Mode = true`
3. **Play** the game
4. **Open pause menu** ? Toggle AI settings
5. **Watch Console** - should see:
```
[AISettingsUI] Toggle changed to True
[AISettingsUI] Updated AI_Strategy.useEVOptimization to True
[AISettingsUI] ? EV Optimization ENABLED
```

6. **While PLAYING**, select AI_Strategy in Hierarchy
7. **Inspector** ? Scroll to EV System section
8. **The values ARE CHANGING** (runtime values)!

#### **Key Understanding:**
```
Inspector (NOT playing) = Default values (saved to disk)
Inspector (WHILE playing) = Runtime values (in memory, can change!)
```

**This is how Unity works!** UI controls affect **runtime**, not **default** values. ?

---

### **Issue 2: "Settings don't save between games"**

**STATUS: ? Expected behavior (by design)**

Settings reset each session. To persist them, see **Advanced: Save Settings to PlayerPrefs** section in main guide.

---

### **Issue 3: "AI Strategy not found warning"**

**STATUS: ? Normal in menu scenes**

AI_Strategy only exists in game scenes. In menus, the script auto-disables itself. This is expected.

---

### **Issue 4: "Label doesn't update when slider moves"**

**FIX:** You dragged the GameObject instead of the Text component.

**Solution:**
1. Delete the current reference
2. **Expand** `EVWeightLabel` GameObject in Hierarchy
3. Drag the **Text (TMP)** component (child) into `Ev Weight Label` field
4. The field should show: `Text (TextMeshProUGUI)`

---

### **Issue 5: "EVEvaluationSystem not found warning"**

**STATUS: ? Normal (created dynamically)**

EVEvaluationSystem is created by AI_Strategy at game start (`AI_Strategy.Start()`). 

**To verify:**
1. **Start a game** (not just main menu)
2. **Pause** ? Check console
3. Should see: `[AI_Strategy] EV System initialized`
4. **Hierarchy** ? You'll see `EVSystem` GameObject under AI_Strategy

The warning disappears once you start an actual game!

---

### **Issue 6: "Console shows 'Initialized successfully' but toggles do nothing"**

**FIX:** Check that UI references are connected

**Solution:**
1. **Select AISettingsPanel** in Hierarchy
2. **Inspector** ? AISettingsUI component
3. **Verify ALL references are assigned:**
   - `Ev Optimization Toggle` ? Should show Toggle component
   - `Ev Weight Slider` ? Should show Slider component
   - `Ev Weight Label` ? Should show **Text (TMP)** component (NOT GameObject!)
   - `Ev Logging Toggle` ? Should show Toggle component

4. If any are "None", drag the correct components

---

### **Issue 7: "Build errors about AI_Strategy not found"**

**STATUS: ? FIXED in latest version**

Latest `AISettingsUI.cs` uses reflection to avoid compile-time dependencies.

**To verify you have the fix:**
1. Open `Assets/Scripts/UI/AISettingsUI.cs`
2. Check line 4: Should have `using System.Linq;`
3. Check Start() method: Should use `System.AppDomain.CurrentDomain.GetAssemblies()`

If not, re-download the script or copy from AI_EV_OPTIMIZATION_COMPLETE.md

---

## ? **How to Test Everything Works**

### **Quick Test (5 minutes):**

1. **Play the game** (start an AI match)
2. **Pause** ? AI Settings panel visible? ?
3. **Toggle** "Enable EV Optimization" ? Check console for log ?
4. **Slide** EV Weight ? Does label update? ?
5. **Toggle** "Show EV Debug Logs" ? Check console for log ?
6. **Resume game** ? Does AI make a shot? ?
7. **Check console** ? Any "[EV]" logs if logging enabled? ?

If ALL ? pass ? **System working perfectly!** ??

---

### **Deep Test (10 minutes):**

1. **Enable debug mode** (AISettingsPanel ? Debug Mode = true)
2. **Play game** ? Check startup logs:
```
[AISettingsUI] Starting initialization...
[AISettingsUI] Found AI_Strategy: [GameObject name]
[AISettingsUI] Set toggle to False
[AISettingsUI] Set slider to 0.3
[AISettingsUI] ? Initialized successfully
```

3. **Change each setting** ? Check console for update logs:
```
[AISettingsUI] Toggle changed to True
[AISettingsUI] Updated AI_Strategy.useEVOptimization to True
[AISettingsUI] Updated EVEvaluationSystem.useEVEvaluation to True
[AISettingsUI] ? EV Optimization ENABLED
```

4. **Resume game** ? AI makes shot ? Check for EV logs (if enabled):
```
[EV] Evaluating shot (Rock 2)
[EV Calc] RemoveThreat: Success=70%, EV=3.90
[EV Calc] ScorePoints: Success=85%, EV=7.48
[EV] ? OVERRIDE! ScorePoints over RemoveThreat
```

If you see ALL these logs ? **PERFECT! System 100% functional!** ???

---

## ?? **Expected Console Output (Full Session)**

```
// ===== GAME START =====
[AI_Strategy] EV System initialized (Enabled: False, Weight: 0.30)

// ===== PAUSE MENU OPENED =====
[AISettingsUI] Starting initialization...
[AISettingsUI] Found AI_Strategy: AIManager
[AISettingsUI] Initializing UI - useEV=False, weight=0.3, logging=False
[AISettingsUI] Set toggle to False
[AISettingsUI] Set slider to 0.3
[AISettingsUI] Set logging toggle to False
[AISettingsUI] EV Optimization toggle listener added
[AISettingsUI] EV Weight slider listener added
[AISettingsUI] EV Logging toggle listener added
[AISettingsUI] ? Initialized successfully

// ===== USER TOGGLES SETTINGS =====
[AISettingsUI] Toggle changed to True
[AISettingsUI] Updated AI_Strategy.useEVOptimization to True
[AISettingsUI] Updated EVEvaluationSystem.useEVEvaluation to True
[AISettingsUI] ? EV Optimization ENABLED

[AISettingsUI] Slider changed to 0.7
[AISettingsUI] Updated AI_Strategy.evWeight to 0.7
[AISettingsUI] Updated EVEvaluationSystem.evWeight to 0.7
[AISettingsUI] ? EV Weight set to 70%

[AISettingsUI] Logging toggle changed to True
[AISettingsUI] Updated AI_Strategy.evVerboseLogging to True
[AISettingsUI] Updated EVEvaluationSystem.verboseLogging to True
[AISettingsUI] ? EV Logging ENABLED

// ===== RESUME GAME, AI TAKES SHOT =====
[IntentBased] ConservativeSteal - middle phase
[EV] Evaluating shot (Rock 4)
[EV Calc] RemoveThreat: Success=75%, Reward=12.0, Penalty=9.0, EV=6.75
[EV Calc] ScorePoints: Success=80%, Reward=10.0, Penalty=4.0, EV=7.20
[EV Calc] CreateOpportunity: Success=78%, Reward=8.0, Penalty=2.0, EV=5.80
[EV] ? OVERRIDE! ScorePoints (EV: 7.20) over intent RemoveThreat (EV: 6.75)
[ConservativeSteal] ? Intent-based shot selected!
```

**If you see this ? EVERYTHING WORKING PERFECTLY!** ?????

---

## ?? **Still Having Issues?**

1. **Set debug mode = true** in AISettingsUI
2. **Copy ALL console output** (full log)
3. **Share the logs** - we can diagnose from there!

Most issues are just misunderstanding Unity's runtime vs inspector behavior. The system IS working - you just need to test it correctly! ??

---

**Built with ?? and ?? by GitHub Copilot**
