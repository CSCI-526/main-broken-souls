using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuZone : MonoBehaviour
{
    [Header("Audio Control")]
    public bool unpauseAudio = true;
    
    [Header("Tutorial Completion")]
    [Tooltip("If true, marks tutorial as completed when player reaches this zone")]
    public bool markTutorialComplete = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Mark tutorial as completed if this is the end zone
            if (markTutorialComplete)
            {
                PlayerPrefs.SetInt("TutorialCompleted", 1);
                PlayerPrefs.Save();
                Debug.Log("[ReturnToMenuZone] Tutorial marked as completed!");
            }
            
            Time.timeScale = 1f;
            if (unpauseAudio)
                AudioListener.pause = false;
            SceneManager.LoadScene("StartMenu");
        }
    }
}
