using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SkillSpendTests
{
    GameObject go;
    Slider slider;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("slider");
        slider = go.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.wholeNumbers = true;
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(go);

    [Test]
    public void Add_SpendsOnePointAndRaisesTheStat()
    {
        slider.value = 45;

        int points = SkillSpend.Add(slider, 3);

        Assert.AreEqual(46f, slider.value);
        Assert.AreEqual(2, points);
    }

    [Test]
    public void Subtract_AtZero_DoesNotCreatePoints()
    {
        slider.value = 0;

        int points = 0;
        for (int i = 0; i < 20; i++) points = SkillSpend.Subtract(slider, points);

        Assert.AreEqual(0, points, "tapping Minus on a stat at 0 used to add a point every tap");
        Assert.AreEqual(0f, slider.value);
    }

    [Test]
    public void Add_AtMaximum_DoesNotWastePoints()
    {
        slider.value = 100;

        int points = SkillSpend.Add(slider, 5);

        Assert.AreEqual(5, points);
        Assert.AreEqual(100f, slider.value);
    }

    [Test]
    public void Add_WithNoPoints_ChangesNothing()
    {
        slider.value = 45;

        int points = SkillSpend.Add(slider, 0);

        Assert.AreEqual(0, points);
        Assert.AreEqual(45f, slider.value, "a stat must not rise without a point to pay for it");
    }

    [Test]
    public void Subtract_RefundsOnePointWhenTheStatDrops()
    {
        slider.value = 50;

        int points = SkillSpend.Subtract(slider, 2);

        Assert.AreEqual(49f, slider.value);
        Assert.AreEqual(3, points);
    }
}
