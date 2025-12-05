using UnityEngine;

public class GravityFollower : MonoBehaviour
{
    private PlayerController player;
    private EndlessGround groundManager;

    [Tooltip("Vertical offset from tile center when attached.")]
    public float attachOffset = 3f;

    [Tooltip("How fast power-up moves when gravity flips.")]
    public float gravityMoveSpeed = 12f;

    private bool lastGravityFlipped = false;
    private bool isMoving = false;
    private Vector3 targetPos;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        groundManager = FindObjectOfType<EndlessGround>();

        if (player != null)
            lastGravityFlipped = player.IsGravityFlipped();
    }

    void Update()
    {
        if (player == null || groundManager == null) return;

        bool current = player.IsGravityFlipped();

        // gravity changed -> start smooth movement
        if (current != lastGravityFlipped)
        {
            BeginSmoothFlip(current);
            lastGravityFlipped = current;
        }

        // If currently moving, smoothly approach the target
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                gravityMoveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
                isMoving = false; // finished moving
        }
    }

    private void BeginSmoothFlip(bool flipped)
    {
        // Flip sprite instantly (visual only)
        Vector3 s = transform.localScale;
        s.y = flipped ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);
        transform.localScale = s;

        // Compute new target Y position
        Transform[] tiles = flipped ? groundManager.ceilingTiles : groundManager.groundTiles;
        Transform closest = null;
        float bestDist = Mathf.Infinity;

        foreach (var t in tiles)
        {
            if (t == null) continue;

            float dist = Mathf.Abs(t.position.x - transform.position.x);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = t;
            }
        }

        if (closest != null)
        {
            float newY = closest.position.y + (flipped ? -Mathf.Abs(attachOffset) : Mathf.Abs(attachOffset));

            // Set movement target
            targetPos = new Vector3(transform.position.x, newY, transform.position.z);
            isMoving = true;
        }
    }
}
