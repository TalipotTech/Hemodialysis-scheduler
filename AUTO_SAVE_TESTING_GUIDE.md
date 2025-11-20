# Auto-Save Feature - Quick Testing Guide

## How to Test the New LocalStorage Auto-Save

### Prerequisites
- ✅ Backend running on http://localhost:5001
- ✅ Frontend running on http://localhost:4200
- Browser: Chrome, Edge, or Firefox (with localStorage enabled)

---

## Test Scenario 1: Basic Auto-Save

### Steps
1. Open browser to http://localhost:4200
2. Login as admin or nurse
3. Navigate to **Patients** list
4. Click on any patient (e.g., Vidhya)
5. Click **"Schedule HD Session"** button
6. Start filling the form:
   - Select **Treatment Date**
   - Select **Slot** (e.g., Slot 2 - Afternoon)
   - Bed will auto-assign (e.g., Bed 1)
   - Enter **Dry Weight**: 45 kg
   - Select **Access Type**: AVF
   - Enter **Prescribed Duration**: 4 hours
   - Select **Dialyser Type**: LO
   - Enter **Pre-Weight**: 45 kg

### Expected Results
✅ After 2 seconds of inactivity, top-right corner shows:
   - Orange badge: "Saving draft..." (with spinner)
   - Then green badge: "✓ Saved HH:MM" (with checkmark)
   - Badge fades after 3 seconds

---

## Test Scenario 2: Draft Restoration

### Steps
1. Continue from Test 1 - form should have partial data
2. **Don't submit the form** - just close the tab or navigate away
3. Return to the same patient
4. Click **"Schedule HD Session"** again

### Expected Results
✅ At top of page, Material Snackbar appears:
   ```
   Found unsaved draft from 12/20/2024, 10:45 AM. Restore it?
   [Restore] [×]
   ```

✅ Click **"Restore"**:
   - All form fields populate with saved data
   - Bed assignment restored
   - Slot selection restored
   - Success message: "Draft restored successfully"

---

## Test Scenario 3: Clear Draft

### Steps
1. Follow Test 1 to create a draft
2. Navigate away without submitting
3. Return to form (draft prompt should appear)
4. **Dismiss** the first prompt (click X or wait 10 seconds)
5. Wait 0.5 seconds

### Expected Results
✅ Second Snackbar appears:
   ```
   Draft not restored. Clear it?
   [Clear] [×]
   ```

✅ Click **"Clear"**:
   - Confirmation: "Draft cleared"
   - Form remains empty
   - No draft on next visit

---

## Test Scenario 4: Draft Cleanup After Save

### Steps
1. Create a draft (follow Test 1)
2. Complete all required fields:
   - Treatment Date ✓
   - Slot ID ✓
   - Bed Number ✓
   - Dry Weight ✓
   - HD Start Date ✓
   - Access Type ✓
   - Prescribed Duration ✓
   - Dialyser Type ✓
   - Pre-Weight ✓
3. Click **"Schedule Session"** button
4. Wait for success message
5. Return to same patient
6. Click **"Schedule HD Session"** again

### Expected Results
✅ No draft restoration prompt appears
✅ Form is empty (clean slate)
✅ Draft was automatically cleared after successful save

---

## Test Scenario 5: Multiple Patients

### Steps
1. Create draft for Patient 1 (e.g., Vidhya)
   - Fill some fields
   - Navigate away
2. Create draft for Patient 2 (e.g., different patient)
   - Fill different data
   - Navigate away
3. Return to Patient 1's form
4. Return to Patient 2's form

### Expected Results
✅ Each patient has their own independent draft
✅ Patient 1 draft contains their specific data
✅ Patient 2 draft contains their specific data
✅ No cross-contamination between patients

---

## Test Scenario 6: 24-Hour Expiry

### Steps (Manual - Requires Time Manipulation)
1. Create a draft
2. Use browser DevTools to modify draft timestamp:
   ```javascript
   // Press F12, go to Console
   let key = Object.keys(localStorage).find(k => k.includes('hd_session_draft'));
   let draft = JSON.parse(localStorage.getItem(key));
   draft.timestamp = new Date(Date.now() - 25 * 60 * 60 * 1000).toISOString(); // 25 hours ago
   localStorage.setItem(key, JSON.stringify(draft));
   ```
3. Refresh page
4. Navigate to form

### Expected Results
✅ No draft restoration prompt appears
✅ Old draft automatically removed

---

## Test Scenario 7: Browser DevTools Inspection

### Steps
1. Create a draft (follow Test 1)
2. Press **F12** to open DevTools
3. Go to **Application** tab (Chrome/Edge) or **Storage** tab (Firefox)
4. Expand **Local Storage** → http://localhost:4200
5. Look for key: `hd_session_draft_123` (where 123 is patient ID)

### Expected Results
✅ Key exists in localStorage
✅ Value is valid JSON with structure:
   ```json
   {
     "formData": {...},
     "timestamp": "2024-12-20T...",
     "patientId": 123,
     "bedAssignmentMode": "auto",
     "autoAssignedBed": 1,
     "selectedBed": null,
     "selectedSlot": 2
   }
   ```

---

## Test Scenario 8: Responsive Design

### Steps
1. Create a draft on desktop
2. Resize browser window to mobile size (≤480px)
3. Observe auto-save indicator

### Expected Results
✅ Desktop (>768px): Indicator in top-right corner
✅ Tablet (≤768px): Indicator aligns to right, smaller font
✅ Mobile (≤480px): Full-width indicator, centered

---

## Troubleshooting

### Issue: No draft restoration prompt appears
**Solutions**:
- Check browser console for errors (F12 → Console tab)
- Verify localStorage is enabled in browser settings
- Check if draft key exists in DevTools → Application → Local Storage
- Ensure patientId is correctly passed in URL

### Issue: "Saving..." indicator never appears
**Solutions**:
- Verify form has changes (type in any field)
- Wait full 2 seconds without typing
- Check browser console for JavaScript errors
- Ensure component is not in edit mode

### Issue: Draft doesn't save all fields
**Solutions**:
- Check if all form fields are in sessionForm FormGroup
- Verify no TypeScript compilation errors
- Check browser console for localStorage quota errors

### Issue: Multiple tabs conflict
**Expected Behavior**: Last save wins - this is by design
**Note**: Multi-tab support would require backend sync (future enhancement)

---

## Success Criteria

### All Tests Pass When:
- ✅ Draft saves automatically after 2 seconds
- ✅ Visual indicator shows saving/saved states
- ✅ Draft restoration prompt appears on return
- ✅ "Restore" button populates form correctly
- ✅ "Clear" button removes draft
- ✅ Draft clears after successful submission
- ✅ Multiple patients have independent drafts
- ✅ Responsive design works on all screen sizes

---

## Manual Testing Checklist

**Print this and check off as you test:**

- [ ] Basic auto-save works
- [ ] Visual indicator displays correctly
- [ ] Orange "Saving..." state appears
- [ ] Green "Saved HH:MM" state appears
- [ ] Indicator fades after 3 seconds
- [ ] Draft restoration prompt shows
- [ ] "Restore" button works
- [ ] "Clear" button works
- [ ] Draft clears after successful save
- [ ] Multiple patients have separate drafts
- [ ] Works on desktop (>768px)
- [ ] Works on tablet (≤768px)
- [ ] Works on mobile (≤480px)
- [ ] No console errors
- [ ] localStorage contains draft
- [ ] Bed assignment state restored
- [ ] Slot selection restored

---

## Next Steps After Testing

### If All Tests Pass:
1. ✅ Mark feature as production-ready
2. ✅ Train medical staff on new functionality
3. ✅ Monitor localStorage usage in production
4. ✅ Gather user feedback for improvements

### If Issues Found:
1. Document specific issue with screenshot
2. Note browser version and OS
3. Report in GitHub issues or project tracker
4. Developer will investigate and fix

---

**Feature Status**: 🟢 **READY FOR TESTING**

**Estimated Testing Time**: 15-20 minutes

**Last Updated**: December 2024
