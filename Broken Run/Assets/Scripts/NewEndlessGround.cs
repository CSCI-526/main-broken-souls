using UnityEngine;

public class NewEndlessGround : MonoBehaviour
{
    [Header("Tiles Settings")]
    public GameObject[] tilePrefabs;
    public int tilesLeft = 5;
    public int tilesRight = 20;
    public float tileWidth = 10f;
    public float yPos = -4.5f;

    [Header("Scrolling")]
    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.01f;
    public float maxScrollSpeed = 20f;

    [Header("Player")]
    public Transform player;

    [Header("Killer")]
    public GameObject killerPrefab;

    [Header("Collectibles")]
    public GameObject coinPrefab;

    [HideInInspector] public Transform[] groundTiles;

    // ===== Ceiling =====
    [Header("Ceiling (copy of ground)")]
    public float ceilingY = 4.5f; 
    [HideInInspector] public Transform[] ceilingTiles;

    [Header("Slope")]
    [Range(0f, 45f)] public float slopeAngleDeg = 15f;
    [Range(0f, 1f)] public float slopeChance = 0.35f;

    [Header("Startup Flats")]
    public int initialFlatCount = 6;

    private int totalTiles;
    private float startX = -12.2f;
    private float leftBoundary = -30f;
    private float ceilingOffsetY; // = ceilingY - yPos

    void Start()
    {
        Debug.Assert(tilePrefabs != null && tilePrefabs.Length > 0, "tilePrefabs 为空！");
        totalTiles = tilesLeft + tilesRight;
        groundTiles = new Transform[totalTiles];
        ceilingTiles = new Transform[totalTiles];
        ceilingOffsetY = ceilingY - yPos;


        for (int i = 0; i < totalTiles; i++)
        {
            Transform leftN = (i == 0) ? null : groundTiles[i - 1];

            // initialFlatCount must be flat
            TileKind? force = (i < initialFlatCount) ? TileKind.Flat : (TileKind?)null;

            SpawnTile(i, startX + (i - tilesLeft) * tileWidth, leftN, force);
        }

        // Place player on first tile
        if (player != null)
            player.position = new Vector3(startX + tileWidth / 2f, yPos + 1f, 0);

        // Killer
        if (killerPrefab != null && FindObjectOfType<KillerController>() == null)
            Instantiate(killerPrefab);
    }

    void Update()
    {
        // speed up
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxScrollSpeed);

        // player speed
        var pc = (player != null) ? player.GetComponent<PlayerController>() : null;
        if (pc != null) pc.AdjustToWorldSpeed(scrollSpeed);

        // recycle
        for (int i = 0; i < totalTiles; i++)
        {
            var g = groundTiles[i];
            if (g == null) continue;

            // synchro ceiling and floor
            Vector3 delta = Vector3.left * scrollSpeed * Time.deltaTime;
            g.position += delta;
            if (ceilingTiles[i] != null) ceilingTiles[i].position += delta;

            if (g.position.x < leftBoundary)
            {
                float maxX = float.MinValue;
                Transform rightMost = null;
                foreach (var t in groundTiles)
                {
                    if (t == null) continue;
                    if (t.position.x > maxX) { maxX = t.position.x; rightMost = t; }
                }

                if (groundTiles[i] != null) Destroy(groundTiles[i].gameObject);
                if (ceilingTiles[i] != null) Destroy(ceilingTiles[i].gameObject);

                TileKind? forceKind = null;
                if (rightMost != null)
                {
                    var lm = rightMost.GetComponent<TileMeta>();
                    if (lm != null && (lm.kind == TileKind.UpSlope || lm.kind == TileKind.DownSlope))
                        forceKind = TileKind.Flat;
                }

                SpawnTile(i, maxX + tileWidth, rightMost, forceKind);
            }
        }


    }

    // ========== helper function ==========

    Transform GetAnchor(Transform tile, string name)
    {
        var a = tile.Find(name);
        if (a == null) Debug.LogError($"Anchor '{name}' not found under {tile.name}");
        return a;
    }

    void ApplyKindRotation(Transform t, TileKind kind)
    {
        float z = (kind == TileKind.UpSlope) ? +slopeAngleDeg :
                  (kind == TileKind.DownSlope) ? -slopeAngleDeg : 0f;
        t.rotation = Quaternion.Euler(0, 0, z);
    }

    void AlignRightToLeft(Transform left, TileKind leftKind, Transform right, TileKind rightKind)
    {
        Transform l_rt = GetAnchor(left,  "righttop");
        Transform l_rb = GetAnchor(left,  "rightbottom");
        Transform r_lt = GetAnchor(right, "lefttop");
        Transform r_lb = GetAnchor(right, "leftbottom");
        if (!l_rt || !l_rb || !r_lt || !r_lb) return;

        Vector3 target, source;

        // 5 cases
        if (leftKind == TileKind.Flat && rightKind == TileKind.Flat)
        { target = l_rt.position; source = r_lt.position; }
        else if (leftKind == TileKind.Flat && rightKind == TileKind.DownSlope)
        { target = l_rt.position; source = r_lt.position; }
        else if (leftKind == TileKind.Flat && rightKind == TileKind.UpSlope)
        { target = l_rb.position; source = r_lb.position; }
        else if (leftKind == TileKind.DownSlope && rightKind == TileKind.Flat)
        { target = l_rb.position; source = r_lb.position; }
        else if (leftKind == TileKind.UpSlope && rightKind == TileKind.Flat)
        { target = l_rt.position; source = r_lt.position; }
        else
        {
            // default
            right.position = new Vector3(left.position.x + tileWidth, yPos, 0);
            return;
        }

        right.position += (target - source);
    }

    // ========== spawn ground and ceiling ==========

    void SpawnTile(int index, float xPos, Transform leftNeighbor = null, TileKind? forcedKind = null)
    {
        GameObject prefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
        if (prefab == null) return;

        // spawn ground
        GameObject gObj = Instantiate(prefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
        Transform gTr = gObj.transform;
        groundTiles[index] = gTr;

        // decide slope
        var meta = gObj.GetComponent<TileMeta>(); if (!meta) meta = gObj.AddComponent<TileMeta>();
        TileKind rightKind = TileKind.Flat;

        if (forcedKind.HasValue) rightKind = forcedKind.Value;
        else if (leftNeighbor != null)
        {
            var lm = leftNeighbor.GetComponent<TileMeta>();
            TileKind leftKind = (lm != null) ? lm.kind : TileKind.Flat;
            if (leftKind == TileKind.Flat && Random.value < slopeChance)
                rightKind = (Random.value < 0.5f) ? TileKind.UpSlope : TileKind.DownSlope;
        }
        meta.kind = rightKind;
        ApplyKindRotation(gTr, rightKind);

        if (leftNeighbor != null)
        {
            var lm = leftNeighbor.GetComponent<TileMeta>();
            TileKind leftKind = (lm != null) ? lm.kind : TileKind.Flat;
            AlignRightToLeft(leftNeighbor, leftKind, gTr, rightKind);
        }

        GameObject cObj = Instantiate(prefab, gTr.position + new Vector3(0, ceilingOffsetY, 0), gTr.rotation);
        ceilingTiles[index] = cObj.transform;

        // coin  
        if (Random.value < 0.30f)
        {
            TrySpawnCoinOnSurface(gTr, isCeiling:false, offset:1f);
        }
        // coin on ceiling
        if (ceilingTiles[index] != null && Random.value < 0.30f)
        {
            TrySpawnCoinOnSurface(ceilingTiles[index], isCeiling:true, offset:1f);
        }
    }


    (Vector3 mid, Vector3 upNormal, Vector3 downNormal) EdgeInfo(Transform a, Transform b)
    {
        Vector3 p1 = a.position;
        Vector3 p2 = b.position;
        Vector3 dir = (p2 - p1).normalized;             
        Vector3 mid = (p1 + p2) * 0.5f;     
        Vector3 n = new Vector3(-dir.y, dir.x, 0f).normalized;
        Vector3 upN = (Vector3.Dot(n, Vector3.up) >= 0f) ? n : -n;
        Vector3 downN = -upN;
        return (mid, upN, downN);
    }

    void TrySpawnCoinOnSurface(Transform tile, bool isCeiling, float offset)
    {
        if (coinPrefab == null) return;
        Transform lt = tile.Find(isCeiling ? "leftbottom"  : "lefttop");
        Transform rt = tile.Find(isCeiling ? "rightbottom" : "righttop");
        if (lt == null || rt == null) { Debug.LogWarning("Coin spawn: anchors missing"); return; }

        var info = EdgeInfo(lt, rt);
        Vector3 basePos = info.mid + (isCeiling ? info.downNormal : info.upNormal) * offset;
        float checkRadius = 0.35f;
        LayerMask obstacleLayer = LayerMask.GetMask("Obstacle");
        bool overlaps = Physics2D.OverlapCircle(basePos, checkRadius, obstacleLayer);

        if (!overlaps)
        {
            GameObject coin = Instantiate(coinPrefab, basePos, Quaternion.identity);
            coin.transform.SetParent(tile);
        }
    }
}
