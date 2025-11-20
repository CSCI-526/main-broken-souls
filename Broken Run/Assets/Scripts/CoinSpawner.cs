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

[Header("Collision Checks")]
public LayerMask obstacleLayer;             // assign all obstacles (ground & air)
public Vector2 coinCheckSize = new Vector2(0.5f, 0.5f);

WaitForSeconds wait;

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
    if (coinPrefab == null || player == null || groundManager == null) return;

    // gravity state
    bool flipped = false;
    var pc = player.GetComponent<PlayerController>();
    if (pc != null) flipped = pc.IsGravityFlipped();

    float spawnX = player.position.x + spawnDistance;

    // choose tiles based on gravity
    Transform[] tiles = flipped ? groundManager.ceilingTiles : groundManager.groundTiles;

    // find active tile at spawnX
    Transform chosen = null;
    float half = groundManager.tileWidth * 0.5f;
    foreach (var t in tiles)
    {
        if (t == null) continue;
        float left = t.position.x - half;
        float right = t.position.x + half;
        if (spawnX >= left && spawnX <= right) { chosen = t; break; }
    }
    if (chosen == null) return;

    // base Y position
    float signedYOffset = flipped ? -Mathf.Abs(yOffset) : Mathf.Abs(yOffset);
    Vector3 pos = new Vector3(spawnX, chosen.position.y + signedYOffset, 0f);

    // attempt to find free space if obstacles are present
    int attempts = 10;             // more attempts
    float step = 0.3f;             // move per attempt
    for (int i = 0; i < attempts; i++)
    {
        Collider2D hit = Physics2D.OverlapBox(pos, coinCheckSize, 0f, obstacleLayer);
        if (hit == null) break; // free spot found
        pos.y += flipped ? -step : step;
    }

    // final check — skip if blocked
    if (Physics2D.OverlapBox(pos, coinCheckSize, 0f, obstacleLayer) != null) return;

    // add small random jitter
    pos.y += Random.Range(-randomYJitter, randomYJitter);

    // spawn coin
    GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);

    // make it scroll & despawn
    var rb = coin.GetComponent<Rigidbody2D>();
    if (rb == null) rb = coin.AddComponent<Rigidbody2D>();
    rb.bodyType = RigidbodyType2D.Kinematic;

    var mover = coin.GetComponent<CoinMover>();
    if (mover == null) mover = coin.AddComponent<CoinMover>();
    mover.ground = groundManager;
    mover.despawnX = player.position.x - 20f;
}

// optional: visualize check in editor
void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    if (player != null)
    {
        Gizmos.DrawWireCube(player.position, coinCheckSize);
    }
}

}
