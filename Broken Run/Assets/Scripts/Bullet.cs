using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Hit Settings")]
    public LayerMask obstacleMask;         // set to Obstacles
    public GameObject explosionPrefab;
    public float lifeSeconds = 3f;

    [Header("Ignore Coins By Layer")]
    public LayerMask coinMask;             // set to Collectible (coins)

    void Start()
    {
        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayerBit = 1 << other.gameObject.layer;

        // Ignore coins entirely
        if ((coinMask.value & otherLayerBit) != 0)
            return;

        // Only react to obstacles
        if ((obstacleMask.value & otherLayerBit) == 0)
            return;

        // Find the obstacle root that scrolls (so we only delete the obstacle)
        var mover = other.GetComponentInParent<ObstacleMover>();
        if (mover == null) return;

        var obstacleRoot = mover.gameObject;

        // VFX
        if (explosionPrefab != null)
        {
            // Collider2D has bounds in world space
            Vector3 vfxPos = other.bounds.center;
            Instantiate(explosionPrefab, vfxPos, Quaternion.identity);
        }

        // Detach any coin children so they survive (coins might be children of the obstacle)
        var children = obstacleRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in children)
        {
            if (t == obstacleRoot.transform) continue;

            // If a child is on the coin layer, detach and keep it moving
            if ((coinMask.value & (1 << t.gameObject.layer)) != 0)
            {
                t.SetParent(null, true);
                var coinMover = t.GetComponent<ObstacleMover>();
                if (coinMover == null) coinMover = t.gameObject.AddComponent<ObstacleMover>();
                coinMover.speed = mover.speed;
                coinMover.despawnX = mover.despawnX;
            }
        }

        Destroy(obstacleRoot); // destroy only the obstacle
        Destroy(gameObject);   // destroy the bullet
    }
}

