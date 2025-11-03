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

    private void Start()
    {
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
        StartCoroutine(FadeOutAndLoadScene("SampleScene"));
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
