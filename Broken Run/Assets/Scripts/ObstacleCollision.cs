using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float bounceForceX = 12f;
    public float bounceForceY = 8f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Killer collision
        if (collision.collider.CompareTag("Killer"))
        {
            if (explosionPrefab != null)
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return;
        }

        // Spike collision
        if (collision.collider.CompareTag("Player") && gameObject.CompareTag("Spike"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (player != null)
            {
                // Always bounce to the right
                rb.linearVelocity = new Vector2(bounceForceX, bounceForceY);

                // ✅ Prevent multiple damage hits in quick succession
                if (player.CanTakeDamage())
                {
                    if (player.hasShield)
                    {
                        player.hasShield = false;
                        player.DeactivateShield();
                        Debug.Log("🛡 Shield absorbed spike! No damage taken.");
                    }
                    else
                    {
                        player.TakeDamage(25f);
                    }

                    player.RegisterDamageTime(); // Start cooldown
                }
            }
        }
    }
}
