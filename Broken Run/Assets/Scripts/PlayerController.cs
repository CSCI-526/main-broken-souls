using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;
    public float minX = -1f;
    public float maxX = 1f;
    public LayerMask groundLayer;

    [Header("Base Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float baseJumpForce = 7f;
    public float speedScaleFactor = 0.1f;

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Crouch Collider Animation")]
    public BoxCollider2D playerCollider;   // Assign your BoxCollider2D
    public Vector2 standSize = new Vector2(0.5f, 1.5f);
    public Vector2 crouchSize = new Vector2(0.5f, 0.8f);
    public Vector2 standOffset = new Vector2(0f, 0f);
    public Vector2 crouchOffset = new Vector2(0f, -0.35f);
    public float crouchSmoothSpeed = 10f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color flippedColor = Color.red;
    public Color gravityFlippedColor = Color.blue;

    [Header("Power-Ups")]
    public bool hasShield = false;
    public float shieldBounceForce = 6f;

    [Header("UI")]
    public HealthBar healthBar;
    [SerializeField] private ModeUIController modeUI; // Drag your ModeUIController here

    [Header("Damage")]
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    [Header("Random Effect Timing")]
    public float minInterval = 8f;
    public float maxInterval = 14f;
    public float minEffectDuration = 3f;
    public float maxEffectDuration = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isGrounded = false;
    private bool isCrouching = false;

    private bool controlsFlipped = false;
    private bool gravityFlipped = false;

    private float groundY = -3.487f;
    private float originalGravityScale = 3f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        if (playerCollider == null)
            playerCollider = GetComponent<BoxCollider2D>();

        originalGravityScale = rb.gravityScale;
        SetPlayerColor(false);
    }

    void Start()
    {
        moveSpeed = baseMoveSpeed;
        jumpForce = baseJumpForce;

        StartCoroutine(RandomEffectRoutine()); // start random flip/gravity routine

        if (healthBar != null) healthBar.ResetHealth();

        // Snap player to exact ground at start
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, groundY, pos.z);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // --- GROUND CHECK ---
        float skinWidth = 0.01f;
        Vector2 feetDir = gravityFlipped ? Vector2.up : Vector2.down;
        Vector2 boxCenter = (Vector2)transform.position + playerCollider.offset + feetDir * (playerCollider.size.y / 2 + skinWidth);
        Vector2 boxSize = new Vector2(playerCollider.size.x * 0.9f, 0.05f);
        isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);

        // --- INPUTS ---
        bool rawDownHold = keyboard.downArrowKey.isPressed;
        bool rawUpHold = keyboard.upArrowKey.isPressed;
        bool rawDownPressed = keyboard.downArrowKey.wasPressedThisFrame;
        bool rawUpPressed = keyboard.upArrowKey.wasPressedThisFrame;

        bool wHold = keyboard.wKey.isPressed;
        bool sHold = keyboard.sKey.isPressed;
        bool wPressed = keyboard.wKey.wasPressedThisFrame;
        bool sPressed = keyboard.sKey.wasPressedThisFrame;
        bool aHold = keyboard.aKey.isPressed;
        bool dHold = keyboard.dKey.isPressed;

        // Merge W/↑ as "up", S/↓ as "down"
        bool upHold = rawUpHold || wHold;
        bool downHold = rawDownHold || sHold;
        bool upPressed = rawUpPressed || wPressed;
        bool downPressed = rawDownPressed || sPressed;

        // If exactly one of the two flips is active, invert vertical input mapping (XOR)
        bool invertVertical = controlsFlipped ^ gravityFlipped;
        bool vUpHold = invertVertical ? downHold : upHold;
        bool vDownHold = invertVertical ? upHold : downHold;
        bool vUpPressed = invertVertical ? downPressed : upPressed;
        bool vDownPressed = invertVertical ? upPressed : downPressed;

        // --- CROUCH ---
        isCrouching = vDownHold;

        // --- JUMP ---
        bool jumpPressed = vUpPressed;
        if (isGrounded && !isCrouching && jumpPressed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, gravityFlipped ? -jumpForce : jumpForce);

        // --- HORIZONTAL MOVEMENT ---
        float moveInput = 0f;
        if (keyboard.leftArrowKey.isPressed || aHold) moveInput = -1f;
        if (keyboard.rightArrowKey.isPressed || dHold) moveInput = 1f;
        if (controlsFlipped) moveInput *= -1f;

        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        rb.position += new Vector2(moveInput * currentSpeed * Time.deltaTime, 0f);

        // --- CROUCH ANIMATION ---
        HandleCrouchAnimation();
    }

    private void HandleCrouchAnimation()
    {
        if (playerCollider != null)
        {
            playerCollider.size = Vector2.Lerp(
                playerCollider.size,
                isCrouching ? crouchSize : standSize,
                Time.deltaTime * crouchSmoothSpeed
            );

            playerCollider.offset = Vector2.Lerp(
                playerCollider.offset,
                isCrouching ? crouchOffset : standOffset,
                Time.deltaTime * crouchSmoothSpeed
            );
        }

        // Only scale sprite, do NOT move Y position
        float targetYScale = isCrouching ? crouchSize.y / standSize.y : 1f;
        sr.transform.localScale = new Vector3(
            1f,
            Mathf.Lerp(sr.transform.localScale.y, targetYScale, Time.deltaTime * crouchSmoothSpeed),
            1f
        );
    }

    // ========= 2b) Minimal RandomEffectRoutine so it compiles =========
    private IEnumerator RandomEffectRoutine()
    {
        while (true)
        {
            // wait before next mode
            float wait = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(wait);

            // pick a mode (ModeType lives in its own file)
            ModeType nextMode = (Random.value < 0.5f) ? ModeType.ReversedControls : ModeType.AntiGravity;

            // 1) 3s forecast
            if (modeUI != null) modeUI.ShowForecast(nextMode, 3f);
            yield return new WaitForSeconds(3f);

            // 2) activate for duration
            float duration = Random.Range(minEffectDuration, maxEffectDuration);
            if (nextMode == ModeType.ReversedControls) SetControlFlip(true);
            else SetGravityFlip(true);

            if (modeUI != null)
            {
                modeUI.PlayWarningBanner(nextMode);
                modeUI.StartModeTimer(nextMode, duration);
            }

            yield return new WaitForSeconds(duration);

            // 3) clear
            if (nextMode == ModeType.ReversedControls) SetControlFlip(false);
            else SetGravityFlip(false);

            if (modeUI != null) modeUI.HideAll();
        }
    }

    // Toggle helpers
    private void SetControlFlip(bool on)
    {
        controlsFlipped = on;
        SetPlayerColor(on || gravityFlipped); // keep red while any effect is active
    }

    private void SetGravityFlip(bool on)
    {
        gravityFlipped = on;
        rb.gravityScale = originalGravityScale * (on ? -1f : 1f);
        SetPlayerColor(on || controlsFlipped); // keep red while any effect is active
    }

    public void TakeDamage(float amount)
    {
        if (healthBar == null) return;

        float newHealth = healthBar.healthSlider.value - amount;
        healthBar.SetHealth(newHealth);

        Debug.Log($"Player took {amount} damage. New health: {newHealth}");

        if (newHealth <= 0)
        {
            Debug.Log("💀 Player died!");
            ScoreManager.Instance.GameOver();
            #if UNITY_2022_2_OR_NEWER
            FindFirstObjectByType<GameOverUI>().ShowGameOver();
            #else
            FindObjectOfType<GameOverUI>().ShowGameOver();
            #endif

            Time.timeScale = 0f;
        }
    }

    public bool CanTakeDamage()
    {
        return Time.time - lastDamageTime >= damageCooldown;
    }

    public void RegisterDamageTime()
    {
        lastDamageTime = Time.time;
    }

    public void AdjustToWorldSpeed(float worldSpeed)
    {
        moveSpeed = baseMoveSpeed + worldSpeed * speedScaleFactor;
        jumpForce = baseJumpForce + worldSpeed * (speedScaleFactor * 0.5f);
    }

   private void SetPlayerColor(bool anyFlip)
{
    if (sr != null)
    {
        if (gravityFlipped)             // gravity flip has priority
            sr.color = gravityFlippedColor;
        else if (controlsFlipped)       // control flip
            sr.color = flippedColor;
        else
            sr.color = normalColor;     // no flip
    }
}


    private void OnDrawGizmos()
    {
        if (playerCollider != null)
        {
            float skinWidth = 0.01f;
            // visualize the feet probe toward "gravity direction"
            Vector2 feetDir = gravityFlipped ? Vector2.up : Vector2.down;
            Vector2 boxCenter = (Vector2)transform.position + playerCollider.offset + feetDir * (playerCollider.size.y / 2 + skinWidth);
            Vector2 boxSize = new Vector2(playerCollider.size.x * 0.9f, 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(boxCenter, boxSize);
        }
    }

    // Public method for other scripts to check gravity state
    public bool IsGravityFlipped()
    {
        return gravityFlipped;
    }
}
