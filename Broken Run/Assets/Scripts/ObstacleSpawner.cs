using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacles")]
    public GameObject[] obstaclePrefabs;      // Ground obstacles
    public GameObject[] airObstaclePrefabs;   // Air obstacles

    [Header("Shield Power-Up")]
    public GameObject shieldPrefab;
    [Range(0f, 1f)] public float shieldSpawnChance = 0.1f;

    [Header("References")]
    public Transform player;
    public EndlessGround groundManager;

    [Header("Spawn Settings")]
    public float spawnDistance = 25f;
    public float spawnInterval = 5f;

    [Header("Air Obstacle Settings")]
    public float airOffset = 2.5f;      // Height above ground
    public float airSpawnChance = 0.4f; // Chance for air obstacle

    private float timer;

void Update()
{
  
    // NORMAL OBSTACLE SPAWN
    timer += Time.deltaTime;
    if (timer >= spawnInterval)
    {
        timer = 0f;
        SpawnObject();
    }
}


    // ======================================================
    // ========== MAIN OBSTACLE + SHIELD SPAWNER ============
    // ======================================================

    void SpawnObject()
    {
        GameObject prefabToSpawn = null;
        bool isShield = false;
        bool spawnAir = false;

        PlayerController pc = player.GetComponent<PlayerController>();
        bool gravityFlipped = pc != null && pc.IsGravityFlipped();

        // SHIELD FIRST
        if (Random.value < shieldSpawnChance && shieldPrefab != null)
        {
            prefabToSpawn = shieldPrefab;
            isShield = true;
        }
        else
        {
            spawnAir = airObstaclePrefabs.Length > 0 && Random.value < airSpawnChance;
            if (spawnAir)
                prefabToSpawn = airObstaclePrefabs[Random.Range(0, airObstaclePrefabs.Length)];
            else
                prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        }

        if (prefabToSpawn == null) return;

        float spawnX = player.position.x + spawnDistance;

        Transform[] tiles = gravityFlipped ? groundManager.ceilingTiles : groundManager.groundTiles;

        Transform tileToSpawnOn = GetTileAtX(tiles, spawnX);
        if (tileToSpawnOn == null) return;

        Vector3 spawnPos;

        if (isShield)
        {
            spawnPos = new Vector3(spawnX, player.position.y + Random.Range(-0.5f, 0.5f), 0);
        }
        else if (spawnAir)
        {
            float airDir = gravityFlipped ? -airOffset : airOffset;
            spawnPos = new Vector3(spawnX, tileToSpawnOn.position.y + airDir, 0);
        }
        else
        {
            float offset = gravityFlipped ? -0.5f : 0.5f;
            spawnPos = new Vector3(spawnX, tileToSpawnOn.position.y + offset, 0);
        }

        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        if (gravityFlipped && !isShield)
            obj.transform.localScale = new Vector3(obj.transform.localScale.x, -Mathf.Abs(obj.transform.localScale.y), obj.transform.localScale.z);

        AddMover(obj);
    }

   
    // ======================================================
    // ================= UTILITY FUNCTIONS ==================
    // ======================================================

    Transform GetTileAtX(Transform[] tiles, float x)
    {
        foreach (var tile in tiles)
        {
            if (tile == null) continue;

            float left = tile.position.x - groundManager.tileWidth / 2f;
            float right = tile.position.x + groundManager.tileWidth / 2f;

            if (x >= left && x <= right)
                return tile;
        }
        return null;
    }

    void AddMover(GameObject obj)
    {
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        ObstacleMover mover = obj.GetComponent<ObstacleMover>();
        if (mover == null) mover = obj.AddComponent<ObstacleMover>();
        mover.speed = groundManager.scrollSpeed;
        mover.despawnX = player.position.x - 20f;
    }
}
