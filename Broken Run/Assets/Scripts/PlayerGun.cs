using TMPro;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;                 // child of Player at the tip
    public GameObject bulletPrefab;
    public TextMeshProUGUI gunIndicatorText;

    [Header("Muzzle Visual")]
    public SpriteRenderer muzzleSprite;      // small square/rect on the muzzle
    public Color unarmedColor = new Color(0.85f, 0.85f, 0.85f);  // light grey
    public Color armedColor = new Color(0.59f, 0.29f, 0.0f);   // brown (#964B00)

    [Header("Shoot Settings")]
    public float bulletSpeed = 28f;
    public float shootCooldown = 0.12f;

    public bool HasGun { get; private set; }
    float _lastShotTime;

    void Start()
    {
        UpdateIndicator();
        UpdateMuzzleColor();
    }

    void Update()
    {
        // Space fires
        if (HasGun && Input.GetKeyDown(KeyCode.Space) && Time.time >= _lastShotTime + shootCooldown)
        {
            Shoot();
        }
    }

    public void GiveGun()
    {
        HasGun = true;
        UpdateIndicator();
        UpdateMuzzleColor();
    }

    public void Shoot()
    {
        if (!HasGun || bulletPrefab == null) return;

        // Find a safe spawn point (muzzle if assigned, else player + small offset)
        Vector3 spawnPos;
        if (muzzle != null)
        {
            spawnPos = muzzle.position;
        }
        else
        {
            // tiny nudge to the right of the player so it’s not inside the collider
            spawnPos = transform.position + new Vector3(0.8f, 0f, 0f);
            Debug.LogWarning("[PlayerGun] Muzzle not set; spawning from player.");
        }

        GameObject b = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // Push right
        var rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.right * bulletSpeed;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        _lastShotTime = Time.time;

        // One-use gun
        HasGun = false;
        UpdateIndicator();
        UpdateMuzzleColor();
    }

    void UpdateIndicator()
    {
        if (gunIndicatorText != null)
            gunIndicatorText.text = HasGun ? "Gun: READY" : "Gun: none";
    }

    void UpdateMuzzleColor()
    {
        if (muzzleSprite != null)
            muzzleSprite.color = HasGun ? armedColor : unarmedColor;
    }

#if UNITY_EDITOR
    // Helpful gizmo to see where the muzzle is in the scene
    void OnDrawGizmosSelected()
    {
        if (muzzle != null)
        {
            Gizmos.color = Color.brown;
            Gizmos.DrawSphere(muzzle.position, 0.08f);
        }
    }
#endif
}
