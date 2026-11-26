using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TrainingRowUI
{
    public Button button;
    public Text name;          // the track
    public Text cost;          // the next level's price, or MAXED
    public Text next;          // the next level's name
    public Text text;          // what it gives, or why it cannot be bought yet
    public Image image;        // the track's icon: hidden until it has a sprite
    public Image[] pips;       // one square per level, filled when owned
}

/// <summary>
/// The Training screen on the Arena_Selector menu: one row per track showing the level owned, what the next level gives and what it
/// costs; tapping a row asks for confirmation, then pays and saves. Built like EquipmentManager's screen (same rows, same confirm box);
/// the row layout comes from Editor/TrainingScreenLayout and can be adjusted by hand. What the bought levels do lives in TrainingCatalog.
/// </summary>
public class TrainingManager : MonoBehaviour
{
    public GameObject panel;
    public TrainingRowUI[] rows;
    public GameObject confirmMenu;
    public Text confirmText;
    public TeamMenu teamMenu;

    public Color buttonEnabledColor = Color.white;
    public Color buttonDisabledColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Row colours")]
    public Color priceColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    public Color priceTooHighColor = new Color(0.72f, 0.16f, 0.12f, 1f);
    public Color pipOwnedColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    public Color pipEmptyColor = new Color(0.196f, 0.196f, 0.196f, 0.18f);

    int pendingTrack = -1;
    CareerManager cm;

    CareerManager Career
    {
        get { if (cm == null) cm = FindFirstObjectByType<CareerManager>(); return cm; }
    }

    /// <summary>Opens the screen (TournySelector.TrainingWindow calls this).</summary>
    public void Show()
    {
        if (panel != null) panel.SetActive(true);
        pendingTrack = -1;
        if (confirmMenu != null) confirmMenu.SetActive(false);
        Refresh();
    }

    public void Hide()
    {
        pendingTrack = -1;
        if (confirmMenu != null) confirmMenu.SetActive(false);
        if (panel != null) panel.SetActive(false);
    }

    public void Refresh()
    {
        var c = Career;
        int[] owned = c != null ? c.trainingLevels : null;
        float cash = c != null ? c.cash : 0f;
        int playerLevel = c != null ? c.level : 1;

        for (int i = TrainingCatalog.Tracks; rows != null && i < rows.Length; i++)
            if (rows[i].button != null) rows[i].button.gameObject.SetActive(false);   // the screen was laid out with more rows once

        for (int i = 0; rows != null && i < rows.Length && i < TrainingCatalog.Tracks; i++)
        {
            var track = (TrainingTrack)i;
            int have = TrainingCatalog.Owned(owned, track);
            var next = TrainingCatalog.Next(owned, track);
            var row = rows[i];
            bool levelOk = TrainingCatalog.LevelAllows(owned, track, playerLevel);
            bool tooDear = next != null && levelOk && cash < next.cost;

            string levelName = next != null ? next.name : TrainingCatalog.Level(track, TrainingCatalog.MaxLevel).name;
            string detail = next == null ? "Fully trained" : !levelOk ? "Reach player level " + next.requiredLevel : next.effect;

            if (row.name != null) row.name.text = TrainingCatalog.TrackName(track);
            if (row.next != null) row.next.text = next != null ? "Next: " + levelName : levelName;
            if (row.text != null) row.text.text = row.next != null ? detail : levelName + ": " + detail;
            if (row.cost != null)
            {
                row.cost.text = next != null ? "$" + next.cost.ToString("n0") : "MAXED";
                row.cost.color = tooDear ? priceTooHighColor : priceColor;
            }
            if (row.pips != null)
                for (int p = 0; p < row.pips.Length; p++)
                    if (row.pips[p] != null) row.pips[p].color = p < have ? pipOwnedColor : pipEmptyColor;
            if (row.image != null)
            {
                bool hasArt = row.image.sprite != null;
                row.image.enabled = hasArt;
                if (row.image.transform.parent != null && row.image.transform.parent != row.button.transform)
                    row.image.transform.parent.gameObject.SetActive(hasArt);
            }

            bool canBuy = next != null && levelOk && cash >= next.cost;
            if (row.button != null)
            {
                row.button.interactable = canBuy;
                var img = row.button.GetComponent<Image>();
                if (img != null) img.color = canBuy ? buttonEnabledColor : buttonDisabledColor;
            }
        }
    }

    /// <summary>A track's row was tapped: ask before spending.</summary>
    public void OnRowTapped(int track)
    {
        var c = Career;
        var next = c != null ? TrainingCatalog.Next(c.trainingLevels, (TrainingTrack)track) : null;
        if (next == null || c.level < next.requiredLevel) return;
        pendingTrack = track;
        if (confirmText != null)
            confirmText.text = "Train " + next.name + " for $" + next.cost.ToString("n0") + "?\n" + next.effect;
        if (confirmMenu != null) confirmMenu.SetActive(true);
    }

    /// <summary>The tick (true) or cross (false) on the confirm box.</summary>
    public void Confirm(bool buy)
    {
        if (confirmMenu != null) confirmMenu.SetActive(false);
        int track = pendingTrack;
        pendingTrack = -1;
        if (!buy || track < 0) return;

        var c = Career;
        if (c == null) return;
        var next = TrainingCatalog.Next(c.trainingLevels, (TrainingTrack)track);
        if (next == null || c.level < next.requiredLevel || c.cash < next.cost) { Refresh(); return; }

        c.cash -= next.cost;
        c.trainingLevels = TrainingCatalog.WithLevelBought(c.trainingLevels, (TrainingTrack)track);
        if (teamMenu != null) teamMenu.CashDeltaText(-next.cost);
        Debug.Log("[Training] bought " + TrainingCatalog.TrackName((TrainingTrack)track) + " level " + TrainingCatalog.Owned(c.trainingLevels, (TrainingTrack)track) + " (" + next.name + ") for $" + next.cost);

        // Like an equipment purchase: cash and the new level are only in memory until now, so save before anything can lose them.
        c.SaveCareer();
        Refresh();
    }
}
