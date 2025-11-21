using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;    // Assign in Inspector
    public AudioClip backgroundMusic;  // Assign your custom music clip

    void Awake()
    {
        // Ensure AudioSource exists
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.clip = backgroundMusic;

        // Start music on scene load
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Pause();
    }

    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f; // Reset position for restart
        }
    }
}
