using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinAutoAdjuster : MonoBehaviour
{
    [Header("Overlap Settings")]
    public LayerMask obstacleLayer;
    public float adjustStep;
    public int maxAttempts; 

    [Header("Gravity State")]
    public bool gravityFlipped = false;

    private Collider2D col;
    private int iterations;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning("CoinAutoAdjuster requires a Collider2D on the coin.");
        }
    }

    void Start()
    {
        ResolveOverlapImmediate();
    }

    void FixedUpdate()
    {
        if (col == null || iterations >= maxAttempts) return;

        if (col.IsTouchingLayers(obstacleLayer))
        {
            MoveOneStep();
        }
    }

    void ResolveOverlapImmediate()
    {
        if (col == null) return;

        iterations = 0;
        int safety = maxAttempts;
        while (col.IsTouchingLayers(obstacleLayer) && safety-- > 0)
        {
            MoveOneStep();
            Physics2D.SyncTransforms();
        }
    }

    void MoveOneStep()
    {
        float dir = gravityFlipped ? -1f : 1f;
        transform.position += new Vector3(0f, dir * adjustStep, 0f);
        iterations++;
    }

    void OnDrawGizmosSelected()
    {
        if (col == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
