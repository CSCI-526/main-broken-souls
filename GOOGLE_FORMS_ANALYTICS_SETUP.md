# 📝 Google Forms Analytics Setup (CORS-Free!)

This method works perfectly with WebGL builds and has **NO CORS ISSUES**! ✅

---

## 🚀 Quick Setup (10 minutes)

### **Step 1: Create Google Form**

1. Go to: https://forms.google.com
2. Click **"+ Blank"** to create new form
3. **Title**: "Broken Souls Game Analytics"
4. **Description**: "Automatic analytics collection for game testing"

### **Step 2: Add Fields**

Add these **6 fields** (all as "Short answer" type):

1. **Question 1**: "Session Number"
2. **Question 2**: "Session ID"
3. **Question 3**: "Survival Time"
4. **Question 4**: "Final Score"
5. **Question 5**: "Game Speed"
6. **Question 6**: "Timestamp"

**Important**: Keep them in this exact order!

### **Step 3: Get Form ID**

1. Click the **Send** button (top right)
2. Look at the URL or click the link icon
3. Your form URL looks like:
   ```
   https://docs.google.com/forms/d/e/1FAIpQLSd_XXXXXXXXXXXXXXXXXXXXX/viewform
   ```
4. **Copy this part**: `1FAIpQLSd_XXXXXXXXXXXXXXXXXXXXX`
   (Everything between `/d/e/` and `/viewform`)

### **Step 4: Get Entry IDs**

1. **Open your form** in a new tab (the one students would fill out)
2. **Right-click anywhere** → **View Page Source**
3. **Press Ctrl+F (or Cmd+F)** and search for: `entry.`
4. You'll find lines like:
   ```html
   <input type="hidden" name="entry.123456789" ...>
   <input type="hidden" name="entry.987654321" ...>
   ```
5. **Write down the 6 entry numbers** in order (they match your questions):

```
Session Number:  entry.___________
Session ID:      entry.___________
Survival Time:   entry.___________
Final Score:     entry.___________
Game Speed:      entry.___________
Timestamp:       entry.___________
```

---

## 🎮 Unity Setup

### **Step 5: Add GoogleFormAnalytics Component**

1. **Open Unity**
2. **Open SampleScene**
3. **Select** (or create) **AnalyticsManager** GameObject
4. **Click "Add Component"**
5. Search: **"GoogleFormAnalytics"**
6. Add it

### **Step 6: Configure Inspector**

In the **GoogleFormAnalytics** component:

1. **Form ID**: Paste the ID from Step 3
2. **Session Number Entry**: Paste `entry.123456789` (your first entry)
3. **Session Id Entry**: Paste `entry.987654321` (your second entry)
4. **Survival Time Entry**: Paste `entry.111111111` (your third entry)
5. **Final Score Entry**: Paste `entry.222222222` (your fourth entry)
6. **Game Speed Entry**: Paste `entry.333333333` (your fifth entry)
7. **Timestamp Entry**: Paste `entry.444444444` (your sixth entry)
8. **Enable Analytics**: ✅ Checked

### **Step 7: Disable Old Analytics (Optional)**

If you had `SurvivalAnalytics` component:
- You can **disable it** (uncheck the checkbox)
- Or **leave it** (both will work, Google Forms will be used first)

### **Step 8: Save Scene**

Press **Cmd+S** to save!

---

## ✅ Test It!

### **In Unity Editor:**

1. **Press Play**
2. **Play the game until you die**
3. **Check Console** - should see:
   ```
   📊 Started Analytics Session #1
   ✅ Analytics submitted! Session #1, Survival: 45.23s, Score: 156
   ```

### **Check Your Google Sheet:**

1. **Open your Google Form**
2. **Click "Responses" tab** (top)
3. **Click the green spreadsheet icon** (opens linked sheet)
4. **You should see your data!**

Example:
```
Session Number | Session ID | Survival Time | Final Score | Game Speed | Timestamp
1              | abc-123... | 45.23         | 156         | 7.50       | 2025-10-24 14:30
```

---

## 🌐 Deploy to WebGL

### **Step 9: Rebuild WebGL**

1. **File → Build Settings**
2. **Platform**: WebGL
3. **Click "Build"** (or "Build and Run")
4. Choose your output folder (e.g., `Alpha-Milestone/`)

### **Step 10: Push to GitHub**

```bash
git add .
git commit -m "Switch to Google Forms analytics (CORS-free)"
git push origin main
```

### **Step 11: Test on GitHub Pages**

1. Open: `https://csci-526.github.io/main-broken-souls/Alpha-Milestone/`
2. **Play the game** and die
3. **Open Browser Console** (F12) - should see:
   ```
   📊 Started Analytics Session #1
   ✅ Analytics submitted!
   ```
4. **Check your Google Sheet** - new row should appear!
5. **NO MORE CORS ERRORS!** ✅

---

## 📊 Viewing Data

### **Real-Time Responses:**

1. **Open Google Form**
2. **Click "Responses" tab**
3. **See live data** as students play!

### **Export as Spreadsheet:**

1. Click the **green spreadsheet icon** in Responses tab
2. Opens a Google Sheet with all data
3. **Create graphs**, calculate averages, etc.
4. **Download as CSV**: File → Download → CSV

### **Example Analysis:**

```
Average Survival Time: =AVERAGE(C2:C100)
Min: =MIN(C2:C100)
Max: =MAX(C2:C100)

Create graph:
- Select columns A (Session Number) and C (Survival Time)
- Insert → Chart → Line chart
```

---

## ✅ Advantages of Google Forms Method

- ✅ **No CORS issues** - works perfectly in WebGL!
- ✅ **No coding required** for the backend
- ✅ **Real-time data** visible in Responses tab
- ✅ **Automatic spreadsheet** creation
- ✅ **Free and reliable**
- ✅ **Easy to share** data with team
- ✅ **Works on all platforms** (Unity Editor, WebGL, Standalone)

---

## 🎯 Ready for Tomorrow!

Once setup is complete:

1. ✅ Google Form created
2. ✅ Unity configured with entry IDs
3. ✅ Tested in Unity Editor
4. ✅ WebGL build deployed
5. ✅ Tested on GitHub Pages
6. ✅ Data appears in Google Sheet

**Students just play the game - data is collected automatically!** 🎉

---

## 🐛 Troubleshooting

### **Problem: No data appearing in Google Sheet**

**Solution 1**: Check entry IDs are correct
- View form source again
- Verify all 6 entry numbers match

**Solution 2**: Check Form ID is correct
- Should be the long string between `/d/e/` and `/viewform`

**Solution 3**: Check Console for errors
- Press F12 in browser
- Look for error messages

### **Problem: Console shows "not found"**

**Solution**: Make sure `GoogleFormAnalytics` component is in the scene
- Check AnalyticsManager GameObject has the component
- Verify "Enable Analytics" is checked

---

## 💡 Tips

- **Test with a few games** before giving to students
- **Share the Responses link** with your team for live monitoring
- **Keep form open** during student testing to see data in real-time
- **Download CSV** after testing for backup and analysis

---

You're all set! No more CORS issues! 🚀

