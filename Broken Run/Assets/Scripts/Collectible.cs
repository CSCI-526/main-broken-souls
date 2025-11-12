using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int scoreValue = 50; // no longer used for adding score
    public GameObject collectEffect;   // optional VFX prefab

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Spawn optional pickup VFX
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);



        // ✅ Show floating "+1" popup at the coin position
        FloatingTextSpawner.I?.Show("+1", transform.position);

        // --- NEW: notify PlayerGun to count coin toward ammo ---
        var gun = other.GetComponent<PlayerGun>();
        if (gun == null) gun = other.GetComponentInChildren<PlayerGun>();
        if (gun != null) gun.OnCoinCollected();

        // Track coin collection for analytics
        if (EnhancedAnalytics.Instance != null)
        {
            EnhancedAnalytics.Instance.OnCoinCollected();
        }

        Debug.Log("Collected! +1");

        // Remove coin
        Destroy(gameObject);
    }
}