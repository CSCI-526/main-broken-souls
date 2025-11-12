using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 20f;  // Increased from 15s - more time to reach it!

    private void Start()
    {
        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[GunPickup] Trigger hit by: {other.name} with tag: {other.tag}");
        
        if (!other.CompareTag("Player"))
        {
            Debug.LogWarning($"[GunPickup] Not player! Tag is: {other.tag}");
            return;
        }

        var gun = other.GetComponent<PlayerGun>();
        if (gun == null)
        {
            Debug.LogError("[GunPickup] Player has no PlayerGun component!");
            return;
        }

        // Always allow pickup - it refills ammo to max!
        Debug.Log($"🔫 Gun collected! Refilling ammo to max ({gun.maxAmmo})");
        gun.GiveGun();
        
        // Track power-up collection for analytics
        if (EnhancedAnalytics.Instance != null)
        {
            EnhancedAnalytics.Instance.OnPowerUpCollected();
        }
        
        Destroy(gameObject);
    }
}
