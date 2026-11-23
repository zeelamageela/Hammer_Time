using UnityEngine;
using System.Text;
public static class ModelCheck
{
    public static string Go()
    {
        var p = RockPhysicsProfile.Arcade();
        var sb = new StringBuilder();
        var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, 6f));
        int rel = -1;
        for (int i = 1; i <= 1500; i++)
        {
            RockPhysics.Step(ref s, p, 0.02f);
            if (!s.sliding && s.position.y > RockPhysics.ReleaseY) { RockPhysics.Release(ref s, p, false); rel = i; sb.AppendLine("release step " + i + " y=" + s.position.y.ToString("F4")); }
            if (i % 250 == 0 || s.asleep && s.sleepTimer > 0 && i % 1 == 0 && false) sb.AppendLine("t=" + (i * 0.02f).ToString("F2") + " x=" + s.position.x.ToString("F4") + " y=" + s.position.y.ToString("F4") + " v=" + s.velocity.magnitude.ToString("F5") + " spin=" + s.spin.ToString("F3") + " asleep=" + s.asleep);
            if (s.asleep) { sb.AppendLine("asleep at t=" + (i * 0.02f).ToString("F2") + " pos=" + s.position); break; }
        }
        return sb.ToString();
    }
}
