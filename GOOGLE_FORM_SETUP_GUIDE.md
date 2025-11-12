# 📊 Google Form Setup for Enhanced Analytics

## ✅ What's Changed

Your new `EnhancedAnalytics.cs` now **sends data to Google Forms** just like your old analytics!

It will submit **17 fields** with all 4 new metrics.

---

## 🎯 Two Options

### **Option A: Create New Google Form** (Recommended)
- Keep your old form for comparison
- Start fresh with all 17 new fields
- Both systems can run side-by-side

### **Option B: Update Existing Form**
- Add 13 new fields to your current form
- Keep all existing data
- More complex setup

**I recommend Option A** - let's create a NEW form!

---

## 📝 Step 1: Create Google Form

1. Go to https://forms.google.com
2. Click **+ Blank form**
3. Title: "Broken Souls - Enhanced Analytics"
4. Description: "Analytics with 4 improved metrics"

---

## 📋 Step 2: Add These 17 Questions

**Copy these EXACTLY** (question text matters for finding entry IDs later):

### **Basic Info (3 questions)**

1. **Session Number**
   - Type: Short answer
   - Question: "Session Number"

2. **Session ID**
   - Type: Short answer
   - Question: "Session ID"

3. **Timestamp**
   - Type: Short answer
   - Question: "Timestamp"

---

### **METRIC 1: Normalized Progress Rate (3 questions)**

4. **Active Gameplay Time**
   - Type: Short answer
   - Question: "Active Gameplay Time"

5. **Obstacles Encountered**
   - Type: Short answer
   - Question: "Obstacles Encountered"

6. **Normalized Progress**
   - Type: Short answer
   - Question: "Normalized Progress"

---

### **METRIC 2: Obstacle Lethality (1 question)**

7. **Cause of Death**
   - Type: Short answer
   - Question: "Cause of Death"

---

### **METRIC 3: Survival Distribution (1 question)**

8. **Survival Time**
   - Type: Short answer
   - Question: "Survival Time"

---

### **METRIC 4: Session Quality (5 questions)**

9. **Longest Survival This Session**
   - Type: Short answer
   - Question: "Longest Survival This Session"

10. **Showed Improvement**
    - Type: Short answer
    - Question: "Showed Improvement"

11. **Deaths Without Improvement**
    - Type: Short answer
    - Question: "Deaths Without Improvement"

12. **Average Retry Delay**
    - Type: Short answer
    - Question: "Average Retry Delay"

13. **Quality Score**
    - Type: Short answer
    - Question: "Quality Score"

---

### **Additional Data (4 questions)**

14. **Final Score**
    - Type: Short answer
    - Question: "Final Score"

15. **Game Speed**
    - Type: Short answer
    - Question: "Game Speed"

16. **Power Ups Collected**
    - Type: Short answer
    - Question: "Power Ups Collected"

17. **Coins Collected**
    - Type: Short answer
    - Question: "Coins Collected"

---

## 🔍 Step 3: Get Entry IDs

1. Click **Send** button (top right)
2. Click the **link icon** (🔗)
3. Copy the form URL (looks like: `https://docs.google.com/forms/d/e/FORM_ID_HERE/viewform`)
4. Open that URL in a new tab
5. Right-click anywhere → **View Page Source** (or press Ctrl+U / Cmd+Option+U)
6. Press Ctrl+F / Cmd+F to search
7. Search for `"entry.` (with quotes!)

You'll see lines like:
```html
[[123456789,"Session Number",null,0,
[[987654321,"Session ID",null,0,
```

The numbers are your **entry IDs**!

---

## 📝 Step 4: Find All 17 Entry IDs

Make a note like this:

```
Session Number: entry.123456789
Session ID: entry.987654321
Timestamp: entry.444444444
Active Gameplay Time: entry.111111111
Obstacles Encountered: entry.222222222
Normalized Progress: entry.333333333
Cause of Death: entry.555555555
Survival Time: entry.666666666
Longest Survival This Session: entry.777777777
Showed Improvement: entry.888888888
Deaths Without Improvement: entry.999999999
Average Retry Delay: entry.101010101
Quality Score: entry.121212121
Final Score: entry.131313131
Game Speed: entry.141414141
Power Ups Collected: entry.151515151
Coins Collected: entry.161616161
```

**Pro tip:** Search for each question text in the source to find its entry ID!

---

## 🎮 Step 5: Configure EnhancedAnalytics in Unity

1. **Add EnhancedAnalytics to Scene:**
   - Open SampleScene
   - Create Empty GameObject → rename to "EnhancedAnalytics"
   - Add Component → EnhancedAnalytics script

2. **Fill in Form ID:**
   - Copy your form URL: `https://docs.google.com/forms/d/e/FORM_ID_HERE/viewform`
   - The `FORM_ID_HERE` part is your **formId**
   - Paste it into the inspector

3. **Fill in ALL 17 Entry IDs:**
   - Open the inspector for EnhancedAnalytics
   - You'll see sections for each metric
   - Paste your entry IDs from Step 4

**Example:**
```
Form ID: 1FAIpQLSe_EXAMPLE_FORM_ID_abc123

[Basic]
Session Number Entry: entry.123456789
Session ID Entry: entry.987654321
Timestamp Entry: entry.444444444

[Metric 1: Normalized Progress]
Active Gameplay Time Entry: entry.111111111
Obstacles Encountered Entry: entry.222222222
Normalized Progress Entry: entry.333333333

[Metric 2: Lethality]
Cause Of Death Entry: entry.555555555

[Metric 3: Distribution]
Survival Time Entry: entry.666666666

[Metric 4: Quality]
Longest Survival Entry: entry.777777777
Showed Improvement Entry: entry.888888888
Deaths Without Improvement Entry: entry.999999999
Avg Retry Delay Entry: entry.101010101
Quality Score Entry: entry.121212121

[Additional]
Final Score Entry: entry.131313131
Game Speed Entry: entry.141414141
Power Ups Collected Entry: entry.151515151
Coins Collected Entry: entry.161616161
```

4. **Enable Tracking:**
   - Check ✅ "Enable Tracking"
   - Check ✅ "Send To Google Forms"
   - Check ✅ "Debug Logs" (to see if it's working)

---

## 🧪 Step 6: Test It!

1. Play your game
2. Die once
3. Check Unity Console for: `✅ Enhanced analytics submitted to Google Form!`
4. Check your Google Form responses: https://docs.google.com/forms/d/YOUR_FORM_ID/edit#responses
5. You should see 1 response with all 17 fields filled!

---

## 📊 What Your Responses Will Look Like

| Session | Active Time | Obstacles | Progress | Cause | Survival | Quality | Score |
|---------|-------------|-----------|----------|-------|----------|---------|-------|
| 1       | 23.45s      | 15        | 0.640    | Spike | 25.30s   | 1.8     | 156   |
| 2       | 18.20s      | 10        | 0.549    | Enemy | 19.50s   | -0.4    | 98    |
| 3       | 35.60s      | 24        | 0.674    | Fall  | 38.10s   | 2.1     | 240   |

---

## 🔄 Running Both Old and New Analytics

You can keep **both systems running**:

### **Old System (GoogleFormAnalytics.cs or SurvivalAnalytics.cs)**
- Sends to your old form
- Tracks: sessionNumber, sessionId, survivalTime, finalScore, gameSpeed
- Keep it running!

### **New System (EnhancedAnalytics.cs)**
- Sends to new form
- Tracks: All 4 new metrics + old metrics
- Add tracking calls (see ENHANCED_ANALYTICS_GUIDE.md)

**Both can coexist!** You'll have:
- Old form = basic metrics (for comparison)
- New form = enhanced metrics (addresses feedback)

---

## 📈 Analyzing Your Data

### **In Google Sheets:**

1. Open your form responses
2. Click green Sheets icon (top right)
3. Creates spreadsheet with all data

### **Key Columns to Watch:**

**METRIC 1: Normalized Progress**
- `Normalized Progress` < 0.5 → Players struggling to pass obstacles
- `Normalized Progress` > 0.8 → Players finding it too easy

**METRIC 2: Lethality**
- Filter by `Cause of Death`
- Count how many deaths per type
- Calculate: Deaths / Total encounters (you'll need to track encounters manually)

**METRIC 3: Distribution**
- Sort `Survival Time` column
- Find median (middle value)
- Find 25th and 75th percentile

**METRIC 4: Quality**
- `Quality Score` < 0 → Frustrated players
- `Quality Score` > 1 → Engaged players
- `Deaths Without Improvement` > 5 → Players stuck, might quit

---

## 🐛 Troubleshooting

### **"No responses showing up"**
- Check Unity Console for error messages
- Make sure `Send To Google Forms` is checked
- Verify formId is correct (no extra spaces)
- Make sure you're connected to internet

### **"Some fields are empty"**
- Check entry IDs match exactly
- Make sure you copied the full entry ID (e.g., `entry.123456789`)
- Verify question text matches exactly

### **"Getting error about CORS"**
- This shouldn't happen with forms, but if it does:
- Make sure form URL uses `/formResponse` not `/viewform`
- Script automatically adds `/formResponse`

### **"Need to test without playing full game"**
You can manually call:
```csharp
if (EnhancedAnalytics.Instance != null)
{
    EnhancedAnalytics.Instance.OnPlayerDeath("Test", 10f, 100, 5f);
}
```

---

## ✅ Quick Checklist

- [ ] Create new Google Form with 17 questions
- [ ] Get all 17 entry IDs from source code
- [ ] Add EnhancedAnalytics GameObject to scene
- [ ] Fill in Form ID in inspector
- [ ] Fill in all 17 entry IDs
- [ ] Check "Send To Google Forms"
- [ ] Test and verify responses appear
- [ ] Add tracking calls to game scripts
- [ ] Playtest and collect data!

---

## 🎯 Summary

✅ **EnhancedAnalytics now sends to Google Forms**  
✅ **17 fields covering all 4 new metrics**  
✅ **Same setup process as your old form**  
✅ **Can run alongside old analytics**  
✅ **Data appears in Google Sheets for analysis**  

Ready to set up! Let me know if you need help with any step!

