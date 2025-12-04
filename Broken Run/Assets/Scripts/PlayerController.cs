using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

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
    public LayerMask obstacleLayer;

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
    public float shieldDuration = 10f;
    public float shieldBounceForce = 6f;

    [Header("UI")]
    public HealthBar healthBar;
    [SerializeField] public ModeUIController modeUI; // Drag your ModeUIController here
    //new
    [SerializeField] public PlayerHealthCover coverSync;
    public GameObject HBar;

    [Header("Damage")]
    public float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    [Header("Random Effect Timing")]
    public float minInterval = 8f;
    public float maxInterval = 14f;
    public float minEffectDuration = 10f;
    public float maxEffectDuration = 15f;

    [Header("UI image")]
    public Image image;

    [Header("Shield Visual")]
    public GameObject shieldVisual;

    [Header("Animation")]
    public Animator animator;                         // NEW：Animator Reference
    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isGrounded = false;
    private bool isCrouching = false;

    [Header("Fall Death")]
    public float deathY = -10f;               // set in Inspector
    public GameOverUI gameOverUI;  

    [Header("Slow Motion Power-Up")]
    public bool isSlowMoActive = false;
    public float slowMoScale = 0.5f;      // how much slower the world runs (0.5 = half speed)
    public float slowMoDuration = 10f;  

    [Header("Sprite Transform")]
    public Transform spriteTransform; 

    [Header("Trail")]   
    [SerializeField] private TrailRenderer trail;
    [SerializeField, Range(0f, 1f)] private float trailStartAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float trailEndAlpha = 0f;

    private Coroutine shieldRoutine;

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

        standSize   = playerCollider.size;
        standOffset = playerCollider.offset;

        // NEW: FIND Animator
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        originalGravityScale = rb.gravityScale;
        SetPlayerColor(false);
    }

    void Start()
    {
        sr = HBar.GetComponent<SpriteRenderer>();
        sr.color = normalColor;
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
        CheckFallDeath();
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // --- GROUND CHECK ---
        float skinWidth = 0.01f;
        Vector2 feetDir = gravityFlipped ? Vector2.up : Vector2.down;
        Vector2 boxCenter = (Vector2)transform.position + playerCollider.offset + feetDir * (playerCollider.size.y / 2 + skinWidth);
        Vector2 boxSize = new Vector2(playerCollider.size.x * 0.9f, 0.1f); // slightly taller

        bool groundedOnFloor = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
        bool groundedOnObstacle = Physics2D.OverlapBox(boxCenter, boxSize, 0f, obstacleLayer);

        isGrounded = groundedOnFloor || groundedOnObstacle;

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

        // --- UPDATE ANIMATION ---
        UpdateAnimationState();
    }

    // NEW ANIMATION UPDATE
    private void UpdateAnimationState()
    {
        if (animator == null) return;

        int state = 0; // 0 = Ground
        float vy = rb.linearVelocity.y;


        float gravityDir = -Mathf.Sign(rb.gravityScale);

        if (!isGrounded)
        {
            bool isFalling = Mathf.Sign(vy) == gravityDir && Mathf.Abs(vy) > 0.05f;
            bool isRising  = Mathf.Sign(vy) == -gravityDir && Mathf.Abs(vy) > 0.05f;

            if (isRising)
                state = 1;  // Jump
            else if (isFalling)
                state = 2;  // Fall
        }

        if (isGrounded && isCrouching)
        {
            state = 3;      // Crouch
        }

        animator.SetInteger(AnimStateHash, state);
    }

    void CheckFallDeath()
    {
        if (transform.position.y < deathY)
        {
            Die();
        }
    }

    private void HandleCrouchAnimation()
    {
        if (playerCollider != null)
        {
            // playerCollider.size = Vector2.Lerp(
            //     playerCollider.size,
            //     isCrouching ? crouchSize : standSize,
            //     Time.deltaTime * crouchSmoothSpeed
            // );

            // playerCollider.offset = Vector2.Lerp(
            //     playerCollider.offset,
            //     isCrouching ? crouchOffset : standOffset,
            //     Time.deltaTime * crouchSmoothSpeed
            // );
            Vector2 targetSize   = isCrouching ? crouchSize   : standSize;
            Vector2 targetOffset = isCrouching ? crouchOffset : standOffset;
            if (gravityFlipped)
            {
                targetOffset.y = -targetOffset.y;
            }
            playerCollider.size   = targetSize;
            playerCollider.offset = targetOffset;
        }

        // Only scale sprite, do NOT move Y position
        // float targetYScale = isCrouching ? crouchSize.y / standSize.y : 1f;
        // spriteTransform.transform.localScale = new Vector3(
        //     1f,
        //     Mathf.Lerp(spriteTransform.transform.localScale.y, targetYScale, Time.deltaTime * crouchSmoothSpeed),
        //     1f
        // );

        // if (spriteTransform != null)
        // {
        //     Vector3 targetPos = isCrouching ? new Vector3(0f, -0.4f, 0f) : Vector3.zero;
        //     spriteTransform.localPosition = Vector3.Lerp(
        //         spriteTransform.localPosition,
        //         targetPos,
        //         Time.deltaTime * crouchSmoothSpeed
        //     );
        // }
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
            if(nextMode == ModeType.ReversedControls){
                if (image != null) image.color = flippedColor;//
            }
            if(nextMode == ModeType.AntiGravity){
                if (image != null) image.color = gravityFlippedColor;
            }
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
        if (spriteTransform != null)
        {
            Vector3 s = spriteTransform.localScale;
            float baseY = Mathf.Abs(s.y);
            s.y = on ? -baseY : baseY;
            spriteTransform.localScale = s;
        }
    }

    public void TakeDamage(float amount)
    {
        if (healthBar == null) return;

        float newHealth = healthBar.healthSlider.value - amount;
        healthBar.SetHealth(newHealth);

        //new
        if (coverSync != null)
        {
            coverSync.SetHealth(newHealth);
            Debug.Log("Player took {amount} damage. New health: {newHealth}");
        }

        Debug.Log($"Player took {amount} damage. New health: {newHealth}");

        if (newHealth <= 0)
        {
            Debug.Log("💀 Player died!");
            ScoreManager.Instance.GameOver();
            
            // Stop the survival timer
            SurvivalTimer timer = FindObjectOfType<SurvivalTimer>();
            if (timer != null) timer.StopTimer();
            
            // Show game over UI (which will trigger analytics automatically)
            #if UNITY_2022_2_OR_NEWER
            FindFirstObjectByType<GameOverUI>().ShowGameOver();
            #else
            FindObjectOfType<GameOverUI>().ShowGameOver();
            #endif
            StopAllCoroutines();
            Time.timeScale = 0f;
        }
    }

    void Die()
    {
        // Stop the player so he cannot move or fall faster
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        // Disable movement script immediately
        this.enabled = false;

        // Show game over screen
        if (gameOverUI != null)
            gameOverUI.ShowGameOver();

        // Freeze game
        Time.timeScale = 0f;
    }

    public bool CanTakeDamage()
    {
        return Time.time - lastDamageTime >= damageCooldown;
    }

    public void RegisterDamageTime()
    {
        lastDamageTime = Time.time;
    }

    public void SmoothAdjustMoveSpeed(float targetSpeed, float duration)
    {
        StopCoroutine(nameof(SmoothMoveSpeedCoroutine));
        StartCoroutine(SmoothMoveSpeedCoroutine(targetSpeed, duration));
    }

    private IEnumerator SmoothMoveSpeedCoroutine(float targetSpeed, float duration)
    {
        float startSpeed = moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            moveSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsed / duration);
            yield return null;
        }

        moveSpeed = targetSpeed;
    }


    public void AdjustToWorldSpeed(float worldSpeed)
    {
        moveSpeed = baseMoveSpeed + worldSpeed * (speedScaleFactor * 0.001f);
        jumpForce = baseJumpForce + worldSpeed * (speedScaleFactor * 0.001f);
    }

    private void SetPlayerColor(bool anyFlip)
    {
        if (sr != null)
        {
            Color c;
            if (gravityFlipped)             // gravity flip has priority
                c = gravityFlippedColor;
            else if (controlsFlipped)       // control flip
                c = flippedColor;
            else
                c = normalColor;     // no flip
            
            sr.color = c;
            SetTrailColor(c);
        }
    }

    private void SetTrailColor(Color c)
    {
        if (trail == null) return;
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(c, 0f),
                new GradientColorKey(c, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(trailStartAlpha, 0f),
                new GradientAlphaKey(trailEndAlpha,   1f)
            }
        );
        trail.colorGradient = grad;
    }


    public void ActivateShield()
    {
        // If shield is already active, extend duration by 10s
        if (hasShield && shieldRoutine != null)
        {
            remainingShieldTime += 10f;
            Debug.Log($"🛡 Shield extended by 10s! Remaining: {remainingShieldTime:F1}s");
            return;
        }

        // Otherwise, activate a new shield
        hasShield = true;
        remainingShieldTime = shieldDuration;

        Debug.Log("🛡 Shield activated!");

        if (shieldVisual != null)
            shieldVisual.SetActive(true);

        shieldRoutine = StartCoroutine(ShieldTimer());
    }

    private float remainingShieldTime;

    private IEnumerator ShieldTimer()
    {
        while (remainingShieldTime > 0f)
        {
            // ❌ Removed all blink logic

            remainingShieldTime -= 0.25f;
            yield return new WaitForSeconds(0.25f);
        }

        hasShield = false;
        shieldRoutine = null;
        Debug.Log("🕒 Shield expired");

        if (shieldVisual != null)
            shieldVisual.SetActive(false);
    }

    public void DeactivateShield()
    {
        hasShield = false;

        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
            shieldRoutine = null;
        }

        remainingShieldTime = 0f;

        if (shieldVisual != null)
            shieldVisual.SetActive(false);

        Debug.Log("🧊 Shield deactivated manually!");
    }

    public void ActivateSlowMotion()
    {
        if (isSlowMoActive) return; // prevent overlapping effects
        StartCoroutine(SlowMotionRoutine());
    }

    private IEnumerator SlowMotionRoutine()
    {
        isSlowMoActive = true;
        Time.timeScale = slowMoScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("🌀 Slow Motion Activated!");

        yield return new WaitForSecondsRealtime(slowMoDuration);

        // ✅ Check if player is still alive before restoring time
        if (healthBar != null && healthBar.healthSlider.value > 0)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        isSlowMoActive = false;
        Debug.Log("⏱️ Slow Motion Ended!");
    }

    private void OnDrawGizmos()
    {
        if (playerCollider != null)
        {
            float skinWidth = 0.01f;
            // visualize the feet probe toward "gravity direction"
            Vector2 feetDir = gravityFlipped ? Vector2.up : Vector2.down;
            Vector2 boxCenter = (Vector2)transform.position + playerCollider.offset + feetDir * (playerCollider.size.y / 2 + skinWidth);
            Vector2 boxSize = new Vector2(playerCollider.size.x * 0.9f, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(boxCenter, boxSize);
        }
    }

    // Public method for other scripts to check gravity state
    public bool IsGravityFlipped()
    {
        return gravityFlipped;
    }
    
    // Public method for other scripts to check controls state
    public bool IsControlsFlipped()
    {
        return controlsFlipped;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SlowMoPowerUp"))
        {
            ActivateSlowMotion();
            Destroy(other.gameObject); // remove power-up after use
        }
    }
}
