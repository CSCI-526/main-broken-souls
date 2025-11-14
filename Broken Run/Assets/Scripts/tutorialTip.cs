using UnityEngine;
using System.Collections;

public class tutorialTip : MonoBehaviour
{
    [Tooltip("How long to pause the game at the start (in seconds, real time).")]
    public float pauseDuration = 2f;

    void Awake()
    {
        Debug.Log("StartBannerPause Awake: pausing game");
        Time.timeScale = 0f;
    }

    void OnEnable()
    {
        StartCoroutine(HideAndResume());
    }

    private IEnumerator HideAndResume()
    {
        yield return new WaitForSecondsRealtime(pauseDuration);
        gameObject.SetActive(false);
        Debug.Log("StartBannerPause: resuming game");
        Time.timeScale = 1f;
    }
}
