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
    public bool enableCeiling = true;          // Toggle ceiling on/off
    public float ceilingY = 4.5f;              // Y position of the ceiling tiles
    public bool mirrorCeilingVisual = false;   // If true, visually flip the ceiling tile vertically
    public bool spawnCoinsOnCeiling = false;   // Spawn coins on ceiling as well
    [Range(0f, 1f)] public float ceilingCoinChance = 0.2f; // Coin spawn chance for ceiling tiles

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
        if (enableCeiling)
        {
            ceilingTiles = new Transform[totalTiles];
            for (int i = 0; i < totalTiles; i++)
            {
                float x = startX + (i - tilesLeft) * tileWidth;
                SpawnCeilingTile(i, x); // Same prefab choice, different Y, optional visual mirror
            }
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
        if (enableCeiling && ceilingTiles != null)
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
            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            coin.transform.SetParent(tile.transform);
        }
    }

    // ===== NEW: SpawnCeilingTile uses the same prefab array and mirrors visuals if needed =====
    void SpawnCeilingTile(int index, float xPos)
    {
        GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        if (prefab == null) return;

        GameObject tile = Instantiate(prefab, new Vector3(xPos, ceilingY, 0), Quaternion.identity);

        // Optional purely visual vertical flip (does not change collider normals)
        if (mirrorCeilingVisual)
        {
            Vector3 s = tile.transform.localScale;
            tile.transform.localScale = new Vector3(s.x, -Mathf.Abs(s.y), s.z);
        }

        // Store in ceiling array
        if (ceilingTiles == null || ceilingTiles.Length != totalTiles)
            ceilingTiles = new Transform[totalTiles];
        ceilingTiles[index] = tile.transform;

        // Optional coin on ceiling with independent chance
        if (spawnCoinsOnCeiling && coinPrefab != null && Random.value < ceilingCoinChance)
        {
            Vector3 spawnPos = new Vector3(xPos, ceilingY - 1.5f, 0); // place slightly below ceiling tile
            GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
            coin.transform.SetParent(tile.transform);
        }
    }
    // ===== END NEW =====
}
