using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TutorialZone : MonoBehaviour
{
    [Header("Effect Settings")]
    public ZoneEffectType effect = ZoneEffectType.ReversedControls; // 教程区想触发的效果
    [Tooltip("Forecast 秒数（预告）。<=0 则不展示预告")]
    public float forecastSeconds = 3f;
    [Tooltip("效果生效持续时间（秒）")]
    public float effectDuration = 5f;

    [Header("Trigger Behavior")]
    [Tooltip("是否只触发一次")]
    public bool oneShot = true;
    [Tooltip("再次进入前的冷却时间（秒），仅当 oneShot=false 时生效")]
    public float reenterCooldown = 0f;
    [Tooltip("只响应带此 Tag 的对象（通常为 Player）")]
    public string playerTag = "Player";

    [Header("References")]
    [Tooltip("不填会自动查找场景中的 TutorialPlayerController")]
    public TutorialPlayerController tutorialController;

    // 内部状态
    private bool _firedOnce = false;
    private float _nextAllowedTime = 0f;

    void Awake()
    {
        // 自动找控制器
        if (tutorialController == null)
            tutorialController = FindObjectOfType<TutorialPlayerController>();

        if (tutorialController == null)
            Debug.LogError("[TutorialZone] 未找到 TutorialPlayerController，无法触发效果。");
    }

    void Reset()
    {
        // 确保触发器
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnValidate()
    {
        // 校验触发器属性
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

        // 一次性触发保护
        if (oneShot && _firedOnce) return;

        // 冷却检查
        if (!oneShot && Time.time < _nextAllowedTime) return;

        // 触发
        if (forecastSeconds > 0f)
        {
            // 带预告版本
            tutorialController.TriggerEffectWithForecast(effect, forecastSeconds, effectDuration);
        }
        else
        {
            // 无预告，直接生效
            tutorialController.TriggerEffect(effect, effectDuration);
        }

        _firedOnce = true;
        _nextAllowedTime = Time.time + reenterCooldown;

        // 调试日志
        Debug.Log($"[TutorialZone] Trigger -> effect={effect}, forecast={forecastSeconds}s, duration={effectDuration}s");
    }

    // 可视化区域（编辑器里好看）
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
            // 其他碰撞器简单画个十字
            Gizmos.DrawLine(transform.position + Vector3.left, transform.position + Vector3.right);
            Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.down);
        }
    }

    private Color GetGizmoColor(ZoneEffectType e)
    {
        switch (e)
        {
            case ZoneEffectType.ReversedControls: return new Color(1f, 0.65f, 0f, 1f); // 橙
            case ZoneEffectType.AntiGravity:      return new Color(0f, 0.8f, 1f, 1f);   // 青
            default:                               return Color.yellow;
        }
    }
}


// 与教程控制器一致的枚举
public enum ZoneEffectType
{
    AntiGravity,
    ReversedControls
}
