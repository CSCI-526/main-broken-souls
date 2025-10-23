using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Camera Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(3f, 0f, 0f); 
    // positive X offset = player stays left, more space on right

    private float fixedY;
    private float fixedZ;

    void Start()
    {
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Target position follows player, shifted ahead by offset
        Vector3 targetPos = new Vector3(player.position.x + offset.x, fixedY, fixedZ);

        // Smooth follow movement
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}
