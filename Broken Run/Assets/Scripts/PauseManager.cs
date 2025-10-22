using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Optional: works if you're using the new Input System
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup pauseMenu;          // Drag your PauseMenu panel's CanvasGroup
    [SerializeField] private Selectable defaultSelected;     // Drag ResumeButton (Button/Selectable)

    [Header("Game Over (optional)")]
    [SerializeField] private GameOverUI gameOverUI;          // Drag your GameOverUI (to block pausing on death)

    [Header("Audio (optional)")]
    [SerializeField] private bool pauseAudioListener = true; // Toggle audio pause with menu

    [Header("Input (optional - New Input System)")]
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference pauseAction; // Bind to <Keyboard>/escape (and/or gamepad Start)
#endif

    private bool isPaused;

    void Awake()
    {
        // Ensure menu starts hidden
        SetMenuVisible(false, setSelection: false);
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;
    }

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
#endif
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (pauseAction != null && pauseAction.action != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
#endif
    }

    void Update()
    {
        // Fallback: if no InputAction assigned, listen for Escape
#if ENABLE_INPUT_SYSTEM
        bool hasBoundAction = (pauseAction != null && pauseAction.action != null);
        if (!hasBoundAction)
#endif
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();
        }
    }

#if ENABLE_INPUT_SYSTEM
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }
#endif

    public void TogglePause()
    {
        // Block pause if GameOver is visible
        if (gameOverUI != null && gameOverUI.gameOverPanel != null && gameOverUI.gameOverPanel.activeSelf)
            return;

        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseAudioListener) AudioListener.pause = true;

        SetMenuVisible(true, setSelection: true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;

        SetMenuVisible(false, setSelection: false);
        // Clear selection so gameplay UI regains focus naturally
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestartScene()
    {
        // Always unpause before reloading
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;

        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void QuitGame()
{
    // Always unpause before switching scenes
    Time.timeScale = 1f;
    if (pauseAudioListener) AudioListener.pause = false;

    // Load Start Menu scene
    SceneManager.LoadScene("StartMenu");
}

    private void SetMenuVisible(bool visible, bool setSelection)
    {
        if (pauseMenu == null) return;

        pauseMenu.alpha = visible ? 1f : 0f;
        pauseMenu.interactable = visible;
        pauseMenu.blocksRaycasts = visible;

        if (visible && setSelection && defaultSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultSelected.gameObject);
        }
    }
}
