using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton controller for displaying dialogue.
/// Simple API: DialogueController.Instance.Show(dialogueData)
/// </summary>
public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private Image dialogueBackground;
    [SerializeField] private Text nameText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Text continueButtonText;
    
    [Header("Character Sprites")]
    [SerializeField] private GameObject coachHead;
    [SerializeField] private GameObject announcerHead;
    
    [Header("Auto-Positioning (Spotlight Avoidance)")]
    [Tooltip("The RectTransform to reposition when a spotlight is active. Leave empty to use dialogueCanvas itself.")]
    [SerializeField] private RectTransform dialoguePanel;
    [Tooltip("anchoredPosition.y when dialogue moves to the top of screen")]
    [SerializeField] private float dialogueTopY = 250f;
    [Tooltip("anchoredPosition.y when dialogue moves to the bottom of screen")]
    [SerializeField] private float dialogueBottomY = -250f;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool skipTypingOnClick = true;
    [SerializeField] private int forceSortingOrder = 500;
    [SerializeField] private float inputDebounceSeconds = 0.2f;
    
    // Current dialogue state
    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private Action onCompleteCallback;
    private CareerManager careerManager;
    private GameManager gameManager;
    private Canvas dialogueRootCanvas;
    private float allowInputAtTime;
    private bool waitForMouseRelease;
    private Vector2 dialoguePanelDefaultPos;
    private bool dialoguePanelDefaultCached;
    
    // Events
    public event Action<DialogueData> OnDialogueStart;
    public event Action<DialogueData> OnDialogueEnd;
    public event Action<int> OnLineAdvanced;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResolveMissingReferences();
        
        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonPressed);
            continueButton.onClick.AddListener(OnContinueButtonPressed);
        }
    }

    private void Start()
    {
        ResolveMissingReferences();
        careerManager = FindFirstObjectByType<CareerManager>();
        gameManager = FindFirstObjectByType<GameManager>();
        HideDialogue();
    }
    
    private void Update()
    {
        // Allow clicking anywhere to advance if typing is done
        if (Input.GetMouseButtonDown(0) && dialogueCanvas != null && dialogueCanvas.activeSelf)
        {
            if (Time.unscaledTime < allowInputAtTime)
                return;

            if (waitForMouseRelease)
                return;

            if (isTyping && skipTypingOnClick)
            {
                SkipTyping();
            }
            else if (!isTyping)
            {
                OnContinueButtonPressed();
            }
        }

        if (waitForMouseRelease && !Input.GetMouseButton(0))
        {
            waitForMouseRelease = false;
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Show dialogue from DialogueData asset
    /// </summary>
    public void Show(DialogueData dialogue, Action onComplete = null)
    {
        ResolveMissingReferences();

        if (dialogue == null || dialogue.LineCount == 0)
        {
            Debug.LogWarning("DialogueController: Attempted to show null or empty dialogue");
            return;
        }

        if (dialogueCanvas == null)
        {
            Debug.LogError("DialogueController: dialogueCanvas is missing. Cannot show tutorial dialogue.");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        onCompleteCallback = onComplete;
        allowInputAtTime = Time.unscaledTime + inputDebounceSeconds;
        waitForMouseRelease = Input.GetMouseButton(0);

        ShowDialogue();
        SetupDialogueUI();

        if (dialogueText == null)
        {
            Debug.LogError("DialogueController: dialogueText is null — canvas is visible but text won't display. Check Inspector wiring.");
            return;
        }

        DisplayCurrentLine();
        
        OnDialogueStart?.Invoke(dialogue);
    }
    
    /// <summary>
    /// Show a single line of dialogue (quick message)
    /// </summary>
    public void ShowQuickMessage(string message, string characterName = "Coach", Action onComplete = null)
    {
        // Create temporary dialogue data
        DialogueData tempDialogue = ScriptableObject.CreateInstance<DialogueData>();
        tempDialogue.characterName = characterName;
        tempDialogue.dialogueLines = new string[] { message };
        tempDialogue.pauseGame = false;
        tempDialogue.useTextReplacement = false;
        
        Show(tempDialogue, () =>
        {
            onComplete?.Invoke();
            Destroy(tempDialogue);
        });
    }
    
    /// <summary>
    /// Hide dialogue immediately
    /// </summary>
    public void Hide()
    {
        EndDialogue();
    }
    
    /// <summary>
    /// Check if dialogue is currently showing
    /// </summary>
    public bool IsShowing => dialogueCanvas != null && dialogueCanvas.activeSelf;
    
    /// <summary>
    /// Control dialogue background visibility (useful when using spotlight overlay)
    /// </summary>
    public void SetBackgroundVisible(bool visible)
    {
        if (dialogueBackground != null)
        {
            dialogueBackground.enabled = visible;
        }
    }

    /// <summary>
    /// Reposition the dialogue panel to avoid overlapping the spotlight.
    /// normalizedSpotlightPos is the spotlight center in 0-1 screen space (0=bottom, 1=top).
    /// Dialogue moves to the opposite vertical half.
    /// </summary>
    public void PositionAroundSpotlight(Vector2 normalizedSpotlightPos)
    {
        RectTransform panel = GetDialoguePanel();
        if (panel == null) return;

        CacheDefaultPanelPos(panel);

        float targetY = normalizedSpotlightPos.y > 0.5f ? dialogueBottomY : dialogueTopY;
        panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, targetY);
        Debug.Log($"[DialogueController] Panel repositioned to Y={targetY} (spotlight at normalized {normalizedSpotlightPos.y:F2})");
    }

    /// <summary>
    /// Return the dialogue panel to its original position.
    /// </summary>
    public void ResetDialoguePosition()
    {
        if (!dialoguePanelDefaultCached) return;
        RectTransform panel = GetDialoguePanel();
        if (panel != null)
            panel.anchoredPosition = dialoguePanelDefaultPos;
    }
    
    #endregion
    
    #region Private Methods
    
    private void SetupDialogueUI()
    {
        if (nameText != null)
            nameText.text = currentDialogue.characterName;
        
        // Show appropriate character sprite
        ShowCharacterSprite(currentDialogue.characterName);
        
        // Handle game pause
        if (currentDialogue.pauseGame)
            Time.timeScale = 0f;
    }
    
    private void ShowCharacterSprite(string characterName)
    {
        // Hide all heads first
        if (coachHead != null) coachHead.SetActive(false);
        if (announcerHead != null) announcerHead.SetActive(false);
        
        // Show appropriate character
        string lowerName = characterName.ToLower();
        if (lowerName.Contains("coach") || lowerName.Contains("skip"))
        {
            if (coachHead != null) coachHead.SetActive(true);
        }
        else if (lowerName.Contains("announcer"))
        {
            if (announcerHead != null) announcerHead.SetActive(true);
        }
    }
    
    private void ShowDialogue()
    {
        if (dialogueCanvas != null)
        {
            // Enable any inactive parents up the hierarchy so the canvas actually renders
            Transform t = dialogueCanvas.transform.parent;
            while (t != null)
            {
                if (!t.gameObject.activeSelf)
                    t.gameObject.SetActive(true);
                t = t.parent;
            }
            dialogueCanvas.SetActive(true);
            EnsureDialogueOnTop();
        }
    }
    
    private void HideDialogue()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
        
        if (coachHead != null) coachHead.SetActive(false);
        if (announcerHead != null) announcerHead.SetActive(false);
    }
    
    private void DisplayCurrentLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.LineCount)
        {
            EndDialogue();
            return;
        }
        
        string line = currentDialogue.GetLine(currentLineIndex, careerManager, gameManager);
        
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        // Show typing effect or instant
        if (typingSpeed > 0)
        {
            typingCoroutine = StartCoroutine(TypeText(line));
        }
        else
        {
            if (dialogueText != null)
                dialogueText.text = line;
            isTyping = false;
        }
        
        OnLineAdvanced?.Invoke(currentLineIndex);
        
        // Auto-advance if configured
        if (currentDialogue.autoAdvanceDelay > 0)
        {
            StartCoroutine(AutoAdvance(currentDialogue.autoAdvanceDelay));
        }
    }
    
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        if (dialogueText == null)
        {
            isTyping = false;
            yield break;
        }

        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            
            // Handle time scale for paused games
            if (currentDialogue.pauseGame)
                yield return new WaitForSecondsRealtime(typingSpeed);
            else
                yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    private void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        string line = currentDialogue.GetLine(currentLineIndex, careerManager, gameManager);
        if (dialogueText != null)
            dialogueText.text = line;
        isTyping = false;
    }
    
    private IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!isTyping)
            AdvanceDialogue();
    }
    
    private void OnContinueButtonPressed()
    {
        if (Time.unscaledTime < allowInputAtTime)
            return;

        if (waitForMouseRelease)
            return;

        if (isTyping)
        {
            SkipTyping();
        }
        else
        {
            AdvanceDialogue();
        }
    }
    
    private void AdvanceDialogue()
    {
        if (currentDialogue == null) return;

        currentLineIndex++;

        if (currentLineIndex < currentDialogue.LineCount)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }
    
    private void EndDialogue()
    {
        OnDialogueEnd?.Invoke(currentDialogue);
        
        HideDialogue();
        
        // Resume game if it was paused
        if (currentDialogue != null && currentDialogue.pauseGame)
            Time.timeScale = 1f;
        
        // Trigger callback
        onCompleteCallback?.Invoke();
        onCompleteCallback = null;
        
        currentDialogue = null;
        currentLineIndex = 0;
    }

    private void ResolveMissingReferences()
    {
        if (dialogueCanvas == null)
        {
            Canvas[] canvases = GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                string lower = canvas.gameObject.name.ToLower();
                if (lower.Contains("dialogue"))
                {
                    dialogueCanvas = canvas.gameObject;
                    break;
                }
            }

            if (dialogueCanvas == null && canvases.Length > 0)
                dialogueCanvas = canvases[0].gameObject;
        }

        if (dialogueCanvas != null)
        {
            dialogueRootCanvas = dialogueCanvas.GetComponent<Canvas>();
            if (dialogueRootCanvas == null)
                dialogueRootCanvas = dialogueCanvas.GetComponentInParent<Canvas>();
        }

        if (dialogueText == null || nameText == null || continueButtonText == null)
        {
            Text[] texts = GetComponentsInChildren<Text>(true);
            foreach (Text t in texts)
            {
                string lower = t.gameObject.name.ToLower();
                if (dialogueText == null && (lower.Contains("dialogue") || lower.Contains("body")))
                    dialogueText = t;
                else if (nameText == null && lower.Contains("name"))
                    nameText = t;
                else if (continueButtonText == null && (lower.Contains("continue") || lower.Contains("next")))
                    continueButtonText = t;
            }
        }

        if (continueButton == null)
            continueButton = GetComponentInChildren<Button>(true);

        if (dialogueBackground == null)
            dialogueBackground = GetComponentInChildren<Image>(true);
    }

    private RectTransform GetDialoguePanel()
    {
        if (dialoguePanel != null) return dialoguePanel;
        if (dialogueCanvas != null) return dialogueCanvas.GetComponent<RectTransform>();
        return null;
    }

    private void CacheDefaultPanelPos(RectTransform panel)
    {
        if (dialoguePanelDefaultCached) return;
        dialoguePanelDefaultPos = panel.anchoredPosition;
        dialoguePanelDefaultCached = true;
    }

    private void EnsureDialogueOnTop()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.transform.SetAsLastSibling();

        if (dialogueRootCanvas != null)
        {
            dialogueRootCanvas.overrideSorting = true;
            if (dialogueRootCanvas.sortingOrder < forceSortingOrder)
                dialogueRootCanvas.sortingOrder = forceSortingOrder;
        }

        // Any nested Canvas with overrideSorting (e.g. character head images) must sort above
        // the root canvas, otherwise they render behind Panel (1) regardless of sibling order.
        if (dialogueCanvas != null)
        {
            foreach (Canvas nested in dialogueCanvas.GetComponentsInChildren<Canvas>(true))
            {
                if (nested == dialogueRootCanvas) continue;
                if (nested.overrideSorting)
                    nested.sortingOrder = forceSortingOrder + 1;
            }
        }

        // SpriteRenderer-based character heads live in world space and need their sorting
        // layer/order bumped to render in front of the dialogue canvas.
        BumpSpriteRenderers(coachHead);
        BumpSpriteRenderers(announcerHead);
    }

    private void BumpSpriteRenderers(GameObject go)
    {
        if (go == null) return;
        string layerName = dialogueRootCanvas != null ? dialogueRootCanvas.sortingLayerName : "Default";
        foreach (SpriteRenderer sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = layerName;
            sr.sortingOrder = forceSortingOrder + 1;
        }
    }
    
    #endregion
}
