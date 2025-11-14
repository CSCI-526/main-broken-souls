using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TutorialZone : MonoBehaviour
{
    [Header("Effect Settings")]
    public ZoneEffectType effect = ZoneEffectType.ReversedControls;
    public float forecastSeconds = 3f;
    public float effectDuration = 5f;

    [Header("Trigger Behavior")]
    public bool oneShot = true;
    public float reenterCooldown = 0f;
    public string playerTag = "Player";

    [Header("References")]
    public TutorialPlayerController tutorialController;

    private bool _firedOnce = false;
    private float _nextAllowedTime = 0f;

    void Awake()
    {

        if (tutorialController == null)
            tutorialController = FindObjectOfType<TutorialPlayerController>();

        if (tutorialController == null)
            Debug.LogError("[TutorialZone] didn't find TutorialPlayerController");
    }

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnValidate()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;

        if (effectDuration < 0f) effectDuration = 0f;
        if (forecastSeconds < 0f) forecastSeconds = 0f;
        if (reenterCooldown < 0f) reenterCooldown = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (tutorialController == null) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

        if (oneShot && _firedOnce) return;

        if (!oneShot && Time.time < _nextAllowedTime) return;

        if (forecastSeconds > 0f)
        {
            tutorialController.TriggerEffectWithForecast(effect, forecastSeconds, effectDuration);
        }
        else
        {
            tutorialController.TriggerEffect(effect, effectDuration);
        }

        _firedOnce = true;
        _nextAllowedTime = Time.time + reenterCooldown;
        Debug.Log($"[TutorialZone] Trigger -> effect={effect}, forecast={forecastSeconds}s, duration={effectDuration}s");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = GetGizmoColor(effect);
        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            var m = transform.localToWorldMatrix;
            var size = Vector3.Scale(box.size, transform.lossyScale);
            var center = transform.TransformPoint(box.offset);
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = old;
        }
        else if (col is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(circle.offset), circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y));
        }
        else
        {
            Gizmos.DrawLine(transform.position + Vector3.left, transform.position + Vector3.right);
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.down);
        }
    }

    private Color GetGizmoColor(ZoneEffectType e)
    {
        switch (e)
        {
            case ZoneEffectType.ReversedControls: return new Color(1f, 0.65f, 0f, 1f); 
            case ZoneEffectType.AntiGravity:      return new Color(0f, 0.8f, 1f, 1f);   
            default:                               return Color.yellow;
        }
    }
}

public enum ZoneEffectType
{
    AntiGravity,
    ReversedControls
}
