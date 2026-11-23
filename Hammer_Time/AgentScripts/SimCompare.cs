using UnityEngine;
using System.Text;
using System.Collections.Generic;
public static class SimCompare
{
    public static string Go()
    {
        var sb = new StringBuilder();
        sb.AppendLine("TrajectorySimulator(0.62 damping, same as AI/preview) vs real physics table, out-turn, from hack (0,-25). Final point of simulated path:");
        sb.AppendLine("  v0  | sim end (x,y) | sim path pts");
        var sim = new TrajectorySimulator(0.62f, 0.25f);
        foreach (float v in new[] { 5f, 6f, 8f, 8.5f, 9.35f, 10f, 12.4f, 13.75f, 15.8f })
        {
            var path = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, v), false, 600, null, false);
            var end = path[path.Count - 1];
            sb.AppendLine(string.Format("{0,5:F2} | ({1:F2},{2:F2}) | {3}", v, end.x, end.y, path.Count));
        }
        return sb.ToString();
    }
}
