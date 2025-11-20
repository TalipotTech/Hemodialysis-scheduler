# Equipment Usage Alert - Visual Guide

## How It Appears in the Application

### Location
The equipment usage alerts appear in the **HD Session Schedule** form, right after you enter the Dialyser and Blood Tubing reuse numbers in **Step 2: Session Details**.

---

## Alert Examples

### 1. ✅ Normal Usage (Green Status)
```
┌─────────────────────────────────────────────────────┐
│ ✓ Dialyser Usage                          [  OK  ]  │
│                                                      │
│ 2 / 7 uses                                          │
│ 5 use(s) remaining                                  │
│                                                      │
│ [████████░░░░░░░░] 29% used                         │
│                                                      │
│ ✓ Dialyser usage is normal (2/7).                  │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ ✓ Blood Tubing Usage                      [  OK  ]  │
│                                                      │
│ 3 / 12 uses                                         │
│ 9 use(s) remaining                                  │
│                                                      │
│ [████░░░░░░░░░░░░] 25% used                         │
│                                                      │
│ ✓ Blood Tubing usage is normal (3/12).             │
└─────────────────────────────────────────────────────┘
```

---

### 2. ⚠️ Warning Level (Yellow Status)
```
┌─────────────────────────────────────────────────────┐
│ ℹ️ Dialyser Usage                      [ Warning ]  │
│                                                      │
│ 5 / 7 uses                                          │
│ 2 use(s) remaining                                  │
│                                                      │
│ [████████████████░░] 71% used                       │
│                                                      │
│ ⚠️ NOTICE: Dialyser usage at 5/7.                   │
│ 2 use(s) remaining.                                 │
└─────────────────────────────────────────────────────┘
```

---

### 3. 🔶 Critical Level (Orange Status with Pulse)
```
┌─────────────────────────────────────────────────────┐
│ ⚠️ Dialyser Usage                      [ Critical ] │
│                 ⚠️ PULSING CARD ⚠️                   │
│ 6 / 7 uses                                          │
│ 1 use(s) remaining                                  │
│                                                      │
│ [██████████████████░] 86% used                      │
│                                                      │
│ ⚠️ WARNING: Dialyser is nearing maximum usage.      │
│ Current: 6/7. 1 use(s) remaining.                   │
│ Please prepare replacement.                         │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ ⚠️ Blood Tubing Usage                  [ Critical ] │
│                 ⚠️ PULSING CARD ⚠️                   │
│ 10 / 12 uses                                        │
│ 2 use(s) remaining                                  │
│                                                      │
│ [████████████████░░] 83% used                       │
│                                                      │
│ ⚠️ WARNING: Blood Tubing is nearing maximum usage.  │
│ Current: 10/12. 2 use(s) remaining.                 │
│ Please prepare replacement.                         │
└─────────────────────────────────────────────────────┘
```

---

### 4. 🔴 EXPIRED (Red Status with Blinking Animation)
```
┌─────────────────────────────────────────────────────┐
│ ⛔ Dialyser Usage                      [ EXPIRED ]  │
│           🚨 BLINKING RED CARD 🚨                   │
│ 7 / 7 uses                                          │
│ 0 use(s) remaining                                  │
│                                                      │
│ [████████████████████] 100% used                    │
│                                                      │
│ ⚠️ CRITICAL: Dialyser has reached maximum usage     │
│ limit (7 times). MUST be replaced before next      │
│ session!                                            │
│                                                      │
│ ┌───────────────────────────────────────────────┐  │
│ │ [!] REPLACEMENT REQUIRED                      │  │
│ │ Please inform patient to bring new dialyser   │  │
│ └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│ ⛔ Blood Tubing Usage                  [ EXPIRED ]  │
│           🚨 BLINKING RED CARD 🚨                   │
│ 12 / 12 uses                                        │
│ 0 use(s) remaining                                  │
│                                                      │
│ [████████████████████] 100% used                    │
│                                                      │
│ ⚠️ CRITICAL: Blood Tubing has reached maximum       │
│ usage limit (12 times). MUST be replaced before    │
│ next session!                                       │
│                                                      │
│ ┌───────────────────────────────────────────────┐  │
│ │ [!] REPLACEMENT REQUIRED                      │  │
│ │ Please inform patient to bring new tubing     │  │
│ └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

---

### 5. ⛔ DANGER - Exceeded Limit (Red with Fast Blinking)
```
┌─────────────────────────────────────────────────────┐
│ ⛔ Dialyser Usage                      [ EXPIRED ]  │
│       ⚠️⚠️⚠️ FAST BLINKING RED ⚠️⚠️⚠️                │
│ 8 / 7 uses ❌ OVER LIMIT!                           │
│ -1 use(s) remaining                                 │
│                                                      │
│ [████████████████████] 114% used ⚠️                 │
│                                                      │
│ ⛔ DANGER: Dialyser has EXCEEDED maximum usage      │
│ limit! Current: 8, Max: 7. Replace immediately!    │
│                                                      │
│ ┌───────────────────────────────────────────────┐  │
│ │ [!] REPLACEMENT REQUIRED                      │  │
│ │ DO NOT PROCEED - UNSAFE EQUIPMENT             │  │
│ └───────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

---

## Color Scheme

| Status | Background | Border | Progress Bar | Text Color |
|--------|------------|--------|--------------|------------|
| **OK** | White | Green (4px left) | Green | Black |
| **Warning** | Light Yellow (#fff3e0) | Orange (4px left) | Orange | Black |
| **Critical** | Light Red (#ffebee) | Red (4px left) | Red | Black |
| **Expired** | Light Red (#ffebee) | Red (4px left) | Red | Black |

---

## Animation Effects

### 1. Pulse Effect (Critical Status)
```
Card shadow pulses outward:
0%   → No shadow
50%  → 10px red shadow
100% → No shadow
Repeat every 2 seconds
```

### 2. Blink Effect (Expired Status)
```
Replacement warning banner blinks:
0%   → Full opacity
25%  → 70% opacity
50%  → Full opacity
75%  → 70% opacity  
100% → Full opacity
Repeat every 1.5 seconds
```

---

## Icons Used

| Status | Icon | Material Icon Name |
|--------|------|-------------------|
| OK | ✓ | check_circle |
| Warning | ℹ️ | info |
| Critical | ⚠️ | warning |
| Expired | ⛔ | error |
| Replacement | [!] | priority_high |

---

## User Interaction Flow

```
1. Nurse/Doctor opens HD Session form
         ↓
2. Enters patient basic information (Step 1)
         ↓
3. Proceeds to Session Details (Step 2)
         ↓
4. Enters treatment details:
   - Start Time
   - Pre-Weight
   - Pre-BP
   - Pre-Temperature
   - 📝 Dialyser Reuse Number ← HERE
   - 📝 Blood Tubing Reuse Number ← HERE
         ↓
5. 🎯 ALERT COMPONENT ACTIVATES INSTANTLY
   - Shows current usage for both equipment
   - Displays color-coded status
   - Shows progress bar
   - Displays warning messages
         ↓
6. Staff reads alert:
   
   IF GREEN (OK):
   ✓ Continue with session normally
   
   IF YELLOW (Warning):
   ⚠️ Make note to prepare replacement
   ⚠️ Continue with current session
   ⚠️ Plan for next session
   
   IF ORANGE (Critical):
   ⚠️ Alert patient during session
   ⚠️ Ensure new equipment for next time
   ⚠️ Document in notes
   
   IF RED (Expired):
   ⛔ STOP - Do not proceed!
   📞 Contact patient immediately
   📅 Reschedule if needed
   📝 Document incident
         ↓
7. Complete remaining form steps
         ↓
8. Submit session
         ↓
9. Alert logged to database for audit
```

---

## Responsive Behavior

### Desktop (> 768px)
```
┌─────────────────────────────────────────┐
│ [Icon] Equipment  [Chip]                │
│ Usage info side-by-side                 │
│ Full width progress bar                 │
│ Complete message text                   │
└─────────────────────────────────────────┘
```

### Mobile (< 768px)
```
┌───────────────────┐
│ [Icon] Equipment  │
│ [Chip]           │
│                   │
│ Usage info        │
│ stacked           │
│                   │
│ Full width bar    │
│                   │
│ Wrapped message   │
└───────────────────┘
```

---

## Example Session Flow

### Scenario: Patient with Dialyser at 6 uses

```
Step-by-Step:

1. Open session form for patient "John Doe"
   
2. Fill prescription details
   
3. Enter "6" in Dialyser Reuse Number
   
4. 🔶 ORANGE ALERT APPEARS:
   ┌────────────────────────────────────────┐
   │ ⚠️ Dialyser Usage         [ Critical ] │
   │           PULSING CARD                 │
   │ 6 / 7 uses                             │
   │ 1 use(s) remaining                     │
   │ [██████████████████░] 86% used         │
   │                                        │
   │ ⚠️ WARNING: Dialyser is nearing       │
   │ maximum usage. Current: 6/7.           │
   │ 1 use(s) remaining.                    │
   │ Please prepare replacement.            │
   └────────────────────────────────────────┘
   
5. Nurse takes action:
   ✓ Reads the warning
   ✓ Makes note: "Patient to bring new dialyser next visit"
   ✓ Verbally informs patient during session
   ✓ Documents in patient notes
   ✓ Continues with current session (safe - still 1 use left)
   
6. For next session:
   - New dialyser count will be "1"  
   - System shows green (OK) status
   - Patient brings new equipment
```

---

## Medical Staff Training Guide

### Quick Reference Card
```
╔═══════════════════════════════════════════════════╗
║   EQUIPMENT USAGE ALERT QUICK REFERENCE           ║
╠═══════════════════════════════════════════════════╣
║                                                    ║
║  ✓ GREEN (OK)         → Proceed normally          ║
║                                                    ║
║  ⚠️ YELLOW (Warning)  → Prepare replacement       ║
║                                                    ║
║  🔶 ORANGE (Critical) → Alert patient             ║
║                          Replacement needed soon  ║
║                                                    ║
║  🔴 RED (Expired)     → STOP! Do not proceed      ║
║                          Contact patient now      ║
║                          Reschedule if needed     ║
║                                                    ║
╚═══════════════════════════════════════════════════╝
```

### What to Tell Patients

**For Warning/Critical Status:**
> "Your dialyser has been used [X] times out of a maximum of 7. 
> For your safety, please bring a new dialyser to your next session."

**For Expired Status:**
> "Your dialyser has reached its maximum safe usage limit. 
> We cannot proceed with today's session for your safety. 
> Please obtain a new dialyser and we will reschedule your treatment."

---

## System Integration Points

### Where Alerts Appear:
1. ✅ HD Session Creation Form (Step 2)
2. ✅ HD Session Edit Form (Step 2)
3. 🔜 Patient Dashboard (Summary view)
4. 🔜 Daily Schedule View (Inline alerts)
5. 🔜 Patient History (Equipment timeline)

### API Integration:
- Alerts auto-generated on session save
- Historical alerts stored in database
- Staff can acknowledge alerts
- Reports can query alert history

---

## Accessibility Features

- **Color + Text**: Not relying on color alone
- **Icon + Message**: Multiple indicators
- **High Contrast**: Clear visual distinction
- **Animation**: Attention-grabbing for critical items
- **Tooltip**: Additional info on hover
- **Screen Reader**: All text is readable
- **Keyboard Navigation**: Fully accessible

---

**This visual guide helps medical staff quickly understand and respond to equipment usage alerts!**

🏥 **Patient Safety First!** 🏥
