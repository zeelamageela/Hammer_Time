using UnityEngine;
using System.Text;
using System.Collections.Generic;
public static class ModelCheck
{
    public static string Go()
    {
        var p = RockPhysicsProfile.Arcade(); var sb = new StringBuilder();
        sb.AppendLine("weight turn | max speed rise | slow over last 1s | heading swing last 0.5s | stop t | hogspeed");
        foreach (bool inTurn in new[] { false, true })
        foreach (float w in new[] { 5f, 5.5f, 6f, 6.5f, 7f, 8f, 8.5f, 9f, 9.35f, 10f, 11f, 12.4f, 13.75f, 15.8f })
        {
            var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, p.LaunchSpeedFromWeight(w)), p);
            var sp = new List<float>(); var hd = new List<float>(); var tt = new List<float>(); float t = 0, hog = 0;
            for (int i = 0; i < 20000 && !s.asleep; i++)
            {
                RockPhysics.Step(ref s, p, 0.02f); t += 0.02f;
                if (s.freeFlight && s.position.y > RockPhysics.ReleaseY) { RockPhysics.Release(ref s, p, inTurn); hog = s.velocity.magnitude; }
                if (!s.freeFlight && s.velocity.sqrMagnitude > 0) { sp.Add(s.velocity.magnitude); tt.Add(t); hd.Add(Mathf.Atan2(s.velocity.x, s.velocity.y) * Mathf.Rad2Deg); }
            }
            float rise = 0; for (int i = 1; i < sp.Count; i++) rise = Mathf.Max(rise, sp[i] - sp[i - 1]);
            float end = tt[tt.Count - 1]; int j = tt.FindIndex(x => x >= end - 1f); int k = tt.FindIndex(x => x >= end - 0.5f);
            sb.AppendLine(string.Format("{0,5:F2} {1,-3} | {2,10:F5} | {3,10:F3} | {4,12:F1} | {5,6:F1} | {6,5:F2}", w, inTurn ? "in" : "out", rise, sp[j] - sp[sp.Count - 1], Mathf.Abs(hd[hd.Count - 1] - hd[k]), t, hog));
        }
        return sb.ToString();
    }
}
