# Discharged Patients History - Quick Reference

## Access
**Patient List → Discharged History Tab**

## Features at a Glance

### 📋 What You'll See
- Only patients who have completed dialysis sessions
- Expansion panels with patient summary
- Equipment usage and purchase tracking
- Quick access to full treatment history

### 🔍 Search & Filter
- Search by patient name or ID
- Real-time filtering
- Clear visual indicators

### 📊 Equipment Information Display

Each patient shows:
```
┌─────────────────────────────────────────┐
│ DIALYSER USAGE                          │
│ Current Count: X / 7                    │
│ Total Purchased: Y dialysers            │
│ [====Progress Bar====]                  │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ BLOOD TUBING USAGE                      │
│ Current Count: X / 12                   │
│ Total Purchased: Y sets                 │
│ [====Progress Bar====]                  │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ TOTAL SESSIONS                          │
│ X dialysis sessions completed           │
└─────────────────────────────────────────┘
```

### 📄 Patient Information
- Name and Patient ID
- Age and gender
- Dry weight
- HD cycle and start date
- Dialyser type

### 🔗 Actions
**View Full Treatment History** → Navigate to complete treatment log

---

## How It Works

### Auto-Tracking Logic
```
When dialyser count reaches 7:
  ✓ Reset count to 1
  ✓ Increment DialysersPurchased by 1

When blood tubing count reaches 12:
  ✓ Reset count to 1
  ✓ Increment BloodTubingPurchased by 1
```

### Background Process
- Runs every 5 minutes
- Checks for completed sessions
- Auto-discharges patients
- Increments equipment counts
- Updates purchase counters

---

## Visual Guide

### Tab Navigation
```
┌────────────────┬───────────────────┐
│ Active Patients│ Discharged History│
└────────────────┴───────────────────┘
```

### Expansion Panel (Collapsed)
```
┌─────────────────────────────────────────────────────┐
│ 👤 John Doe (ID: P001) • Age 45 • Male            ▼│
└─────────────────────────────────────────────────────┘
```

### Expansion Panel (Expanded)
```
┌─────────────────────────────────────────────────────┐
│ 👤 John Doe (ID: P001) • Age 45 • Male            ▲│
├─────────────────────────────────────────────────────┤
│ ℹ️  PATIENT INFORMATION                             │
│   Age: 45 | Gender: Male | Dry Weight: 70.5 kg    │
│   HD Cycle: 3x/week | Started: 2024-01-15         │
│   Dialyser Type: High-Flux                         │
│                                                     │
│ 🔧 EQUIPMENT SUMMARY                                │
│   ┌───────────┬───────────────┬───────────┐       │
│   │ 🩺 DIALYSER│ 🧪 BLOOD TUBING│ 📊 SESSIONS│      │
│   │ 5 / 7      │ 8 / 12        │ 156        │      │
│   │ (12 bought)│ (13 bought)   │ completed  │      │
│   └───────────┴───────────────┴───────────┘       │
│                                                     │
│   [View Full Treatment History]                    │
└─────────────────────────────────────────────────────┘
```

---

## Use Cases

### 📈 Equipment Audit
Track total equipment purchased per patient for:
- Cost analysis
- Inventory management
- Patient-specific usage patterns

### 📊 Treatment Review
Quick access to:
- Treatment duration
- Equipment consumption rates
- Session completion statistics

### 🔍 Historical Data
View discharged patient records:
- Past treatment details
- Equipment usage history
- Complete medical timeline

---

## Color Coding

🔵 **Dialyser** - Blue gradient  
🟣 **Blood Tubing** - Purple gradient  
🟢 **Sessions** - Green gradient  

---

## Keyboard Shortcuts
(If implemented in future)
- `Ctrl + F`: Focus search
- `Enter`: View full history for selected patient
- `Escape`: Collapse all panels

---

## Tips

✅ **Best Practices:**
- Check discharged history regularly for audits
- Use search to quickly find specific patients
- Review equipment purchase trends monthly

⚠️ **Important Notes:**
- Purchase counters are lifetime values (never reset)
- Only patients with completed sessions appear
- Active patients remain in the "Active Patients" tab

---

## Troubleshooting

**No patients showing?**
- Verify patients have completed dialysis sessions
- Check `totalDialysisCompleted > 0` in database

**Equipment counts seem wrong?**
- Verify background service is running
- Check `SessionCompletionService` logs
- Review database patient records

**Search not working?**
- Clear search term and try again
- Check for typos in patient name/ID
- Verify patient exists in discharged list

---

**For more details, see:** `EQUIPMENT_PURCHASE_TRACKING_COMPLETE.md`
