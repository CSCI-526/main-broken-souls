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
    public float speedScaleFactor = 0.2f;   // How much player speed scales with world speed

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.5f;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color flippedColor = Color.red;

    [Header("Power-Ups")]
    public bool hasShield = false;
    public float shieldBounceForce = 6f;

    [Header("UI")]
    public HealthBar healthBar;  // Reference to HealthBar

    [Header("Damage")]
    public float damageCooldown = 0.5f;   // time between damage instances
    private float lastDamageTime = -999f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isGrounded = false;
    private bool isCrouching = false;
    private bool controlsFlipped = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        SetPlayerColor(false);
    }

    void Start()
    {
        moveSpeed = baseMoveSpeed;
        jumpForce = baseJumpForce;

        StartCoroutine(FlipControlsRoutine());
        if (healthBar != null) healthBar.ResetHealth();
    }

    void Update()
    {
        // Check if player is grounded
        isGrounded = Physics2D.OverlapBox(transform.position + Vector3.down * 0.6f,
                                           new Vector2(0.5f, 0.1f), 0f, groundLayer);

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Arrow keys
        bool rawDownHold = keyboard.downArrowKey.isPressed;
        bool rawUpHold = keyboard.upArrowKey.isPressed;
        bool rawDownPressed = keyboard.downArrowKey.wasPressedThisFrame;
        bool rawUpPressed = keyboard.upArrowKey.wasPressedThisFrame;

        // WASD keys
        bool wHold = keyboard.wKey.isPressed;
        bool sHold = keyboard.sKey.isPressed;
        bool wPressed = keyboard.wKey.wasPressedThisFrame;
        bool sPressed = keyboard.sKey.wasPressedThisFrame;
        bool aHold = keyboard.aKey.isPressed;
        bool dHold = keyboard.dKey.isPressed;

        // Determine crouch and jump (supporting both arrow keys and WASD)
        isCrouching = controlsFlipped ? (rawUpHold || wHold) : (rawDownHold || sHold);
        bool jumpPressed = controlsFlipped ? (rawDownPressed || sPressed) : (rawUpPressed || wPressed);

        // Horizontal movement
        float moveInput = 0f;
        if (keyboard.leftArrowKey.isPressed || aHold) moveInput = -1f;
        if (keyboard.rightArrowKey.isPressed || dHold) moveInput = 1f;
        if (controlsFlipped) moveInput *= -1f;

        // Apply movement
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        rb.position += new Vector2(moveInput * currentSpeed * Time.deltaTime, 0f);

        // Jump
        if (isGrounded && !isCrouching && jumpPressed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + Vector3.down * 0.6f, new Vector3(0.5f, 0.1f, 0.1f));
    }
}
