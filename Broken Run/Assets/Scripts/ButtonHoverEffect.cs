using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public float hoverScale = 1.1f;
    [HideInInspector] public float animSpeed = 0.2f;
    
    private Vector3 originalScale;
    private bool isHovering = false;
    private float currentLerpTime = 0f;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (isHovering)
        {
            currentLerpTime += Time.deltaTime / animSpeed;
            if (currentLerpTime > 1f) currentLerpTime = 1f;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * hoverScale, currentLerpTime);
        }
        else
        {
            currentLerpTime -= Time.deltaTime / animSpeed;
            if (currentLerpTime < 0f) currentLerpTime = 0f;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * hoverScale, currentLerpTime);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}

