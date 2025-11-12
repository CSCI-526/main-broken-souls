using UnityEngine;

/// <summary>
/// Simple component to track what killed the player for analytics
/// Attach to Player GameObject
/// </summary>
public class CauseOfDeathTracker : MonoBehaviour
{
    public static string LastCauseOfDeath { get; private set; } = "Unknown";
    
    /// <summary>
    /// Call this whenever the player takes damage or collides with something deadly
    /// </summary>
    public static void RecordCause(string cause)
    {
        LastCauseOfDeath = cause;
        Debug.Log($"[CauseOfDeath] Recorded: {cause}");
    }
    
    /// <summary>
    /// Reset for new game
    /// </summary>
    public static void Reset()
    {
        LastCauseOfDeath = "Unknown";
    }
}

