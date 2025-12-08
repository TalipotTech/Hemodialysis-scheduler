# AI Integration Quick Start Guide

## 🎯 How to Use Gemini AI in HD Scheduler

### Step 1: Configure AI Settings (✅ DONE)

You've already completed this! Your Gemini API key is configured and stored securely.

### Step 2: Test the AI Connection

1. **Navigate to AI Settings**
   - Click on **Admin Dashboard** → **AI Integration** in the sidebar
   
2. **Test the Connection**
   - Click the **"Test AI Connection"** button
   - This will send a test scheduling request to Gemini
   - You'll see a success message with a sample recommendation

### Step 3: Use AI Features

#### 🤖 **AI Scheduling Recommendations**

The AI can suggest optimal slot and bed assignments based on:
- Patient medical profile (age, weight, dialyser type, HD cycle)
- Historical scheduling patterns
- Current bed availability
- Similar patient outcomes

**How to use:**
```typescript
// From any component that injects AIService
const recommendation = await this.aiService.getSchedulingRecommendation({
  patientId: 123,
  preferredSlotId: 1,  // Optional
  preferredDate: '2025-01-16'  // Optional
}).toPromise();

console.log('Recommended Slot:', recommendation.recommendedSlotId);
console.log('Recommended Bed:', recommendation.recommendedBedNumber);
console.log('Confidence:', recommendation.confidence);
console.log('Reasoning:', recommendation.reasoning);
```

#### 📊 **Monitor AI Usage & Costs**

- Navigate to **AI Settings** → **Usage & Costs** tab
- View real-time statistics:
  - Today's cost and request count
  - Monthly totals
  - Usage by request type
  - Cost trends over last 7 days

#### 🔒 **Budget Controls**

The system automatically:
- Tracks daily and monthly costs
- Warns when approaching 80% of budget
- Disables AI when budget is exceeded
- Resets counters at midnight (daily) and month start

### Next Steps: Add AI to Schedule Forms

#### Option 1: Add "Get AI Recommendation" Button

Add this to your patient scheduling form:

```typescript
// In your schedule component
getAIRecommendation(): void {
  if (!this.selectedPatient) return;
  
  this.loadingRecommendation = true;
  this.aiService.getSchedulingRecommendation({
    patientId: this.selectedPatient.patientId,
    preferredDate: this.selectedDate.toISOString()
  }).subscribe({
    next: (rec) => {
      // Auto-fill form with AI suggestion
      this.scheduleForm.patchValue({
        slotId: rec.recommendedSlotId,
        bedNumber: rec.recommendedBedNumber
      });
      
      // Show reasoning to user
      this.snackBar.open(
        `AI Suggestion: Slot ${rec.recommendedSlotId}, Bed ${rec.recommendedBedNumber} (${(rec.confidence * 100).toFixed(0)}% confidence) - ${rec.reasoning}`,
        'Close',
        { duration: 8000 }
      );
      
      this.loadingRecommendation = false;
    },
    error: (err) => {
      this.snackBar.open('AI recommendation failed', 'Close', { duration: 3000 });
      this.loadingRecommendation = false;
    }
  });
}
```

Add the button to your template:
```html
<button mat-raised-button 
        color="accent"
        (click)="getAIRecommendation()"
        [disabled]="!selectedPatient || loadingRecommendation">
  <mat-icon>{{ loadingRecommendation ? 'hourglass_empty' : 'psychology' }}</mat-icon>
  {{ loadingRecommendation ? 'Getting AI Suggestion...' : 'Get AI Recommendation' }}
</button>
```

#### Option 2: Show AI Suggestions Automatically

```typescript
// When patient is selected, automatically show AI suggestion
onPatientSelected(patient: Patient): void {
  this.selectedPatient = patient;
  this.loadAIRecommendation();  // Auto-show suggestion
}

private loadAIRecommendation(): void {
  this.aiService.getSchedulingRecommendation({
    patientId: this.selectedPatient.patientId
  }).subscribe({
    next: (rec) => {
      this.aiSuggestion = rec;
      this.showAISuggestionCard = true;  // Display in UI
    }
  });
}
```

### API Response Structure

```typescript
interface AIScheduleRecommendation {
  recommendedSlotId: number;         // 1, 2, 3, or 4
  recommendedBedNumber: number;      // 1-10
  recommendedDate: string;           // ISO date string
  confidence: number;                // 0.0 to 1.0
  reasoning: string;                 // Human-readable explanation
  factors: string[];                 // Key factors considered
  alternatives: {                    // Alternative suggestions
    slotId: number;
    bedNumber: number;
    confidence: number;
    reason: string;
  }[];
  modelUsed: string;                // "gemini-1.5-flash"
  processingTimeMs: number;         // Response time
  cost: number;                     // Cost in USD
}
```

### Cost Information

**Gemini 1.5 Flash Pricing:**
- Input: $0.00035 per 1K characters
- Output: $0.0014 per 1K characters
- **Average cost per recommendation:** $0.001 - $0.003 (0.1¢ to 0.3¢)

**Budget Recommendations:**
- **Small clinic (50-100 requests/day):** $5-10/month
- **Medium clinic (200-500 requests/day):** $20-50/month
- **Large clinic (1000+ requests/day):** $100-200/month

### Monitoring & Troubleshooting

#### Check if AI is Available
```typescript
this.aiService.isAIAvailable().subscribe(available => {
  console.log('AI Available:', available);
});
```

#### Check AI Status
```typescript
this.aiService.getStatus().subscribe(status => {
  console.log('AI Enabled:', status.enabled);
  console.log('Provider:', status.provider);
  console.log('Has API Key:', status.hasApiKey);
});
```

#### View Backend Logs
The backend logs all AI requests:
```
info: HDScheduler.API.Services.AI.AIService[0]
      Processing AI scheduling recommendation for patient 123
info: HDScheduler.API.Services.AI.GeminiClient[0]
      Gemini request successful - Tokens: 450, Cost: $0.0021
```

### Security & Privacy

✅ **HIPAA Compliant:**
- API keys are encrypted in database (AES-256)
- Patient data is anonymized before sending to AI
- All AI interactions are logged for audit
- No PHI is permanently stored by Google

✅ **Access Control:**
- Only Admin role can configure AI settings
- AI features require authentication
- Budget limits prevent runaway costs

### Getting Help

**Common Issues:**

1. **"Invalid Gemini API key"**
   - Verify your API key at https://makersuite.google.com/app/apikey
   - Make sure it's active and not expired
   - Re-save in AI Settings

2. **"Budget limit reached"**
   - Check Usage & Costs tab
   - Increase daily/monthly limits in Settings
   - Limits reset automatically

3. **"AI recommendation failed"**
   - Check backend logs for details
   - Verify patient data is valid
   - Ensure network connectivity to Google API

**Support Resources:**
- Technical Spec: `HD_Scheduler_AI_Integration_Spec_Gemini_OpenAi_Claude.md`
- Deployment Guide: `AI_INTEGRATION_COMPLETE.md`

---

## 🚀 You're Ready to Use AI!

Your HD Scheduler now has intelligent scheduling capabilities powered by Google Gemini. Start by testing the connection, then integrate AI recommendations into your scheduling workflows!
