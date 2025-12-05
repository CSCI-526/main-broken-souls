using UnityEngine;
using System.Collections;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject coinPrefab;

    [Header("References")]
    public Transform player;
    public EndlessGround groundManager;

    [Header("Spawn Timing")]
    public float spawnInterval = 2.0f;
    public float spawnDistance = 22f;

    [Header("Placement")]
    public float yOffset = 1.8f;
    public float randomYJitter = 0.1f;

    [Header("Collision / Adjust Settings (for CoinAutoAdjuster)")]
    [Tooltip("Layer that contains spikes / blocks / coffins / barrels / statues / etc.")]
    public LayerMask obstacleLayer;

    [Tooltip("How many times the coin moves to find a free spot.")]
    public int maxAttempts = 10;

    [Tooltip("How much to move each attempt (world units).")]
    public float adjustStep = 0.3f;

    private WaitForSeconds wait;

    void OnEnable()
    {
        wait = new WaitForSeconds(Mathf.Max(0.05f, spawnInterval));
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (enabled)
        {
            TrySpawnCoin();
            yield return wait;
        }
    }

    void TrySpawnCoin()
    {
        if (coinPrefab == null || player == null || groundManager == null)
            return;

        // 1. Check if gravity is flipped
        bool flipped = false;
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
            flipped = pc.IsGravityFlipped();

        // 2. X position in front of the player
        float spawnX = player.position.x + spawnDistance;

        // 3. Choose ground or ceiling tiles based on gravity
        Transform[] tiles = flipped ? groundManager.ceilingTiles : groundManager.groundTiles;

        // 4. Find which tile this X is over
        Transform chosen = null;
        float half = groundManager.tileWidth * 0.5f;

        foreach (var t in tiles)
        {
            if (t == null) continue;
            float left = t.position.x - half;
            float right = t.position.x + half;

            if (spawnX >= left && spawnX <= right)
            {
                chosen = t;
                break;
            }
        }

        if (chosen == null)
            return;

        // 5. Base Y position relative to that tile
        float signedYOffset = flipped ? -Mathf.Abs(yOffset) : Mathf.Abs(yOffset);
        Vector3 pos = new Vector3(spawnX, chosen.position.y + signedYOffset, 0f);

        // 6. Small random jitter so coins aren't in a perfect line
        pos.y += Random.Range(-randomYJitter, randomYJitter);

        // 7. Spawn the coin
        GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);

        // 7.1 send data to CoinAutoAdjuster
        var adjuster = coin.GetComponent<CoinAutoAdjuster>();
        if (adjuster != null)
        {
            adjuster.obstacleLayer  = obstacleLayer;
            adjuster.maxAttempts    = maxAttempts;
            adjuster.adjustStep     = adjustStep;
            adjuster.gravityFlipped = flipped;
        }

        // 8. Ensure it scrolls with the world
        var rb = coin.GetComponent<Rigidbody2D>();
        if (rb == null) rb = coin.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var mover = coin.GetComponent<CoinMover>();
        if (mover == null) mover = coin.AddComponent<CoinMover>();
        mover.ground = groundManager;
        mover.despawnX = player.position.x - 20f;
    }

    // Optional debug gizmo to roughly visualize spawn area
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.yellow;

        float spawnX = player.position.x + spawnDistance;
        float baseY = player.position.y + yOffset;

        Gizmos.DrawWireCube(new Vector3(spawnX, baseY, 0f), new Vector2(0.6f, 0.6f));
    }
}
