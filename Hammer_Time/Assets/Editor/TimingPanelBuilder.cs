using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Makes the starting version of Assets/Resources/TimingPanel.prefab (and a TextMesh Pro font from Staatliches). It runs by itself
/// when the prefab is missing and never overwrites one that exists, so edits to the prefab are safe; a prefab from before the weight
/// row gets that row added (a copy of its Split row, so it keeps your look). To start over, delete the prefab and use
/// Hammer Time > Create Timing Panel Prefab.
/// </summary>
public static class TimingPanelBuilder
{
    const string PrefabPath = "Assets/Resources/TimingPanel.prefab";
    const string SourceFontPath = "Assets/Font/Staatliches-Regular.ttf";
    const string FontAssetPath = "Assets/Font/Staatliches SDF.asset";

    static readonly Color Background = new Color(0.07f, 0.12f, 0.2f, 0.82f);
    static readonly Color LabelColour = new Color(0.65f, 0.75f, 0.85f, 1f);
    static readonly Color RangeColour = new Color(0.35f, 0.72f, 1f, 1f);   // the aim aids' cold blue
    static readonly Color LiveColour = Color.white;

    [InitializeOnLoadMethod]
    static void CreateIfMissing()
    {
        if (Application.isBatchMode) return;
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(PrefabPath)) Create();
            else AddMissingWeightRow();
        };
    }

    [MenuItem("Hammer Time/Create Timing Panel Prefab")]
    public static void Create()
    {
        if (File.Exists(PrefabPath))
        {
            Debug.Log("[TimingPanel] " + PrefabPath + " already exists; delete it first to rebuild it.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        TMP_FontAsset font = FontAsset();

        var root = new GameObject("TimingPanel", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup), typeof(TimingPanel));
        root.layer = LayerMask.NameToLayer("UI");
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1125f, 2436f);   // same as the game's own UI
        scaler.matchWidthOrHeight = 1f;
        var group = root.GetComponent<CanvasGroup>();
        group.interactable = false;
        group.blocksRaycasts = false;

        var safe = Child("SafeArea", root.transform);
        safe.anchorMin = Vector2.zero;
        safe.anchorMax = Vector2.one;
        safe.offsetMin = safe.offsetMax = Vector2.zero;

        var box = Child("Box", safe);
        box.anchorMin = box.anchorMax = box.pivot = new Vector2(0f, 1f);
        box.anchoredPosition = new Vector2(24f, -24f);
        var image = box.gameObject.AddComponent<Image>();
        image.color = Background;
        image.raycastTarget = false;
        var column = box.gameObject.AddComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 12, 12);
        column.spacing = 4f;
        Control(column);
        var fit = box.gameObject.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var panel = root.GetComponent<TimingPanel>();
        panel.group = group;
        panel.safeArea = safe;
        Row("Split Row", "Split", box, font, out panel.splitRange, out panel.splitLive);
        Row("Hog Row", "Hog-Hog", box, font, out panel.hogRange, out panel.hogLive);
        AddWeightRow(panel);

        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
        var saved = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Selection.activeObject = saved;
        Debug.Log("[TimingPanel] Created " + PrefabPath);
    }

    static void AddMissingWeightRow()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        var asset = AssetDatabase.LoadAssetAtPath<TimingPanel>(PrefabPath);
        if (asset == null || asset.weightName != null) return;
        var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (AddWeightRow(contents.GetComponent<TimingPanel>()))
        {
            PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
            Debug.Log("[TimingPanel] Added the weight row to " + PrefabPath);
        }
        PrefabUtility.UnloadPrefabContents(contents);
    }

    /// <summary>A copy of the Split row, placed above it: its label, and one text as wide as the range and clock together.</summary>
    static bool AddWeightRow(TimingPanel panel)
    {
        if (panel == null || panel.splitRange == null || panel.splitLive == null) return false;
        Transform source = panel.splitRange.transform.parent;
        if (panel.splitLive.transform.parent != source) return false;

        var row = Object.Instantiate(source.gameObject, source.parent, false).transform;
        row.name = "Weight Row";
        row.SetSiblingIndex(source.GetSiblingIndex());
        var value = row.GetChild(panel.splitRange.transform.GetSiblingIndex()).GetComponent<TMP_Text>();
        var live = row.GetChild(panel.splitLive.transform.GetSiblingIndex()).gameObject;

        var width = value.GetComponent<LayoutElement>();
        var liveWidth = live.GetComponent<LayoutElement>();
        var layout = row.GetComponent<HorizontalLayoutGroup>();
        if (width != null && liveWidth != null)
            width.preferredWidth += liveWidth.preferredWidth + (layout != null ? layout.spacing : 0f);
        Object.DestroyImmediate(live);

        foreach (var text in row.GetComponentsInChildren<TMP_Text>(true))
            if (text != value) text.text = "Weight";
        value.name = "Weight Name";
        value.gameObject.SetActive(true);
        value.alignment = TextAlignmentOptions.Left;
        value.text = "7 - Button";
        panel.weightName = value;
        return true;
    }

    static void Row(string name, string label, RectTransform parent, TMP_FontAsset font, out TMP_Text range, out TMP_Text live)
    {
        var row = Child(name, parent);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        Control(layout);
        Text("Label", label, row, font, 170f, LabelColour, TextAlignmentOptions.Left);
        range = Text("Range", "03:40-03:90", row, font, 270f, RangeColour, TextAlignmentOptions.Center);
        live = Text("Live", "--:--", row, font, 130f, LiveColour, TextAlignmentOptions.Right);
    }

    static void Control(HorizontalOrVerticalLayoutGroup g)
    {
        g.childControlWidth = g.childControlHeight = true;
        g.childForceExpandWidth = g.childForceExpandHeight = false;
    }

    static TMP_Text Text(string name, string value, RectTransform parent, TMP_FontAsset font, float width, Color colour, TextAlignmentOptions align)
    {
        var rect = Child(name, parent);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = value;
        text.fontSize = 46f;
        text.color = colour;
        text.alignment = align;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        var size = rect.gameObject.AddComponent<LayoutElement>();
        size.preferredWidth = width;
        size.preferredHeight = 56f;
        return text;
    }

    static RectTransform Child(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    /// <summary>The Staatliches TextMesh Pro font, made on first use. Null (TMP's default font is used) if the TTF has moved.</summary>
    static TMP_FontAsset FontAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null) return existing;
        var source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (source == null) return null;
        var asset = TMP_FontAsset.CreateFontAsset(source);   // dynamic: glyphs are added as they are used
        if (asset == null) return null;
        asset.name = "Staatliches SDF";
        AssetDatabase.CreateAsset(asset, FontAssetPath);
        asset.material.name = asset.name + " Material";
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        foreach (var atlas in asset.atlasTextures)
        {
            atlas.name = asset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, asset);
        }
        asset.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:-+ ");
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return asset;
    }
}
