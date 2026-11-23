using UnityEngine;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Recorder : MonoBehaviour
{
    public List<Vector3> samples = new List<Vector3>();   // x = time, y = pos.y, z = speed
    public List<float> xs = new List<float>();
    Rigidbody2D rb; float t0;
    void Start() { rb = GetComponent<Rigidbody2D>(); t0 = Time.fixedTime; }
    void FixedUpdate() { if (rb == null) return; samples.Add(new Vector3(Time.fixedTime - t0, rb.position.y, rb.linearVelocity.magnitude)); xs.Add(rb.position.x); }
}

public static class Measure
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    const float Dt = 0.02f;
    const float NearHog = -16.15f, FarHog = 0f, Tee = 6.5f;

    class Result
    {
        public float v0, tNear = -1, tFar = -1, tTee = -1, tSlow = -1, tRest = -1, path, vNear, vFar, xFar, xRest, yRest;
    }

    static MethodInfo slideFu, forceFu;
    // what the engine does each fixed step before physics: RockSlide (order -10) then Rock_Force
    static void Tick(Rock_Force rf)
    {
        if (slideFu == null) slideFu = typeof(RockSlide).GetMethod("FixedUpdate", Any);
        if (forceFu == null) forceFu = typeof(Rock_Force).GetMethod("FixedUpdate", Any);
        slideFu.Invoke(rf.GetComponent<RockSlide>(), null);
        if (rf.enabled) forceFu.Invoke(rf, null);
    }

    static Rock_Force Rock()
    {
        var all = Object.FindObjectsByType<Rock_Force>(FindObjectsInactive.Include);
        Rock_Force rf = null;
        foreach (var c in all) if (c.name == "YELLERRR 1") rf = c;
        if (rf == null) rf = all[0];
        rf.GetComponent<Rigidbody2D>().simulated = true;
        foreach (var other in Object.FindObjectsByType<Rock_Info>(FindObjectsInactive.Include))
            if (other.gameObject != rf.gameObject) { var orb = other.GetComponent<Rigidbody2D>(); if (orb != null) orb.simulated = false; }
        return rf;
    }

    static void Reset(Rock_Force rf, float v0, bool inTurn, float startY)
    {
        ResetV(rf, new Vector2(0f, v0), inTurn, new Vector2(0f, startY));
    }

    static void ResetV(Rock_Force rf, Vector2 vel, bool inTurn, Vector2 start)
    {
        float startY = start.y;
        var rb = rf.GetComponent<Rigidbody2D>();
        var info = rf.GetComponent<Rock_Info>();
        var rel = rf.GetComponent<Rock_Release>();
        var sj = rf.GetComponent<SpringJoint2D>();
        if (sj != null) sj.enabled = false;
        var col = rf.GetComponent<Collider2D>();
        col.enabled = false;
        rf.GetComponent<Rigidbody2D>().position = new Vector2(0f, startY);
        Physics2D.SyncTransforms();
        Physics2D.Simulate(Dt); Physics2D.Simulate(Dt);   // flush stale trigger contacts from the previous shot
        col.enabled = true;
        rel.enabled = true;
        rf.enabled = true;
        rf.flipAxis = inTurn;
        rf.curl = Vector2.zero;
        rel.released = false;
        info.released = false; info.rest = false; info.stopped = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        var slide = rf.GetComponent<RockSlide>(); if (slide == null) slide = rf.gameObject.AddComponent<RockSlide>();
        slide.BeginFlight();
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.angularVelocity = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.position = start;
        rb.rotation = 0f;
        rf.transform.position = new Vector3(start.x, start.y, 0f);
        Physics2D.SyncTransforms();
        rb.linearVelocity = vel;
    }

    static Result Fire(Rock_Force rf, float v0, bool inTurn, float startY)
    {
        var rb = rf.GetComponent<Rigidbody2D>();
        Reset(rf, v0, inTurn, startY);
        var fu = rf.GetType().GetMethod("FixedUpdate", Any);
        var res = new Result { v0 = v0 };
        Vector2 prev = rb.position;
        float t = 0f;
        int steps = 0;
        while (steps < 9000)
        {
            Tick(rf);
            Physics2D.Simulate(Dt);
            steps++; t += Dt;
            Vector2 p = rb.position;
            res.path += Vector2.Distance(p, prev);
            float sp = rb.linearVelocity.magnitude;
            Cross(ref res.tNear, NearHog, prev, p, t, sp, ref res.vNear, p.x, ref res.xFar, false);
            Cross(ref res.tFar, FarHog, prev, p, t, sp, ref res.vFar, p.x, ref res.xFar, true);
            float d1 = 0, d2 = 0;
            Cross(ref res.tTee, Tee, prev, p, t, sp, ref d1, p.x, ref d2, false);
            prev = p;
            if (p.y > NearHog)
            {
                if (res.tSlow < 0 && sp < 0.1f) res.tSlow = t;
                if (sp < 0.01f) { res.tRest = t; break; }
            }
        }
        res.xRest = rb.position.x; res.yRest = rb.position.y;
        return res;
    }

    static void Cross(ref float tCross, float line, Vector2 prev, Vector2 cur, float t, float speed, ref float speedOut, float x, ref float xOut, bool recordX)
    {
        if (tCross >= 0f) return;
        if (prev.y < line && cur.y >= line)
        {
            float f = (line - prev.y) / (cur.y - prev.y);
            tCross = t - Dt + f * Dt;
            speedOut = speed;
            if (recordX) xOut = Mathf.Lerp(prev.x, cur.x, f);
        }
    }

    static string Row(Result r)
    {
        float h2h = (r.tFar >= 0 && r.tNear >= 0) ? r.tFar - r.tNear : -1;
        return string.Format("{0,5:F1} | {1,5:F2} | {2,6:F2} | {3,6:F2} | {4,6:F2} | {5,6:F2} | {6,6:F2} | {7,5:F2} | {8,5:F2} | {9,6:F2} | {10,5:F1} | ({11:F2},{12:F2})",
            r.v0, r.tNear, h2h, r.tTee >= 0 ? r.tTee - r.tNear : -1, r.tSlow, r.tRest, r.vNear, r.vFar, r.xFar, r.xRest, r.path, r.xRest, r.yRest);
    }

    public static string RunAI()
    {
        var rf = Rock();
        var sb = new StringBuilder();
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            sb.AppendLine("  v0  | hack>NH | NH>FH  | NH>tee | <0.1   | rest   | vNH   | vFH  | xAtFH | path |  rest(x,y)");
            foreach (float v0 in new[] { 8.5f, 9.35f, 12.4f, 13.75f, 15.8f })
                sb.AppendLine(Row(Fire(rf, v0, false, -25f)));
        }
        finally { Physics2D.simulationMode = mode; }
        return sb.ToString();
    }

    public static string Spin()
    {
        var rf = Rock();
        var rb = rf.GetComponent<Rigidbody2D>();
        var sb = new StringBuilder();
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            foreach (float v0 in new[] { 6f, 8f, 12f })
            {
                Reset(rf, v0, false, -25f);
                var fu = rf.GetType().GetMethod("FixedUpdate", Any);
                int released = -1;
                for (int i = 0; i < 400; i++)
                {
                    Tick(rf); Physics2D.Simulate(Dt);
                    if (released < 0 && rf.GetComponent<Rock_Release>().released) { released = i; }
                    if (released >= 0 && i == released + 2) break;
                }
                sb.AppendLine("v0=" + v0 + " mass=" + rb.mass + " inertia=" + rb.inertia + " angVel(just after release)=" + rb.angularVelocity.ToString("F4")
                    + " curl=" + rf.curl + " scaleFactor=" + rf.scaleFactor + " angDamp=" + rb.angularDamping + " linDamp=" + rb.linearDamping + " y=" + rb.position.y.ToString("F3") + " vy=" + rb.linearVelocity.y.ToString("F4")
                    + " useAutoMass=" + rb.useAutoMass + " centerOfMass=" + rb.centerOfMass + " radius=" + ((CircleCollider2D)rf.GetComponent<Collider2D>()).radius + " scale=" + rf.transform.lossyScale);
            }
        }
        finally { Physics2D.simulationMode = mode; }
        return sb.ToString();
    }

    public static string Golden()
    {
        var rf = Rock();
        var rb = rf.GetComponent<Rigidbody2D>();
        var sb = new StringBuilder();
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            foreach (var spec in new[] { new Vector2(6f, 0f), new Vector2(8.5f, 0f), new Vector2(12.4f, 0f), new Vector2(8.5f, 1f) })
            {
                Reset(rf, spec.x, spec.y > 0.5f, -25f);
                var fu = rf.GetType().GetMethod("FixedUpdate", Any);
                sb.AppendLine("v0=" + spec.x + " inTurn=" + (spec.y > 0.5f) + "  [t, x, y, vx, vy, spin]");
                int relStep = -1;
                for (int step = 1; step <= 1500; step++)
                {
                    Tick(rf); Physics2D.Simulate(Dt);
                    if (relStep < 0 && rf.GetComponent<Rock_Release>().released) { relStep = step; sb.AppendLine("  released at step " + step + " t=" + (step * Dt).ToString("F2") + " y=" + rb.position.y.ToString("F4") + " vy=" + rb.linearVelocity.y.ToString("F4")); }
                    if (step % 50 == 0 || step == relStep + 1 || step == relStep + 2)
                        sb.AppendLine("  " + (step * Dt).ToString("F2") + ", " + rb.position.x.ToString("F4") + ", " + rb.position.y.ToString("F4") + ", " + rb.linearVelocity.x.ToString("F5") + ", " + rb.linearVelocity.y.ToString("F5") + ", " + rb.angularVelocity.ToString("F3"));
                }
            }
        }
        finally { Physics2D.simulationMode = mode; }
        return sb.ToString();
    }

    // Throw the way the player does: rock pulled back to P, velocity toward the launcher with speed from the pullback formula.
    public static string Angled()
    {
        var rf = Rock();
        var rb = rf.GetComponent<Rigidbody2D>();
        var tl = Object.FindFirstObjectByType<TrajectoryLine>();
        var sb = new StringBuilder();
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        var sim = new TrajectorySimulator(0.62f, 0.25f);
        Vector2 launcher = new Vector2(0f, -25f);
        try
        {
            sb.AppendLine("trajLine mult=" + tl.velocityMultiplier + " pull " + tl.minPullbackDistance + ".." + tl.maxPullbackDistance + " speed " + tl.minVelocity + ".." + tl.maxVelocity);
            sb.AppendLine("pull P (x,y)   | turn | speed | live rest (x,y)  | sim end (x,y)    | error");
            foreach (var P in new[] { new Vector2(0.3f, -27.0f), new Vector2(-0.3f, -27.0f), new Vector2(0.15f, -28.5f), new Vector2(-0.3f, -29.0f), new Vector2(0.3f, -30.0f), new Vector2(-0.2f, -30.4f), new Vector2(0f, -28.0f) })
            foreach (bool inTurn in new[] { false, true })
            {
                Vector2 vel = TrajectorySimulator.CalculateInitialVelocityFromPullback(P, launcher, tl.velocityMultiplier, tl.minPullbackDistance, tl.maxPullbackDistance, tl.minVelocity, tl.maxVelocity);
                ResetV(rf, vel, inTurn, P);
                for (int i = 0; i < 2500; i++)
                {
                    Tick(rf); Physics2D.Simulate(Dt);
                    if (rf.GetComponent<RockSlide>().AtRest && rb.linearVelocity.magnitude < 0.005f && rb.position.y > -15f) break;
                }
                var path = sim.SimulateTrajectory(launcher, vel, inTurn, 600, null, false);
                var end = path[path.Count - 1];
                sb.AppendLine(string.Format("({0,5:F2},{1,6:F2}) | {2,-5} | {3,5:F2} | ({4,6:F2},{5,6:F2})   | ({6,6:F2},{7,6:F2})   | ({8,5:F2},{9,5:F2})",
                    P.x, P.y, inTurn ? "in" : "out", vel.magnitude, rb.position.x, rb.position.y, end.x, end.y, end.x - rb.position.x, end.y - rb.position.y));
            }
        }
        finally { Physics2D.simulationMode = mode; }
        return sb.ToString();
    }

    public static string Run()
    {
        var rf = Rock();
        var sb = new StringBuilder();
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            sb.AppendLine("OUT-TURN (flipAxis=false), start at hack y=-25, no sweeping. Times in seconds; -1 = never reached.");
            sb.AppendLine("  v0  | hack>NH | NH>FH  | NH>tee | <0.1   | rest   | vNH   | vFH  | xAtFH | path |  rest(x,y)");
            foreach (float v0 in new[] { 5f, 5.5f, 6f, 6.5f, 7f, 7.5f, 8f, 8.5f, 9f, 9.5f, 10f, 11f, 12f, 14f, 16f })
                sb.AppendLine(Row(Fire(rf, v0, false, -25f)));
            sb.AppendLine();
            sb.AppendLine("IN-TURN (flipAxis=true)");
            foreach (float v0 in new[] { 6f, 8f, 9f })
                sb.AppendLine(Row(Fire(rf, v0, true, -25f)));
            sb.AppendLine();
            sb.AppendLine("REPEATABILITY: v0=8 x3");
            for (int i = 0; i < 3; i++) sb.AppendLine(Row(Fire(rf, 8f, false, -25f)));
        }
        finally { Physics2D.simulationMode = mode; }
        return sb.ToString();
    }

    static float realStart;
    public static string RealStart()
    {
        var rf = Rock();
        Reset(rf, 8f, false, -25f);
        realStart = Time.fixedTime;
        return "start fixedTime=" + Time.fixedTime;
    }
    public static string Poll()
    {
        var rf = Object.FindFirstObjectByType<Rock_Force>();
        var rb = rf.GetComponent<Rigidbody2D>();
        return "t=" + (Time.fixedTime - realStart).ToString("F2") + " (fixedTime " + Time.fixedTime.ToString("F2") + ") y=" + rb.position.y.ToString("F3") + " x=" + rb.position.x.ToString("F3") + " speed=" + rb.linearVelocity.magnitude.ToString("F3") + " damp=" + rb.linearDamping.ToString("F2");
    }
    // manual-stepped trajectory of the same shot, to compare with Poll output at equal elapsed times
    public static string Predict()
    {
        var rf = Rock();
        var rb = rf.GetComponent<Rigidbody2D>();
        var sb = new StringBuilder();
        var mode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;
        try
        {
            Reset(rf, 8f, false, -25f);
            var fu = rf.GetType().GetMethod("FixedUpdate", Any);
            for (int step = 1; step <= 1000; step++)
            {
                Tick(rf);
                Physics2D.Simulate(Dt);
                if (step % 50 == 0) sb.AppendLine("t=" + (step * Dt).ToString("F2") + " y=" + rb.position.y.ToString("F3") + " x=" + rb.position.x.ToString("F3") + " speed=" + rb.linearVelocity.magnitude.ToString("F3"));
            }
        }
        finally { Physics2D.simulationMode = mode; }
        return sb.ToString();
    }
}
