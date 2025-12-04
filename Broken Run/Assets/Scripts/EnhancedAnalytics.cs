using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enhanced analytics with 4 improved metrics addressing feedback concerns
/// Sends data to Google Forms (same as your existing analytics)
/// </summary>
[System.Serializable]
public class EnhancedSessionData
{
    // Basic data
    public string sessionID;
    public int sessionNumber;
    public string timestamp;
    
    // METRIC 1: Normalized Progress Rate (addresses "distance" redundancy)
    public float activeGameplayTime;      // Excludes pauses/invincibility
    public int obstaclesEncountered;      // Count of obstacles passed
    public float normalizedProgress;      // obstacles / activeTime
    
    // METRIC 2: Obstacle Lethality (addresses "frequency" meaninglessness)
    public Dictionary<string, int> obstacleEncounters = new Dictionary<string, int>();
    public Dictionary<string, int> obstacleDeaths = new Dictionary<string, int>();
    public string causeOfDeath;           // What killed the player
    
    // METRIC 3: Distribution Stats (addresses mean skewing)
    public float survivalTime;
    // Percentiles calculated across all sessions, not per-session
    
    // METRIC 4: Session Quality (addresses retry/playtime confusion)
    public float longestSurvivalThisSession;
    public int consecutiveDeathsWithoutImprovement;
    public bool showedImprovement;
    public float averageRetryDelay;
    public float sessionQualityScore;
    
    // Additional useful data
    public int finalScore;
    public float gameSpeed;
    public int powerUpsCollected;
    public int coinsCollected;
    
    // Death state tracking
    public bool diedWithGravityFlipped;    // Was gravity flipped when player died?
    public bool diedWithControlsReversed; // Were controls reversed when player died?
}

public class EnhancedAnalytics : MonoBehaviour
{
    public static EnhancedAnalytics Instance { get; private set; }
    
    [Header("Google Form Settings")]
    [Tooltip("Get this from your Google Form URL after '/viewform?'")]
    [SerializeField] private string formId = "YOUR_FORM_ID_HERE";
    [SerializeField] private bool sendToGoogleForms = true;
    
    [Header("Form Field Entry IDs - Basic")]
    [SerializeField] private string sessionNumberEntry = "entry.123456789";
    [SerializeField] private string sessionIdEntry = "entry.987654321";
    [SerializeField] private string timestampEntry = "entry.444444444";
    
    [Header("Form Field Entry IDs - Metric 1: Normalized Progress")]
    [SerializeField] private string activeGameplayTimeEntry = "entry.111111111";
    [SerializeField] private string obstaclesEncounteredEntry = "entry.222222222";
    [SerializeField] private string normalizedProgressEntry = "entry.333333333";
    
    [Header("Form Field Entry IDs - Metric 2: Lethality")]
    [SerializeField] private string causeOfDeathEntry = "entry.555555555";
    
    [Header("Form Field Entry IDs - Metric 3: Distribution")]
    [SerializeField] private string survivalTimeEntry = "entry.666666666";
    
    [Header("Form Field Entry IDs - Metric 4: Quality")]
    [SerializeField] private string longestSurvivalEntry = "entry.777777777";
    [SerializeField] private string showedImprovementEntry = "entry.888888888";
    [SerializeField] private string deathsWithoutImprovementEntry = "entry.999999999";
    [SerializeField] private string avgRetryDelayEntry = "entry.101010101";
    [SerializeField] private string qualityScoreEntry = "entry.121212121";
    
    [Header("Form Field Entry IDs - Additional")]
    [SerializeField] private string finalScoreEntry = "entry.131313131";
    [SerializeField] private string gameSpeedEntry = "entry.141414141";
    [SerializeField] private string powerUpsCollectedEntry = "entry.151515151";
    [SerializeField] private string coinsCollectedEntry = "entry.161616161";
    
    [Header("Form Field Entry IDs - Death State")]
    [SerializeField] private string diedWithGravityFlippedEntry = "entry.171717171";
    [SerializeField] private string diedWithControlsReversedEntry = "entry.181818181";
    
    [Header("Settings")]
    [SerializeField] private bool enableTracking = true;
    [SerializeField] private bool debugLogs = true;
    
    private string baseUrl => $"https://docs.google.com/forms/d/e/{formId}/formResponse";
    
    // Current session tracking
    private EnhancedSessionData currentSession;
    private List<EnhancedSessionData> allSessions = new List<EnhancedSessionData>();
    
    // Real-time tracking
    private float sessionStartTime;
    private float lastDeathTime;
    private float totalPausedTime = 0f;
    private float totalInvincibleTime = 0f;
    private bool isPaused = false;
    private bool isInvincible = false;
    private float invincibilityStartTime;
    
    // Session performance tracking
    private float bestSurvivalInCurrentSession = 0f;
    private float lastRunSurvivalTime = 0f;
    private int deathsWithoutImprovement = 0;
    private List<float> retryDelays = new List<float>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartNewGameSession();
    }
    
    public void StartNewGameSession()
    {
        currentSession = new EnhancedSessionData
        {
            sessionID = System.Guid.NewGuid().ToString(),
            sessionNumber = allSessions.Count + 1,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        sessionStartTime = Time.time;
        totalPausedTime = 0f;
        totalInvincibleTime = 0f;
        bestSurvivalInCurrentSession = 0f;
        lastRunSurvivalTime = 0f;
        deathsWithoutImprovement = 0;
        retryDelays.Clear();
        
        if (debugLogs)
            Debug.Log($"📊 Enhanced Analytics Session #{currentSession.sessionNumber} started");
    }
    
    /// <summary>
    /// Call this when player encounters an obstacle (passes it)
    /// </summary>
    public void OnObstacleEncountered(string obstacleType)
    {
        if (!enableTracking) return;
        
        currentSession.obstaclesEncountered++;
        
        if (!currentSession.obstacleEncounters.ContainsKey(obstacleType))
        {
            currentSession.obstacleEncounters[obstacleType] = 0;
            currentSession.obstacleDeaths[obstacleType] = 0;
        }
        
        currentSession.obstacleEncounters[obstacleType]++;
        
        if (debugLogs)
            Debug.Log($"📍 Obstacle encountered: {obstacleType} (Total: {currentSession.obstaclesEncountered})");
    }
    
    /// <summary>
    /// Call this when player dies
    /// </summary>
    public void OnPlayerDeath(string causeOfDeath, float survivalTime, int finalScore, float gameSpeed, bool gravityFlipped = false, bool controlsReversed = false)
    {
        if (!enableTracking) return;
        
        // Track cause of death for lethality
        currentSession.causeOfDeath = causeOfDeath;
        if (!currentSession.obstacleDeaths.ContainsKey(causeOfDeath))
        {
            currentSession.obstacleDeaths[causeOfDeath] = 0;
        }
        currentSession.obstacleDeaths[causeOfDeath]++;
        
        // METRIC 1: Calculate normalized progress
        float activeTime = Time.time - sessionStartTime - totalPausedTime - totalInvincibleTime;
        currentSession.activeGameplayTime = Mathf.Max(0.1f, activeTime); // Avoid division by zero
        currentSession.normalizedProgress = currentSession.obstaclesEncountered / currentSession.activeGameplayTime;
        
        // METRIC 3: Survival time
        currentSession.survivalTime = survivalTime;
        
        // METRIC 4: Session quality tracking
        currentSession.longestSurvivalThisSession = Mathf.Max(bestSurvivalInCurrentSession, survivalTime);
        
        // Check if player improved
        if (survivalTime > lastRunSurvivalTime)
        {
            currentSession.showedImprovement = true;
            deathsWithoutImprovement = 0;
        }
        else
        {
            deathsWithoutImprovement++;
        }
        
        currentSession.consecutiveDeathsWithoutImprovement = deathsWithoutImprovement;
        
        // Track retry delay
        if (lastDeathTime > 0)
        {
            float retryDelay = Time.time - lastDeathTime;
            retryDelays.Add(retryDelay);
        }
        lastDeathTime = Time.time;
        
        currentSession.averageRetryDelay = retryDelays.Count > 0 ? retryDelays.Average() : 0f;
        
        // Calculate session quality score
        currentSession.sessionQualityScore = CalculateQualityScore();
        
        // Store other data
        currentSession.finalScore = finalScore;
        currentSession.gameSpeed = gameSpeed;
        
        // Track death state (gravity/controls flip)
        currentSession.diedWithGravityFlipped = gravityFlipped;
        currentSession.diedWithControlsReversed = controlsReversed;
        
        // Update best survival
        if (survivalTime > bestSurvivalInCurrentSession)
        {
            bestSurvivalInCurrentSession = survivalTime;
        }
        lastRunSurvivalTime = survivalTime;
        
        // Save and send
        SaveSession();
        LogMetrics();
    }
    
    private float CalculateQualityScore()
    {
        float score = 0f;
        
        // Positive: Showed improvement
        if (currentSession.showedImprovement)
            score += 1.0f;
        
        // Negative: Too many deaths without improvement (frustration)
        score -= currentSession.consecutiveDeathsWithoutImprovement * 0.2f;
        
        // Positive: Quick retries (engagement)
        if (currentSession.averageRetryDelay < 2f && currentSession.averageRetryDelay > 0)
            score += 0.5f;
        
        // Positive: Made progress (passed obstacles)
        if (currentSession.obstaclesEncountered > 5)
            score += 0.3f;
        
        return score;
    }
    
    /// <summary>
    /// Call when game is paused
    /// </summary>
    public void OnGamePaused()
    {
        if (!isPaused)
        {
            isPaused = true;
            sessionStartTime -= Time.deltaTime; // Will be accumulated
        }
    }
    
    /// <summary>
    /// Call when game is resumed
    /// </summary>
    public void OnGameResumed()
    {
        isPaused = false;
    }
    
    /// <summary>
    /// Call when player becomes invincible (shield, etc.)
    /// </summary>
    public void OnInvincibilityStart()
    {
        if (!isInvincible)
        {
            isInvincible = true;
            invincibilityStartTime = Time.time;
        }
    }
    
    /// <summary>
    /// Call when invincibility ends
    /// </summary>
    public void OnInvincibilityEnd()
    {
        if (isInvincible)
        {
            isInvincible = false;
            totalInvincibleTime += Time.time - invincibilityStartTime;
        }
    }
    
    /// <summary>
    /// Call when power-up is collected
    /// </summary>
    public void OnPowerUpCollected()
    {
        currentSession.powerUpsCollected++;
    }
    
    /// <summary>
    /// Call when coin is collected
    /// </summary>
    public void OnCoinCollected()
    {
        currentSession.coinsCollected++;
    }
    
    void Update()
    {
        // Track pause time
        if (isPaused)
        {
            totalPausedTime += Time.deltaTime;
        }
    }
    
    private void SaveSession()
    {
        allSessions.Add(currentSession);
        
        // Keep only last 100 sessions
        if (allSessions.Count > 100)
        {
            allSessions.RemoveAt(0);
        }
        
        // Save to PlayerPrefs (simplified - you can enhance this)
        PlayerPrefs.SetInt("TotalEnhancedSessions", allSessions.Count);
        PlayerPrefs.Save();
        
        // Send to Google Forms
        if (sendToGoogleForms)
        {
            StartCoroutine(SubmitToGoogleForm(currentSession));
        }
        
        if (debugLogs)
            Debug.Log($"💾 Session #{currentSession.sessionNumber} saved");
    }
    
    private IEnumerator SubmitToGoogleForm(EnhancedSessionData session)
    {
        // Build form data with all 4 metrics
        WWWForm form = new WWWForm();
        
        // Basic info
        form.AddField(sessionNumberEntry, session.sessionNumber.ToString());
        form.AddField(sessionIdEntry, session.sessionID);
        form.AddField(timestampEntry, session.timestamp);
        
        // METRIC 1: Normalized Progress Rate
        form.AddField(activeGameplayTimeEntry, session.activeGameplayTime.ToString("F2"));
        form.AddField(obstaclesEncounteredEntry, session.obstaclesEncountered.ToString());
        form.AddField(normalizedProgressEntry, session.normalizedProgress.ToString("F3"));
        
        // METRIC 2: Obstacle Lethality (just cause of death for now)
        form.AddField(causeOfDeathEntry, session.causeOfDeath);
        
        // METRIC 3: Distribution
        form.AddField(survivalTimeEntry, session.survivalTime.ToString("F2"));
        
        // METRIC 4: Session Quality
        form.AddField(longestSurvivalEntry, session.longestSurvivalThisSession.ToString("F2"));
        form.AddField(showedImprovementEntry, session.showedImprovement ? "1" : "0");
        form.AddField(deathsWithoutImprovementEntry, session.consecutiveDeathsWithoutImprovement.ToString());
        form.AddField(avgRetryDelayEntry, session.averageRetryDelay.ToString("F2"));
        form.AddField(qualityScoreEntry, session.sessionQualityScore.ToString("F2"));
        
        // Additional data
        form.AddField(finalScoreEntry, session.finalScore.ToString());
        form.AddField(gameSpeedEntry, session.gameSpeed.ToString("F2"));
        form.AddField(powerUpsCollectedEntry, session.powerUpsCollected.ToString());
        form.AddField(coinsCollectedEntry, session.coinsCollected.ToString());
        
        // Death state data
        form.AddField(diedWithGravityFlippedEntry, session.diedWithGravityFlipped ? "1" : "0");
        form.AddField(diedWithControlsReversedEntry, session.diedWithControlsReversed ? "1" : "0");
        
        // Submit to Google Form
        using (UnityWebRequest www = UnityWebRequest.Post(baseUrl, form))
        {
            yield return www.SendWebRequest();
            
            // Google Forms returns a redirect, so any result is success
            if (www.result != UnityWebRequest.Result.ConnectionError)
            {
                if (debugLogs)
                    Debug.Log($"✅ Enhanced analytics submitted to Google Form! Session #{session.sessionNumber}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not submit to Google Form: {www.error}");
            }
        }
    }
    
    private void LoadData()
    {
        int totalSessions = PlayerPrefs.GetInt("TotalEnhancedSessions", 0);
        if (debugLogs)
            Debug.Log($"📂 Loaded {totalSessions} previous enhanced sessions");
    }
    
    private void LogMetrics()
    {
        Debug.Log("=== 📊 ENHANCED ANALYTICS ===");
        Debug.Log($"Session #{currentSession.sessionNumber}");
        Debug.Log("");
        
        Debug.Log("METRIC 1: Normalized Progress Rate");
        Debug.Log($"  Active Gameplay Time: {currentSession.activeGameplayTime:F2}s");
        Debug.Log($"  Obstacles Encountered: {currentSession.obstaclesEncountered}");
        Debug.Log($"  Progress Rate: {currentSession.normalizedProgress:F2} obstacles/sec");
        Debug.Log("");
        
        Debug.Log("METRIC 2: Obstacle Lethality");
        Debug.Log($"  Cause of Death: {currentSession.causeOfDeath}");
        foreach (var kvp in currentSession.obstacleEncounters)
        {
            int deaths = currentSession.obstacleDeaths.ContainsKey(kvp.Key) ? currentSession.obstacleDeaths[kvp.Key] : 0;
            float lethality = deaths / (float)kvp.Value * 100f;
            Debug.Log($"  {kvp.Key}: {deaths}/{kvp.Value} deaths ({lethality:F1}% lethality)");
        }
        Debug.Log("");
        
        Debug.Log("METRIC 3: Survival Distribution");
        Debug.Log($"  Survival Time: {currentSession.survivalTime:F2}s");
        Debug.Log($"  Median (all sessions): {CalculateMedianSurvival():F2}s");
        Debug.Log($"  25th Percentile: {CalculatePercentile(25):F2}s");
        Debug.Log($"  75th Percentile: {CalculatePercentile(75):F2}s");
        Debug.Log("");
        
        Debug.Log("METRIC 4: Session Quality");
        Debug.Log($"  Best Survival This Session: {currentSession.longestSurvivalThisSession:F2}s");
        Debug.Log($"  Showed Improvement: {currentSession.showedImprovement}");
        Debug.Log($"  Deaths Without Improvement: {currentSession.consecutiveDeathsWithoutImprovement}");
        Debug.Log($"  Avg Retry Delay: {currentSession.averageRetryDelay:F2}s");
        Debug.Log($"  Quality Score: {currentSession.sessionQualityScore:F2}");
        Debug.Log("");
        
        Debug.Log($"Final Score: {currentSession.finalScore}");
        Debug.Log($"Game Speed: {currentSession.gameSpeed:F2}");
        Debug.Log($"Died with Gravity Flipped: {currentSession.diedWithGravityFlipped}");
        Debug.Log($"Died with Controls Reversed: {currentSession.diedWithControlsReversed}");
        Debug.Log("==============================");
    }
    
    // METRIC 3 helpers: Calculate percentiles
    private float CalculateMedianSurvival()
    {
        if (allSessions.Count == 0) return 0f;
        var sorted = allSessions.Select(s => s.survivalTime).OrderBy(t => t).ToList();
        return sorted[sorted.Count / 2];
    }
    
    private float CalculatePercentile(int percentile)
    {
        if (allSessions.Count == 0) return 0f;
        var sorted = allSessions.Select(s => s.survivalTime).OrderBy(t => t).ToList();
        int index = (int)(sorted.Count * (percentile / 100f));
        return sorted[Mathf.Clamp(index, 0, sorted.Count - 1)];
    }
    
    // METRIC 2: Get lethality for specific obstacle
    public float GetObstacleLethality(string obstacleType)
    {
        int totalEncounters = 0;
        int totalDeaths = 0;
        
        foreach (var session in allSessions)
        {
            if (session.obstacleEncounters.ContainsKey(obstacleType))
                totalEncounters += session.obstacleEncounters[obstacleType];
            
            if (session.obstacleDeaths.ContainsKey(obstacleType))
                totalDeaths += session.obstacleDeaths[obstacleType];
        }
        
        if (totalEncounters == 0) return 0f;
        return (float)totalDeaths / totalEncounters;
    }
    
    /// <summary>
    /// Export all 4 metrics as CSV for analysis
    /// </summary>
    public string ExportEnhancedDataAsCSV()
    {
        System.Text.StringBuilder csv = new System.Text.StringBuilder();
        
        // Headers for all 4 metrics
        csv.AppendLine("SessionNumber,Timestamp," +
                      "ActiveGameplayTime,ObstaclesEncountered,NormalizedProgress," + // Metric 1
                      "CauseOfDeath," + // Metric 2 (lethality calculated separately)
                      "SurvivalTime," + // Metric 3
                      "LongestSurvival,ShowedImprovement,DeathsWithoutImprovement,AvgRetryDelay,QualityScore," + // Metric 4
                      "FinalScore,GameSpeed");
        
        foreach (var session in allSessions)
        {
            csv.AppendLine($"{session.sessionNumber},{session.timestamp}," +
                          $"{session.activeGameplayTime:F2},{session.obstaclesEncountered},{session.normalizedProgress:F3}," +
                          $"{session.causeOfDeath}," +
                          $"{session.survivalTime:F2}," +
                          $"{session.longestSurvivalThisSession:F2},{session.showedImprovement},{session.consecutiveDeathsWithoutImprovement},{session.averageRetryDelay:F2},{session.sessionQualityScore:F2}," +
                          $"{session.finalScore},{session.gameSpeed:F2}");
        }
        
        return csv.ToString();
    }
}

