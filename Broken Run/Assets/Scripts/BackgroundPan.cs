using UnityEngine;
using UnityEngine.UI;

public class BackgroundPan : MonoBehaviour
{
    public float speedX = 0.02f;
    public float speedY = 0.01f;

    private RawImage img;
    private Vector2 offset;

    void Start()
    {
        img = GetComponent<RawImage>();
    }

    void Update()
    {
        offset.x += speedX * Time.deltaTime;
        offset.y += speedY * Time.deltaTime;
        img.uvRect = new Rect(offset, img.uvRect.size);
    }
}
