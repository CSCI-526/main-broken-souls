using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillerController : MonoBehaviour
{
    [Header("Ground & Position")]
    public float groundY = -4f;
    public float spawnX = -12.2f;

    [Header("Oscillation Settings")]
    public float oscillationAmplitude = 0.3f;
    public float oscillationSpeed = 0.628f;

    [Header("Explosion Effect")]
    public GameObject explosionPrefab;

    private float startTime;
    private BoxCollider2D solidCollider;
    private BoxCollider2D triggerCollider;

    void Start()
    {
        startTime = Time.time;
        transform.position = new Vector3(spawnX, groundY + 1.5f, 0);
        transform.localScale = new Vector3(1f, 8f, 1f);

        solidCollider = gameObject.AddComponent<BoxCollider2D>();
        solidCollider.isTrigger = false;

        triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        gameObject.tag = "Killer";
    }

    void Update()
    {
        float offset = Mathf.Sin((Time.time - startTime) * oscillationSpeed) * oscillationAmplitude;
        transform.position = new Vector3(spawnX + offset, groundY + 1.5f, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController pc = collision.collider.GetComponent<PlayerController>();
            Rigidbody2D prb = collision.collider.GetComponent<Rigidbody2D>();

            if (pc != null && pc.hasShield && prb != null)
            {
                Vector2 bounce = new Vector2(0f, pc.shieldBounceForce * 2f);
                prb.linearVelocity = Vector2.zero;
                prb.AddForce(bounce, ForceMode2D.Impulse);

                pc.hasShield = false;
                Debug.Log("Shield absorbed the hit! Player bounced!");
            }
            else
            {
                Debug.Log("Game Over! Player touched the killer!");

                // Set health bar to zero
                if (pc != null && pc.healthBar != null)
                    pc.healthBar.SetHealth(0f);

                ScoreManager.Instance.GameOver();
                #if UNITY_2022_2_OR_NEWER
                FindFirstObjectByType<GameOverUI>().ShowGameOver();
                #else
                FindObjectOfType<GameOverUI>().ShowGameOver();
                #endif

                Time.timeScale = 0f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Spike") )
        {
            if (explosionPrefab != null)
            {
                GameObject fx = Instantiate(explosionPrefab, other.transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
            Destroy(other.gameObject);
        }
    }
}

