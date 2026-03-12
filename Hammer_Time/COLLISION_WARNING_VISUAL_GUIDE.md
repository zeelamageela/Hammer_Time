# Collision Warning System - Visual Guide

## Layout When Aiming with Collision

```
                    ICE SURFACE VIEW
                    
    Y = 8.4 ????????????????????????????????????????
            ?                                      ?
            ?         VERTICAL GUIDE LINE          ?
            ?              (WHITE)                 ?
    Y = 8.0 ?                 ?                    ?  <- HORIZONTAL LINE (team color)
            ?                 ?                    ?     Shows weight/distance
            ?                 ?                    ?
            ?                 ?                    ?
    Y = 7.0 ?                 ?         ?????     ?
            ?                 ?         ? X ? <------- COLLISION WARNING LINE (RED)
    Y = 6.5 ? ? ? ? ? ? ? ? ??? ? ? ? ????? ? ? ?  <- T-LINE
            ?                 ?    /              ?
            ?                 ?   /               ?
    Y = 6.0 ?                 ?  /                ?
            ?                 ? /    ?            ?  <- Guard Rock (will be hit)
    Y = 5.5 ?                 ?/ /                ?
            ?                /  /                 ?
    Y = 5.0 ?               /  /                  ?
            ?              /  /                   ?     CURL LINE (gradient fade)
    Y = 4.5 ?             /  /                    ?     Shows turn direction
            ?            /  /                     ?
    Y = 4.0 ?           /  /                      ?
            ?          /  /? TRAJECTORY DOTS      ?
    Y = 3.5 ?   ?     /  /   (variable size)     ?  <- House Rocks
            ?        /  /                         ?
    Y = 3.0 ?       /  /                          ?
            ?      /  /                           ?
    Y = 2.5 ?     /  /                            ?
            ?    /  /                             ?
    Y = 2.0 ?   /  /                              ?
            ?  /  /                               ?
    Y = 1.5 ? /  /                                ?
            ?/  /                                 ?
    Y = 1.0 ?  /                                  ?
            ? /                                   ?
    Y = 0.5 ?/                                    ?
            ?                                     ?
    Y = 0.0 ????????????????????????????????????????
                        HACK LINE
```

---

## Component Breakdown

### 1. Vertical Guide Line (White)
```
Purpose: Shows straight-line lateral aim
Logic: Straight-line projection from pullback
X Position: Based on pullback direction, NOT trajectory
Y Range: From trajectory endpoint up to Y=8.4
Width: Skill-based (0.04 - 0.10)
Color: Greyish-black #3A3A3A
```

### 2. Horizontal Line (Team Color)
```
Purpose: Shows weight/distance target
Logic: Fixed at trajectory endpoint Y
X Range: Left edge (-2.23) to right edge (2.25)
Width: Skill-based (0.04 - 0.10)
Color: Team color from shooting knob
```

### 3. Curl Line (Gradient Team Color)
```
Purpose: Shows turn and curl direction
Logic: From vertical line X to aim circle X
Y Position: At curl zone with 30/70 short bias
Width: Weight accuracy error range (skill-based)
Gradient: Skill-based fade (sharp/moderate/gradual)
```

### 4. Collision Warning Line (RED)
```
Purpose: Shows collision point on trajectory
Logic: Extracted from trajectory simulation
X Position: Exactly at collision point X
Y Range: ±0.25 units from collision point Y (0.5 total height)
Width: 0.06 (thin, subtle)
Color: Red rgba(255, 0, 0, 0.8)
```

---

## Comparison: Clear Path vs Collision Path

### Clear Path (No Collision)
```
        VERTICAL LINE (straight aim)
              ?
              ?
              ?
              ?
              ?         ? Target rock (away from path)
              ?        /
              ?       /
              ?      /
              ?     /
              ?    /
              ?   /
              ?  /  CURL LINE
              ? /
              ?/
             
NO COLLISION WARNING
? Clean visualization
? Player can see clear path
```

### Collision Path (Guard in Way)
```
        VERTICAL LINE (straight aim)
              ?
              ?
              ?   ?????  <- COLLISION WARNING (RED)
              ?   ? X ?     0.5 units tall
              ?   ?????
              ?    /
              ?   ?  Guard (will be hit!)
              ?  / \
              ? /   \
              ?/     \ TRAJECTORY curls into guard
             
COLLISION WARNING SHOWN
?? Player sees warning
?? Must adjust aim themselves
```

---

## Key Design Decisions

### ? What We DON'T Do
```
VERTICAL LINE FOLLOWING TRAJECTORY:
    
    Vertical line moves with trajectory:
              ?
               \      <- Follows curl
                \
                 \
                  ?  Target rock
                 
    PROBLEM: Makes hitting rocks TOO EASY
    Player just aims vertical line at target!
```

### ? What We DO
```
VERTICAL LINE STAYS STRAIGHT + COLLISION WARNING:
    
    Vertical line (straight):          Collision indicator:
              ?                               ?????
              ?                               ? X ?
              ?         Trajectory:           ?????
              ?           /                     /
              ?          /                     /
              ?         /                     /
              ?        ?  Target rock        ?
    
    Player sees:
    - Where straight-line aim points (vertical line)
    - Where trajectory will actually hit (collision warning)
    - Must judge lateral offset themselves (skill!)
```

---

## Skill Impact on Visualization

### High Skill Player (Aim: 90, Weight: 90)
```
THIN PRECISE LINES:
    ?      <- Vertical: 0.04 width
    ?
   ???     <- Horizontal: 0.04 width
    ?
    ?      <- Curl: 0.5 width (tight error)
     ?
      ?
      
Sharp fade on curl line (70% of distance)
Small weight error cone (±0.11m)
```

### Low Skill Player (Aim: 30, Weight: 30)
```
THICK UNCERTAIN LINES:
    ?      <- Vertical: 0.10 width
    ?
  ?????    <- Horizontal: 0.10 width
    ?
    ??     <- Curl: 1.8 width (wide error)
    ? ?
    ?  ?
       ?
       
Gradual fade on curl line (20% of distance)
Large weight error cone (±0.69m)
```

---

## Collision Warning States

### State 1: No Collision
```
collisionWarningLine.enabled = false;

No rocks in trajectory path
Clean shot to target
```

### State 2: Collision Detected (Aim Circle OFF)
```
collisionWarningLine.enabled = true;
Position: Centered at collision point
Height: 0.5 units (±0.25 from center)
Color: Red with 80% opacity

Rock detected in trajectory path
Warning shown at impact location
Part of guide lines system
```

### State 3: Aim Circle ON
```
collisionWarningLine.enabled = false;

Guide lines hidden (including collision warning)
Aim circle mode active
```

**Note:** Collision warning is part of the guide lines system, NOT the collision visualization toggle.

---

## Coordinate Examples

### Example 1: Button Draw with Guard
```
Pullback: (0.0, -25.0)
Launcher: (0.0, -25.0)
Velocity: (0.0, 12.5) - straight shot

Guard at: (0.2, 3.5)
Collision Point: (0.18, 3.48) - very close!

Vertical Line:
  X = 0.0 (straight-line aim)
  Y = 5.0 to 8.4
  
Collision Warning:
  X = 0.18 (actual collision X)
  Y = 3.23 to 3.73 (±0.25 from 3.48)
  Color = Red
  
Result: Player sees vertical line at X=0, warning at X=0.18
        Must aim slightly left to miss guard!
```

### Example 2: In-Turn Draw to Left Four Foot
```
Pullback: (0.0, -25.0)
Launcher: (0.0, -25.0)
Velocity: (0.0, 12.0) with in-turn

Target: (-0.61, 6.5) - Left Four Foot
Trajectory: Curls LEFT from straight-line

No collision detected

Vertical Line:
  X = 0.0 (straight-line would go center)
  Y = 6.0 to 8.4
  
Curl Line:
  Start X = 0.0 (vertical line)
  End X = -0.61 (aim circle)
  Gradient fade showing curl direction
  
Collision Warning:
  HIDDEN (no collision)
  
Result: Player sees how much rock will curl LEFT
        Clean path visualization
```

---

## Debug Information

### When Collision Warning Appears
```
[Collision Warning] Indicator drawn at (1.23, 4.56) - height: 0.5
```

### When Collision Warning Hidden
```
(No logs - simply disabled)
```

### Vertical Line Position
```
[Vertical Line] Using FINAL TRAJECTORY DOT Y position for endpoints: Y=6.50
```

---

## Visual Hierarchy (Sorting Order)

```
Layer 0: Ice surface
Layer 0: Trajectory line (main line renderer)
Layer 0: Trajectory dots (variable size)
Layer 1: Horizontal line (team color)
Layer 1: Vertical line (white)
Layer 1: Curl line (gradient)
Layer 2: Collision warning line (RED) <- TOPMOST
```

---

## Testing Scenarios

### ? Test 1: Clear Path
- Aim at button
- No rocks in path
- Should see: Vertical, Horizontal, Curl lines
- Should NOT see: Collision warning

### ? Test 2: Guard in Path
- Aim at button
- Guard at Y=3.5
- Should see: All lines + RED collision warning at guard

### ? Test 3: Toggle Collision Lines OFF
- Aim at button with guard
- Turn OFF collision lines toggle
- Should see: Vertical, Horizontal, Curl lines
- Should NOT see: Collision warning (even though collision exists)

### ? Test 4: Vertical Line Stays Straight
- Aim with in-turn curl
- Should see: Vertical line at straight-line X position
- Should NOT see: Vertical line following curl

### ? Test 5: Multiple Rocks
- Aim through crowded house
- Should see: Only FIRST collision warning
- Future enhancement: Show all collisions

---

## Summary

### Problem Solved
- Players need collision feedback
- But making vertical line follow trajectory = too easy

### Solution Implemented
- **Vertical Line**: Straight-line aim guide (skill-based)
- **Collision Warning**: Small red indicator at impact point
- **Separation**: Each serves different purpose

### Player Experience
- ? See where straight-line aim points (vertical line)
- ? See if trajectory will hit rock (collision warning)
- ? Must adjust aim themselves (skill required)
- ? Collision warning is informative, not instructive

---

## Build Status
? **Build Successful** - All visualization working correctly
