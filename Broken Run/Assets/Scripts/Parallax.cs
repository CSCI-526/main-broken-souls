using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float speedMultiplier = 0.1f;
    private Transform cam;
    private Vector3 lastCamPos;

    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    void Update()
    {
        Vector3 delta = cam.position - lastCamPos;
        transform.position += delta * speedMultiplier;
        lastCamPos = cam.position;
    }
}
