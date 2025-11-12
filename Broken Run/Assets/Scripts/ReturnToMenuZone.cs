using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuZone : MonoBehaviour
{
    [Header("Audio Control")]
    public bool unpauseAudio = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 1f;
            if (unpauseAudio)
                AudioListener.pause = false;
            SceneManager.LoadScene("StartMenu");
        }
    }
}
