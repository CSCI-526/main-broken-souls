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
    public float speedScaleFactor = 0.2f;

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

    [Header("Power-Ups")]
    public bool hasShield = false;
    public float shieldBounceForce = 6f;

    [Header("UI")]
    public HealthBar healthBar;

    [Header("Damage")]
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isGrounded = false;
    private bool isCrouching = false;
    private bool controlsFlipped = false;

    private float groundY = -3.487f; // exact top of ground

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 3f;
        rb.freezeRotation = true;

        if (playerCollider == null)
            playerCollider = GetComponent<BoxCollider2D>();

        SetPlayerColor(false);
    }

    void Start()
    {
        moveSpeed = baseMoveSpeed;
        jumpForce = baseJumpForce;

        StartCoroutine(FlipControlsRoutine());
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
        float skinWidth = 0.01f; // tiny offset to ensure contact
        Vector2 boxCenter = (Vector2)transform.position + playerCollider.offset + Vector2.down * (playerCollider.size.y / 2 + skinWidth);
        Vector2 boxSize = new Vector2(playerCollider.size.x * 0.9f, 0.05f); // thin slice at feet
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

        // --- CROUCH ---
        isCrouching = controlsFlipped ? (rawUpHold || wHold) : (rawDownHold || sHold);

        // --- JUMP ---
        bool jumpPressed = controlsFlipped ? (rawDownPressed || sPressed) : (rawUpPressed || wPressed);
        if (isGrounded && !isCrouching && jumpPressed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

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
            playerCollider.size = Vector2.Lerp(playerCollider.size,
                                               isCrouching ? crouchSize : standSize,
                                               Time.deltaTime * crouchSmoothSpeed);
            playerCollider.offset = Vector2.Lerp(playerCollider.offset,
                                                 isCrouching ? crouchOffset : standOffset,
                                                 Time.deltaTime * crouchSmoothSpeed);
        }

        // Only scale sprite, do NOT move Y position
        float targetYScale = isCrouching ? crouchSize.y / standSize.y : 1f;
        sr.transform.localScale = new Vector3(1f, Mathf.Lerp(sr.transform.localScale.y, targetYScale, Time.deltaTime * crouchSmoothSpeed), 1f);
    }

    private IEnumerator FlipControlsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
            controlsFlipped = true;
            SetPlayerColor(true);
            Debug.Log("⚠ Controls FLIPPED");

            yield return new WaitForSeconds(10f);
            controlsFlipped = false;
            SetPlayerColor(false);
            Debug.Log("✅ Controls NORMAL");
        }
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
            FindObjectOfType<GameOverUI>().ShowGameOver();
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

    private void SetPlayerColor(bool flipped)
    {
        if (sr != null)
            sr.color = flipped ? flippedColor : normalColor;
    }

    private void OnDrawGizmos()
    {
        if (playerCollider != null)
        {
            float skinWidth = 0.01f;
            Vector2 boxCenter = (Vector2)transform.position + playerCollider.offset + Vector2.down * (playerCollider.size.y / 2 + skinWidth);
            Vector2 boxSize = new Vector2(playerCollider.size.x * 0.9f, 0.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(boxCenter, boxSize);
        }
    }
}



