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
    public float randomYJitter = 0.25f;

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

        // y placement relative to tile center
        float signedYOffset = flipped ? -Mathf.Abs(yOffset) : Mathf.Abs(yOffset);
        Vector3 pos = new Vector3(
            spawnX,
            chosen.position.y + signedYOffset + Random.Range(-randomYJitter, randomYJitter),
            0f
        );

        // spawn coin
        GameObject coin = Instantiate(coinPrefab, pos, Quaternion.identity);

        // make it scroll & despawn
        var rb = coin.GetComponent<Rigidbody2D>();
        if (rb == null) rb = coin.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var mover = coin.GetComponent<ObstacleMover>();
        if (mover == null) mover = coin.AddComponent<ObstacleMover>();
        mover.speed = groundManager.scrollSpeed;
        mover.despawnX = player.position.x - 20f;
    }
}
