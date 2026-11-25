using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the skill fuzz is SHOWN (see AimFuzz for how it is rolled). Three soft, shimmering shapes, all faint when the shooter is skilled and
/// wide and vague when not:
///  - a vertical band around the aim guide line: as wide as the biggest sideways error the throw could have, soft left and right only,
///  - a horizontal band around the distance line: as tall as the biggest long/short error, soft up and down only,
///  - a cone along the trajectory that widens from nothing at the hack to the full sideways error at the end, with blurred edges, and
///    hollowed out in the middle toward the end so it is the inverse of the vertical band it runs into.
/// The rock always goes where the physics says, and the drawn end is never further than the maximum error from the truth, so the truth
/// is inside the solid part of the band. The softness comes from a small generated texture on each line (its V axis runs across the line),
/// so the edges are smooth and the ends of a band are square: nothing fades along the band's length.
/// </summary>
public class AimFuzzBands : MonoBehaviour
{
    static readonly Color guideGrey = new Color(0.23f, 0.23f, 0.23f);

    /// <summary>Opacity of the solid middle of a guide band.</summary>
    const float BandAlpha = 0.30f;
    /// <summary>Opacity of the cone at its strongest (half what it was).</summary>
    const float ConeAlpha = 0.08f;
    /// <summary>The solid part of a band, as a share of its half-width; the rest is the soft edge.</summary>
    const float BandSolid = 0.55f;

    LineRenderer vertical, horizontal, cone;
    static Texture2D bandTexture, coneTexture;

    // what was last drawn, so the shimmer can keep moving it
    float verticalFull, horizontalFull, coneFull, shimmerStrength;
    Color horizontalColor = Color.white;
    bool anyShown;

    /// <summary>The widest sideways error (half-width, metres) the line can have at this aim skill.</summary>
    public static float AimHalfWidth(float aimSkill) { return AimFuzz.Enabled ? AimFuzz.AimSigma(aimSkill) * AimFuzz.MaxSigmas : 0f; }

    /// <summary>The largest long/short error (half-height, metres) at this weight skill.</summary>
    public static float WeightHalfHeight(float weightSkill) { return AimFuzz.Enabled ? AimFuzz.WeightSigma(weightSkill) * AimFuzz.MaxSigmas : 0f; }

    // ---- the soft edges -------------------------------------------------------------------------------------------------

    /// <summary>Opacity (0-1) across a band: solid in the middle, then a smooth fall to nothing. across = 0 at the centre, 1 at the edge.</summary>
    public static float BandProfile(float across)
    {
        return 1f - Smooth(BandSolid, 1f, Mathf.Abs(across));
    }

    /// <summary>Opacity (0-1) of the cone at a place across it (0 centre, 1 edge) and along it (0 hack end, 1 far end).</summary>
    public static float ConeProfile(float across, float along)
    {
        across = Mathf.Abs(across);
        float soft = 1f - Smooth(0.45f, 1f, across);                 // blurred outer edge
        float middle = 1f - Smooth(0.05f, 0.55f, across);            // where the vertical band is solid
        float masked = Smooth(0.25f, 1f, along);                     // the mask grows toward the far end
        return soft * (1f - 0.92f * masked * middle);
    }

    static float Smooth(float a, float b, float x)
    {
        float t = Mathf.Clamp01((x - a) / (b - a));
        return t * t * (3f - 2f * t);
    }

    static Texture2D MakeTexture(int u, int v, System.Func<float, float, float> alpha)
    {
        var tex = new Texture2D(u, v, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        var pixels = new Color32[u * v];
        for (int y = 0; y < v; y++)
            for (int x = 0; x < u; x++)
            {
                float a = alpha(x / (float)(u - 1), (y / (float)(v - 1)) * 2f - 1f);
                pixels[y * u + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
            }
        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        return tex;
    }

    LineRenderer Make(string name, Texture2D tex)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var l = go.AddComponent<LineRenderer>();
        l.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = tex };
        l.textureMode = LineTextureMode.Stretch;
        l.sortingOrder = 0;
        l.useWorldSpace = true;
        l.numCapVertices = 0;   // square ends: the band only softens across, never along
        l.numCornerVertices = 0;
        l.enabled = false;
        return l;
    }

    void Build()
    {
        if (vertical != null) return;
        if (bandTexture == null) bandTexture = MakeTexture(2, 128, (along, across) => BandProfile(across));
        if (coneTexture == null) coneTexture = MakeTexture(64, 128, (along, across) => ConeProfile(across, along));
        vertical = Make("FuzzBandVertical", bandTexture);
        horizontal = Make("FuzzBandHorizontal", bandTexture);
        cone = Make("FuzzCone", coneTexture);
        cone.widthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    }

    // ---- showing ---------------------------------------------------------------------------------------------------------

    /// <summary>Hides everything (a new turn, the rock was thrown).</summary>
    public void Hide()
    {
        anyShown = false;
        if (vertical == null) return;
        vertical.enabled = horizontal.enabled = cone.enabled = false;
    }

    /// <summary>Hides the two guide bands but leaves the cone (the aim circle took over the guide lines).</summary>
    public void HideGuides()
    {
        if (vertical == null) return;
        vertical.enabled = horizontal.enabled = false;
    }

    /// <summary>The soft band around the vertical aim guide and the horizontal distance line.</summary>
    public void ShowGuides(float verticalX, float topY, float bottomY, float distanceY, float iceLeft, float iceRight,
                           float aimHalf, float weightHalf, float aimSkill, float weightSkill, Color teamColor)
    {
        Build();
        // the solid part of each band spans the largest possible error; the soft edge reaches out beyond it
        verticalFull = aimHalf * 2f / BandSolid;
        horizontalFull = weightHalf * 2f / BandSolid;
        horizontalColor = teamColor;
        shimmerStrength = Shimmer(Mathf.Min(aimSkill, weightSkill));

        vertical.enabled = verticalFull > 0.001f;
        vertical.positionCount = 2;
        vertical.SetPosition(0, new Vector3(verticalX, topY, 0f));
        vertical.SetPosition(1, new Vector3(verticalX, bottomY, 0f));

        horizontal.enabled = horizontalFull > 0.001f;
        horizontal.positionCount = 2;
        horizontal.SetPosition(0, new Vector3(iceLeft, distanceY, 0f));
        horizontal.SetPosition(1, new Vector3(iceRight, distanceY, 0f));

        anyShown = true;
        Apply(0f);
    }

    /// <summary>The cone along the drawn trajectory. Called every time the aim is redrawn, so it follows the aim.</summary>
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
        // the shimmer moves the width a little and the strength more, out of step between the shapes
        float wobbleV = 1f + 0.10f * shimmerStrength * (Mathf.PerlinNoise(t * 1.3f, 0.2f) * 2f - 1f);
        float wobbleH = 1f + 0.10f * shimmerStrength * (Mathf.PerlinNoise(t * 1.1f, 7.1f) * 2f - 1f);
        float pulseV = 1f + 0.35f * shimmerStrength * (Mathf.PerlinNoise(t * 2.1f, 3.3f) * 2f - 1f);
        float pulseH = 1f + 0.35f * shimmerStrength * (Mathf.PerlinNoise(t * 1.9f, 9.7f) * 2f - 1f);

        Style(vertical, verticalFull * wobbleV, With(guideGrey, BandAlpha * pulseV));
        Style(horizontal, horizontalFull * wobbleH, With(horizontalColor, BandAlpha * pulseH));
        if (cone != null && cone.enabled)
        {
            cone.widthMultiplier = coneFull * wobbleV;
            cone.startColor = cone.endColor = With(guideGrey, ConeAlpha * pulseV);
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
