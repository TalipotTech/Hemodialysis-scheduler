# AI Recommendation Button Integration Guide

## Overview

This guide shows how to add an "AI Recommendation" button to the schedule/patient form to get AI-powered scheduling suggestions.

## Quick Integration (Optional Feature)

### Option 1: Add to HD Session Form

If you want to add AI recommendations to the HD session scheduling form:

**Location**: `Frontend/hd-scheduler-app/src/app/features/hd-session/hd-session-form/`

**Component Code** (add to existing form component):

```typescript
import { AIService, AIScheduleRecommendationRequest } from '../../../services/ai.service';

// In component class:
aiService = inject(AIService);
gettingRecommendation = false;
aiAvailable = false;

ngOnInit() {
  // Check if AI is available
  this.aiService.isAIAvailable().subscribe(available => {
    this.aiAvailable = available;
  });
}

getAIRecommendation() {
  if (!this.sessionForm.get('patientID')?.value) {
    this.snackBar.open('Please select a patient first', 'Close', { duration: 3000 });
    return;
  }

  this.gettingRecommendation = true;
  
  const request: AIScheduleRecommendationRequest = {
    patientId: this.sessionForm.get('patientID')!.value,
    preferredDate: this.sessionForm.get('sessionDate')?.value,
    preferredSlotId: this.sessionForm.get('slotID')?.value
  };

  this.aiService.getSchedulingRecommendation(request).subscribe({
    next: (recommendation) => {
      // Apply recommendation to form
      this.sessionForm.patchValue({
        slotID: recommendation.recommendedSlotId,
        bedNumber: recommendation.recommendedBedNumber,
        sessionDate: recommendation.recommendedDate
      });

      // Show reasoning
      const message = `AI Recommendation (${(recommendation.confidence * 100).toFixed(0)}% confidence): ${recommendation.reasoning}`;
      this.snackBar.open(message, 'Close', { duration: 8000 });
      
      this.gettingRecommendation = false;
    },
    error: (error) => {
      console.error('AI recommendation error:', error);
      const errorMsg = error.error?.message || 'Failed to get AI recommendation';
      this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
      this.gettingRecommendation = false;
    }
  });
}
```

**Template Code** (add to HTML):

```html
<!-- Add this button near the slot selection field -->
<button mat-raised-button
        color="accent"
        type="button"
        *ngIf="aiAvailable"
        (click)="getAIRecommendation()"
        [disabled]="gettingRecommendation || !sessionForm.get('patientID')?.value"
        matTooltip="Get AI-powered scheduling recommendation">
  <mat-icon>psychology</mat-icon>
  {{ gettingRecommendation ? 'Getting Recommendation...' : 'AI Recommendation' }}
</button>
```

### Option 2: Add to Schedule Component

If you want to add it to the main schedule view:

**Location**: `Frontend/hd-scheduler-app/src/app/features/schedule/`

Similar implementation, but might show recommendations in a dialog:

```typescript
openAIRecommendationDialog(patient: Patient) {
  const dialogRef = this.dialog.open(AIRecommendationDialogComponent, {
    width: '600px',
    data: { patientId: patient.patientID }
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      // Apply selected recommendation
      this.applyRecommendation(result);
    }
  });
}
```

## AI Recommendation Dialog Component (Optional)

Create a dedicated dialog for showing AI recommendations with alternatives:

```typescript
// ai-recommendation-dialog.component.ts
@Component({
  selector: 'app-ai-recommendation-dialog',
  template: `
    <h2 mat-dialog-title>
      <mat-icon>psychology</mat-icon>
      AI Scheduling Recommendation
    </h2>
    
    <mat-dialog-content>
      <div *ngIf="loading" class="loading">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Analyzing schedule and patient data...</p>
      </div>
      
      <div *ngIf="recommendation && !loading">
        <mat-card class="primary-recommendation">
          <h3>Recommended Option</h3>
          <div class="recommendation-details">
            <p><strong>Slot:</strong> {{ getSlotName(recommendation.recommendedSlotId) }}</p>
            <p><strong>Bed:</strong> {{ recommendation.recommendedBedNumber }}</p>
            <p><strong>Date:</strong> {{ recommendation.recommendedDate | date }}</p>
            <p><strong>Confidence:</strong> {{ (recommendation.confidence * 100).toFixed(0) }}%</p>
          </div>
          <mat-chip-list class="factors">
            <mat-chip *ngFor="let factor of recommendation.factors">{{ factor }}</mat-chip>
          </mat-chip-list>
          <p class="reasoning">{{ recommendation.reasoning }}</p>
        </mat-card>
        
        <div *ngIf="recommendation.alternatives?.length > 0">
          <h4>Alternative Options</h4>
          <mat-card *ngFor="let alt of recommendation.alternatives" class="alternative">
            <p><strong>Slot {{ alt.slotId }}, Bed {{ alt.bedNumber }}</strong></p>
            <p>{{ alt.reason }}</p>
            <p>Confidence: {{ (alt.confidence * 100).toFixed(0) }}%</p>
          </mat-card>
        </div>
        
        <div class="cost-info">
          <mat-icon>info</mat-icon>
          <span>Cost: ${{ recommendation.cost.toFixed(4) }} | Processing: {{ recommendation.processingTimeMs }}ms</span>
        </div>
      </div>
      
      <mat-error *ngIf="error">{{ error }}</mat-error>
    </mat-dialog-content>
    
    <mat-dialog-actions>
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button 
              color="primary" 
              (click)="onApply()"
              [disabled]="!recommendation">
        Apply Recommendation
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .loading {
      text-align: center;
      padding: 40px;
    }
    
    .primary-recommendation {
      margin-bottom: 16px;
      background-color: #e3f2fd;
    }
    
    .recommendation-details {
      margin: 16px 0;
    }
    
    .factors {
      margin: 12px 0;
    }
    
    .reasoning {
      margin-top: 12px;
      font-style: italic;
      color: #666;
    }
    
    .alternative {
      margin-bottom: 8px;
      background-color: #f5f5f5;
    }
    
    .cost-info {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-top: 16px;
      font-size: 12px;
      color: #666;
    }
  `]
})
export class AIRecommendationDialogComponent implements OnInit {
  loading = true;
  recommendation: AIScheduleRecommendation | null = null;
  error: string = '';

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { patientId: number },
    private dialogRef: MatDialogRef<AIRecommendationDialogComponent>,
    private aiService: AIService
  ) {}

  ngOnInit() {
    this.getRecommendation();
  }

  getRecommendation() {
    const request: AIScheduleRecommendationRequest = {
      patientId: this.data.patientId
    };

    this.aiService.getSchedulingRecommendation(request).subscribe({
      next: (rec) => {
        this.recommendation = rec;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to get recommendation';
        this.loading = false;
      }
    });
  }

  getSlotName(slotId: number): string {
    // Map slot ID to name (implement based on your slot configuration)
    const slotNames = { 1: 'Morning', 2: 'Afternoon', 3: 'Evening' };
    return slotNames[slotId] || `Slot ${slotId}`;
  }

  onCancel() {
    this.dialogRef.close();
  }

  onApply() {
    this.dialogRef.close(this.recommendation);
  }
}
```

## Testing the Integration

1. **Enable AI Features**
   - Navigate to Admin Dashboard → AI Integration
   - Enter your Gemini API key
   - Enable "Scheduling Recommendations"
   - Save settings

2. **Test Recommendation**
   - Go to schedule or patient form
   - Select a patient
   - Click "AI Recommendation" button
   - Verify recommendation appears
   - Check console for any errors

3. **Monitor Costs**
   - Go back to AI Integration → Usage & Costs tab
   - Verify request was logged
   - Check cost tracking is working

## Error Handling

The AI service automatically handles common errors:

- **AI Disabled**: Button won't show if AI is disabled
- **No API Key**: Returns error message
- **Budget Exceeded**: Returns "cost limit reached" error
- **Invalid Patient**: Returns validation error
- **API Failure**: Shows generic error with retry option

## Best Practices

1. **Show AI Status**: Display indicator when AI is available
2. **Loading States**: Show spinner during recommendation fetch
3. **Confidence Scores**: Display confidence percentage to users
4. **Allow Override**: Let users accept or reject recommendations
5. **Log Acceptance**: Track if recommendations are accepted (for analytics)

## Performance Considerations

- **Cache Results**: Consider caching recommendations for same patient/date
- **Timeout**: Set reasonable timeout (30 seconds)
- **Progressive Enhancement**: Form should work without AI
- **Fallback**: Provide manual scheduling if AI fails

## Optional Enhancements

1. **Batch Recommendations**: Get recommendations for multiple patients
2. **Schedule Optimization**: AI suggests full day schedule
3. **Conflict Resolution**: AI helps resolve scheduling conflicts
4. **Waiting List**: AI prioritizes patients from waiting list

## Current Status

✅ **Implemented**:
- Backend API endpoint for recommendations
- Frontend AI service with recommendation method
- AI settings UI for configuration
- Cost tracking and monitoring

⏳ **Not Yet Implemented** (Optional):
- AI recommendation button in forms
- Recommendation dialog component
- Acceptance tracking
- Batch recommendations

The core infrastructure is ready. Adding the UI button is optional and can be done when needed.

---

**Note**: The AI recommendation feature is production-ready on the backend. Frontend integration is optional and can be added based on user feedback and requirements.
