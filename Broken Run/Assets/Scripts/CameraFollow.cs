using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;
    public Transform killer;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(3.5f, 1.57f, -10f); // Default camera distance

    void Start()
    {
        if (player == null || killer == null)
        {
            Debug.LogWarning("Player or Killer not assigned to CameraFollow!");
            return;
        }

        // Find midpoint between player & killer
        Vector3 midpoint = (player.position + killer.position) / 2f;

        // Set camera position once so both are visible
        transform.position = midpoint + offset;
    }

    void LateUpdate()
    {
        // Do nothing – camera stays fixed
    }
}
