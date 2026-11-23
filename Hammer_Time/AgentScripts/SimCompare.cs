using UnityEngine;
using System.Text;
using System.Collections.Generic;
public static class SimCompare
{
    // live rock rest points measured in the game (AgentScripts/Measure.cs Run/RunAI), out-turn from the hack
    static readonly float[][] Live =
    {
        new[] { 5.0f, -1.04f, -2.67f }, new[] { 6.0f, -1.01f, 0.01f }, new[] { 8.0f, -0.94f, 5.31f }, new[] { 8.5f, -0.92f, 6.52f },
        new[] { 9.35f, -0.92f, 8.74f }, new[] { 10.0f, -0.72f, 10.49f }, new[] { 12.4f, -0.40f, 16.78f }, new[] { 13.75f, -0.33f, 20.51f }, new[] { 15.8f, -0.24f, 25.72f },
    };

    public static string Go()
    {
        var sb = new StringBuilder();
        sb.AppendLine("TrajectorySimulator (now RockPhysics) vs the live rock's measured rest point");
        sb.AppendLine("  v0   | live (x,y)        | sim (x,y)         | error (dx,dy) | points");
        var sim = new TrajectorySimulator(0.62f, 0.25f);
        foreach (var row in Live)
        {
            var path = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, row[0]), false, 600, null, false);
            var end = path[path.Count - 1];
            sb.AppendLine(string.Format("{0,5:F2} | ({1,5:F2},{2,6:F2})  | ({3,5:F2},{4,6:F2})  | ({5,5:F2},{6,5:F2}) | {7}", row[0], row[1], row[2], end.x, end.y, end.x - row[1], end.y - row[2], path.Count));
        }
        // collision against a stationary rock on the centre line near the button
        var go = new GameObject("TestRock"); go.transform.position = new Vector3(0f, 6.5f, 0f);
        var path2 = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, 12.4f), false, 600, new List<GameObject> { go }, false);
        var ci = sim.GetCollisionInfo();
        sb.AppendLine("takeout 12.4 onto a rock at (0,6.5): hit=" + ci.hasCollision + " at " + ci.shooterCenterAtCollision + " thrown ends " + ci.finalPosition + " struck ends " + ci.hitRockFinalPosition + " points=" + path2.Count);
        Object.DestroyImmediate(go);
        return sb.ToString();
    }
}
