# 📊 Survival Analytics Implementation Guide

## Overview
This analytics system tracks **Metric #1: Average Survival Time** to measure how long players survive before dying. This helps test the hypothesis about game speed and player performance.

---

## 🎯 Metric Being Tracked

**Metric #1: Average Survival Time**
- **Description:** Measures how long a player survives (in seconds) before dying in a single run
- **Visualization:** Line graph showing session number vs. average survival time
- **Purpose:** Reflects game difficulty and player skill growth

### Hypothesis
> If the game speed increases steadily over time, the average survival time will decrease, and slowing the speed increase rate by 10% will increase the average survival time by at least 15%.

---

## 🚀 Setup Instructions

### Step 1: Add SurvivalAnalytics to Your Scene

1. Open your game scene (`SampleScene`)
2. Create an empty GameObject: `GameObject → Create Empty`
3. Name it **"AnalyticsManager"**
4. Add the `SurvivalAnalytics` component to it
. In the Inspector, configure:
   - ✅ **Send To Web**: Check if you want to send to Google Sheets
   - ✅ **Save Locally**: Always keep checked (saves data locally)
   - **Max Sessions Stored**: 100 (de5fault)

### Step 2: Add Data Exporter (Optional but Recommended)

1. On the same GameObject (or create a new one)
2. Add the `AnalyticsDataExporter` component
3. This allows you to export data to CSV files

### Step 3: Add Debug UI (Optional)

1. Create a new Canvas if you don't have one
2. Add a Panel for the debug display
3. Add a TextMeshProUGUI element to show stats
4. Create an empty GameObject and add `AnalyticsDebugUI`
5. Assign the Panel and Text in the Inspector

---

## 📈 Data Collected Per Session

Each game session collects:
- **Session ID**: Unique identifier (GUID)
- **Survival Time**: How long the player survived (seconds)
- **Final Score**: Player's score when they died
- **Game Speed**: Current scroll speed when player died
- **Timestamp**: When the session occurred
- **Session Number**: Sequential number (1, 2, 3, ...)

---

## 🎮 How It Works

### Automatic Tracking
1. **Game Starts** → Analytics session starts automatically
2. **Player Dies** → Data is collected and saved
3. **Game Restarts** → New session begins

### Data Storage
- **Local Storage**: Saved in `PlayerPrefs` (persistent across game sessions)
- **Web Storage**: Optionally sent to Google Sheets for team analysis
- **CSV Export**: Can export all data to CSV file for Excel/Python analysis

---

## 🔑 Keyboard Controls

While playing the game or in Unity Editor:

| Key | Action |
|-----|--------|
| **E** | Export all data to CSV file |
| **S** | Show detailed statistics in Console |
| **Shift+C** | Clear all analytics data (careful!) |
| **F1** | Toggle debug UI (if AnalyticsDebugUI is set up) |

---

## 📊 Viewing Analytics

### Method 1: Unity Console (Quick Stats)

Press **S** while playing to see:
```
╔══════════════════════════════════════╗
║     📊 SURVIVAL ANALYTICS STATS     ║
╚══════════════════════════════════════╝
Total Sessions Played: 10
Sessions in Memory: 10

🕒 SURVIVAL TIME:
  Average: 45.23s
  Min: 12.50s
  Max: 89.30s
  Total: 452.30s

🎯 SCORE:
  Average: 156

⚡ GAME SPEED:
  Average: 7.82

📈 TREND ANALYSIS:
  ✅ Players improving! 23.5% increase
```

### Method 2: CSV Export (For Graphs)

1. Press **E** while playing
2. Check the Console for the file path
3. Open the CSV in Excel, Google Sheets, or Python
4. Create line graphs: Session Number (X) vs Survival Time (Y)

**File Location:**
- **Windows**: `C:/Users/[YourName]/AppData/LocalLow/[CompanyName]/[ProjectName]/SurvivalAnalytics.csv`
- **Mac**: `~/Library/Application Support/[CompanyName]/[ProjectName]/SurvivalAnalytics.csv`
- **WebGL**: Browser's IndexedDB (use web export instead)

### Method 3: Google Sheets (Team Collaboration)

If `sendToWeb` is enabled, data is automatically sent to your Google Sheets after each session.

---

## 📉 Creating Visualizations

### In Excel/Google Sheets:

1. Open the exported CSV file
2. Select columns: `SessionNumber` and `SurvivalTime`
3. Insert → Chart → Line Graph
4. Customize:
   - X-axis: Session Number
   - Y-axis: Survival Time (seconds)
   - Title: "Average Survival Time Over Sessions"

### In Python (Advanced):

```python
import pandas as pd
import matplotlib.pyplot as plt

# Load data
df = pd.read_csv('SurvivalAnalytics.csv')

# Plot
plt.figure(figsize=(10, 6))
plt.plot(df['SessionNumber'], df['SurvivalTime'], marker='o')
plt.xlabel('Session Number')
plt.ylabel('Survival Time (seconds)')
plt.title('Player Survival Time Trend')
plt.grid(True)
plt.show()

# Calculate average
avg_time = df['SurvivalTime'].mean()
print(f"Average Survival Time: {avg_time:.2f}s")
```

---

## 🧪 Testing Your Hypothesis

### Hypothesis Reminder:
> Slowing the speed increase rate by 10% will increase average survival time by at least 15%.

### Testing Steps:

1. **Baseline Data Collection** (10-20 sessions)
   - Play the game normally
   - Export baseline data: `baseline.csv`
   - Calculate average survival time

2. **Modify Game Speed** 
   - Open `EndlessGround.cs`
   - Find the speed increase rate
   - Reduce it by 10%
   - Example: If `speedIncreaseRate = 0.05`, change to `0.045`

3. **Test Data Collection** (10-20 sessions)
   - Clear analytics data: Press `Shift+C`
   - Play the game with new settings
   - Export test data: `test.csv`
   - Calculate new average survival time

4. **Compare Results**
   ```
   Improvement = ((New Avg - Old Avg) / Old Avg) × 100%
   
   If Improvement ≥ 15% → Hypothesis CONFIRMED ✅
   If Improvement < 15% → Hypothesis REJECTED ❌
   ```

---

## 🔧 Customization

### Changing Data Storage Limit

In Unity Inspector (SurvivalAnalytics component):
- Increase `Max Sessions Stored` if you want more historical data
- Default: 100 sessions

### Adding More Metrics

To track additional metrics, modify `SessionData` in `SurvivalAnalytics.cs`:

```csharp
[System.Serializable]
public class SessionData
{
    public string sessionID;
    public float survivalTime;
    public int finalScore;
    public float gameSpeed;
    public string timestamp;
    public int sessionNumber;
    
    // Add your new metrics here:
    public int coinsCollected;
    public int obstaclesHit;
    public float maxSpeed;
}
```

---

## 🐛 Troubleshooting

### "Analytics Not Available" Message
- **Solution**: Make sure `SurvivalAnalytics` component is in your scene
- Check that the GameObject is active

### No Data Being Saved
- **Solution**: Check `saveLocally` is enabled in Inspector
- Make sure game actually reaches Game Over (player dies)
- Check Console for error messages

### Can't Find CSV File
- **Solution**: Press `S` to show stats - file path is in the Console
- Or check: `Debug.Log(Application.persistentDataPath);`

### Data Not Sending to Google Sheets
- **Solution**: 
  - Check internet connection
  - Verify `webAppUrl` is correct
  - Check Console for network errors
  - Google Sheets script must be deployed as Web App

---

## 📝 Interpretation Guide

### What the Data Tells You:

**Longer Survival Times Over Sessions:**
- ✅ Players are learning and improving
- ⚠️ Game might be getting too easy
- 💡 Consider increasing difficulty curve

**Shorter Survival Times Over Sessions:**
- ⚠️ Game might be too hard or unfair
- ⚠️ New mechanics are confusing players
- 💡 Consider reducing difficulty or adding tutorial

**Steady Average (±5%):**
- ✅ Balanced challenge
- ✅ Good engagement curve
- ✅ Difficulty is appropriate

**High Variance (Min/Max very different):**
- Some sessions much easier/harder than others
- Random elements might be too unpredictable
- Consider more consistent obstacle generation

---

## 🎓 Best Practices

1. **Collect Enough Data**: Get at least 10-20 sessions before making conclusions
2. **Test Multiple Players**: Different skill levels give better insights
3. **Document Changes**: Note any game changes in a separate file
4. **Regular Exports**: Export CSV files regularly to prevent data loss
5. **Backup Data**: Keep copies of exported CSV files
6. **Statistical Significance**: More data = more reliable results

---

## 📚 API Reference

### SurvivalAnalytics Methods:

```csharp
// Get statistics
float GetAverageSurvivalTime()
float GetMinSurvivalTime()
float GetMaxSurvivalTime()
int GetTotalSessions()
List<SessionData> GetAllSessionData()

// Data management
string ExportDataAsCSV()
void ClearAllData()
void StartNewSession()

// Called automatically on player death
void OnPlayerDeath(int finalScore, float gameSpeed)
```

---

## 📞 Support

If you encounter issues:
1. Check the Console for error messages
2. Verify all components are properly assigned in Inspector
3. Ensure scene references are correct
4. Check that `GameOverUI` is calling analytics correctly

---

## ✅ Quick Checklist

Before starting data collection:

- [ ] `SurvivalAnalytics` component added to scene
- [ ] `AnalyticsDataExporter` component added (optional)
- [ ] Played a test game and checked Console for analytics messages
- [ ] Pressed `S` to verify statistics are being tracked
- [ ] Pressed `E` to verify CSV export works
- [ ] Can see session numbers incrementing
- [ ] Understand where CSV files are saved
- [ ] Know how to clear data (`Shift+C`)

---

## 🎉 You're Ready!

Start playing and collecting data! Remember:
- Each death saves data automatically
- Press `S` anytime to see current statistics
- Press `E` to export for visualization
- Keep playing to gather meaningful data for your hypothesis!

Good luck with your analytics! 🚀

