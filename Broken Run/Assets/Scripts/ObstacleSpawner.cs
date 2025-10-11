// using UnityEngine;

// public class ObstacleSpawner : MonoBehaviour
// {
//     [Header("Obstacles")]
//     public GameObject[] obstaclePrefabs;

//     [Header("Shield Power-Up")]
//     public GameObject shieldPrefab;        // Drag your Shield prefab here
//     [Range(0f, 1f)] public float shieldSpawnChance = 0.1f; // 10% chance per spawn

//     [Header("References")]
//     public Transform player;
//     public EndlessGround groundManager;

//     [Header("Spawn Settings")]
//     public float spawnDistance = 20f;
//     public float spawnInterval = 2f;

//     private float timer;

//     void Update()
//     {
//         timer += Time.deltaTime;
//         if (timer >= spawnInterval)
//         {
//             timer = 0f;
//             SpawnObject();
//         }
//     }

//     void SpawnObject()
//     {
//         GameObject prefabToSpawn = null;
//         bool isShield = false;

//         // Decide: spawn shield or obstacle
//         if (Random.value < shieldSpawnChance && shieldPrefab != null)
//         {
//             prefabToSpawn = shieldPrefab;
//             isShield = true;
//         }
//         else
//         {
//             prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
//         }

//         if (prefabToSpawn == null) return;

//         float spawnX = player.position.x + spawnDistance;

//         // Find ground tile under spawnX
//         Transform tileToSpawnOn = null;
//         foreach (var tile in groundManager.groundTiles)
//         {
//             if (tile == null) continue;
//             float left = tile.position.x - groundManager.tileWidth / 2f;
//             float right = tile.position.x + groundManager.tileWidth / 2f;
//             if (spawnX >= left && spawnX <= right)
//             {
//                 tileToSpawnOn = tile;
//                 break;
//             }
//         }

//         if (tileToSpawnOn == null) return;

//         Vector3 spawnPos;

//         if (isShield)
//         {
//             // Spawn shield near player's Y position (~3.8)
//             spawnPos = new Vector3(spawnX, player.position.y + Random.Range(-0.5f, 0.5f), 0);
//         }
//         else
//         {
//             // Spawn obstacle slightly above ground
//             spawnPos = new Vector3(spawnX, tileToSpawnOn.position.y + 0.5f, 0);
//         }

//         GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

//         // Attach mover so shield & obstacles scroll with ground
//         Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
//         if (rb == null) rb = obj.AddComponent<Rigidbody2D>();
//         rb.bodyType = RigidbodyType2D.Kinematic;

//         ObstacleMover mover = obj.AddComponent<ObstacleMover>();
//         mover.speed = groundManager.scrollSpeed;
//         mover.despawnX = player.position.x - 20f;

//         // Example: attach coin above obstacles only
//         if (!isShield && groundManager.coinPrefab != null && Random.value < 0.5f)
//         {
//             Vector3 coinPos = obj.transform.position + Vector3.up * 2.5f;
//             GameObject coin = Instantiate(groundManager.coinPrefab, coinPos, Quaternion.identity);
//             coin.transform.SetParent(obj.transform);
//         }
//     }
// }



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
    public float spawnDistance = 20f;
    public float spawnInterval = 2f;

    [Header("Air Obstacle Settings")]
    public float airOffset = 2.5f;      // How high above ground for air obstacles
    public float airSpawnChance = 0.4f; // Chance to spawn in air instead of ground

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnObject();
        }
    }

    void SpawnObject()
    {
        GameObject prefabToSpawn = null;
        bool isShield = false;
        bool spawnAir = false;

        // Decide shield first
        if (Random.value < shieldSpawnChance && shieldPrefab != null)
        {
            prefabToSpawn = shieldPrefab;
            isShield = true;
        }
        else
        {
            // Decide air vs ground obstacle
            spawnAir = airObstaclePrefabs.Length > 0 && Random.value < airSpawnChance;
            if (spawnAir)
                prefabToSpawn = airObstaclePrefabs[Random.Range(0, airObstaclePrefabs.Length)];
            else
                prefabToSpawn = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        }

        if (prefabToSpawn == null) return;

        float spawnX = player.position.x + spawnDistance;

        // Find ground tile under spawnX
        Transform tileToSpawnOn = null;
        foreach (var tile in groundManager.groundTiles)
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

        if (tileToSpawnOn == null) return;

        // Determine Y position
        Vector3 spawnPos;
        if (isShield)
        {
            spawnPos = new Vector3(spawnX, player.position.y + Random.Range(-0.5f, 0.5f), 0);
        }
        else if (spawnAir)
        {
            spawnPos = new Vector3(spawnX, tileToSpawnOn.position.y + airOffset, 0);
        }
        else
        {
            spawnPos = new Vector3(spawnX, tileToSpawnOn.position.y + 0.5f, 0);
        }

        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Rigidbody2D so it moves with the ground
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = obj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // ObstacleMover scrolls the object left
        ObstacleMover mover = obj.AddComponent<ObstacleMover>();
        mover.speed = groundManager.scrollSpeed;
        mover.despawnX = player.position.x - 20f;

        // Optionally attach coin above obstacles
        if (!isShield && groundManager.coinPrefab != null && Random.value < 0.5f)
        {
            Vector3 coinPos = obj.transform.position + Vector3.up * 2.5f;
            GameObject coin = Instantiate(groundManager.coinPrefab, coinPos, Quaternion.identity);
            coin.transform.SetParent(obj.transform);
        }
    }
}
