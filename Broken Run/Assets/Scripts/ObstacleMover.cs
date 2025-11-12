using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObstacleMover : MonoBehaviour
{
    public float speed = 5f;        // match ground scroll speed
    public float despawnX = -20f;   // when to destroy obstacle
    public GameObject coinPrefab;
    
    private Rigidbody2D rb;
    private bool encounterCounted = false; // Track if we already counted this obstacle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Prevents pushing player
    }

    void FixedUpdate()
    {
        // Move left using physics-safe method
        rb.MovePosition(rb.position + Vector2.left * speed * Time.fixedDeltaTime);

        // Track when player successfully passes this obstacle (before it despawns)
        if (!encounterCounted && rb.position.x < -5f) // -5f is behind player
        {
            encounterCounted = true;
            
            // Track obstacle encounter for analytics
            if (EnhancedAnalytics.Instance != null)
            {
                string obstacleType = DetermineObstacleType();
                EnhancedAnalytics.Instance.OnObstacleEncountered(obstacleType);
            }
        }

        // Destroy if out of screen
        if (rb.position.x < despawnX)
        {
            Destroy(gameObject);
        }
    }
    
    private string DetermineObstacleType()
    {
        // Determine obstacle type from tag or name
        if (gameObject.CompareTag("Spike"))
            return "Spike";
        else if (gameObject.CompareTag("Obstacle"))
            return "Ground Obstacle";
        else if (gameObject.name.Contains("Air") || gameObject.name.Contains("air"))
            return "Air Obstacle";
        else if (gameObject.name.Contains("Enemy") || gameObject.name.Contains("enemy"))
            return "Enemy";
        else
            return "Obstacle";
    }
}
