using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Rock-on-rock hits. The pure contact maths (RockCollision) is checked against physics, and the aim-line simulator is checked against
/// rest positions recorded from the real game (Box2D + RockSlide, Arcade soft stop) by AgentScripts/Collide.cs.
/// </summary>
public class RockCollisionTests
{
    [TearDown]
    public void RestoreProfile() { RockPhysicsSettings.Use(RockPhysicsProfile.Legacy()); }

    // ---- constants stay equal to the physics material the live rocks use ----

    [Test]
    public void ConstantsMatchTheRockMaterial()
    {
        var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materials/Rock Material.physicsMaterial2D");
        Assert.NotNull(m);
        Assert.AreEqual(m.bounciness, RockCollision.Restitution, 1e-6f, "Restitution must equal the material's bounciness");
        Assert.AreEqual(m.friction, RockCollision.Friction, 1e-6f, "Friction must equal the material's friction");
    }

    // ---- the contact itself ----

    static bool Hit(Vector2 pa, ref Vector2 va, ref float wa, Vector2 pb, ref Vector2 vb, ref float wb, float threshold = 1f)
    {
        return RockCollision.Resolve(pa, ref va, ref wa, pb, ref vb, ref wb, threshold);
    }

    [Test]
    public void HeadOnHandsAlmostEverythingOver()
    {
        Vector2 va = new Vector2(0f, 6f), vb = Vector2.zero; float wa = 0f, wb = 0f;
        Assert.IsTrue(Hit(Vector2.zero, ref va, ref wa, new Vector2(0f, 0.28f), ref vb, ref wb));
        Assert.AreEqual(6f * (1f - RockCollision.Restitution) / 2f, va.y, 1e-3f, "shooter keeps (1-e)/2 of its speed");
        Assert.AreEqual(6f * (1f + RockCollision.Restitution) / 2f, vb.y, 1e-3f, "target gets (1+e)/2");
        Assert.AreEqual(0f, va.x, 1e-4f);
        Assert.AreEqual(0f, vb.x, 1e-4f);
    }

    [Test]
    public void MomentumIsConservedAndEnergyNeverGrows()
    {
        var rng = new System.Random(3);
        for (int i = 0; i < 200; i++)
        {
            float ang = (float)(rng.NextDouble() * 1.2 - 0.6);
            Vector2 n = new Vector2(Mathf.Sin(ang), Mathf.Cos(ang));
            Vector2 va = new Vector2((float)rng.NextDouble() * 2f - 1f, 2f + (float)rng.NextDouble() * 8f);
            Vector2 vb = Vector2.zero;
            float wa = (float)rng.NextDouble() * 600f - 300f, wb = 0f;
            Vector2 pa = Vector2.zero, pb = n * 0.28f;
            Vector2 p0 = va + vb;
            float e0 = Energy(va, wa) + Energy(vb, wb);
            if (!Hit(pa, ref va, ref wa, pb, ref vb, ref wb, 0.1f)) continue;
            Assert.AreEqual(p0.x, (va + vb).x, 1e-3f);
            Assert.AreEqual(p0.y, (va + vb).y, 1e-3f);
            Assert.LessOrEqual(Energy(va, wa) + Energy(vb, wb), e0 * 1.0001f, "a hit cannot add energy");
        }
    }

    // kinetic + rotational energy per unit mass (I = m r^2 / 2)
    static float Energy(Vector2 v, float spinDeg)
    {
        float w = spinDeg * Mathf.Deg2Rad;
        return 0.5f * v.sqrMagnitude + 0.5f * 0.5f * RockCollision.Radius * RockCollision.Radius * w * w;
    }

    [Test]
    public void SlowTouchesDoNotBounce()
    {
        // closing at 0.5 m/s, under Box2D's bounce threshold of 1: no restitution, the pair just ends up together
        Vector2 va = new Vector2(0f, 0.5f), vb = Vector2.zero; float wa = 0f, wb = 0f;
        Assert.IsTrue(Hit(Vector2.zero, ref va, ref wa, new Vector2(0f, 0.28f), ref vb, ref wb));
        Assert.AreEqual(0.25f, va.y, 1e-3f);
        Assert.AreEqual(0.25f, vb.y, 1e-3f);
    }

    [Test]
    public void NotClosingChangesNothing()
    {
        Vector2 va = new Vector2(0f, -1f), vb = Vector2.zero; float wa = 5f, wb = 7f;
        Assert.IsFalse(Hit(Vector2.zero, ref va, ref wa, new Vector2(0f, 0.28f), ref vb, ref wb));
        Assert.AreEqual(new Vector2(0f, -1f), va);
        Assert.AreEqual(5f, wa);
        Assert.AreEqual(7f, wb);
    }

    [Test]
    public void GlancingHitSpinsTheStruckRockAgainstTheFriction()
    {
        // shooter passes on the left of the target (target is to the right of the line of travel): the struck rock is dragged and spins
        Vector2 va = new Vector2(0f, 6f), vb = Vector2.zero; float wa = 0f, wb = 0f;
        Vector2 n = new Vector2(Mathf.Sin(0.4f), Mathf.Cos(0.4f));
        Assert.IsTrue(Hit(Vector2.zero, ref va, ref wa, n * 0.28f, ref vb, ref wb));
        Assert.Less(wb, -10f, "recorded live hits give the struck rock a clockwise spin of tens to hundreds of deg/s");
        Assert.AreEqual(wa, wb, 1e-3f, "both rocks are turned by the same friction impulse");
        float cap = RockCollision.Friction;   // the friction impulse is at most mu times the bounce impulse
        Assert.LessOrEqual(Mathf.Abs(vb.x), Mathf.Abs(vb.y) * 2f);
        Assert.Greater(cap, 0f);
    }

    [Test]
    public void FrozenPairIsOneProblem_AndAGapPairIsACradle()
    {
        // touching (overlapping 4 mm) pair behind the target: the three rocks are solved together, shooter bounces back
        Vector2[] pos = { new Vector2(0f, 0f), new Vector2(0f, 0.28f), new Vector2(0f, 0.556f) };
        Vector2[] vel = { new Vector2(0f, 6f), Vector2.zero, Vector2.zero };
        float[] spin = new float[3];
        RockCollision.SolveContacts(pos, vel, spin, new[] { 0, 1 }, new[] { 1, 2 }, 1f);
        Assert.AreEqual(6f * (1f + RockCollision.Restitution) / 3f, vel[1].y, 0.1f, "target and partner leave together at (1+e)/3 of the shooter's speed");
        Assert.AreEqual(vel[1].y, vel[2].y, 0.1f);
        Assert.Less(vel[0].y, -1f, "the shooter bounces back");

        // a rock a hair away is struck afterwards: one pair at a time gives the newton's cradle
        Vector2 va = new Vector2(0f, 6f), vb = Vector2.zero; float wa = 0f, wb = 0f;
        Assert.IsTrue(Hit(pos[0], ref va, ref wa, pos[1], ref vb, ref wb));
        Vector2 vc = Vector2.zero; float wc = 0f;
        Assert.IsTrue(Hit(pos[1], ref vb, ref wb, new Vector2(0f, 0.5605f), ref vc, ref wc));
        Assert.Greater(vc.y, 5.8f, "the partner takes nearly all the speed");
        Assert.Less(Mathf.Abs(vb.y), 0.1f, "the rock in the middle stops");
    }

    // ---- the simulator against rest positions recorded from the real game (Arcade soft stop) ----

    struct Recorded { public float weight, offset; public bool frozen; public Vector2 shooter, target, partner; }

    static readonly Recorded[] Live =
    {
        new Recorded { weight = 8.5f,  offset = 0f,    shooter = new Vector2( 0.03f, 3.73f), target = new Vector2(-0.12f, 5.36f) },
        new Recorded { weight = 14f,   offset = 0f,    shooter = new Vector2( 0.04f, 3.75f), target = new Vector2(-0.11f, 10.11f) },
        new Recorded { weight = 8.5f,  offset = 0.12f, shooter = new Vector2(-0.73f, 4.00f), target = new Vector2( 0.34f, 5.16f) },
        new Recorded { weight = 8.5f,  offset = 0.20f, shooter = new Vector2(-1.21f, 4.73f), target = new Vector2( 0.44f, 4.70f) },
        new Recorded { weight = 14f,   offset = 0.05f, shooter = new Vector2(-1.17f, 3.98f), target = new Vector2( 0.82f, 9.94f) },
        new Recorded { weight = 11f,   offset = 0f,    frozen = true, shooter = new Vector2( 0.07f, 1.81f), target = new Vector2(-0.12f, 6.36f), partner = new Vector2(0.00f, 6.63f) },
        new Recorded { weight = 8.5f,  offset = 0.10f, frozen = true, shooter = new Vector2(-0.75f, 3.37f), target = new Vector2( 0.21f, 4.73f), partner = new Vector2(0.10f, 5.00f) },
    };

    // the launch that puts the shooter through (targetX - offset, targetY) with no rocks in the way (same aiming as the harness)
    static Vector2 Aim(TrajectorySimulator sim, Vector2 from, float weight, float tx, float ty)
    {
        float lo = -0.1f, hi = 0.1f;
        for (int i = 0; i < 40; i++)
        {
            float mid = 0.5f * (lo + hi);
            var v = new Vector2(Mathf.Sin(mid), Mathf.Cos(mid)) * weight;
            var path = sim.SimulateTrajectory(from, v, false, 1500, null, false);
            float x = path[path.Count - 1].x;
            for (int k = 1; k < path.Count; k++)
                if (path[k - 1].y < ty && path[k].y >= ty) { x = Mathf.Lerp(path[k - 1].x, path[k].x, (ty - path[k - 1].y) / (path[k].y - path[k - 1].y)); break; }
            if (x > tx) hi = mid; else lo = mid;
        }
        float a = 0.5f * (lo + hi);
        return new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * weight;
    }

    [Test]
    public void SimulatorHitsLandWhereTheRealGameLandsThem()
    {
        RockPhysicsSettings.Use(RockPhysicsProfile.Arcade());
        var sim = new TrajectorySimulator(0, 0);
        Vector2 hack = new Vector2(0f, -25f), T = new Vector2(0f, 4f);
        const float tol = 0.25f;

        foreach (var r in Live)
        {
            var target = new GameObject("target"); target.transform.position = T;
            GameObject partner = null;
            var rocks = new List<GameObject> { target };
            if (r.frozen) { partner = new GameObject("partner"); partner.transform.position = T + new Vector2(0f, 0.276f); rocks.Add(partner); }
            try
            {
                Vector2 vel = Aim(sim, hack, r.weight, T.x - r.offset, T.y);
                sim.SimulateTrajectory(hack, vel, false, 1500, rocks, false);
                var ci = sim.GetCollisionInfo();
                string what = "w" + r.weight + " offset " + r.offset + (r.frozen ? " frozen" : "");
                Assert.IsTrue(ci.hasCollision, what + ": the simulator must see the hit");
                Assert.Less(Vector2.Distance(ci.finalPosition, r.shooter), tol, what + ": shooter rest " + ci.finalPosition + " vs real " + r.shooter);
                Assert.Less(Vector2.Distance(ci.hitRockFinalPosition, r.target), tol, what + ": target rest " + ci.hitRockFinalPosition + " vs real " + r.target);
                if (r.frozen)
                {
                    Vector2 p = T + new Vector2(0f, 0.276f);
                    foreach (var kv in ci.otherRocksMoved) if (kv.Key == partner) p = kv.Value;
                    Assert.Less(Vector2.Distance(p, r.partner), tol, what + ": partner rest " + p + " vs real " + r.partner);
                }
            }
            finally
            {
                Object.DestroyImmediate(target);
                if (partner != null) Object.DestroyImmediate(partner);
            }
        }
    }

    [Test]
    public void RockWithNothingInItsWayIsUnchanged()
    {
        RockPhysicsSettings.Use(RockPhysicsProfile.Arcade());
        var sim = new TrajectorySimulator(0, 0);
        var far = new GameObject("far"); far.transform.position = new Vector2(1.5f, 4f);
        try
        {
            var with = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, 12f), false, 1500, new List<GameObject> { far }, false);
            Assert.IsFalse(sim.GetCollisionInfo().hasCollision);
            var without = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, 12f), false, 1500, null, false);
            Assert.AreEqual(without[without.Count - 1].x, with[with.Count - 1].x, 1e-5f);
            Assert.AreEqual(without[without.Count - 1].y, with[with.Count - 1].y, 1e-5f);
        }
        finally { Object.DestroyImmediate(far); }
    }
}
