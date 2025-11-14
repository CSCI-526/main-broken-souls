using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject instructionsPanel;
    public GameObject tipsPanel;
    public GameObject mainMenuPanel;
    
    [Header("UI Elements")]
    public CanvasGroup mainMenuCanvasGroup;
    public CanvasGroup instructionsCanvasGroup;
    public CanvasGroup tipsCanvasGroup;
    public Transform titleTransform;
    public Button[] menuButtons;
    
    [Header("Animation Settings")]
    public float fadeSpeed = 2f;
    public float buttonHoverScale = 1.1f;
    public float buttonAnimSpeed = 0.2f;
    
    [Header("Tutorial Settings")]
    [Tooltip("Button for Play Game - will be disabled until tutorial completed")]
    public Button playGameButton;

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

        // Start with fade-in animation
        StartCoroutine(FadeInMainMenu());
        
        // Add button hover effects
        AddButtonHoverEffects();
        
        // Update Play Game button state (will be enabled/disabled based on tutorial completion)
        UpdatePlayGameButtonState(tutorialCompleted);
    }
    
    private void UpdatePlayGameButtonState(bool tutorialCompleted)
    {
        // Keep button always enabled so user can click it
        // The PlayGame() method will handle redirecting to tutorial if needed
        if (playGameButton != null)
        {
            playGameButton.interactable = true; // Always enabled
            
            // Optional: Visual feedback - slightly dimmed if tutorial not completed
            var colors = playGameButton.colors;
            if (!tutorialCompleted)
            {
                // Slightly dimmed but still clickable
                colors.normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            }
            else
            {
                colors.normalColor = Color.white;
            }
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
        // Check if tutorial has been completed
        int tutorialStatus = PlayerPrefs.GetInt("TutorialCompleted", 0);
        bool tutorialCompleted = tutorialStatus == 1;
        
        Debug.Log($"[MainMenu] PlayGame clicked - Tutorial status: {tutorialStatus} (0=not completed, 1=completed)");
        
        if (!tutorialCompleted)
        {
            // Tutorial not completed - force player to complete it first
            Debug.Log("[MainMenu] Cannot play game - tutorial must be completed first! Redirecting to tutorial...");
            PlayTutorial(); // Redirect to tutorial instead
            return;
        }
        
        // Tutorial completed - allow playing the game
        Debug.Log("[MainMenu] Tutorial completed - loading game scene");
        StartCoroutine(FadeOutAndLoadScene("SampleScene"));
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
