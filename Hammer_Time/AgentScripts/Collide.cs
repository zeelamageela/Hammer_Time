using UnityEngine;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

// Phase 3 collision harness (measure only, changes nothing): a shooter hits a stationary rock in the REAL game
// (Box2D + RockSlide + Rock_Colliders) and the same shot is run through TrajectorySimulator. Prints speeds, angles and rest points.
public static class Collide
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    const float Dt = 0.02f;
    const float R = 0.14f;

    static MethodInfo slideFu, forceFu;
    static void TickSlide(GameObject go)
    {
        if (slideFu == null) slideFu = typeof(RockSlide).GetMethod("FixedUpdate", Any);
        var s = go.GetComponent<RockSlide>();
        if (s == null) s = go.AddComponent<RockSlide>();
        slideFu.Invoke(s, null);
    }
    static void TickShooter(Rock_Force rf)
    {
        if (forceFu == null) forceFu = typeof(Rock_Force).GetMethod("FixedUpdate", Any);
        TickSlide(rf.gameObject);
        if (rf.enabled) forceFu.Invoke(rf, null);
    }

    static GameObject Named(string n)
    {
        foreach (var info in Object.FindObjectsByType<Rock_Info>(FindObjectsInactive.Include))
            if (info.name == n) return info.gameObject;
        return null;
    }

    static void Place(GameObject go, Vector2 pos, RockPhysicsProfile p)
    {
        go.SetActive(true);
        var rb = go.GetComponent<Rigidbody2D>();
        var col = go.GetComponent<Collider2D>();
        var sj = go.GetComponent<SpringJoint2D>(); if (sj != null) sj.enabled = false;
        col.enabled = false;
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f;
        rb.position = pos; rb.rotation = 0f;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        rb.linearDamping = 0f; rb.angularDamping = 0f;
        var slide = go.GetComponent<RockSlide>(); if (slide == null) slide = go.AddComponent<RockSlide>();
        typeof(RockSlide).GetField("state", Any).SetValue(slide, RockPhysics.NewRestingRock(p));
        var info = go.GetComponent<Rock_Info>();
        info.stopped = true; info.rest = true; info.moving = false; info.released = false; info.inPlay = true;
        var rf = go.GetComponent<Rock_Force>(); if (rf != null) rf.enabled = false;
        Physics2D.SyncTransforms();
        Physics2D.Simulate(Dt); Physics2D.Simulate(Dt);   // flush stale contacts, then let it collide again
        col.enabled = true;
        Physics2D.SyncTransforms();
    }

    static Vector2 AimVel(TrajectorySimulator sim, Vector2 S, float weight, float tx, float ty)
    {
        float lo = -0.1f, hi = 0.1f;
        for (int i = 0; i < 40; i++)
        {
            float mid = 0.5f * (lo + hi);
            var v = new Vector2(Mathf.Sin(mid), Mathf.Cos(mid)) * weight;
            var path = sim.SimulateTrajectory(S, v, false, 1500, null, false);
            float x = path[path.Count - 1].x;
            for (int k = 1; k < path.Count; k++)
                if (path[k - 1].y < ty && path[k].y >= ty) { float f = (ty - path[k - 1].y) / (path[k].y - path[k - 1].y); x = Mathf.Lerp(path[k - 1].x, path[k].x, f); break; }
            if (x > tx) hi = mid; else lo = mid;
        }
        float a = 0.5f * (lo + hi);
        return new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * weight;
    }

    static float Ang(Vector2 from, Vector2 to) { return Vector2.SignedAngle(from, to); }

    public static string Run()
    {
        Application.runInBackground = true;
        var sb = new StringBuilder();
        var frozenLog = new List<string>();
        Application.LogCallback cb = (msg, st, type) => { if (msg.Contains("[Frozen Rock]")) frozenLog.Add(msg); };
        Application.logMessageReceived += cb;
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        var all = Object.FindObjectsByType<Rock_Force>(FindObjectsInactive.Include);
        Rock_Force rf = null; foreach (var c in all) if (c.name == "YELLERRR 1") rf = c;
        if (rf == null) rf = all[0];
        var shooter = rf.gameObject;
        var srb = shooter.GetComponent<Rigidbody2D>();
        var tgt = Named("YELLERRR 2"); var par = Named("YELLERRR 3");
        foreach (var other in Object.FindObjectsByType<Rock_Info>(FindObjectsInactive.Include))
            if (other.gameObject != shooter && other.gameObject != tgt && other.gameObject != par) { var orb = other.GetComponent<Rigidbody2D>(); if (orb != null) orb.simulated = false; }
        try
        {
            RockPhysicsSettings.Use(RockPhysicsProfile.Arcade());
            var prof = RockPhysicsSettings.Current;
            sb.AppendLine("rock material: bounce " + shooter.GetComponent<Collider2D>().sharedMaterial.bounciness + " (combine " + "max" + "), friction " + shooter.GetComponent<Collider2D>().sharedMaterial.friction + "; mass " + srb.mass + "; Arcade soft stop");
            sb.AppendLine("Live = Box2D + RockSlide + Rock_Colliders. Sim = TrajectorySimulator. speeds in m/s of the real body; angles relative to the shooter's incoming direction (deg, + = left).");
            sb.AppendLine();
            sb.AppendLine("case                   | impact | offset | pre v | live v shooter / target | ratio S / T    | exit angle S / T | target spin | live rest shooter      | live rest target       | sim shooter end        | sim target end         | err shooter / target (m)");

            var sim = new TrajectorySimulator(0, 0);
            Vector2 hack = new Vector2(0f, -25f);
            foreach (bool frozen in new[] { false, true })
            foreach (float weight in new[] { 8.5f, 11f, 14f })
            foreach (float off in frozen ? new[] { 0f, 0.10f } : new[] { 0f, 0.05f, 0.12f, 0.20f, 0.26f })
            {
                frozenLog.Clear();
                Vector2 T = new Vector2(0f, 4.0f);
                Vector2 P = T + new Vector2(0f, 0.2805f);   // frozen partner touching, straight behind
                // aim so the shooter's centre passes `off` to the side of the target's centre
                Vector2 vel = AimVel(sim, hack, weight, T.x - off, T.y - 0.0f);
                // ---- prediction (before anything moves)
                Place(tgt, T, prof);
                var list = new List<GameObject> { tgt };
                if (frozen) { Place(par, P, prof); list.Add(par); }
                var path = sim.SimulateTrajectory(hack, vel, false, 1500, list, false);
                var ci = sim.lastCollisionInfo;

                // ---- live
                Place(tgt, T, prof);
                if (frozen) Place(par, P, prof);
                // shooter
                {
                    var rel = shooter.GetComponent<Rock_Release>(); var info = shooter.GetComponent<Rock_Info>();
                    var sj = shooter.GetComponent<SpringJoint2D>(); if (sj != null) sj.enabled = false;
                    var col = shooter.GetComponent<Collider2D>(); col.enabled = false;
                    srb.position = hack; Physics2D.SyncTransforms(); Physics2D.Simulate(Dt); Physics2D.Simulate(Dt);
                    col.enabled = true; rel.enabled = true; rf.enabled = true; rf.flipAxis = false; rf.curl = Vector2.zero; rel.released = false;
                    info.released = false; info.rest = false; info.stopped = false; info.inPlay = true;
                    srb.bodyType = RigidbodyType2D.Dynamic;
                    var slide = shooter.GetComponent<RockSlide>(); if (slide == null) slide = shooter.AddComponent<RockSlide>();
                    slide.BeginFlight();
                    srb.linearDamping = 0f; srb.angularDamping = 0f; srb.angularVelocity = 0f; srb.linearVelocity = Vector2.zero;
                    srb.position = hack; srb.rotation = 0f; shooter.transform.position = new Vector3(hack.x, hack.y, 0f);
                    Physics2D.SyncTransforms();
                    srb.linearVelocity = prof.LaunchVelocityFromWeightVelocity(vel);
                }
                var trb = tgt.GetComponent<Rigidbody2D>(); var prb = frozen ? par.GetComponent<Rigidbody2D>() : null;

                int contactStep = -1; Vector2 preV = Vector2.zero, prevV = Vector2.zero, impactDir = Vector2.up; float impactOffset = 0f;
                Vector2 vS = Vector2.zero, vT = Vector2.zero; float spin = 0f; Vector2 vP = Vector2.zero;
                for (int step = 0; step < 9000; step++)
                {
                    prevV = srb.linearVelocity;
                    TickShooter(rf); TickSlide(tgt); if (frozen) TickSlide(par);
                    Physics2D.Simulate(Dt);
                    if (contactStep < 0 && (trb.linearVelocity.magnitude > 0.05f || Vector2.Distance(srb.position, trb.position) <= 2f * R + 0.004f))
                    {
                        contactStep = step; preV = prevV;
                        impactDir = preV.normalized;
                        Vector2 toT = (trb.position - srb.position).normalized;
                        impactOffset = Mathf.Abs(Vector2.SignedAngle(impactDir, toT)) * Mathf.Deg2Rad;
                        impactOffset = Mathf.Sin(impactOffset) * 2f * R;
                    }
                    if (contactStep >= 0 && step == contactStep + 2)
                    {
                        vS = srb.linearVelocity; vT = trb.linearVelocity; spin = trb.angularVelocity; if (prb != null) vP = prb.linearVelocity;
                    }
                    if (contactStep >= 0 && step > contactStep + 30 && srb.linearVelocity.magnitude < 0.01f && trb.linearVelocity.magnitude < 0.01f && (prb == null || prb.linearVelocity.magnitude < 0.01f)) break;
                }
                float pre = preV.magnitude;
                Vector2 sr = srb.position, tr = trb.position;
                Vector2 se = ci.hasCollision ? ci.finalPosition : path[path.Count - 1];
                Vector2 te = ci.hasCollision ? ci.hitRockFinalPosition : T;
                sb.AppendLine(string.Format("{0,-22} | {1,-6} | {2,6:F3} | {3,5:F2} | {4,5:F2} ({5,6:F2},{6,6:F2}) / {7,5:F2} ({8,6:F2},{9,6:F2}) | {10,5:F3} / {11,5:F3} | {12,6:F1} / {13,6:F1} | {14,9:F0} | ({15,6:F2},{16,6:F2}) | ({17,6:F2},{18,6:F2}) | ({19,6:F2},{20,6:F2}) | ({21,6:F2},{22,6:F2}) | {23,5:F2} / {24,5:F2}",
                    (frozen ? "frozen " : "single ") + "w" + weight.ToString("F1") + " aim" + off.ToString("F2"),
                    contactStep >= 0 ? "yes" : "NO", impactOffset, pre,
                    vS.magnitude, vS.x, vS.y, vT.magnitude, vT.x, vT.y,
                    pre > 0 ? vS.magnitude / pre : 0, pre > 0 ? vT.magnitude / pre : 0,
                    vS.sqrMagnitude > 1e-4f ? Ang(impactDir, vS) : 0f, vT.sqrMagnitude > 1e-4f ? Ang(impactDir, vT) : 0f,
                    spin, sr.x, sr.y, tr.x, tr.y, se.x, se.y, te.x, te.y, Vector2.Distance(sr, se), Vector2.Distance(tr, te)));
                if (frozen)
                {
                    sb.AppendLine(string.Format("   partner: speed just after hit {0:F2}, rest ({1:F2},{2:F2}); sim assumes it does not move.  frozen-rule hits: {3}", vP.magnitude, prb.position.x, prb.position.y, frozenLog.Count));
                    foreach (var l in frozenLog) sb.AppendLine("     " + l.Replace("\n", " ").Substring(0, Mathf.Min(140, l.Length)));
                }
            }
        }
        finally
        {
            Application.logMessageReceived -= cb;
            RockPhysicsSettings.Use(RockPhysicsProfile.Legacy());
            Physics2D.simulationMode = mode;
        }
        return sb.ToString();
    }
}
