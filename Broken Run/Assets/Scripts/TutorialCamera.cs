using UnityEngine;

public class TutorialCamera : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(3.5f, 3f, -10f); // Default camera distance
    private float fixedY;
    private float fixedZ;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not assigned to CameraFollow!");
            return;
        }

        // Find midpoint between player & killer\
        transform.position = player.position + offset;
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not assigned to CameraFollow!");
            return;
        }
        transform.position = new Vector3(player.position.x + offset.x, fixedY, fixedZ);
    }
}