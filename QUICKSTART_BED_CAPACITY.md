# Quick Setup: Bed Capacity Management

## 🚀 Quick Start (3 Steps)

### Step 1: Initialize Database
```powershell
.\initialize-slots.ps1
```

### Step 2: Start Backend
```powershell
cd Backend
dotnet run
```

### Step 3: Start Frontend
```powershell
cd Frontend\hd-scheduler-app
npm start
```

## 📍 Access the Feature

Navigate to: **http://localhost:4200/admin/system-settings**

Login with admin credentials, then select the **"Bed Capacity"** tab.

## ✨ What You Can Do

### View Capacity
- See all 4 time slots (Morning, Afternoon, Evening, Night)
- Check current usage and availability
- Monitor occupancy rates

### Edit Capacity
1. Click **"Edit Capacity"** on any slot
2. Enter new bed count (1-100)
3. Click **"Save"**

### Rules
- ✅ Capacity range: 1-100 beds
- ❌ Cannot reduce below current usage
- ℹ️ Changes apply immediately

## 📊 Default Configuration

| Slot | Time | Default Beds |
|------|------|--------------|
| Morning | 06:00 - 10:00 | 10 |
| Afternoon | 11:00 - 15:00 | 10 |
| Evening | 16:00 - 20:00 | 10 |
| Night | 21:00 - 01:00 | 10 |

**Total**: 40 beds

## 🔧 Troubleshooting

### "Cannot reduce capacity below current usage"
**Fix**: Wait for beds to be freed up or discharge patients first

### "401 Unauthorized"
**Fix**: Login with admin account

### Changes not showing
**Fix**: Refresh browser (Ctrl+F5)

---

For detailed documentation, see **BED_CAPACITY_MANAGEMENT.md**
