using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Re-lays the Training screen in Arena_Selector as three tall rows: icon on the left, track name and price on top, then the next
/// level, what it gives, and four progress squares. It keeps the existing frame, fonts and confirm box, and any icon sprites.
/// It runs by itself once (while the scene has no progress squares); after that adjust the rows by hand. To lay them out again
/// use Hammer Time > Lay Out Training Screen. Separately, Fit keeps the three rows inside the frame and the confirm box on top of
/// them without touching anything inside the rows.
/// </summary>
public static class TrainingScreenLayout
{
    const string ScenePath = "Assets/Scenes/Arena_Selector.unity";
    const float RowWidth = 1000f;   // inside the frame and its shadow
    const float RowHeight = 212f, RowSpacing = 8f;
    const int TopPadding = 16;   // clears the title banner
    const float IconSize = 180f, Left = 30f, TextLeft = 240f, Right = 34f;

    [InitializeOnLoadMethod]
    static void Watch()
    {
        if (Application.isBatchMode) return;
        EditorApplication.delayCall += RunPending;
        EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += RunPending; };
    }

    /// <summary>The one-time layout if it has never been done, otherwise the fit if the scene still needs it.</summary>
    static void RunPending()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(ScenePath)) return;
        if (!File.ReadAllText(ScenePath).Contains("m_Name: Pips")) Run();
        else WithManager(NeedsFit, Fit, "Fitted the rows and confirm box");
    }

    [MenuItem("Hammer Time/Lay Out Training Screen")]
    public static void Run()
    {
        WithManager(m => true, m => { Lay(m); Fit(m); }, "Laid out the rows");
    }

    static void WithManager(System.Func<TrainingManager, bool> needed, System.Action<TrainingManager> change, string done)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        bool hadEdits = !opened && scene.isDirty;
        try
        {
            var manager = scene.GetRootGameObjects().Select(g => g.GetComponentInChildren<TrainingManager>(true)).FirstOrDefault(m => m != null);
            if (manager == null || manager.rows == null || manager.rows.Length < TrainingCatalog.Tracks)
            {
                Debug.LogWarning("[Training layout] No TrainingManager with " + TrainingCatalog.Tracks + " rows in " + ScenePath);
                return;
            }
            if (!needed(manager)) return;
            change(manager);
            EditorUtility.SetDirty(manager);
            EditorSceneManager.MarkSceneDirty(scene);
            if (hadEdits) Debug.Log("[Training layout] " + done + ". Arena_Selector had unsaved edits, so save it yourself to keep this.");
            else
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Training layout] " + done + " and saved " + ScenePath);
            }
        }
        finally
        {
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }
    }

    /// <summary>True while the screen has not had this fit: the confirm box is stacked under the rows, or the rows are too wide.</summary>
    static bool NeedsFit(TrainingManager manager)
    {
        if (manager.confirmMenu == null) return false;
        var element = manager.confirmMenu.GetComponent<LayoutElement>();
        return element == null || !element.ignoreLayout || ((RectTransform)manager.rows[0].button.transform).sizeDelta.x > RowWidth + 1f;
    }

    static void Fit(TrainingManager manager)
    {
        // The confirm box's parent stacks its children, so the box has to opt out to sit over the middle of the rows.
        if (manager.confirmMenu != null)
        {
            var element = manager.confirmMenu.GetComponent<LayoutElement>();
            if (element == null) element = manager.confirmMenu.AddComponent<LayoutElement>();
            element.ignoreLayout = true;
            var box = (RectTransform)manager.confirmMenu.transform;
            Vector2 size = box.rect.size;
            box.anchorMin = box.anchorMax = box.pivot = new Vector2(0.5f, 0.5f);
            box.sizeDelta = size;
            box.anchoredPosition = Vector2.zero;
            box.SetAsLastSibling();
            EditorUtility.SetDirty(element);
            EditorUtility.SetDirty(box);
        }

        // the question in the top of the box, the cross and tick clear of it underneath
        if (manager.confirmText != null)
        {
            var question = manager.confirmText.rectTransform;
            question.anchorMin = question.anchorMax = question.pivot = new Vector2(0.5f, 0.5f);
            question.anchoredPosition = new Vector2(0f, 65f);
            question.sizeDelta = new Vector2(940f, 200f);
            Style(manager.confirmText, 58, TextAnchor.MiddleCenter);
            EditorUtility.SetDirty(question);
            EditorUtility.SetDirty(manager.confirmText);
        }
        if (manager.confirmMenu != null)
            foreach (var name in new[] { "YesButton", "NoButton" })
            {
                var button = manager.confirmMenu.transform.Find(name) as RectTransform;
                if (button == null) continue;
                button.anchoredPosition = new Vector2(button.anchoredPosition.x, 95f);
                EditorUtility.SetDirty(button);
            }

        var column = manager.rows[0].button.transform.parent.GetComponent<VerticalLayoutGroup>();
        if (column != null)
        {
            column.spacing = RowSpacing;
            column.padding = new RectOffset(column.padding.left, column.padding.right, TopPadding, 0);
            EditorUtility.SetDirty(column);
        }
        foreach (var row in manager.rows.Take(TrainingCatalog.Tracks))
        {
            var rect = (RectTransform)row.button.transform;
            rect.sizeDelta = new Vector2(RowWidth, RowHeight);
            var size = row.button.GetComponent<LayoutElement>();
            if (size != null && size.preferredWidth > 0f) size.preferredWidth = RowWidth;
            if (size != null) size.minHeight = size.preferredHeight = RowHeight;
            EditorUtility.SetDirty(rect);
        }
    }

    static void Lay(TrainingManager manager)
    {
        // the rows the screen was first built with, beyond the three tracks
        foreach (var extra in manager.rows.Skip(TrainingCatalog.Tracks))
            if (extra.button != null) Object.DestroyImmediate(extra.button.gameObject);
        manager.rows = manager.rows.Take(TrainingCatalog.Tracks).ToArray();

        var column = manager.rows[0].button.transform.parent.GetComponent<VerticalLayoutGroup>();
        if (column != null)
        {
            column.spacing = RowSpacing;
            column.childForceExpandHeight = false;
            column.childControlHeight = false;
            column.childAlignment = TextAnchor.MiddleCenter;
        }

        for (int i = 0; i < manager.rows.Length; i++)
        {
            TrainingRowUI row = manager.rows[i];
            var rect = (RectTransform)row.button.transform;
            rect.name = "Track_" + i + "_" + (TrainingTrack)i;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, RowHeight);
            var size = row.button.GetComponent<LayoutElement>();
            if (size == null) size = row.button.gameObject.AddComponent<LayoutElement>();
            size.minHeight = size.preferredHeight = RowHeight;

            // icon square on the left (the sprite goes on Track_n/Panel/Image)
            if (row.image != null && row.image.transform.parent != rect)
            {
                var frame = (RectTransform)row.image.transform.parent;
                frame.anchorMin = frame.anchorMax = frame.pivot = new Vector2(0f, 0.5f);
                frame.anchoredPosition = new Vector2(Left, 0f);
                frame.sizeDelta = new Vector2(IconSize, IconSize);
                var art = (RectTransform)row.image.transform;
                art.anchorMin = Vector2.zero;
                art.anchorMax = Vector2.one;
                art.offsetMin = new Vector2(8f, 8f);
                art.offsetMax = new Vector2(-8f, -8f);
                row.image.preserveAspect = true;
            }

            Band(row.name, 10f, 82f, TextLeft, 260f);
            Style(row.name, 64, TextAnchor.MiddleLeft);

            Band(row.cost, 10f, 82f, 0f, Right);
            var cost = (RectTransform)row.cost.transform;
            cost.anchorMin = new Vector2(1f, 1f);
            cost.offsetMin = new Vector2(-250f, cost.offsetMin.y);
            Style(row.cost, 60, TextAnchor.MiddleRight);

            if (row.next == null) row.next = Copy(row.name, rect, "Next");
            Band(row.next, 86f, 134f, TextLeft, Right);
            Style(row.next, 44, TextAnchor.MiddleLeft);

            Band(row.text, 134f, 178f, TextLeft, Right);
            Style(row.text, 36, TextAnchor.MiddleLeft);

            row.pips = Pips(rect);

            row.name.text = TrainingCatalog.TrackName((TrainingTrack)i);
            row.next.text = "Next: " + TrainingCatalog.Level((TrainingTrack)i, 1).name;
            row.text.text = TrainingCatalog.Level((TrainingTrack)i, 1).effect;
            row.cost.text = "$" + TrainingCatalog.LevelCost[0].ToString("n0");
            foreach (var t in new[] { row.name, row.cost, row.next, row.text }) EditorUtility.SetDirty(t);
        }
    }

    /// <summary>A strip of the row from `top` to `bottom` pixels down, inset from the left and right edges.</summary>
    static void Band(Text text, float top, float bottom, float left, float right)
    {
        var r = (RectTransform)text.transform;
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = new Vector2(left, -bottom);
        r.offsetMax = new Vector2(-right, -top);
    }

    static void Style(Text text, int size, TextAnchor align)
    {
        text.fontSize = size;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = size;
        text.alignment = align;
        text.raycastTarget = false;
    }

    static Text Copy(Text source, Transform parent, string name)
    {
        var copy = Object.Instantiate(source.gameObject, parent, false);
        copy.name = name;
        return copy.GetComponent<Text>();
    }

    static Image[] Pips(RectTransform row)
    {
        const float Side = 30f, Gap = 14f, Top = 186f;
        var holder = row.Find("Pips") as RectTransform;
        if (holder != null) Object.DestroyImmediate(holder.gameObject);
        holder = Child("Pips", row);
        holder.anchorMin = holder.anchorMax = holder.pivot = new Vector2(0f, 1f);
        holder.anchoredPosition = new Vector2(TextLeft, -Top);
        holder.sizeDelta = new Vector2(TrainingCatalog.MaxLevel * (Side + Gap) - Gap, Side);

        var pips = new Image[TrainingCatalog.MaxLevel];
        for (int p = 0; p < pips.Length; p++)
        {
            var square = Child("Pip_" + (p + 1), holder);
            square.anchorMin = square.anchorMax = square.pivot = new Vector2(0f, 1f);
            square.anchoredPosition = new Vector2(p * (Side + Gap), 0f);
            square.sizeDelta = new Vector2(Side, Side);
            pips[p] = square.gameObject.AddComponent<Image>();
            pips[p].color = new Color(0.196f, 0.196f, 0.196f, 0.18f);
            pips[p].raycastTarget = false;
        }
        return pips;
    }

    static RectTransform Child(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }
}
