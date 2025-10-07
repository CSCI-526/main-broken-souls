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
}
