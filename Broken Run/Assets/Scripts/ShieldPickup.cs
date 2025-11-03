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
            Destroy(gameObject);
        }
    }
}
