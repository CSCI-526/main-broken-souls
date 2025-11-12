using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.ActivateShield(); // ✅ Use method instead of hasShield=true
            }
            
            // Also check for NewPlayerController if it exists
            var newPlayer = other.GetComponent<NewPlayerController>();
            if (newPlayer != null)
            {
                newPlayer.ActivateShield();
            }
            
            // Track power-up collection for analytics
            if (EnhancedAnalytics.Instance != null)
            {
                EnhancedAnalytics.Instance.OnPowerUpCollected();
            }
            
            Destroy(gameObject);
        }
    }
}
