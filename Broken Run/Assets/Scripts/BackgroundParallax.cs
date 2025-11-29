using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    // How fast the background moves compared to ground
    public float parallaxSpeed = 0.2f;

    void Update()
    {
        // Move slowly left over time
        transform.position += Vector3.left * parallaxSpeed * Time.deltaTime;
    }
}
