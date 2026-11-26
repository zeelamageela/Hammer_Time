using UnityEngine;

/// <summary>
/// The arrow on a rock in an aimed hit: it points the way the rock leaves the contact, and its length says how far it will go.
/// A rock that barely moves (the shooter on a nose hit) gets no arrow, because its direction means nothing.
/// </summary>
public static class HitArrow
{
    /// <summary>A rock that would slide less than this after the hit (about a rock and a half) gets no arrow.</summary>
    public const float MinTravel = 0.4f;
    public const float MinLength = 0.3f;
    public const float MaxLength = 1.0f;
    /// <summary>Travel at which the arrow is at its longest.</summary>
    public const float FullLengthTravel = 5f;

    /// <summary>The arrow's length for a rock leaving the contact at this velocity; 0 = no arrow.</summary>
    public static float Length(Vector2 velocityAfterHit, RockPhysicsProfile p)
    {
        float travel = p.StopDistance(velocityAfterHit.magnitude);
        if (travel < MinTravel) return 0f;
        return Mathf.Lerp(MinLength, MaxLength, Mathf.InverseLerp(MinTravel, FullLengthTravel, travel));
    }
}
