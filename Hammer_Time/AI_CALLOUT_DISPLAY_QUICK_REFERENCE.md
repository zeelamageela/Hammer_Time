# ?? AI CALLOUT DISPLAY - QUICK REFERENCE

## **Display Rules:**

### **1-2 Systems: One-Line Format**
```
"AI: SKILL"
"AI: SKILL + CLUTCH"
"AI: SKILL + EV OVERRIDE"
```

### **3+ Systems: Multi-Line Format**
```
"AI SYSTEMS:
SKILL
HIGH CLUTCH
COUNTER"
```

---

## **Common Callout Scenarios:**

### **Early Game (Rock 1-4):**
```
???????????????
? AI: SKILL   ?  ? 1 system
???????????????
```

### **Mid Game (Rock 6-10):**
```
???????????????????????
? AI: SKILL + CLUTCH  ?  ? 2 systems
???????????????????????
```

### **Late Game Pressure (Rock 12-16):**
```
????????????????????
? AI SYSTEMS:      ?  ? 3+ systems
? SKILL            ?
? HIGH CLUTCH      ?
? COUNTER          ?
????????????????????
```

### **Maximum Complexity (All Systems):**
```
????????????????????
? AI SYSTEMS:      ?  ? 4 systems active!
? SKILL            ?
? HIGH CLUTCH      ?
? COUNTER          ?
? EV OVERRIDE      ?
????????????????????
```

---

## **System Tags Reference:**

| Tag | Meaning | When It Appears |
|-----|---------|----------------|
| **SKILL** | Skill-based shot selection | Every AI shot |
| **CLUTCH** | Medium pressure (30-60) | Close games, late ends |
| **HIGH CLUTCH** | High pressure (60-100) | Last end tied, critical moments |
| **COUNTER** | Counter-strategy active | After detecting player pattern (3+ similar shots) |
| **EV** | EV evaluation used | When EV system enabled (didn't change shot) |
| **EV OVERRIDE** | EV changed the shot | When EV system overrides strategy choice |

---

## **Code Implementation:**

```csharp
// In ExecuteShot() method:
if (activeSystems.Count > 2)
{
    // Multi-line format for 3+ systems
    systemsText = "AI SYSTEMS:\n" + string.Join("\n", activeSystems);
}
else
{
    // One-line format for 1-2 systems
    systemsText = "AI: " + string.Join(" + ", activeSystems);
}
```

---

## **Visual Examples:**

### **Scenario 1: Skill Only**
```
Early game, no pressure:
???????????????
? AI: SKILL   ?
???????????????
```

### **Scenario 2: Skill + Clutch**
```
Late game, leading by 1:
???????????????????????
? AI: SKILL + CLUTCH  ?
???????????????????????
```

### **Scenario 3: Skill + High Clutch + Counter**
```
Last end tied, detected pattern:
????????????????????
? AI SYSTEMS:      ?
? SKILL            ?
? HIGH CLUTCH      ?
? COUNTER          ?
????????????????????
```

### **Scenario 4: All Systems Engaged**
```
Last shot, high pressure, pattern detected, EV override:
????????????????????
? AI SYSTEMS:      ?
? SKILL            ?
? HIGH CLUTCH      ?
? COUNTER          ?
? EV OVERRIDE      ?
????????????????????

This is the AI at MAXIMUM intelligence! ??
```

---

## **Benefits of Multi-Line Display:**

### **Readability:**
- ? Old: "AI: SKILL + HIGH CLUTCH + COUNTER + EV OVERRIDE" (too long!)
- ? New: "AI SYSTEMS:\nSKILL\nHIGH CLUTCH\nCOUNTER\nEV OVERRIDE" (clear!)

### **Scalability:**
- Can add more systems without text overflow
- Clean vertical stacking
- Easy to scan at a glance

### **Professional:**
- Looks more polished
- Easier to read during gameplay
- Better UX for players

---

## **Testing Checklist:**

- [ ] **1 System** - Shows "AI: SKILL"
- [ ] **2 Systems** - Shows "AI: SKILL + CLUTCH"
- [ ] **3 Systems** - Switches to multi-line "AI SYSTEMS:\n..."
- [ ] **4 Systems** - All systems show in multi-line format
- [ ] **Text Readable** - Not too small, easy to read
- [ ] **Follows Rock** - Callout moves with rock
- [ ] **Proper Duration** - Shows for 3 seconds

---

## **Build Status:**

**? BUILD SUCCESSFUL** - Multi-line callouts implemented!

---

## **Summary:**

**Smart Display Logic:**
- 1-2 systems ? One-line format
- 3+ systems ? Multi-line format

**Result:** Callouts are always readable, even when all enhancement systems are active! ???
