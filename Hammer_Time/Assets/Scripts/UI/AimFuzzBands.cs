using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the skill fuzz is SHOWN (see AimFuzz for how it is rolled). Three soft, shimmering shapes, all faint when the shooter is skilled and
/// wide and vague when not:
///  - a vertical band around the aim guide line: as wide as the biggest sideways error the throw could have,
///  - a horizontal band around the distance line: as tall as the biggest long/short error (the top and bottom edge are the worst cases),
///  - a cone along the trajectory that widens from nothing at the hack to the full sideways error at the end.
/// The rock always goes where the physics says, and the drawn end is never further than the maximum error from the truth, so the truth
/// is always inside the band. The bands are stacks of translucent lines (wide and faint under narrow and stronger) so their edges are soft.
/// </summary>
public class AimFuzzBands : MonoBehaviour
{
    const int Layers = 3;
    static readonly Color guideGrey = new Color(0.23f, 0.23f, 0.23f);

    LineRenderer[] vertical, horizontal;
    LineRenderer cone;

    // what was last drawn, so the shimmer can keep moving it
    float verticalFull, horizontalFull, coneFull, shimmerStrength;
    Color horizontalColor = Color.white;
    bool anyShown;

    /// <summary>The widest sideways error (half-width, metres) the line can have at this aim skill.</summary>
    public static float AimHalfWidth(float aimSkill) { return AimFuzz.Enabled ? AimFuzz.AimSigma(aimSkill) * AimFuzz.MaxSigmas : 0f; }

    /// <summary>The largest long/short error (half-height, metres) at this weight skill.</summary>
    public static float WeightHalfHeight(float weightSkill) { return AimFuzz.Enabled ? AimFuzz.WeightSigma(weightSkill) * AimFuzz.MaxSigmas : 0f; }

    LineRenderer Make(string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var l = go.AddComponent<LineRenderer>();
        l.material = new Material(Shader.Find("Sprites/Default"));
        l.sortingOrder = order;
        l.useWorldSpace = true;
        l.numCapVertices = 4;
        l.enabled = false;
        return l;
    }

    void Build()
    {
        if (vertical != null) return;
        vertical = new LineRenderer[Layers];
        horizontal = new LineRenderer[Layers];
        for (int i = 0; i < Layers; i++)
        {
            vertical[i] = Make("FuzzBandVertical" + i, 0);
            horizontal[i] = Make("FuzzBandHorizontal" + i, 0);
        }
        cone = Make("FuzzCone", 0);
        cone.numCapVertices = 0;
        cone.widthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    }

    /// <summary>Hides everything (a new turn, the rock was thrown, the aim circle took over).</summary>
    public void Hide()
    {
        anyShown = false;
        if (vertical == null) return;
        for (int i = 0; i < Layers; i++) { vertical[i].enabled = false; horizontal[i].enabled = false; }
        cone.enabled = false;
    }

    public void HideGuides()
    {
        if (vertical == null) return;
        for (int i = 0; i < Layers; i++) { vertical[i].enabled = false; horizontal[i].enabled = false; }
    }

    /// <summary>The soft band around the vertical aim guide and the horizontal distance line.</summary>
    public void ShowGuides(float verticalX, float topY, float bottomY, float distanceY, float iceLeft, float iceRight,
                           float aimHalf, float weightHalf, float aimSkill, float weightSkill, Color teamColor)
    {
        Build();
        verticalFull = aimHalf * 2f;
        horizontalFull = weightHalf * 2f;
        horizontalColor = teamColor;
        shimmerStrength = Shimmer(Mathf.Min(aimSkill, weightSkill));
        for (int i = 0; i < Layers; i++)
        {
            vertical[i].enabled = verticalFull > 0.001f;
            vertical[i].positionCount = 2;
            vertical[i].SetPosition(0, new Vector3(verticalX, topY, 0f));
            vertical[i].SetPosition(1, new Vector3(verticalX, bottomY, 0f));
            horizontal[i].enabled = horizontalFull > 0.001f;
            horizontal[i].positionCount = 2;
            horizontal[i].SetPosition(0, new Vector3(iceLeft, distanceY, 0f));
            horizontal[i].SetPosition(1, new Vector3(iceRight, distanceY, 0f));
        }
        anyShown = true;
        Apply(0f);
    }

    /// <summary>The cone along the drawn trajectory.</summary>
    public void ShowCone(IList<Vector3> path, float aimHalf, float aimSkill)
    {
        Build();
        if (path == null || path.Count < 2 || aimHalf <= 0.001f) { cone.enabled = false; return; }
        coneFull = aimHalf * 2f;
        shimmerStrength = Mathf.Max(shimmerStrength, Shimmer(aimSkill));
        cone.enabled = true;
        cone.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++) cone.SetPosition(i, path[i]);
        anyShown = true;
        Apply(0f);
    }

    /// <summary>0..1: how much the shapes shimmer. Even the best shooter gets a hint of it so they always look alive.</summary>
    static float Shimmer(float skill) { return Mathf.Lerp(1f, 0.15f, Mathf.Clamp01(skill / 100f)); }

    void Update()
    {
        if (anyShown) Apply(Time.unscaledTime);
    }

    void Apply(float t)
    {
        // each stack: widest layer faintest. The shimmer moves the width a little and the strength more, out of step between shapes.
        float wobbleV = 1f + 0.12f * shimmerStrength * (Mathf.PerlinNoise(t * 1.3f, 0.2f) * 2f - 1f);
        float wobbleH = 1f + 0.12f * shimmerStrength * (Mathf.PerlinNoise(t * 1.1f, 7.1f) * 2f - 1f);
        float pulseV = 1f + 0.35f * shimmerStrength * (Mathf.PerlinNoise(t * 2.1f, 3.3f) * 2f - 1f);
        float pulseH = 1f + 0.35f * shimmerStrength * (Mathf.PerlinNoise(t * 1.9f, 9.7f) * 2f - 1f);

        for (int i = 0; i < Layers; i++)
        {
            float share = (Layers - i) / (float)Layers;   // 1, 0.66, 0.33 of the full width
            float alpha = 0.10f;
            Style(vertical[i], verticalFull * share * wobbleV, With(guideGrey, alpha * pulseV));
            Style(horizontal[i], horizontalFull * share * wobbleH, With(horizontalColor, alpha * pulseH));
        }
        if (cone != null && cone.enabled)
        {
            cone.widthMultiplier = coneFull * wobbleV;
            cone.startColor = cone.endColor = With(guideGrey, 0.16f * pulseV);
        }
    }

    static void Style(LineRenderer l, float width, Color c)
    {
        if (l == null) return;
        l.widthMultiplier = Mathf.Max(0f, width);
        l.startColor = l.endColor = c;
    }

    static Color With(Color c, float a) { return new Color(c.r, c.g, c.b, Mathf.Clamp01(a)); }
}
