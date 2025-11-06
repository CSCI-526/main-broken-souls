using UnityEngine;

/// On each call to TryConsumeMilestone, returns true ONCE when the
/// current score has crossed the next milestone.
public class ScoreReader : MonoBehaviour
{
    [Tooltip("First milestone and the step size (e.g., 300).")]
    public int milestoneStep = 300;

    private int _nextMilestone;

    public static ScoreReader Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        ResetMilestones();
    }

    /// Reset to the next 500 boundary (usually 300).
    public void ResetMilestones()
    {
        int current = GetScore();
        // Find the next multiple of milestoneStep strictly greater than current
        _nextMilestone = ((current / milestoneStep) + 1) * milestoneStep;
    }

    /// Called every frame by PowerUpSpawner.Update()
    /// Returns true ONLY ONCE when score crosses the next multiple of 'milestoneStep'.
    public bool TryConsumeMilestone(out int atScore)
    {
        atScore = 0;

        int current = GetScore();
        if (current >= _nextMilestone && _nextMilestone > 0)
        {
            atScore = _nextMilestone;
            _nextMilestone += milestoneStep;   // advance to the following milestone
            return true;
        }
        return false;
    }

    private int GetScore()
    {
        // Uses your ScoreManager. If ScoreManager isn’t present yet, return 0.
        return (ScoreManager.Instance != null) ? ScoreManager.Instance.GetFinalScore() : 0;
    }
}
