using UnityEngine;

/// <summary>
/// How good the player's read of the ice was for a throw: the same throw, with no shooter error and a clear sheet, on the read and on
/// the real ice, and how far apart the two finish. Rocks that run through the house are compared as they cross its back, so a heavy
/// shot is judged on its line. Green is locked on, yellow close, red way off.
/// </summary>
public static class ReadGrade
{
    public const float LockedFeet = 1.5f, CloseFeet = 3.5f, OffFeet = 7f;

    public static readonly Color Green = new Color(0.10f, 0.72f, 0.24f);
    public static readonly Color Yellow = new Color(0.98f, 0.78f, 0.05f);
    public static readonly Color Red = new Color(0.90f, 0.12f, 0.12f);

    const float BackOfHouseY = 8.5f;

    /// <param name="weightVelocity">The throw as aimed (weight units).</param>
    public static float MissFeet(Vector2 launcher, Vector2 weightVelocity, bool inTurn, RockPhysicsProfile read, RockPhysicsProfile real)
    {
        return Vector2.Distance(Finish(launcher, weightVelocity, inTurn, read), Finish(launcher, weightVelocity, inTurn, real))
            * IceUnits.FeetPerUnit;
    }

    public static Color Colour(float missFeet)
    {
        return missFeet <= CloseFeet
            ? Color.Lerp(Green, Yellow, Mathf.InverseLerp(LockedFeet, CloseFeet, missFeet))
            : Color.Lerp(Yellow, Red, Mathf.InverseLerp(CloseFeet, OffFeet, missFeet));
    }

    /// <summary>Where the throw ends up: at rest, or where it crosses the back of the house.</summary>
    static Vector2 Finish(Vector2 launcher, Vector2 weightVelocity, bool inTurn, RockPhysicsProfile p)
    {
        const float dt = 0.02f;
        RockState rock = RockPhysics.Launch(launcher, p.LaunchVelocityFromWeightVelocity(weightVelocity), p);
        for (float t = 0f; t < 60f * p.timeStretch && !rock.asleep; t += dt)
        {
            Vector2 before = rock.position;
            RockPhysics.Step(ref rock, p, dt);
            if (before.y < BackOfHouseY && rock.position.y >= BackOfHouseY)
                return Vector2.Lerp(before, rock.position, Mathf.InverseLerp(before.y, rock.position.y, BackOfHouseY));
            if (rock.freeFlight && rock.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref rock, p, inTurn);
        }
        return rock.position;
    }
}
