using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject instructionsPanel;
    public GameObject mainMenuPanel;
    
    [Header("UI Elements")]
    public CanvasGroup mainMenuCanvasGroup;
    public CanvasGroup instructionsCanvasGroup;
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
            
            // Add hover animation script to each button
            var hoverEffect = button.gameObject.AddComponent<ButtonHoverEffect>();
            hoverEffect.hoverScale = buttonHoverScale;
            hoverEffect.animSpeed = buttonAnimSpeed;
        }
    }

    public void PlayGame()
    {
        StartCoroutine(FadeOutAndLoadScene("SampleScene"));
    }

    public void ShowInstructions()
    {
        StartCoroutine(TransitionToInstructions());
    }

    public void HideInstructions()
    {
        StartCoroutine(TransitionToMainMenu());
    }

    public void QuitGame()
    {
        StartCoroutine(FadeOutAndQuit());
    }

    private IEnumerator TransitionToInstructions()
    {
        // Fade out main menu
        if (mainMenuCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                mainMenuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed);
                yield return null;
            }
            mainMenuPanel.SetActive(false);
        }
        else
        {
            mainMenuPanel.SetActive(false);
        }

        // Fade in instructions
        instructionsPanel.SetActive(true);
        if (instructionsCanvasGroup != null)
        {
            instructionsCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                instructionsCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed);
                yield return null;
            }
            instructionsCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator TransitionToMainMenu()
    {
        // Fade out instructions
        if (instructionsCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                instructionsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed);
                yield return null;
            }
            instructionsPanel.SetActive(false);
        }
        else
        {
            instructionsPanel.SetActive(false);
        }

        // Fade in main menu
        mainMenuPanel.SetActive(true);
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
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
