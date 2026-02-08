# Rock Path Tracer - Line Renderer Improvements

## ?? **Problem**

The line renderer that follows the rock had several issues:
1. **Disappeared after AI turns** - Path was cleared when AI played
2. **Not visible when rock stopped** - Path disappeared when rock came to rest
3. **Cleared on each new rock** - Previous human shots weren't kept visible
4. **Flickering color** - Color recalculated every frame
5. **Collision lines too thin** - Hard to see predicted collision paths

---

## ? **Solution**

### **1. Human-Only Path Tracking**

The actual path line renderer now **only tracks human-thrown rocks**:

```csharp
// HUMAN ROCK PATH TRACKING
// Only track and display path for human-thrown rocks (not AI)
if (rock != null && rockInfo != null && !aiTurn)
{
    if (rockInfo.released && !rockInfo.rest)
    {
        // Rock is moving - actively trace the path
        Vector2 rockPos = new Vector2(rock.transform.position.x, rock.transform.position.y);
        actualPathPoints.Add(new Vector3(rockPos.x, rockPos.y, 0f));
        
        // Update line renderer
        actualPathLine.enabled = true;
        actualPathLine.positionCount = actualPathPoints.Count;
        actualPathLine.SetPositions(actualPathPoints.ToArray());
        
        // Calculate color every 5 frames (reduce flicker)
        if (actualPathPoints.Count % 5 == 0 && points != null && points.Count > 0)
        {
            // Find closest trajectory point
            float minDistance = float.MaxValue;
            foreach (Vector2 point in points)
            {
                float dist = Vector2.Distance(point, rockPos);
                if (dist < minDistance) minDistance = dist;
            }
            
            // On path: blue-green | Off path: orange-red
            float t = Mathf.Clamp01(minDistance / 0.3f);
            currentLineColor = Color.Lerp(
                new Color(0f, 0.8f, 0.6f), // Blue-green
                new Color(1f, 0.4f, 0f),   // Orange-red
                t
            );
        }
        
        actualPathLine.startColor = currentLineColor;
        actualPathLine.endColor = currentLineColor;
    }
    else if (rockInfo.released && rockInfo.rest)
    {
        // Rock has stopped - keep path visible but don't add more points
        actualPathLine.enabled = true;
    }
}
```

---

### **2. Path Persistence**

**Before:** Path cleared when:
- Rock stopped
- AI turn started
- New rock was selected

**After:** Path persists until:
- Next human rock is thrown (cleared in `Release()`)
- `ClearTrajectory()` is explicitly called

```csharp
public void Release()
{
    // Initialize actual path tracking for human player
    if (!aiTurn)
    {
        actualPathPoints.Clear();
        currentLineColor = new Color(0f, 0.8f, 0.6f); // Reset to blue-green
        if (rock != null)
        {
            Vector2 startPos = new Vector2(rock.transform.position.x, rock.transform.position.y);
            actualPathPoints.Add(new Vector3(startPos.x, startPos.y, 0f));
        }
    }
    // ... collision visualization cleanup ...
}
```

---

### **3. Improved Collision Lines**

Made collision prediction lines **2x thicker** and **brighter** for better visibility:

| Line Type | Old Width | New Width | Old Color | New Color |
|-----------|-----------|-----------|-----------|-----------|
| **Thrown Rock Post-Collision** | 0.08-0.04 | **0.15-0.08** | Orange (0.8 alpha) | **Bright Orange (0.9 alpha)** |
| **Hit Rock Post-Collision** | 0.08-0.04 | **0.15-0.08** | Green (0.8 alpha) | **Bright Green (0.9 alpha)** |
| **Hit Rock Direction** | 0.08-0.04 | **0.2-0.12** | Yellow (0.9 alpha) | **Bright Yellow (1.0 alpha)** |

```csharp
// Thrown rock path after collision (orange)
postCollisionLine.startWidth = 0.15f; // Was 0.08f
postCollisionLine.startColor = new Color(1f, 0.5f, 0f, 0.9f); // Brighter

// Hit rock path after collision (green)
hitRockPostCollisionLine.startWidth = 0.15f; // Was 0.08f
hitRockPostCollisionLine.startColor = new Color(0.3f, 1f, 0.3f, 0.9f); // Brighter

// Hit rock exit direction (yellow)
hitRockDirectionLine.startWidth = 0.2f; // Was 0.08f - THICKEST
hitRockDirectionLine.startColor = new Color(1f, 1f, 0f, 1f); // Full opacity
```

---

## ?? **User Flow**

### **Human Turn:**
1. **Pull back rock** ? Trajectory prediction shows (dots + line)
2. **Release rock** ? Rock starts moving
3. **Path traces** ? Blue-green line follows rock
   - Stays **blue-green** if on predicted path
   - Turns **orange-red** if deviating from path
4. **Rock stops** ? Path stays visible
5. **Next human rock** ? Previous path clears, new path starts

### **AI Turn:**
1. **AI shoots** ? No path tracking
2. **Rock moves** ? No line renderer
3. **Next human turn** ? Human path tracking resumes

### **Collision Visualization:**
1. **Prediction Phase** (aiming):
   - Main trajectory (white dots)
   - Collision point marker (if hitting rock)
   - **Orange line** ? Where your rock goes after hit
   - **Yellow line** ? Where hit rock exits (thick!)
   
2. **Actual Shot** (after release):
   - **Blue-green line** ? Your rock's actual path
   - Collision lines hidden (shows actual collision when it happens)

---

## ?? **Line Renderer Summary**

| Line Type | Color | Width | Purpose | When Visible |
|-----------|-------|-------|---------|--------------|
| **Trajectory Prediction** | White dots | Variable | Show predicted path | During aiming |
| **Actual Path** | Blue-green/Orange | 0.06-0.04 | Trace human rock path | During & after human shot |
| **Collision Marker** | Red/Green | - | Mark collision point | During aiming (prediction) |
| **Thrown Rock Exit** | Bright Orange | 0.15-0.08 | Show deflection path | During aiming (prediction) |
| **Hit Rock Exit** | Bright Yellow | **0.2-0.12** | Show hit rock direction | During aiming (prediction) |

---

## ?? **Technical Details**

### **Color Update Frequency:**
- **Before:** Every frame ? flickering
- **After:** Every 5 frames ? smooth

### **Path Tracking Logic:**
```
if (!aiTurn) {  // Human player only
    if (released && !rest) {
        // Add points while moving
    }
    else if (released && rest) {
        // Keep visible when stopped
    }
}
```

### **Clear Conditions:**
1. **Next human shot starts** (`Release()` called)
2. **Explicit clear** (`ClearTrajectory()` called)
3. **NOT cleared on AI turn** - persists through AI shots!

---

## ? **Build Status**

**Build Successful!** ??

---

## ?? **Next Steps**

You mentioned wanting to troubleshoot the **collision lines** next. Here's what we can improve:

### **Collision Lines Enhancement Ideas:**

1. **Show actual collision when it happens** (not just prediction)
   - Draw collision marker at actual impact point
   - Show actual deflection paths (not predicted)

2. **Make hit rock path longer** (currently only 3 points for direction)
   - Show full trajectory of hit rock after collision

3. **Add velocity indicators** (arrows showing speed/direction)
   - Arrow at collision point showing exit velocities

4. **Color code by impact force**
   - Light hit = yellow
   - Medium hit = orange  
   - Heavy hit = red

Let me know which collision improvements you'd like to tackle next! ??
