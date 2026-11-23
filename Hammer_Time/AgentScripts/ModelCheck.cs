using UnityEngine;
using System.Text;
using System.Collections.Generic;
public static class ModelCheck
{
    const float Dt = 0.02f;
    static List<Vector2> Path(RockPhysicsProfile p, float w, bool inTurn, out float stop)
    {
        var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, p.LaunchSpeedFromWeight(w)), p);
        var pts = new List<Vector2>(); float t = 0;
        for (int i = 0; i < 20000 && !s.asleep; i++)
        {
            RockPhysics.Step(ref s, p, Dt); t += Dt;
            if (s.freeFlight && s.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref s, p, inTurn);
            pts.Add(s.position);
        }
        stop = t; return pts;
    }
    static float XAt(List<Vector2> r, float y)
    {
        for (int i = 1; i < r.Count; i++) if (r[i - 1].y <= y && r[i].y >= y) return Mathf.Lerp(r[i - 1].x, r[i].x, (y - r[i - 1].y) / Mathf.Max(1e-6f, r[i].y - r[i - 1].y));
        return r[r.Count - 1].x;
    }
    public static string Go()
    {
        var sb = new StringBuilder();
        var leg = RockPhysicsProfile.Legacy(); var ar = RockPhysicsProfile.Arcade(); var re = RockPhysicsProfile.Real();
        sb.AppendLine("weight | worst|dx| arcade vs legacy @y | end dy | endx old/new || real vs arcade: end dx, end dy, stop ratio");
        foreach (bool turn in new[] { false, true }) foreach (float w in new[] { 5f, 5.5f, 6f, 6.5f, 7f, 8f, 8.5f, 9f, 9.35f, 10f, 11f, 12.4f, 13.75f, 15.8f })
        {
            float sl, sa, sr;
            var a = Path(leg, w, turn, out sl); var b = Path(ar, w, turn, out sa); var c = Path(re, w, turn, out sr);
            float top = Mathf.Min(a[a.Count - 1].y, b[b.Count - 1].y) - 0.1f; float worst = 0, wy = 0;
            for (float y = RockPhysics.ReleaseY + 0.1f; y < top; y += 0.25f) { float d = Mathf.Abs(XAt(a, y) - XAt(b, y)); if (d > worst) { worst = d; wy = y; } }
            var ea = a[a.Count - 1]; var eb = b[b.Count - 1]; var ec = c[c.Count - 1];
            sb.AppendLine((turn ? "IN  " : "OUT ") + string.Format("{0,5:F2} | {1,6:F3} @ {2,6:F1} | {3,6:F3} | {4,6:F3}/{5,6:F3} || {6,6:F3} {7,6:F3} {8,5:F2}", w, worst, wy, eb.y - ea.y, ea.x, eb.x, ec.x - eb.x, ec.y - eb.y, sr / sa));
        }
        return sb.ToString();
    }
}
