using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("Power-Up Settings")]
    public GameObject slowMoPrefab;      // Drag your SlowMo prefab here
    public float spawnInterval = 25f;    // Spawns every 25 seconds

    [Header("References")]
    public Transform player;             // Drag Player
    public EndlessGround groundManager;  // Drag EndlessGround Manager

    [Header("Spawn Settings")]
    public float spawnDistance = 25f;    // How far in front of the player
    public float yOffset = 1.5f;         // Vertical offset from ground/ceiling

    private void Start()
    {
        StartCoroutine(SpawnSlowMoRoutine());
    }

    private IEnumerator SpawnSlowMoRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnSlowMo();
        }
    }

    private void SpawnSlowMo()
{
    if (slowMoPrefab == null || player == null || groundManager == null)
    {
        Debug.LogWarning("⚠️ Missing reference(s) on SlowMoPowerUpSpawner!");
        return;
    }

    // Spawn further ahead than obstacles
    float spawnX = player.position.x + spawnDistance + 5f; // +5 so it's clearly ahead

    PlayerController pc = player.GetComponent<PlayerController>();
    bool gravityFlipped = (pc != null && pc.IsGravityFlipped());

    Transform[] tilesToCheck = gravityFlipped ? groundManager.ceilingTiles : groundManager.groundTiles;
    Transform tileToSpawnOn = null;

    foreach (var tile in tilesToCheck)
    {
        if (tile == null) continue;
        float left = tile.position.x - groundManager.tileWidth / 2f;
        float right = tile.position.x + groundManager.tileWidth / 2f;

        if (spawnX >= left && spawnX <= right)
        {
            tileToSpawnOn = tile;
            break;
        }
    }

    if (tileToSpawnOn == null)
    {
        Debug.Log("⚠️ No tile found for SlowMo spawn!");
        return;
    }

    // ✨ Adjust Y to be well above ground and reachable
    float offsetDir = gravityFlipped ? -3f : 3f;  // used to be yOffset=1.5, now higher
    Vector3 spawnPos = new Vector3(spawnX, tileToSpawnOn.position.y + offsetDir, 0f);

    // ✅ Optional: small random offset so not every spawn looks identical
    spawnPos.y += Random.Range(-0.3f, 0.3f);

    GameObject slowMo = Instantiate(slowMoPrefab, spawnPos, Quaternion.identity);

    // Flip sprite correctly if gravity is flipped
    if (gravityFlipped)
        slowMo.transform.localScale = new Vector3(slowMo.transform.localScale.x, -Mathf.Abs(slowMo.transform.localScale.y), slowMo.transform.localScale.z);

    Rigidbody2D rb = slowMo.GetComponent<Rigidbody2D>();
    if (rb == null) rb = slowMo.AddComponent<Rigidbody2D>();
    rb.bodyType = RigidbodyType2D.Kinematic;

    ObstacleMover mover = slowMo.AddComponent<ObstacleMover>();
    mover.speed = groundManager.scrollSpeed;
    mover.despawnX = player.position.x - 20f;

    Debug.Log("🌀 Slow-Mo Power-Up Spawned at " + spawnPos);
}

}
