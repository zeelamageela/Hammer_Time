using NUnit.Framework;
using UnityEngine;

public class DotMarchTests
{
    [Test]
    public void TheDotsSlideOneSpacingPerCycleAndThenRepeat()
    {
        float spacing = 2f, total = 10f;
        Assert.AreEqual(0f, DotMarch.Arc(0, 0f, spacing, total), 1e-5f);
        Assert.AreEqual(1f, DotMarch.Arc(0, 0.5f, spacing, total), 1e-5f, "half a cycle: half a spacing");
        // a whole cycle later the pattern looks the same: dot 1 is where dot 0 was
        Assert.AreEqual(DotMarch.Arc(1, 0f, spacing, total), DotMarch.Arc(0, 1f, spacing, total), 1e-5f);
        Assert.AreEqual(0f, DotMarch.Arc(5, 0f, spacing, total), 1e-5f, "past the end it wraps to the start");
        Assert.AreEqual(0f, DotMarch.Phase(0f), 1e-5f);
        Assert.AreEqual(0f, DotMarch.Phase(DotMarch.CycleSeconds), 1e-4f);
    }

    [Test]
    public void DotsFadeAtBothEndsSoTheWrapIsInvisible()
    {
        Assert.AreEqual(0f, DotMarch.Fade(0f, 10f), 1e-5f);
        Assert.AreEqual(0f, DotMarch.Fade(10f, 10f), 1e-5f);
        Assert.AreEqual(1f, DotMarch.Fade(5f, 10f), 1e-5f);
    }

    [Test]
    public void APointAlongThePathIsFoundByDistance()
    {
        var path = new System.Collections.Generic.List<Vector3> { new Vector3(0, 0, 0), new Vector3(0, 2, 0), new Vector3(2, 2, 0) };
        var arcs = DotMarch.Arcs(path);
        Assert.AreEqual(4f, arcs[2], 1e-5f);
        int i;
        var p = DotMarch.PointAt(path, arcs, 1f, out i);
        Assert.AreEqual(1f, p.y, 1e-5f);
        Assert.AreEqual(0, i);
        p = DotMarch.PointAt(path, arcs, 3f, out i);
        Assert.AreEqual(1f, p.x, 1e-5f);
        Assert.AreEqual(2f, p.y, 1e-5f);
        Assert.AreEqual(1, i);
    }
}
