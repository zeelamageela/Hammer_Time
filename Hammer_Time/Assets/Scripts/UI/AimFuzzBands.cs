using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the skill fuzz is SHOWN (see AimFuzz for how it is rolled). Two soft, shimmering shapes, both small and faint when the shooter is
/// skilled and wide and vague when not:
///  - a landing patch at the end of the aim line: as wide as the biggest sideways error the throw could have and as tall as the biggest
///    long/short error, densest in the middle (where the rock most likely finishes) and fading to nothing at the edge,
///  - a cone along the trajectory that widens from nothing at the hack to the patch, darkest down the middle, never thinner than a rock.
/// The rock always goes where the physics says, and the drawn end is never further than the maximum error from the truth, so the truth
/// is always inside the patch. The softness comes from small generated textures, so the edges are smooth.
/// </summary>
public class AimFuzzBands : MonoBehaviour
{
    static readonly Color fuzzGrey = new Color(0.23f, 0.23f, 0.23f);

    /// <summary>Opacity of the patch at its centre.</summary>
    const float PatchAlpha = 0.20f;
    /// <summary>Opacity of the cone at its strongest, in the middle.</summary>
    const float ConeAlpha = 0.04f;
    /// <summary>The cone is never thinner than one rock (0.28 m) beyond the error it shows, so it always reads as the rock's path.</summary>
    const float ConeExtraWidth = 0.28f;
    /// <summary>The patch is never smaller than one rock.</summary>
    const float PatchMinSize = 0.28f;
    /// <summary>The dense middle of the patch, as a share of its radius; the rest is the soft edge.</summary>
    const float PatchSolid = 0.25f;

    SpriteRenderer patch;
    LineRenderer cone;
    static Texture2D patchTexture, coneTexture;
    static Sprite patchSprite;

    // what was last drawn, so the shimmer can keep moving it
    Vector2 patchSize;
    float coneFull, shimmerStrength;
    bool anyShown;

    /// <summary>The widest sideways error (half-width, metres) the line can have at this aim skill.</summary>
    public static float AimHalfWidth(float aimSkill) { return AimFuzz.Enabled ? AimFuzz.AimSigma(aimSkill) * AimFuzz.MaxSigmas : 0f; }

    /// <summary>The largest long/short error (half-height, metres) at this weight skill.</summary>
    public static float WeightHalfHeight(float weightSkill) { return AimFuzz.Enabled ? AimFuzz.WeightSigma(weightSkill) * AimFuzz.MaxSigmas : 0f; }

    // ---- the soft edges -------------------------------------------------------------------------------------------------

    /// <summary>Opacity (0-1) of the landing patch at a distance from its centre (0 centre, 1 edge, in the patch's own stretched units).</summary>
    public static float PatchProfile(float radius)
    {
        return 1f - Smooth(PatchSolid, 1f, Mathf.Clamp01(radius));
    }

    /// <summary>Opacity (0-1) of the cone across it (0 centre, 1 edge): darkest in the middle, blurring out to nothing. Same all along its length.</summary>
    public static float ConeProfile(float across, float along)
    {
        return 1f - Smooth(0.1f, 1f, Mathf.Abs(across));
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
                float a = alpha((x / (float)(u - 1)) * 2f - 1f, (y / (float)(v - 1)) * 2f - 1f);
                pixels[y * u + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
            }
        tex.SetPixels32(pixels);
        tex.Apply(false, true);
        return tex;
    }

    void Build()
    {
        if (patch != null) return;
        if (patchTexture == null)
        {
            patchTexture = MakeTexture(64, 64, (x, y) => PatchProfile(Mathf.Sqrt(x * x + y * y)));
            patchSprite = Sprite.Create(patchTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);   // one unit wide, so scale = size in metres
        }
        if (coneTexture == null) coneTexture = MakeTexture(2, 64, (x, y) => ConeProfile(y, 0f));

        var go = new GameObject("FuzzPatch");
        go.transform.SetParent(transform, false);
        patch = go.AddComponent<SpriteRenderer>();
        patch.sprite = patchSprite;
        patch.sortingOrder = 0;
        patch.enabled = false;

        var coneGo = new GameObject("FuzzCone");
        coneGo.transform.SetParent(transform, false);
        cone = coneGo.AddComponent<LineRenderer>();
        cone.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = coneTexture };
        cone.textureMode = LineTextureMode.Stretch;
        cone.sortingOrder = 0;
        cone.useWorldSpace = true;
        cone.numCapVertices = 0;
        cone.widthCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        cone.enabled = false;
    }

    // ---- showing ---------------------------------------------------------------------------------------------------------

    /// <summary>Hides everything (a new turn, the rock was thrown).</summary>
    public void Hide()
    {
        anyShown = false;
        if (patch == null) return;
        patch.enabled = cone.enabled = false;
    }

    /// <summary>Hides the landing patch but leaves the cone (the aim circle took over).</summary>
    public void HidePatch()
    {
        if (patch != null) patch.enabled = false;
    }

    /// <summary>The landing patch: centred where the line ends, as wide and tall as the biggest errors the throw could have.</summary>
    public void ShowPatch(Vector2 centre, float aimHalf, float weightHalf, float aimSkill, float weightSkill)
    {
        Build();
        patchSize = new Vector2(Mathf.Max(PatchMinSize, aimHalf * 2f), Mathf.Max(PatchMinSize, weightHalf * 2f));
        shimmerStrength = Shimmer(Mathf.Min(aimSkill, weightSkill));
        patch.enabled = true;
        patch.transform.position = new Vector3(centre.x, centre.y, 0f);
        anyShown = true;
        Apply(0f);
    }

    /// <summary>The cone along the drawn trajectory. Called every time the aim is redrawn, so it follows the aim.</summary>
    public void ShowCone(IList<Vector3> path, float aimHalf, float aimSkill)
    {
        Build();
        if (path == null || path.Count < 2) { cone.enabled = false; return; }
        coneFull = aimHalf * 2f + ConeExtraWidth;
        shimmerStrength = Mathf.Max(shimmerStrength, Shimmer(aimSkill));
        cone.enabled = true;
        cone.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++) cone.SetPosition(i, path[i]);
        anyShown = true;
        Apply(0f);
    }

    /// <summary>0..1: how much the shapes shimmer. Even the best shooter gets a hint of it so they always look alive.</summary>
    static float Shimmer(float skill) { return Mathf.Lerp(1f, 0.3f, Mathf.Clamp01(skill / 100f)); }

    void Update()
    {
        if (anyShown) Apply(Time.unscaledTime);
    }

    void Apply(float t)
    {
        // the shimmer breathes the size a little and the strength more, out of step between the shapes
        float wobbleX = 1f + 0.18f * shimmerStrength * (Mathf.PerlinNoise(t * 1.8f, 0.2f) * 2f - 1f);
        float wobbleY = 1f + 0.18f * shimmerStrength * (Mathf.PerlinNoise(t * 1.5f, 7.1f) * 2f - 1f);
        float pulse = 1f + 0.55f * shimmerStrength * (Mathf.PerlinNoise(t * 2.8f, 3.3f) * 2f - 1f);
        float conePulse = 1f + 0.55f * shimmerStrength * (Mathf.PerlinNoise(t * 2.5f, 9.7f) * 2f - 1f);

        if (patch != null && patch.enabled)
        {
            patch.transform.localScale = new Vector3(patchSize.x * wobbleX, patchSize.y * wobbleY, 1f);
            patch.color = With(fuzzGrey, PatchAlpha * pulse);
        }
        if (cone != null && cone.enabled)
        {
            cone.widthMultiplier = coneFull * wobbleX;
            cone.startColor = cone.endColor = With(fuzzGrey, ConeAlpha * conePulse);
        }
    }

    static Color With(Color c, float a) { return new Color(c.r, c.g, c.b, Mathf.Clamp01(a)); }
}
