using UnityEngine;

public class GravityFollower : MonoBehaviour
{
    private PlayerController player;
    private EndlessGround groundManager;

    [Tooltip("Vertical offset from tile center when attached.")]
    public float attachOffset = 3f;

    private bool lastGravityFlipped = false;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        groundManager = FindObjectOfType<EndlessGround>();
        
        // Cache initial gravity
        if (player != null)
            lastGravityFlipped = player.IsGravityFlipped();
    }

    void Update()
    {
        if (player == null || groundManager == null) return;

        bool current = player.IsGravityFlipped();

        // If gravity flipped, update power-up orientation + position
        if (current != lastGravityFlipped)
        {
            ApplyGravityFlip(current);
            lastGravityFlipped = current;
        }
    }

    private void ApplyGravityFlip(bool flipped)
    {
        // Flip the sprite
        Vector3 s = transform.localScale;
        s.y = flipped ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);
        transform.localScale = s;

        // Snap to nearest tile
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
            transform.position = new Vector3(
                transform.position.x,
                closest.position.y + (flipped ? -Mathf.Abs(attachOffset) : Mathf.Abs(attachOffset)),
                transform.position.z
            );
        }
    }
}
