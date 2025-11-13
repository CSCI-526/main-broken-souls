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
    public float speedIncreaseRate = 0.01f;   // world accelerates over time
    public float maxScrollSpeed = 20f;

    [Header("Player")]
    public Transform player;

    [Header("Killer")]
    public GameObject killerPrefab;           // assign your Killer prefab (tagged "Killer")

    [HideInInspector] public Transform[] groundTiles;

    // ===== Ceiling (uses SAME prefabs) =====
    [Header("Ceiling")]
    public float ceilingY = 4.5f;
    [HideInInspector] public Transform[] ceilingTiles;

    // internals
    int totalTiles;
    float startX = -12.2f;
    float leftBoundary = -30f;

    void Start()
    {
        totalTiles = tilesLeft + tilesRight;

        // Ground
        groundTiles = new Transform[totalTiles];
        for (int i = 0; i < totalTiles; i++)
        {
            float x = startX + (i - tilesLeft) * tileWidth;
            SpawnTile(i, x);
        }

        // Ceiling
        ceilingTiles = new Transform[totalTiles];
        for (int i = 0; i < totalTiles; i++)
        {
            float x = startX + (i - tilesLeft) * tileWidth;
            SpawnCeilingTile(i, x);
        }

        // Place player on first ground tile
        if (player != null)
            player.position = new Vector3(startX + tileWidth * 0.5f, yPos + 1f, 0f);

        // Ensure only one killer
        if (killerPrefab != null && GameObject.FindGameObjectWithTag("Killer") == null)
            Instantiate(killerPrefab);
    }

    void Update()
    {
        // accelerate world
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxScrollSpeed);

        // let player controller react to speed (if it has such a method)
        var pc = player != null ? player.GetComponent<PlayerController>() : null;
        if (pc != null)
            pc.AdjustToWorldSpeed(scrollSpeed);

        // move & recycle ground
        RecycleStrip(groundTiles, yPos);

        // move & recycle ceiling
        RecycleStrip(ceilingTiles, ceilingY);
    }

    void RecycleStrip(Transform[] strip, float stripY)
    {
        if (strip == null) return;

        for (int i = 0; i < strip.Length; i++)
        {
            var t = strip[i];
            if (t == null) continue;

            t.position += Vector3.left * scrollSpeed * Time.deltaTime;

            if (t.position.x < leftBoundary)
            {
                // find rightmost tile in this strip
                float maxX = float.NegativeInfinity;
                for (int k = 0; k < strip.Length; k++)
                {
                    if (strip[k] == null) continue;
                    if (strip[k].position.x > maxX) maxX = strip[k].position.x;
                }

                float nextX = maxX + tileWidth;

                // destroy old tile and respawn a fresh one at nextX
                Destroy(t.gameObject);
                if (Mathf.Approximately(stripY, yPos))
                    SpawnTile(i, nextX);
                else
                    SpawnCeilingTile(i, nextX);
            }
        }
    }

    void SpawnTile(int index, float xPos)
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0) return;
        GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        if (prefab == null) return;

        GameObject tile = Instantiate(prefab, new Vector3(xPos, yPos, 0f), Quaternion.identity);
        groundTiles[index] = tile.transform;
    }

    void SpawnCeilingTile(int index, float xPos)
    {
        if (tilePrefabs == null || tilePrefabs.Length == 0) return;
        GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        if (prefab == null) return;

        GameObject tile = Instantiate(prefab, new Vector3(xPos, ceilingY, 0f), Quaternion.identity);
        ceilingTiles[index] = tile.transform;
    }
}
