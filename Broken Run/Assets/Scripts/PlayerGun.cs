using TMPro;
using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;                 // child of Player at the tip
    public GameObject bulletPrefab;
    public TextMeshProUGUI gunIndicatorText;

    [Header("Muzzle Visual")]
    public Color unarmedColor = new Color(0.85f, 0.85f, 0.85f);  // light grey 
    public Color armedColor = new Color(0.59f, 0.29f, 0.0f);     // brown (#964B00)

    [Header("Shoot Settings")]
    public float bulletSpeed = 28f;
    public float shootCooldown = 0.12f;

    // --- NEW: ammo system ---
    public int maxAmmo = 3;          // maximum 3 shots
    private int currentAmmo = 1;     // start with 1 bullet

    [Header("Ammo Slots (Fill sprites)")]
    public SpriteRenderer[] slotFills = new SpriteRenderer[3];


    public int coinsPerAmmo = 9;     // collect 9 coins -> +1 ammo
    private int coinBank = 0;

    public bool HasGun { get { return currentAmmo > 0; } }   // true if still have ammo
    float _lastShotTime;

    void Start()
    {
        UpdateIndicator();
        UpdateAmmoVisuals();
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
        // When gun is given, refill 2 bullets
        if(currentAmmo + 2 > 3){
            currentAmmo = maxAmmo;
        }else{
            currentAmmo = currentAmmo + 2;
        }
        
        UpdateIndicator();
        UpdateAmmoVisuals();
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

        // --- reduce ammo count ---
        currentAmmo = Mathf.Max(0, currentAmmo - 1);
        UpdateIndicator();
        UpdateAmmoVisuals();
    }

    // --- called by coin pickup ---
    public void OnCoinCollected()
    {
        coinBank++;
        if (coinBank >= coinsPerAmmo)
        {
            coinBank -= coinsPerAmmo;
            int before = currentAmmo;
            currentAmmo = Mathf.Min(maxAmmo, currentAmmo + 1);
            if (currentAmmo != before)
            {
                Debug.Log($"[PlayerGun] +1 ammo from coins. Ammo = {currentAmmo}/{maxAmmo}");
                UpdateIndicator();
                UpdateAmmoVisuals();
            }
        }
    }

    void UpdateIndicator()
    {
        if (gunIndicatorText != null)
            gunIndicatorText.text = HasGun ? $"Gun: {currentAmmo}/{maxAmmo}" : "Gun: none";
    }

    // --- NEW
    void UpdateAmmoVisuals()
    {
        if (slotFills == null) return;
        for (int i = 0; i < slotFills.Length; i++)
        {
            var sr = slotFills[i];
            if (sr == null) continue;

            sr.color = (i < currentAmmo) ? armedColor : unarmedColor;
        }
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
