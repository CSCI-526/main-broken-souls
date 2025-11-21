using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup pauseMenu;
    [SerializeField] private Selectable defaultSelected;

    [Header("Game Over")]
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Audio")]
    [SerializeField] private bool pauseAudioListener = true;
    [SerializeField] private MusicManager musicManager;

#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference pauseAction;
#endif

    private bool isPaused;

    void Awake()
    {
        SetMenuVisible(false, false);
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;
        if (musicManager != null) musicManager.PlayMusic();
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
        if (gameOverUI != null && gameOverUI.gameOverPanel.activeSelf)
            return;

        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseAudioListener) AudioListener.pause = true;
        if (musicManager != null) musicManager.PauseMusic();

        SetMenuVisible(true, true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseAudioListener) AudioListener.pause = false;
        if (musicManager != null) musicManager.PlayMusic();

        SetMenuVisible(false, false);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;
        if (musicManager != null) musicManager.StopMusic();

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        if (pauseAudioListener) AudioListener.pause = false;
        if (musicManager != null) musicManager.StopMusic();

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
