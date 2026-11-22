using UnityEngine.UI;

/// <summary>
/// Skill-point arithmetic for the stat sliders. A point is only spent or refunded when the slider really moved,
/// so Plus at the maximum and Minus at the minimum cannot create or waste points.
/// </summary>
public static class SkillSpend
{
    /// <summary>Spends a point to raise the slider by 1. Returns the points left.</summary>
    public static int Add(Slider slider, int points)
    {
        if (points <= 0) return points;
        float before = slider.value;
        slider.value = before + 1;
        return slider.value != before ? points - 1 : points;
    }

    /// <summary>Refunds a point for lowering the slider by 1. Returns the points after.</summary>
    public static int Subtract(Slider slider, int points)
    {
        float before = slider.value;
        slider.value = before - 1;
        return slider.value != before ? points + 1 : points;
    }
}
