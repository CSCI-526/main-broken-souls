using UnityEngine;
using TMPro;

public class ForceHiddenTMPText : MonoBehaviour
{
    public TMP_Text targetTMP;

    void Awake()
    {
        HideText();
    }

    public void HideText()
    {
        if (targetTMP != null)
        {
            targetTMP.gameObject.SetActive(false);
        }
    }
}
