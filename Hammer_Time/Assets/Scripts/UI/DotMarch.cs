using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The trajectory dots slowly marching down the line (On-Ice Training 1): the pattern of evenly spaced dots slides one spacing along the path
/// every CycleSeconds and starts over, so it reads as a steady flow toward where the rock is going. Pure arithmetic so it can be tested.
/// </summary>
public static class DotMarch
{
    /// <summary>Seconds for the dots to slide one dot-spacing down the line.</summary>
    public const float CycleSeconds = 2.5f;

    /// <summary>Where in the cycle we are (0..1) at a time.</summary>
    public static float Phase(float time) { return Mathf.Repeat(time / CycleSeconds, 1f); }

    /// <summary>The distance along the path (0..total) of dot number k.</summary>
    public static float Arc(int k, float phase, float spacing, float total)
    {
        if (total <= 0f) return 0f;
        return Mathf.Repeat((k + phase) * spacing, total);
    }

    /// <summary>0..1 opacity of a dot at this distance: it fades in over the first few percent of the line and out over the last, so the wrap-around is invisible.</summary>
    public static float Fade(float arc, float total, float edge = 0.08f)
    {
        if (total <= 0f) return 0f;
        float t = arc / total;
        return Mathf.Clamp01(Mathf.Min(t, 1f - t) / edge);
    }

    /// <summary>Cumulative distance to each point of the path (first entry 0).</summary>
    public static float[] Arcs(IList<Vector3> path)
    {
        var arcs = new float[path.Count];
        for (int i = 1; i < path.Count; i++) arcs[i] = arcs[i - 1] + Vector3.Distance(path[i - 1], path[i]);
        return arcs;
    }

    /// <summary>The point on the path at a distance along it, and the index of the path point just before it.</summary>
    public static Vector3 PointAt(IList<Vector3> path, float[] arcs, float arc, out int index)
    {
        index = 0;
        if (path.Count == 0) return Vector3.zero;
        if (path.Count == 1 || arc <= 0f) return path[0];
        int i = 1;
        while (i < path.Count - 1 && arcs[i] < arc) i++;
        index = i - 1;
        float seg = arcs[i] - arcs[i - 1];
        float t = seg > 0.0001f ? Mathf.Clamp01((arc - arcs[i - 1]) / seg) : 0f;
        return Vector3.Lerp(path[i - 1], path[i], t);
    }
}
