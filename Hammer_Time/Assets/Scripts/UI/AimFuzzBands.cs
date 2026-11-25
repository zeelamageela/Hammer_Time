using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the skill fuzz is SHOWN (see AimFuzz for how it is rolled): a beam of light along the shot that ends in a glowing oval.
///  - the oval sits at the aim point (where the rock is expected to finish after its curl). It is as wide as the biggest sideways error the
///    throw could have and as tall as the biggest long/short error, so a skilled shooter gets a small tight oval and a beginner a big vague one.
///    It has a soft glowing outline of even thickness, and a faint fill.
///  - the cone runs from the hack and widens until it is exactly as wide as the oval, meeting the oval's sides, then fades out over the oval's
///    upper half so the two do not double up.
/// Both are screened on (added as light) rather than laid on as grey, and both shimmer, more for a less skilled shooter.
/// The rock always goes where the physics says, and the drawn end is never further than the maximum error from the truth, so the truth is inside the oval.
/// </summary>
public class AimFuzzBands : MonoBehaviour
{
    /// <summary>One opacity for the thin guide lines that lie on the ice (they are drawn as ordinary lines, not light).</summary>
    public const float AimAidAlpha = 0.12f;

    /// <summary>How bright the cone is at its middle.</summary>
    const float ConeStrength = 0.1f;
    /// <summary>The oval's faint fill, its glowing outline and its overall strength.</summary>
    const float OvalFill = 0.1f;
    const float OvalOutline = 0.1f;
    const float OvalStrength = 1.0f;
    /// <summary>Half the thickness of the oval's outline glow, in metres.</summary>
    const float OutlineHalfThickness = 0.05f;
    /// <summary>The oval is never smaller than one rock (0.28 m) either way.</summary>
    public const float OvalMinSize = 0.28f;

    /// <summary>The light's colour (the team colour mixed toward white); set by the caller.</summary>
    public Color tint = Color.white;

    MeshRenderer oval;
    Material ovalMaterial;
    LineRenderer cone;
    static Texture2D coneTexture;
    static Mesh quad;

    // what was last drawn, so the shimmer can keep moving it
    Vector2 ovalSize;
    float coneFade, shimmerStrength;
    readonly Gradient coneGradient = new Gradient();
    bool anyShown;

    /// <summary>The widest sideways error (half-width, metres) the line can have at this aim skill.</summary>
    public static float AimHalfWidth(float aimSkill) { return AimFuzz.Enabled ? AimFuzz.AimSigma(aimSkill) * AimFuzz.MaxSigmas : 0f; }

    /// <summary>The largest long/short error (half-height, metres) at this weight skill.</summary>
    public static float WeightHalfHeight(float weightSkill) { return AimFuzz.Enabled ? AimFuzz.WeightSigma(weightSkill) * AimFuzz.MaxSigmas : 0f; }

    /// <summary>The oval's width and height in metres. Wide: one rock plus the full sideways error range (so a perfect shooter gets one rock wide, and
    /// a beginner several rocks). Tall: the full long/short range, never less than a rock.</summary>
    public static Vector2 OvalSizeFor(float aimHalf, float weightHalf)
    {
        return new Vector2(OvalMinSize + aimHalf * 2f, Mathf.Max(OvalMinSize, weightHalf * 2f));
    }

    // ---- the soft edge of the cone ----------------------------------------------------------------------------------------

    /// <summary>Opacity (0-1) across the cone (0 centre, 1 edge): brightest down the middle, blurring out to nothing. Same all along its length.</summary>
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

    static Mesh Quad()
    {
        if (quad != null) return quad;
        quad = new Mesh { name = "AimOvalQuad" };
        quad.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f), new Vector3(0.5f, 0.5f), new Vector3(-0.5f, 0.5f) };
        quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        quad.triangles = new[] { 0, 2, 1, 0, 3, 2 };
        return quad;
    }

    void Build()
    {
        if (oval != null) return;
        if (coneTexture == null) coneTexture = MakeTexture(2, 64, (x, y) => ConeProfile(y, 0f));

        var ovalGo = new GameObject("AimOval");
        ovalGo.transform.SetParent(transform, false);
        ovalGo.AddComponent<MeshFilter>().sharedMesh = Quad();
        oval = ovalGo.AddComponent<MeshRenderer>();
        var ovalShader = Shader.Find("Hammer/AimOval");
        ovalMaterial = new Material(ovalShader != null ? ovalShader : Shader.Find("Sprites/Default"));
        oval.sharedMaterial = ovalMaterial;
        oval.sortingOrder = 2;
        oval.enabled = false;

        var coneGo = new GameObject("FuzzCone");
        coneGo.transform.SetParent(transform, false);
        cone = coneGo.AddComponent<LineRenderer>();
        var lightShader = Shader.Find("Hammer/AimLight");
        cone.material = new Material(lightShader != null ? lightShader : Shader.Find("Sprites/Default")) { mainTexture = coneTexture };
        cone.textureMode = LineTextureMode.Stretch;
        cone.sortingOrder = 2;
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
        if (oval == null) return;
        oval.enabled = cone.enabled = false;
    }

    public void HidePatch()
    {
        if (oval != null) oval.enabled = false;
    }

    /// <summary>The oval: centred at the aim point, as wide and tall as the biggest errors the throw could have.</summary>
    public void ShowPatch(Vector2 centre, float aimHalf, float weightHalf, float aimSkill, float weightSkill)
    {
        Build();
        ovalSize = OvalSizeFor(aimHalf, weightHalf);
        shimmerStrength = Shimmer(Mathf.Min(aimSkill, weightSkill));
        oval.enabled = true;
        oval.transform.position = new Vector3(centre.x, centre.y, 0f);
        anyShown = true;
        Apply(0f);
    }

    /// <summary>
    /// The cone along the line, widening from nothing at the hack to exactly the oval's width at the aim point, so its sides meet the oval's.
    /// It ends at the oval's centre and fades out over the last half of the oval's height so the two do not double up. Call ShowPatch first.
    /// Called every time the aim is redrawn, so it follows the aim.
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
        coneFade = length > 0.01f ? Mathf.Clamp(1f - (ovalSize.y * 0.5f) / length, 0f, 0.98f) : 0f;

        shimmerStrength = Mathf.Max(shimmerStrength, Shimmer(aimSkill));
        cone.enabled = true;
        cone.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) cone.SetPosition(i, points[i]);
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
        float wobbleX = 1f + 0.12f * shimmerStrength * (Mathf.PerlinNoise(t * 1.8f, 0.2f) * 2f - 1f);
        float wobbleY = 1f + 0.12f * shimmerStrength * (Mathf.PerlinNoise(t * 1.5f, 7.1f) * 2f - 1f);
        float pulse = 1f + 0.45f * shimmerStrength * (Mathf.PerlinNoise(t * 2.8f, 3.3f) * 2f - 1f);
        float conePulse = 1f + 0.45f * shimmerStrength * (Mathf.PerlinNoise(t * 2.5f, 9.7f) * 2f - 1f);

        float width = ovalSize.x * wobbleX, height = ovalSize.y * wobbleY;
        if (oval != null && oval.enabled)
        {
            float margin = OutlineHalfThickness * 3.5f;   // room around the ellipse for its glow
            oval.transform.localScale = new Vector3(width + 2f * margin, height + 2f * margin, 1f);
            ovalMaterial.SetColor("_Color", tint);
            ovalMaterial.SetVector("_Ellipse", new Vector4(width, height, OutlineHalfThickness, OvalStrength * Mathf.Clamp(pulse, 0.2f, 1.6f)));
            ovalMaterial.SetVector("_Quad", new Vector4(width + 2f * margin, height + 2f * margin, 0f, 0f));
            ovalMaterial.SetFloat("_Fill", OvalFill);
            ovalMaterial.SetFloat("_Outline", OvalOutline);
        }
        if (cone != null && cone.enabled)
        {
            cone.widthMultiplier = width;   // the curve runs 0 to 1, so the far end is exactly as wide as the oval
            float a = Mathf.Clamp01(ConeStrength * conePulse);
            coneGradient.SetKeys(
                new[] { new GradientColorKey(tint, 0f), new GradientColorKey(tint, 1f) },
                new[] { new GradientAlphaKey(a, 0f), new GradientAlphaKey(a, coneFade), new GradientAlphaKey(0f, 1f) });
            cone.colorGradient = coneGradient;
        }
    }
}
