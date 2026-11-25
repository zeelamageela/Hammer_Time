using UnityEngine;
using System.Text;
using System.Collections.Generic;
public static class ModelCheck
{
    const float Dt = 0.02f;
    static List<Vector2> Path(RockPhysicsProfile p, float w, bool inTurn, out float stop, out float hh)
    {
        var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, p.LaunchSpeedFromWeight(w)), p);
        var pts = new List<Vector2>(); float t = 0, tNear = -1, tFar = -1; Vector2 prev = s.position;
        for (int i = 0; i < 20000 && !s.asleep; i++)
        {
            RockPhysics.Step(ref s, p, Dt); t += Dt;
            if (s.freeFlight && s.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref s, p, inTurn);
            if (tNear < 0 && prev.y < -16.15f && s.position.y >= -16.15f) tNear = t;
            if (tFar < 0 && prev.y < 0f && s.position.y >= 0f) tFar = t;
            prev = s.position; pts.Add(s.position);
        }
        stop = t; hh = (tNear >= 0 && tFar >= 0) ? tFar - tNear : -1; return pts;
    }
    static float XAt(List<Vector2> r, float y)
    {
        for (int i = 1; i < r.Count; i++) if (r[i - 1].y <= y && r[i].y >= y) return Mathf.Lerp(r[i - 1].x, r[i].x, (y - r[i - 1].y) / Mathf.Max(1e-6f, r[i].y - r[i - 1].y));
        return r[r.Count - 1].x;
    }
    public static string Go()
    {
        var sb = new StringBuilder();
        sb.AppendLine("stop curve | worst path dx (m) @weight | worst end dy @weight | button hog-hog | button stop time");
        var leg = RockPhysicsProfile.Legacy();
        foreach (float power in new[] { 0.5f, 0.6f, 0.7f, 0.8f })
        {
            var ar = RockPhysicsProfile.Arcade(power);
            float worstDx = 0, wDxW = 0, worstDy = 0, wDyW = 0, bhh = 0, bstop = 0;
            foreach (bool turn in new[] { false, true })
            foreach (float w in new[] { 5f, 5.5f, 6f, 6.5f, 7f, 8f, 8.5f, 9f, 9.35f, 10f, 11f, 12.4f, 13.75f, 15.8f })
            {
                float sl, hl, sa, ha;
                var a = Path(leg, w, turn, out sl, out hl); var b = Path(ar, w, turn, out sa, out ha);
                float top = Mathf.Min(a[a.Count - 1].y, b[b.Count - 1].y) - 0.1f;
                for (float y = RockPhysics.ReleaseY + 0.1f; y < top; y += 0.25f) { float d = Mathf.Abs(XAt(a, y) - XAt(b, y)); if (d > worstDx) { worstDx = d; wDxW = w; } }
                float dy = Mathf.Abs(a[a.Count - 1].y - b[b.Count - 1].y); if (dy > worstDy) { worstDy = dy; wDyW = w; }
                if (w == 8.5f && !turn) { bhh = ha; bstop = sa; }
            }
            sb.AppendLine(string.Format("{0:F1}        | {1,6:F3} @ {2,5:F2}        | {3,6:F3} @ {4,5:F2}     | {5,6:F2}         | {6,6:F2}", power, worstDx, wDxW, worstDy, wDyW, bhh, bstop));
        }
        return sb.ToString();
    }
}
