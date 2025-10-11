using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer), typeof(PolygonCollider2D))]
public class EndlessHillGround : MonoBehaviour
{
    [Header("Hill Settings")]
    public float segmentWidth = 5f;
    public int visibleSegments = 30;
    public float hillAmplitude = 3f;
    public float hillFrequency = 0.5f;
    public float baseY = -4.5f;

    [Header("Scrolling")]
    public float scrollSpeed = 5f;
    public float speedIncreaseRate = 0.5f;
    public float maxScrollSpeed = 20f;

    [Header("Player")]
    public Transform player;

    [Header("Killer & Collectibles")]
    public GameObject killerPrefab;
    public GameObject coinPrefab;

    private LineRenderer lineRenderer;
    private PolygonCollider2D polygonCollider;
    private List<Vector3> points = new List<Vector3>();
    private float offset = 0f;
    private float lastX = 0f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = visibleSegments;
        lineRenderer.widthMultiplier = 0.4f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = Color.green;

        polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.isTrigger = false;
        gameObject.layer = LayerMask.NameToLayer("Ground"); // must match player groundLayer

        GenerateInitialHill();

        if (player != null && points.Count > 3)
            player.position = new Vector3(points[3].x, points[3].y + 1.5f, 0);

        if (killerPrefab != null && GameObject.FindGameObjectWithTag("Killer") == null)
            Instantiate(killerPrefab);
    }

    void Update()
    {
        scrollSpeed = Mathf.Min(scrollSpeed + speedIncreaseRate * Time.deltaTime, maxScrollSpeed);
        offset += scrollSpeed * Time.deltaTime * 0.05f;

        UpdateHillShape();

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Coin"))
                child.position += Vector3.left * scrollSpeed * Time.deltaTime;
        }
    }

    void GenerateInitialHill()
    {
        points.Clear();
        lastX = 0f;

        for (int i = 0; i < visibleSegments; i++)
        {
            float x = i * segmentWidth;
            float y = baseY + Mathf.PerlinNoise(i * hillFrequency, 0) * hillAmplitude;
            points.Add(new Vector3(x, y, 0));
        }

        lineRenderer.SetPositions(points.ToArray());
        UpdateColliderShape();
        lastX = points[points.Count - 1].x;
    }

    void UpdateHillShape()
    {
        for (int i = 0; i < points.Count; i++)
            points[i] += Vector3.left * scrollSpeed * Time.deltaTime;

        if (points[0].x < -30f)
        {
            points.RemoveAt(0);
            float newX = lastX + segmentWidth;
            float newY = baseY + Mathf.PerlinNoise(newX * hillFrequency + offset, 0) * hillAmplitude;
            points.Add(new Vector3(newX, newY, 0));
            lastX = newX;

            if (coinPrefab != null && Random.value < 0.3f)
            {
                Vector3 coinPos = new Vector3(newX, newY + 2f, 0);
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                coin.transform.SetParent(transform);
                coin.tag = "Coin";
            }
        }

        lineRenderer.SetPositions(points.ToArray());
        UpdateColliderShape();
    }

    void UpdateColliderShape()
    {
        if (polygonCollider == null || points.Count < 2)
            return;

        List<Vector2> colliderPoints = new List<Vector2>();

        // Top of hill
        foreach (var p in points)
            colliderPoints.Add(new Vector2(p.x, p.y));

        // Bottom of hill: extend sufficiently to avoid tunneling
        float bottomY = baseY - 2f; // 2 units below lowest hill
        colliderPoints.Add(new Vector2(points[points.Count - 1].x, bottomY));
        colliderPoints.Add(new Vector2(points[0].x, bottomY));

        polygonCollider.pathCount = 1;
        polygonCollider.SetPath(0, colliderPoints.ToArray());
        polygonCollider.isTrigger = false;

        polygonCollider.enabled = false;
        polygonCollider.enabled = true;
    }

    public float GetGroundY(float x)
    {
        return baseY + Mathf.PerlinNoise(x * hillFrequency + offset, 0) * hillAmplitude;
    }
}
