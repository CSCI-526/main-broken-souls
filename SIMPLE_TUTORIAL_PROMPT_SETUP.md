# 🎯 Simple Tutorial Prompt Setup Guide

## ✅ What Was Implemented

A simple popup message that appears when players try to play the game without completing the tutorial. It shows a message and automatically closes after 3 seconds. **No buttons needed!**

---

## 🎮 How It Works

**When player clicks "Play Game" without completing tutorial:**
1. ✅ Popup appears with message: "Please complete the tutorial first!"
2. ✅ Popup fades in smoothly
3. ✅ Stays visible for 3 seconds (configurable)
4. ✅ Automatically fades out and closes
5. ✅ Player stays on main menu

**When tutorial is completed:**
- ✅ "Play Game" works normally - no popup

---

## 🛠️ Unity Setup (Quick & Simple)

### **Step 1: Create Tutorial Prompt Panel**

1. Open **StartMenu** scene
2. Right-click **Canvas** → UI → Panel
3. Rename to **"TutorialPromptPanel"**
4. Set **Position:** Center of screen
5. Set **Size:** 500x150 (or adjust to your preference)
6. Set **Color:** Semi-transparent dark background (R:0, G:0, B:0, A:200)

---

### **Step 2: Add Message Text**

1. Right-click **TutorialPromptPanel** → UI → Text - TextMeshPro
2. Rename to **"TutorialPromptText"**
3. **Position:** Center of panel
4. **Font Size:** 28-32
5. **Alignment:** Center (both horizontal and vertical)
6. **Color:** White or Yellow (for visibility)
7. **Text:** "Please complete the tutorial first!"
8. **Width:** 450 (to fit within panel)
9. **Height:** 100

---

### **Step 3: Add CanvasGroup Component**

1. Select **TutorialPromptPanel**
2. Inspector → Add Component → Canvas Group
3. This enables fade animations

---

### **Step 4: Assign References in MainMenuController**

1. Select **MainMenuController** GameObject in scene
   - (Usually attached to Canvas or a separate GameObject)
   - If you can't find it, search in Hierarchy for "MainMenuController"
2. In Inspector, find **"Tutorial Prompt Panel"** field
3. Drag **TutorialPromptPanel** from hierarchy
4. Find **"Tutorial Prompt Canvas Group"** field
5. Drag **TutorialPromptPanel** (it has the CanvasGroup component)
6. Find **"Tutorial Prompt Text"** field
7. Drag **TutorialPromptText** from hierarchy

**Optional Settings:**
- **Prompt Display Duration:** How long popup stays visible (default: 3 seconds)
  - You can change this in Inspector if you want it longer/shorter

---

### **Step 5: Test It!**

1. **Clear PlayerPrefs:** Edit → Clear All PlayerPrefs
2. **Play the game**
3. **Click "Play Game"** → Should show popup with message
4. **Wait 3 seconds** → Popup automatically closes
5. **Complete tutorial** → "Play Game" should work normally

---

## 🎨 Optional: Style the Popup

### **Make it stand out:**

1. **Add border:**
   - Add a child Image with slightly larger size
   - Set color to yellow/orange for warning
   - Position behind main panel

2. **Add background blur:**
   - Increase panel alpha to 220-240
   - Makes it more visible

3. **Change text color:**
   - Yellow or Orange for warning feel
   - White for clean look

---

## 📋 Quick Checklist

- [ ] Created TutorialPromptPanel
- [ ] Added TutorialPromptText with message
- [ ] Added CanvasGroup component to panel
- [ ] Assigned TutorialPromptPanel in MainMenuController
- [ ] Assigned TutorialPromptCanvasGroup in MainMenuController
- [ ] Assigned TutorialPromptText in MainMenuController
- [ ] Tested popup appears when clicking Play Game
- [ ] Tested popup auto-closes after 3 seconds

---

## 🎯 What Happens Now

**First-time player:**
1. Opens game → Sees main menu
2. Clicks "Play Game" → **Popup appears** ✅
3. Sees message: "Please complete the tutorial first!"
4. Popup auto-closes after 3 seconds
5. Player can click "Play Tutorial" button or try "Play Game" again

**Returning player:**
1. Opens game → Sees main menu
2. Clicks "Play Game" → **Game loads directly** (no popup) ✅

---

## ⚙️ Customization

**Change message text:**
- Edit the text in Unity Inspector on TutorialPromptText
- OR change in code: `tutorialPromptText.text = "Your custom message";`

**Change display duration:**
- In MainMenuController Inspector, find "Prompt Display Duration"
- Change from 3 to any number (e.g., 5 seconds)

**Change fade speed:**
- Already uses the same `fadeSpeed` as other UI elements
- Adjust in "Animation Settings" → "Fade Speed"

---

## 🐛 Troubleshooting

### **"Popup doesn't appear"**
- Check TutorialPromptPanel is assigned in MainMenuController
- Check panel is initially inactive (should be hidden)
- Check console for errors
- Verify tutorial completion status: Edit → Clear All PlayerPrefs

### **"Text doesn't show"**
- Check TutorialPromptText is assigned
- Check TextMeshPro is imported (Window → TextMeshPro → Import TMP Essentials)
- Check text color is visible (white/yellow on dark background)

### **"Popup doesn't close"**
- Check CanvasGroup is assigned
- Check console for coroutine errors
- Default duration is 3 seconds - wait a bit longer

### **"Popup always shows"**
- Check tutorial completion status in console
- Clear PlayerPrefs: Edit → Clear All PlayerPrefs
- Complete tutorial to mark it as done

---

## ✅ Summary

**Simple popup with:**
- ✅ No buttons needed
- ✅ Auto-closes after 3 seconds
- ✅ Smooth fade in/out animations
- ✅ Clean, minimal design
- ✅ Easy to setup

**Perfect for:** Quick, non-intrusive reminder that tutorial needs to be completed!

