using UnityEngine;

public class OpenFormButton : MonoBehaviour
{
    [SerializeField] string googleFormURL = "https://docs.google.com/forms/d/1YDs05FHmGKffsaOn_CQREqg0xoDg78xbJ7qJpv27GkE";

    public void OpenGoogleForm()
    {
        Application.OpenURL(googleFormURL);
    }
}