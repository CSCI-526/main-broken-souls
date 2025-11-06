using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns Slow-Mo on an interval (optional) and spawns a Gun
/// every time ScoreReader signals a milestone (e.g., 500, 1000, 1500...).
/// Places pickups on the active ground/ceiling tile ahead of the player.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject slowMoPrefab;   // optional
    public GameObject gunPrefab;      // required for gun spawns

    [Header("Slow-Mo Timing (optional)")]
    [Tooltip("Seconds between Slow-Mo spawn attempts. Set <= 0 to disable.")]
    public float slowMoSpawnInterval = 25f;

    [Header("Gun Milestone Spawning")]
    [Tooltip("Score milestone reader (drag the ScoreReader in the scene).")]
    public ScoreReader scoreReader;

    [Tooltip("If false, do not spawn a gun while the player already has one.")]
    public bool spawnGunEvenIfPlayerHasOne = true;

    [Header("World References")]
    public Transform player;             // Drag Player
    public EndlessGround groundManager;  // Drag EndlessGround

    [Header("Placement")]
    [Tooltip("How far in front of the player to drop the pickup.")]
    public float spawnDistance = 25f;

    [Tooltip("Extra forward offset so it’s clearly ahead of obstacles.")]
    public float extraForward = 5f;

    [Tooltip("Vertical offset from the tile center (positive up / negative down).")]
    public float yOffset = 3f;

    [Tooltip("Small random vertical jitter so spawns aren’t identical.")]
    public float randomYJitter = 0.3f;

    private void Start()
    {
        if (scoreReader == null)
            scoreReader = FindObjectOfType<ScoreReader>();

        if (slowMoSpawnInterval > 0f && slowMoPrefab != null)
            StartCoroutine(SlowMoRoutine());
    }

    private void Update()
    {
        if (scoreReader == null || gunPrefab == null || player == null || groundManager == null)
            return;

        // Fire once per milestone
        if (scoreReader.TryConsumeMilestone(out int atScore))
        {
            // Optional: skip if the player already has a gun
            if (!spawnGunEvenIfPlayerHasOne &&
                player.TryGetComponent<PlayerGun>(out var pg) &&
                pg.HasGun)
            {
                // Do not advance the milestone here (ScoreReader already advanced),
                // we simply skip this spawn; the next milestone will be used.
                return;
            }

            if (TryFindSpawnPosition(out Vector3 pos, out bool gravityFlipped))
            {
                var pickup = Instantiate(gunPrefab, pos, Quaternion.identity);

                if (gravityFlipped)
                    pickup.transform.localScale = new Vector3(
                        pickup.transform.localScale.x,
                        -Mathf.Abs(pickup.transform.localScale.y),
                        pickup.transform.localScale.z);

                EnsureMover(pickup);
                Debug.Log($"🔫 Gun spawned at milestone {atScore} -> {pos}");
            }
            else
            {
                Debug.Log("⚠️ No tile found for Gun spawn!");
            }
        }
    }

    // ----------------- Slow-Mo as before -----------------
    private IEnumerator SlowMoRoutine()
    {
        var wait = new WaitForSeconds(slowMoSpawnInterval);
        while (true)
        {
            TrySpawnSlowMo();
            yield return wait;
        }
    }

    private void TrySpawnSlowMo()
    {
        if (slowMoPrefab == null || player == null || groundManager == null)
            return;

        if (TryFindSpawnPosition(out Vector3 pos, out bool gravityFlipped))
        {
            var slowMo = Instantiate(slowMoPrefab, pos, Quaternion.identity);

            if (gravityFlipped)
                slowMo.transform.localScale = new Vector3(
                    slowMo.transform.localScale.x,
                    -Mathf.Abs(slowMo.transform.localScale.y),
                    slowMo.transform.localScale.z);

            EnsureMover(slowMo);
            Debug.Log($"🌀 Slow-Mo spawned at {pos}");
        }
    }

    // ----------------- Shared helpers -----------------
    private bool TryFindSpawnPosition(out Vector3 spawnPos, out bool gravityFlipped)
    {
        spawnPos = Vector3.zero;
        gravityFlipped = false;

        if (player.TryGetComponent<PlayerController>(out var pc))
            gravityFlipped = pc.IsGravityFlipped();

        float spawnX = player.position.x + spawnDistance + extraForward;

        Transform[] tiles = gravityFlipped ? groundManager.ceilingTiles : groundManager.groundTiles;
        Transform chosen = null;

        foreach (var t in tiles)
        {
            if (t == null) continue;
            float half = groundManager.tileWidth * 0.5f;
            float left = t.position.x - half;
            float right = t.position.x + half;

            if (spawnX >= left && spawnX <= right)
            {
                chosen = t;
                break;
            }
        }

        if (chosen == null) return false;

        float signedYOffset = gravityFlipped ? -Mathf.Abs(yOffset) : Mathf.Abs(yOffset);

        spawnPos = new Vector3(
            spawnX,
            chosen.position.y + signedYOffset + Random.Range(-randomYJitter, randomYJitter),
            0f
        );
        return true;
    }

    private void EnsureMover(GameObject go)
    {
        if (!go.TryGetComponent<Rigidbody2D>(out var rb))
            rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (!go.TryGetComponent<ObstacleMover>(out var mover))
            mover = go.AddComponent<ObstacleMover>();
        mover.speed = groundManager.scrollSpeed;
        mover.despawnX = player.position.x - 20f;
    }
}
