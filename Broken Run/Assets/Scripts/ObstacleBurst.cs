using UnityEngine;

public class ObstacleBurst : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;

    public void Burst()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
