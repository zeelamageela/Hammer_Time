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

    /// <summary>One opacity for the aim aids that sit on the ice (guide lines, the aim circle's fill, the cone, the fuzz patch), so they read as one set.</summary>
    public const float AimAidAlpha = 0.12f;
    /// <summary>The aim circle's thick edge is solid.</summary>
    const float RingStrokeAlpha = 0.6f;
    /// <summary>Diameter of the aim circle in metres. It is a true circle: always scaled the same both ways.</summary>
    public const float RingDiameter = 0.8f;
    /// <summary>The edge's thickness as a share of the radius (0.12 of 0.4 m is about 5 cm).</summary>
    const float RingThickness = 0.12f;
    const float PatchAlpha = AimAidAlpha;
    const float ConeAlpha = AimAidAlpha;
    /// <summary>The patch is never smaller than one rock.</summary>
    const float PatchMinSize = 0.28f;
    /// <summary>The dense middle of the patch, as a share of its radius; the rest is the soft edge.</summary>
    const float PatchSolid = 0.25f;

    SpriteRenderer patch, ring;
    LineRenderer cone;
    static Texture2D patchTexture, coneTexture, ringTexture;
    static Sprite patchSprite, ringSprite;

    // what was last drawn, so the shimmer can keep moving it
    Vector2 patchSize;
    float coneFade, shimmerStrength;   // coneFade: how far along the cone (0-1) it starts to fade out inside the aim circle
    readonly Gradient coneGradient = new Gradient();
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

    /// <summary>
    /// The aim circle across its radius (0 centre, 1 edge): a thick solid edge (1) and a faint fill (0.2), with a slightly soft rim so it is not jagged.
    /// Returns 0 outside. The thickness is relative, so the circle looks the same at any size.
    /// </summary>
    public static float RingProfile(float radius)
    {
        float inner = 1f - RingThickness;
        float rim = 0.03f;
        float stroke = Smooth(inner - rim, inner, radius) * (1f - Smooth(1f - rim, 1f, radius));
        float fill = radius < inner ? 0.2f : 0f;
        return Mathf.Max(stroke, fill);
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
        if (ringTexture == null)
        {
            ringTexture = MakeTexture(128, 128, (x, y) => RingProfile(Mathf.Sqrt(x * x + y * y)));
            ringSprite = Sprite.Create(ringTexture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128f);
        }
        if (coneTexture == null) coneTexture = MakeTexture(2, 64, (x, y) => ConeProfile(y, 0f));

        var go = new GameObject("FuzzPatch");
        go.transform.SetParent(transform, false);
        patch = go.AddComponent<SpriteRenderer>();
        patch.sprite = patchSprite;
        patch.sortingOrder = 0;
        patch.enabled = false;

        var ringGo = new GameObject("AimCircle");
        ringGo.transform.SetParent(transform, false);
        ring = ringGo.AddComponent<SpriteRenderer>();
        ring.sprite = ringSprite;
        ring.sortingOrder = 1;
        ring.transform.localScale = new Vector3(RingDiameter, RingDiameter, 1f);   // the same both ways: never stretched
        ring.enabled = false;

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
        patch.enabled = cone.enabled = ring.enabled = false;
    }

    /// <summary>The aim circle: a true circle with a thick solid edge, at the aim point.</summary>
    public void ShowRing(Vector2 centre, Color colour)
    {
        Build();
        ring.enabled = true;
        ring.transform.position = new Vector3(centre.x, centre.y, 0f);
        // the sprite carries the solid edge (1) and the faint fill (0.2); the colour's alpha scales both
        ring.color = new Color(colour.r, colour.g, colour.b, RingStrokeAlpha);
    }

    public void HideRing()
    {
        if (ring != null) ring.enabled = false;
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

    /// <summary>
    /// The cone along the line, widening from nothing at the hack to exactly the aim circle's width at the aim point, so it runs into the circle
    /// like an ice-cream cone. It ends at the circle's centre and fades out over the last half-diameter (inside the circle) so the two do not
    /// double up. Called every time the aim is redrawn, so it follows the aim.
    /// </summary>
    public void ShowCone(IList<Vector3> path, Vector2 end, float aimSkill)
    {
        Build();
        if (path == null || path.Count < 1) { cone.enabled = false; return; }

        var points = new List<Vector3>(path);
        if (((Vector2)points[points.Count - 1] - end).sqrMagnitude > 0.0001f) points.Add(new Vector3(end.x, end.y, 0f));
        if (points.Count < 2) { cone.enabled = false; return; }

        float length = 0f;
        for (int i = 1; i < points.Count; i++) length += Vector3.Distance(points[i - 1], points[i]);
        coneFade = length > 0.01f ? Mathf.Clamp(1f - (RingDiameter * 0.5f) / length, 0f, 0.98f) : 0f;

        shimmerStrength = Shimmer(aimSkill);
        cone.enabled = true;
        cone.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) cone.SetPosition(i, points[i]);
        cone.widthMultiplier = RingDiameter;   // the curve runs 0 to 1, so the far end is exactly as wide as the circle
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
            float a = Mathf.Clamp01(ConeAlpha * conePulse);
            coneGradient.SetKeys(
                new[] { new GradientColorKey(fuzzGrey, 0f), new GradientColorKey(fuzzGrey, 1f) },
                new[] { new GradientAlphaKey(a, 0f), new GradientAlphaKey(a, coneFade), new GradientAlphaKey(0f, 1f) });
            cone.colorGradient = coneGradient;
        }
    }

    static Color With(Color c, float a) { return new Color(c.r, c.g, c.b, Mathf.Clamp01(a)); }
}
