using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject instructionsPanel;
    public GameObject tipsPanel;
    public GameObject mainMenuPanel;
    public GameObject tutorialPromptPanel; // Simple popup that shows message
    
    [Header("UI Elements")]
    public CanvasGroup mainMenuCanvasGroup;
    public CanvasGroup instructionsCanvasGroup;
    public CanvasGroup tipsCanvasGroup;
    public CanvasGroup tutorialPromptCanvasGroup;
    public Transform titleTransform;
    public Button[] menuButtons;
    
    [Header("Tutorial Prompt")]
    public TextMeshProUGUI tutorialPromptText; // Message text in popup
    [Tooltip("How long the popup stays visible before auto-closing (seconds)")]
    public float promptDisplayDuration = 3f;
    
    [Header("Animation Settings")]
    public float fadeSpeed = 2f;
    public float buttonHoverScale = 1.1f;
    public float buttonAnimSpeed = 0.2f;
    
    [Header("Tutorial Settings")]
    [Tooltip("Button for Play Game - will be disabled until tutorial completed")]
    public Button playGameButton;
    
    private Coroutine promptCoroutine;

    private void Start()
    {
        // Check if tutorial has been completed
        int tutorialStatus = PlayerPrefs.GetInt("TutorialCompleted", 0);
        bool tutorialCompleted = tutorialStatus == 1;
        
        // Debug log to see current status
        Debug.Log($"[MainMenu] Tutorial completion status: {tutorialStatus} (0=not completed, 1=completed)");
        
        // Setup canvas groups if not assigned
        if (mainMenuCanvasGroup == null && mainMenuPanel != null)
            mainMenuCanvasGroup = mainMenuPanel.GetComponent<CanvasGroup>() ?? mainMenuPanel.AddComponent<CanvasGroup>();
        
        if (instructionsCanvasGroup == null && instructionsPanel != null)
            instructionsCanvasGroup = instructionsPanel.GetComponent<CanvasGroup>() ?? instructionsPanel.AddComponent<CanvasGroup>();
        
        if (tipsCanvasGroup == null && tipsPanel != null)
            tipsCanvasGroup = tipsPanel.GetComponent<CanvasGroup>() ?? tipsPanel.AddComponent<CanvasGroup>();
        
        // Setup tutorial prompt panel
        if (tutorialPromptCanvasGroup == null && tutorialPromptPanel != null)
            tutorialPromptCanvasGroup = tutorialPromptPanel.GetComponent<CanvasGroup>() ?? tutorialPromptPanel.AddComponent<CanvasGroup>();
        
        // Hide tutorial prompt panel initially
        if (tutorialPromptPanel != null)
        {
            tutorialPromptPanel.SetActive(false);
        }
        
        // Set tutorial prompt message
        if (tutorialPromptText != null)
        {
            tutorialPromptText.text = "Please complete the tutorial first!";
        }

        // Start with fade-in animation
        StartCoroutine(FadeInMainMenu());
        
        // Add button hover effects
        AddButtonHoverEffects();
        
        // Update Play Game button state (will be enabled/disabled based on tutorial completion)
        UpdatePlayGameButtonState(tutorialCompleted);
    }
    
    private void UpdatePlayGameButtonState(bool tutorialCompleted)
{
    if (playGameButton != null)
    {
        playGameButton.interactable = true;

        // Always normal color
        var colors = playGameButton.colors;
        colors.normalColor = Color.white;
        playGameButton.colors = colors;
    }
}

    private IEnumerator FadeInMainMenu()
    {
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuPanel.SetActive(true);
            
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                mainMenuCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed);
                yield return null;
            }
            mainMenuCanvasGroup.alpha = 1f;
        }
    }

    private void AddButtonHoverEffects()
    {
        if (menuButtons == null || menuButtons.Length == 0)
            menuButtons = mainMenuPanel.GetComponentsInChildren<Button>();

        foreach (var button in menuButtons)
        {
            if (button == null) continue;
            
            var hoverEffect = button.gameObject.AddComponent<ButtonHoverEffect>();
            hoverEffect.hoverScale = buttonHoverScale;
            hoverEffect.animSpeed = buttonAnimSpeed;
        }
    }

    // -------------------- MAIN MENU ACTIONS --------------------

public void PlayGame()
{
    Debug.Log("[MainMenu] PlayGame clicked - loading game scene immediately");
    StartCoroutine(FadeOutAndLoadScene("SampleScene"));
}
    // -------------------- TUTORIAL PROMPT METHODS --------------------
    
    public void ShowTutorialPrompt()
    {
        if (tutorialPromptPanel == null)
        {
            Debug.LogWarning("[MainMenu] Tutorial prompt panel not assigned!");
            return;
        }
        
        // Stop any existing prompt coroutine
        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
        }
        
        // Show the popup
        tutorialPromptPanel.SetActive(true);
        promptCoroutine = StartCoroutine(ShowPromptAndAutoClose());
    }
    
    private IEnumerator ShowPromptAndAutoClose()
    {
        // Fade in
        if (tutorialPromptCanvasGroup != null)
        {
            tutorialPromptCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 0.3f) // Quick fade in
            {
                elapsed += Time.deltaTime * fadeSpeed;
                tutorialPromptCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
                yield return null;
            }
            tutorialPromptCanvasGroup.alpha = 1f;
        }
        
        // Wait for display duration
        yield return new WaitForSeconds(promptDisplayDuration);
        
        // Fade out
        if (tutorialPromptCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 0.3f) // Quick fade out
            {
                elapsed += Time.deltaTime * fadeSpeed;
                tutorialPromptCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                yield return null;
            }
            tutorialPromptCanvasGroup.alpha = 0f;
        }
        
        // Hide panel
        tutorialPromptPanel.SetActive(false);
        promptCoroutine = null;
    }
    
    // Helper method to reset tutorial (for testing purposes)
    // You can call this from Unity Inspector or add a button to test
    public void ResetTutorialCompletion()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        Debug.Log("[MainMenu] Tutorial completion status RESET - next play will require tutorial");
    }
    
    public void PlayTutorial()
    {
        StartCoroutine(FadeOutAndLoadScene("tutorial"));
    }

    public void ShowInstructions()
    {
        StartCoroutine(TransitionToInstructions());
    }

    public void ShowTips()
    {
        StartCoroutine(TransitionToTips());
    }

    public void HideInstructions()
    {
        StartCoroutine(TransitionToMainMenu());
    }

    public void HideTips()
    {
        StartCoroutine(TransitionToMainMenu());
    }

    public void QuitGame()
    {
        StartCoroutine(FadeOutAndQuit());
    }

    // -------------------- TRANSITIONS --------------------

    private IEnumerator TransitionToInstructions()
    {
        // Fade out main menu
        yield return StartCoroutine(FadeOutPanel(mainMenuCanvasGroup, mainMenuPanel));
        
        // Fade in instructions
        instructionsPanel.SetActive(true);
        yield return StartCoroutine(FadeInPanel(instructionsCanvasGroup));
    }

    private IEnumerator TransitionToTips()
    {
        // Fade out main menu
        yield return StartCoroutine(FadeOutPanel(mainMenuCanvasGroup, mainMenuPanel));

        // Fade in tips
        tipsPanel.SetActive(true);
        yield return StartCoroutine(FadeInPanel(tipsCanvasGroup));
    }

    private IEnumerator TransitionToMainMenu()
    {
        // Fade out instructions or tips
        if (instructionsPanel.activeSelf)
            yield return StartCoroutine(FadeOutPanel(instructionsCanvasGroup, instructionsPanel));
        if (tipsPanel.activeSelf)
            yield return StartCoroutine(FadeOutPanel(tipsCanvasGroup, tipsPanel));

        // Fade in main menu
        mainMenuPanel.SetActive(true);
        yield return StartCoroutine(FadeInPanel(mainMenuCanvasGroup));
    }

    // -------------------- GENERIC FADE HELPERS --------------------

    private IEnumerator FadeOutPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        panel.SetActive(false);
    }

    private IEnumerator FadeInPanel(CanvasGroup canvasGroup)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }

    // -------------------- SCENE / EXIT --------------------

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (mainMenuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                mainMenuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed);
                yield return null;
            }
        }
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOutAndQuit()
    {
        if (mainMenuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                mainMenuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed);
                yield return null;
            }
        }

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
