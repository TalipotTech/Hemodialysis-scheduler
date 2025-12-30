# 🛏️ Bed Naming Configuration Guide

## Overview
The Hemodialysis Scheduler now supports **flexible bed naming patterns** that can be configured based on hospital requirements. Choose from multiple pre-defined patterns or create your own custom format.

---

## 🚀 Quick Start

### 1. Initialize Database Configuration
Run the initialization script to add bed naming configuration to your database:

```powershell
.\initialize-bed-naming.ps1
```

### 2. Start Backend API
```powershell
cd Backend
dotnet run
```

### 3. Start Frontend
```powershell
cd Frontend\hd-scheduler-app
ng serve
```

### 4. Configure Bed Naming
Navigate to: **Admin Dashboard → System Settings → Bed Naming**

---

## 📋 Available Bed Naming Patterns

### 1. **NUMERIC** (Default)
**Format:** `1, 2, 3, 4, 5, 6, 7, 8, 9, 10`
- Simple numeric bed numbers
- Best for: Small facilities with straightforward bed identification
- **Example Preview:** 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10

### 2. **PREFIXED_NUMERIC**
**Format:** `Bed 1, Bed 2, Bed 3...` (customizable prefix)
- Numeric with text prefix
- Best for: Facilities that want clear "Bed" identification
- **Configuration Required:** Prefix text (e.g., "Bed", "Room", "Ward")
- **Example Preview:** Bed 1 | Bed 2 | Bed 3 | Bed 4 | Bed 5

### 3. **ALPHA_NUMERIC**
**Format:** `A1, A2, A3, B1, B2, B3...`
- Letter groups with numbers
- Best for: Facilities with ward-based or zone-based organization
- **Configuration Required:** Beds per group (default: 5)
- **Example Preview (5 per group):** A1 | A2 | A3 | A4 | A5 | B1 | B2 | B3 | B4 | B5

**Example with different grouping:**
- **3 per group:** A1 | A2 | A3 | B1 | B2 | B3 | C1 | C2 | C3 | D1
- **10 per group:** A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8 | A9 | A10

### 4. **ALPHABETIC**
**Format:** `A, B, C, D, E, F...`
- Letter-only bed identifiers
- Best for: Small facilities (up to 26 beds) with simple naming
- **Example Preview:** A | B | C | D | E | F | G | H | I | J

### 5. **CUSTOM**
**Format:** Define your own pattern using placeholders
- Complete flexibility with format strings
- Best for: Facilities with specific naming requirements

**Available Placeholders:**
- `{n}` - Bed number (1, 2, 3...)
- `{N}` - Zero-padded bed number (01, 02, 03...)
- `{a}` - Letter group (A, B, C...)
- `{g}` - Number within group (1-5, based on BedsPerGroup setting)

**Custom Format Examples:**
- `Ward-{n}` → Ward-1, Ward-2, Ward-3...
- `ICU-{N}` → ICU-01, ICU-02, ICU-03...
- `Room-{a}{g}` → Room-A1, Room-A2, Room-B1...
- `HD-Unit-{N}` → HD-Unit-01, HD-Unit-02...
- `{a}-Wing-{g}` → A-Wing-1, A-Wing-2, B-Wing-1...

---

## ⚙️ Configuration Steps

### Step 1: Access System Settings
1. Login as **Admin**
2. Navigate to **Admin Dashboard**
3. Click **System Settings**
4. Select the **Bed Naming** tab

### Step 2: Choose Pattern
1. Click the **Naming Pattern** dropdown
2. Select your preferred pattern from the list
3. Review the pattern description

### Step 3: Configure Pattern (if applicable)

#### For PREFIXED_NUMERIC:
- Enter your preferred prefix (e.g., "Bed", "Room", "Ward", "Unit")

#### For ALPHA_NUMERIC:
- Set **Beds Per Group** (1-10)
- Default: 5 (A1-A5, B1-B5...)

#### For CUSTOM:
- Enter your format string using placeholders
- Examples provided in the UI

### Step 4: Preview
- Check the **Live Preview** section
- Shows how the first 10 beds will be displayed
- Make adjustments if needed

### Step 5: Save
1. Click **Save Configuration**
2. Confirmation message appears
3. Changes apply immediately across the entire system

---

## 🎯 Use Cases by Hospital Type

### Small Clinic (10-20 beds)
**Recommended:** NUMERIC or ALPHABETIC
- Simple, easy to remember
- No configuration needed

### Medium Hospital (20-40 beds)
**Recommended:** ALPHA_NUMERIC
- Clear organization by zones
- Example: A1-A10 (Morning), B1-B10 (Afternoon)

### Large Hospital (40+ beds)
**Recommended:** CUSTOM with ward identification
- Example: `Ward-{a}-Bed-{N}`
- Result: Ward-A-Bed-01, Ward-A-Bed-02...

### Multi-Ward Facility
**Recommended:** CUSTOM with unit naming
- Example: `{a}-Unit-{g}`
- Result: A-Unit-1, A-Unit-2, B-Unit-1...

---

## 🔧 Technical Details

### Backend API Endpoints

#### Get Current Configuration
```http
GET /api/configuration/bed-naming
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "pattern": "ALPHA_NUMERIC",
    "prefix": "Bed",
    "bedsPerGroup": 5,
    "customFormat": "Bed {n}",
    "availablePatterns": [...],
    "previewSamples": ["A1", "A2", "A3", "A4", "A5", "B1", "B2", "B3", "B4", "B5"]
  }
}
```

#### Update Configuration
```http
PUT /api/configuration/bed-naming
Authorization: Bearer {token}
Content-Type: application/json

{
  "pattern": "ALPHA_NUMERIC",
  "prefix": "Bed",
  "bedsPerGroup": 5,
  "customFormat": "Bed {n}"
}
```

#### Preview Custom Format
```http
POST /api/configuration/bed-naming/preview
Authorization: Bearer {token}
Content-Type: application/json

{
  "pattern": "CUSTOM",
  "customFormat": "Room-{a}{g}",
  "bedsPerGroup": 5
}
```

### Database Schema

**HospitalConfiguration Table:**
```sql
CREATE TABLE HospitalConfiguration (
    ConfigID INTEGER PRIMARY KEY AUTOINCREMENT,
    ConfigKey TEXT NOT NULL UNIQUE,
    ConfigValue TEXT NOT NULL,
    Description TEXT,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now'))
);
```

**Configuration Keys:**
- `BedNamingPattern` - Selected pattern
- `BedPrefix` - Prefix for PREFIXED_NUMERIC
- `BedsPerGroup` - Grouping for ALPHA_NUMERIC
- `BedCustomFormat` - Format string for CUSTOM

---

## 🔄 Migration Impact

### What Changes?
✅ **Display Format Only** - How beds appear in the UI
✅ **All Components Updated** - Schedule grid, forms, reports

### What Stays the Same?
✅ **Database Bed Numbers** - Still stored as integers (1-10)
✅ **Existing Assignments** - All bed assignments remain intact
✅ **API Responses** - Backend still uses integer bed numbers
✅ **Queries & Reports** - No database query changes needed

### Backward Compatibility
✅ **100% Compatible** - No breaking changes
✅ **Rollback Safe** - Can switch back to NUMERIC anytime
✅ **Zero Downtime** - Change patterns without restarting

---

## 📊 Configuration Examples

### Example 1: Large Hospital with Multiple Wards
```json
{
  "pattern": "CUSTOM",
  "customFormat": "Ward-{a}-{N}",
  "bedsPerGroup": 10
}
```
**Result:** Ward-A-01, Ward-A-02, ..., Ward-A-10, Ward-B-01, Ward-B-02...

### Example 2: ICU with Zone-Based Naming
```json
{
  "pattern": "ALPHA_NUMERIC",
  "bedsPerGroup": 6
}
```
**Result:** A1, A2, A3, A4, A5, A6, B1, B2, B3, B4, B5, B6...

### Example 3: Simple Clinic
```json
{
  "pattern": "PREFIXED_NUMERIC",
  "prefix": "Room"
}
```
**Result:** Room 1, Room 2, Room 3...

---

## ⚠️ Important Notes

1. **Configuration Change Impact:**
   - Changes apply immediately
   - All users will see the new format
   - Existing bed assignments are NOT affected

2. **Capacity Limits:**
   - Numeric: Unlimited (1, 2, 3...)
   - Alphabetic: Best for ≤26 beds (A-Z)
   - Alpha-Numeric: Unlimited (A1...Z99...)
   - Custom: Depends on format

3. **Best Practices:**
   - Test pattern in preview before saving
   - Choose a pattern that matches hospital operations
   - Keep format simple and intuitive
   - Consider staff training when changing patterns

4. **Performance:**
   - Configuration is cached for 5 minutes
   - No performance impact on database queries
   - Formatting happens on display only

---

## 🐛 Troubleshooting

### Preview Not Updating
- Clear browser cache
- Refresh the page
- Check browser console for errors

### Configuration Not Saving
- Verify Admin role permissions
- Check backend API is running
- Review browser network tab for API errors

### Beds Display as Numbers
- Configuration may not be loaded
- Check if database migration ran successfully
- Verify frontend service is initialized

### Custom Format Not Working
- Verify placeholder syntax (`{n}`, `{N}`, `{a}`, `{g}`)
- Check BedsPerGroup setting
- Test format in preview first

---

## 📞 Support

For issues or questions:
1. Check the troubleshooting section
2. Review browser console for errors
3. Check backend logs
4. Verify database configuration

---

## 🎉 Summary

The bed naming configuration system provides:
- ✅ **Flexibility** - Multiple patterns to choose from
- ✅ **Simplicity** - Easy dropdown selection
- ✅ **Preview** - See changes before applying
- ✅ **Safety** - No database migration required
- ✅ **Compatibility** - Works with existing system

**Choose the pattern that best fits your hospital's needs!**
