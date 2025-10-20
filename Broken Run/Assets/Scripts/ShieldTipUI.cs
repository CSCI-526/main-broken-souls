using TMPro;
using UnityEngine;

public class ShieldTipUI : MonoBehaviour
{
    public TextMeshProUGUI tipText;

    void Start()
    {
        tipText.text = "Pick up the <color=#30F500>Shield</color> to block one hit!";
    }
}
