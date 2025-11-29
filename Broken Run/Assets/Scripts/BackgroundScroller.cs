using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll")]
    public float baseSpeed = 5f;          
    public float parallaxFactor = 0.5f;   
    public float tileWidth;         

    private Vector3 startPos;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        tileWidth = sr.bounds.size.x;
        startPos = transform.position;
    }

    void Update()
    {
        float speed = baseSpeed * parallaxFactor;
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x <= startPos.x - tileWidth)
        {
            transform.position += new Vector3(tileWidth * 1f, 0f, 0f);
        }
    }
}
