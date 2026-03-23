# ?? AI EV OPTIMIZATION - PAUSE MENU UI SETUP GUIDE

## ?? **What You're Building**

A pause menu panel that lets players control the AI's "brain" in real-time:
- **Toggle**: Enable/Disable EV optimization
- **Slider**: Adjust EV influence (0-100%)
- **Toggle**: Show debug logs

---

## ?? **Step-by-Step Unity Setup**

### **STEP 1: Open Pause Menu Prefab/Scene**

1. Find your pause menu (probably a Prefab in `Assets/Prefabs/UI/`)
2. Open it for editing
3. Locate the **Canvas** object

---

### **STEP 2: Create AI Settings Panel**

1. **Right-click Canvas** ? UI ? Panel
2. **Rename**: `AISettingsPanel`
3. **RectTransform** settings:
   - Anchor: Bottom-left or wherever you have space
   - Width: 400-500
   - Height: 200-250
   - Position: Adjust to fit your menu layout

4. **Image component** (background):
   - Color: Semi-transparent dark (e.g., `#000000CC`)
   - Material: None
   - Raycast Target: ? (checked)

---

### **STEP 3: Add Title Text**

1. **Right-click AISettingsPanel** ? UI ? Text - TextMeshPro
2. **Rename**: `TitleText`
3. **Settings**:
   - Text: `?? AI Difficulty Settings (Experimental)`
   - Font Size: 20-24
   - Alignment: Center
   - Color: White or Cyan
   - Position: Top of panel
   - Auto-size: OFF

---

### **STEP 4: Add EV Optimization Toggle**

1. **Right-click AISettingsPanel** ? UI ? Toggle
2. **Rename**: `EVOptimizationToggle`
3. **Position**: Below title text

#### **Configure Toggle Children:**

**Background (existing):**
- Image color: `#404040FF` (dark gray)
- Width: 40, Height: 20

**Checkmark (existing):**
- Image color: `#00FF00FF` (green)  
- Scale down to fit background

**Label (existing Text object):**
- **Right-click ? Replace with TextMeshPro** (optional but recommended)
- Text: `Enable EV Optimization (Smarter AI)`
- Font Size: 16
- Color: White
- Position: To the right of toggle

---

### **STEP 5: Add EV Weight Slider**

1. **Right-click AISettingsPanel** ? UI ? Slider
2. **Rename**: `EVWeightSlider`
3. **Position**: Below EV toggle

#### **Configure Slider Component:**
- **Min Value**: 0
- **Max Value**: 1
- **Value**: 0.3 (30% default)
- **Whole Numbers**: ? (UNCHECKED!)
- **Direction**: Left to Right

#### **Configure Slider Children:**

**Background (existing):**
- Image color: `#606060FF` (medium gray)
- Height: 10-15

**Fill Area ? Fill (existing):**
- Image color: `#00AAFFFF` (cyan/blue)
- Fill Amount: Will be controlled by slider

**Handle Slide Area ? Handle (existing):**
- Image color: `#FFFFFFFF` (white)
- Width/Height: 20-25

---

### **STEP 6: Add EV Weight Label**

1. **Right-click AISettingsPanel** ? UI ? Text - TextMeshPro
2. **Rename**: `EVWeightLabel`
3. **Position**: Above or to the right of slider
4. **Settings**:
   - Text: `EV Influence: 30%`
   - Font Size: 14-16
   - Color: White
   - Alignment: Left or Center

---

### **STEP 7: Add Debug Logging Toggle**

1. **Right-click AISettingsPanel** ? UI ? Toggle
2. **Rename**: `EVLoggingToggle`
3. **Position**: Below slider

#### **Configure Toggle:**

Same as Step 4, but:
- **Label Text**: `Show EV Debug Logs (Console)`
- **Checkmark color**: `#FFA500FF` (orange) to differentiate from main toggle

---

### **STEP 8: Add AISettingsUI Script**

1. **Select AISettingsPanel**
2. **Inspector ? Add Component**
3. Type: `AISettingsUI`
4. **Drag references:**

```
???????????????????????????????????????????
? AI Settings UI (Script)                 ?
???????????????????????????????????????????
? Ev Optimization Toggle:  [EVOptimizationToggle] ?
? Ev Weight Slider:        [EVWeightSlider]       ?
? Ev Weight Label:         [EVWeightLabel (Text)]  ?
? Ev Logging Toggle:       [EVLoggingToggle]      ?
???????????????????????????????????????????
```

**CRITICAL**: For `Ev Weight Label`, drag the **Text (TMP)** component itself, not the GameObject!

---

### **STEP 9: Layout (Optional Polish)**

For a cleaner look, add a **Vertical Layout Group** to `AISettingsPanel`:

1. **Select AISettingsPanel**
2. **Add Component** ? Vertical Layout Group
3. **Settings**:
   - Padding: Top 10, Bottom 10, Left 10, Right 10
   - Spacing: 10
   - Child Alignment: Upper Center
   - Control Child Size: Width ?, Height ?
   - Child Force Expand: Width ?, Height ?

---

## ?? **Recommended Layout**

```
????????????????????????????????????????
?   ?? AI Difficulty Settings (Exp)   ?
????????????????????????????????????????
?                                      ?
?  ? Enable EV Optimization (Smarter) ?
?                                      ?
?  EV Influence: 30%                   ?
?  [====??????????????]                ?
?                                      ?
?  ? Show EV Debug Logs (Console)     ?
?                                      ?
????????????????????????????????????????
```

---

## ? **Testing the UI**

### **In Editor:**

1. **Play the game**
2. **Open pause menu**
3. **Check**:
   - ? AI Settings panel is visible
   - ? Toggles respond to clicks
   - ? Slider moves smoothly
   - ? Label updates when slider moves

### **In Console:**

Look for these logs when you interact with UI:

```
[AISettingsUI] Initialized successfully
[AISettingsUI] EV Optimization ENABLED
[AISettingsUI] EV weight set to 50%
[AISettingsUI] EV Logging ENABLED
[EV] System ENABLED
[EV] Weight set to 50%
```

---

## ?? **Testing EV in Action**

### **Quick Test:**

1. **Start a game** against AI
2. **Pause** ? Enable EV Optimization
3. **Set slider** to 80% (strong influence)
4. **Enable** debug logs
5. **Resume** and watch console

### **What to Look For:**

```
[EV] Evaluating shot (Rock 2)
[EV Calc] RemoveThreat: Success=70%, Reward=8.0, Penalty=9.0, EV=3.90
[EV Calc] ScorePoints: Success=85%, Reward=9.5, Penalty=4.0, EV=7.48
[EV] ? OVERRIDE! ScorePoints (EV: 7.48) over RemoveThreat (EV: 3.90)
```

**^ This means AI chose a DRAW instead of TAKEOUT because EV was better!** ???

---

## ?? **UI Element Reference**

### **GameObject Hierarchy:**

```
Canvas
??? PauseMenuPanel
    ??? AISettingsPanel [AISettingsUI.cs]
        ??? TitleText (TMP)
        ??? EVOptimizationToggle
        ?   ??? Background (Image)
        ?   ??? Checkmark (Image)
        ?   ??? Label (TMP)
        ??? EVWeightLabel (TMP)          ? Drag THIS into script!
        ??? EVWeightSlider
        ?   ??? Background (Image)
        ?   ??? Fill Area
        ?   ?   ??? Fill (Image)
        ?   ??? Handle Slide Area
        ?       ??? Handle (Image)
        ??? EVLoggingToggle
            ??? Background (Image)
            ??? Checkmark (Image)
            ??? Label (TMP)
```

---

## ?? **Troubleshooting**

### **"AISettingsUI script not working"**

**Fix**: Make sure `Assets/Scripts/UI/AISettingsUI.cs` exists and compiles successfully.

### **"Label doesn't update when slider moves"**

**Fix**: You dragged the GameObject instead of the Text component. Drag the **Text (TMP)** component itself into `Ev Weight Label` field.

### **"Settings don't persist between games"**

**Expected behavior**! Settings reset each game session. If you want persistence, you'd need to save them to PlayerPrefs (let me know if you want this!).

### **"AI Strategy not found" warning**

**Fix**: This happens if no AI is in the scene. This is normal in menus - the script will auto-disable itself.

---

## ?? **EV Weight Guide**

| Weight | Behavior | Use Case |
|--------|----------|----------|
| **0%** | Pure intent-based (original AI) | Baseline testing |
| **30%** | Slight EV influence (recommended start) | Conservative enhancement |
| **50%** | Balanced blend | Standard difficulty |
| **70%** | Strong EV influence | "Smart AI" mode |
| **100%** | Pure EV decision-making | Experimental/Maximum difficulty |

---

## ?? **Advanced: Save Settings to PlayerPrefs**

If you want settings to persist between sessions, add this to `AISettingsUI.cs`:

```csharp
// In Start(), after InitializeUI():
LoadSettings();

// Add these new methods:
private void LoadSettings()
{
    if (PlayerPrefs.HasKey("AI_EV_Enabled"))
    {
        bool enabled = PlayerPrefs.GetInt("AI_EV_Enabled") == 1;
        if (evOptimizationToggle != null)
            evOptimizationToggle.isOn = enabled;
    }
    
    if (PlayerPrefs.HasKey("AI_EV_Weight"))
    {
        float weight = PlayerPrefs.GetFloat("AI_EV_Weight");
        if (evWeightSlider != null)
            evWeightSlider.value = weight;
    }
}

private void SaveSettings()
{
    if (evOptimizationToggle != null)
        PlayerPrefs.SetInt("AI_EV_Enabled", evOptimizationToggle.isOn ? 1 : 0);
    
    if (evWeightSlider != null)
        PlayerPrefs.SetFloat("AI_EV_Weight", evWeightSlider.value);
    
    PlayerPrefs.Save();
}

// Call SaveSettings() at the end of each OnEV*Changed() method
```

---

## ?? **You're Done!**

Your AI now has a **smart brain** that players can control! ???

**Next Steps:**
1. ? Build the UI (follow steps above)
2. ? Test with different EV weights
3. ? Watch AI make smarter decisions
4. ?? Optional: Tune reward/penalty values in `ExpectedValueCalculator.cs` for different playstyles

---

**Need help with the UI setup?** Just ask! I can walk you through any specific step. ??
