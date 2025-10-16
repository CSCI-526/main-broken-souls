using UnityEngine;

public class EndlessGround : MonoBehaviour
{
    [Header("Tiles Settings")]
    public GameObject[] tilePrefabs;
    public int tilesLeft = 5;
    public int tilesRight = 20;
    public float tileWidth = 10f;
    public float yPos = -4.5f;

    [Header("Scrolling")]
    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.5f;   // How fast world speed increases over time
    public float maxScrollSpeed = 20f;       // Cap on max speed

    [Header("Player")]
    public Transform player;

    [Header("Killer")]
    public GameObject killerPrefab;   // Assign killer prefab here

    [Header("Collectibles")]
    public GameObject coinPrefab; 

    [HideInInspector]
    public Transform[] groundTiles;

    // ===== NEW: Ceiling settings (uses the SAME prefab list as ground) =====
    [Header("Ceiling (uses same prefabs as ground)")]
    public float ceilingY = 4.5f;              // Y position of the ceiling tiles

    [HideInInspector]
    public Transform[] ceilingTiles;           // Internal storage for ceiling tiles
    // ===== END NEW =====

    private int totalTiles;
    private float startX = -12.2f;
    private float leftBoundary = -30f;

    void Start()
    {
        totalTiles = tilesLeft + tilesRight;
        groundTiles = new Transform[totalTiles];

        // Spawn ground tiles
        for (int i = 0; i < totalTiles; i++)
        {
            SpawnTile(i, startX + (i - tilesLeft) * tileWidth);
        }

        // ===== NEW: Spawn ceiling tiles using the same prefab set =====
        ceilingTiles = new Transform[totalTiles];

        // Spawn ceilling tiles
        for (int i = 0; i < totalTiles; i++)
        {
            SpawnCeilingTile(i, startX + (i - tilesLeft) * tileWidth);
        }
        // ===== END NEW =====

        // Place player on first tile
        player.position = new Vector3(startX + tileWidth / 2f, yPos + 1f, 0);

        // Spawn **only one** killer
        if (killerPrefab != null)
        {
            if (GameObject.FindGameObjectWithTag("Killer") == null)
            {
                Instantiate(killerPrefab);
            }
        }
    }

    void Update()
    {
        // 🌀 Increase scroll speed over time
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxScrollSpeed);

        // Update player speed to match world speed
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.AdjustToWorldSpeed(scrollSpeed);
        }

        // Move tiles
        for (int i = 0; i < totalTiles; i++)
        {
            Transform tile = groundTiles[i];
            if (tile == null) continue;

            tile.position += Vector3.left * scrollSpeed * Time.deltaTime;

            if (tile.position.x < leftBoundary)
            {
                float maxX = float.MinValue;
                foreach (var t in groundTiles)
                {
                    if (t == null) continue;
                    if (t.position.x > maxX) maxX = t.position.x;
                }

                SpawnTile(i, maxX + tileWidth);
            }
        }

        // ===== NEW: Move and recycle ceiling tiles in sync with ground =====
        if (ceilingTiles != null)
        {
            for (int i = 0; i < totalTiles; i++)
            {
                Transform tile = ceilingTiles[i];
                if (tile == null) continue;

                tile.position += Vector3.left * scrollSpeed * Time.deltaTime;

                if (tile.position.x < leftBoundary)
                {
                    float maxX = float.MinValue;
                    foreach (var t in ceilingTiles)
                    {
                        if (t == null) continue;
                        if (t.position.x > maxX) maxX = t.position.x;
                    }

                    SpawnCeilingTile(i, maxX + tileWidth);
                }
            }
        }
        // ===== END NEW =====
    }

    void SpawnTile(int index, float xPos)
{
    GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
    if (prefab == null) return;

    GameObject tile = Instantiate(prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
    groundTiles[index] = tile.transform;

    // 30% chance to spawn coin
    if (coinPrefab != null && Random.value < 0.3f)
    {
        Vector3 spawnPos = new Vector3(xPos, yPos + 1.5f, 0);

        // ✅ Prevent spawning coin on obstacle
        float checkRadius = 0.4f; // Adjust based on coin size
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle"); // make sure your obstacles use this layer

        bool overlaps = Physics2D.OverlapCircle(spawnPos, checkRadius, obstacleLayer);
        if (!overlaps)
        {
            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            coin.transform.SetParent(tile.transform);
        }
        else
        {
            Debug.Log("Skipped coin spawn — overlaps obstacle");
        }
    }
}


    // ===== NEW =====
    void SpawnCeilingTile(int index, float xPos)
    {
        GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        if (prefab == null) return;

        GameObject tile = Instantiate(prefab, new Vector3(xPos, ceilingY, 0), Quaternion.identity);
        ceilingTiles[index] = tile.transform;
    }
    // ===== END NEW =====
}
