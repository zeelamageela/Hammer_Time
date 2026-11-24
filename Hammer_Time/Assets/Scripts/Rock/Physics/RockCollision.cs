using UnityEngine;

/// <summary>
/// What happens when rocks touch. The live rocks are resolved by Box2D from the rock's physics material; this is the same
/// contact (same restitution, same Coulomb friction, same bounce threshold, equal masses, solid discs) written as plain
/// arithmetic, so the aim-line simulator can predict a hit the way the game will play it. RockCollisionTests keeps the constants
/// equal to the material asset, and Collide.cs (AgentScripts) compares the two against the real game.
///
/// Like Box2D it solves every touching rock together (a shooter, the rock it hits and a rock frozen behind that one are one problem):
/// each contact gets an impulse along the line of centres (bounce) and across it (friction, which also spins both rocks), and the
/// contacts are swept several times so the impulses settle.
/// </summary>
public static class RockCollision
{
    /// <summary>Rock Material.physicsMaterial2D: bounciness (Maximum combine, so the pair uses this).</summary>
    public const float Restitution = 0.99f;
    /// <summary>Rock Material.physicsMaterial2D: friction (Maximum combine).</summary>
    public const float Friction = 0.25f;
    public const float Radius = 0.14f;
    /// <summary>
    /// Rocks closer than two radii plus this are treated as touching when a hit is solved. Box2D only has a contact once the rocks overlap
    /// (rocks that came to rest against each other overlap by a few mm), so a rock a hair away is hit afterwards, one pair at a time.
    /// </summary>
    public const float TouchMargin = 0.002f;
    /// <summary>Box2D sweeps all contacts this many times per step (Physics2D.velocityIterations).</summary>
    public const int Iterations = 8;

    /// <summary>Box2D only bounces a contact that closes faster than this (Physics2D.bounceThreshold); slower touches are dead.</summary>
    public static float BounceThreshold { get { return Physics2D.bounceThreshold; } }

    /// <summary>
    /// Resolve a contact between two rocks. Positions are the rock centres at the moment of touching; spins are degrees/s
    /// (Rigidbody2D units, + = counter-clockwise). Returns false (and changes nothing) if the rocks are not closing on each other.
    /// </summary>
    public static bool Resolve(Vector2 posA, ref Vector2 velA, ref float spinA, Vector2 posB, ref Vector2 velB, ref float spinB)
    {
        return Resolve(posA, ref velA, ref spinA, posB, ref velB, ref spinB, BounceThreshold);
    }

    public static bool Resolve(Vector2 posA, ref Vector2 velA, ref float spinA, Vector2 posB, ref Vector2 velB, ref float spinB, float bounceThreshold)
    {
        Vector2 d = posB - posA;
        if (d.sqrMagnitude < 1e-12f) return false;
        if (Vector2.Dot(velA - velB, d.normalized) <= 0f) return false;
        var pos = new[] { posA, posB };
        var vel = new[] { velA, velB };
        var spin = new[] { spinA, spinB };
        SolveContacts(pos, vel, spin, new[] { 0 }, new[] { 1 }, bounceThreshold);
        velA = vel[0]; velB = vel[1]; spinA = spin[0]; spinB = spin[1];
        return true;
    }

    /// <summary>
    /// Solve a set of contacts between equal rocks. pairA[k], pairB[k] index the rocks of contact k (the normal points from A to B);
    /// vel (units/s) and spin (degrees/s) are changed in place. Restitution is decided from the closing speeds before any impulse is applied.
    /// </summary>
    public static void SolveContacts(Vector2[] pos, Vector2[] vel, float[] spin, int[] pairA, int[] pairB, float bounceThreshold, int iterations = Iterations)
    {
        int n = pairA.Length;
        var normal = new Vector2[n];
        var bias = new float[n];
        var jn = new float[n];
        var jt = new float[n];

        for (int k = 0; k < n; k++)
        {
            Vector2 d = pos[pairB[k]] - pos[pairA[k]];
            normal[k] = d.sqrMagnitude > 1e-12f ? d.normalized : Vector2.up;
            float approach = Vector2.Dot(vel[pairA[k]] - vel[pairB[k]], normal[k]);
            bias[k] = approach > bounceThreshold ? Restitution * approach : 0f;
        }

        // per unit mass: normal 1/m + 1/m; across the contact add r^2/I for each rock (I = m r^2 / 2, so 2/m each)
        const float kNormal = 2f;
        const float kTangent = 6f;
        float[] w = new float[spin.Length];
        for (int i = 0; i < spin.Length; i++) w[i] = spin[i] * Mathf.Deg2Rad;

        for (int it = 0; it < iterations; it++)
        {
            for (int k = 0; k < n; k++)
            {
                int a = pairA[k], b = pairB[k];
                Vector2 nn = normal[k];
                Vector2 t = new Vector2(-nn.y, nn.x);

                // friction first (Box2D solves the tangent before the normal), capped by what the normal impulse has built up so far
                float slip = Vector2.Dot(vel[b] - vel[a], t) - Radius * (w[a] + w[b]);
                float newJt = Mathf.Clamp(jt[k] - slip / kTangent, -Friction * jn[k], Friction * jn[k]);
                float dJt = newJt - jt[k];
                jt[k] = newJt;
                vel[a] -= dJt * t;
                vel[b] += dJt * t;
                float dw = -2f * dJt / Radius;     // the same friction impulse turns both rocks the same way
                w[a] += dw;
                w[b] += dw;

                // normal: push apart until the closing speed is -(bias) (a bounce), never pull together
                float closing = Vector2.Dot(vel[b] - vel[a], nn);           // negative while approaching
                float newJn = Mathf.Max(jn[k] - (closing - bias[k]) / kNormal, 0f);
                float dJn = newJn - jn[k];
                jn[k] = newJn;
                vel[a] -= dJn * nn;
                vel[b] += dJn * nn;
            }
        }

        for (int i = 0; i < spin.Length; i++) spin[i] = w[i] * Mathf.Rad2Deg;
    }
}
