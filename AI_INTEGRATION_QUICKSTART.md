# AI Integration - Quick Start Guide

## Overview

The AI Integration feature adds intelligent scheduling recommendations to the HD Scheduler using Google's Gemini Pro API. This feature is optional, cost-controlled, and can be enabled/disabled through settings.

## ✅ Backend Implementation Complete

### Features Implemented

- **Cost-Conscious AI**: Uses Gemini Pro ($0.0005/1K input, $0.0015/1K output - 70% cheaper than OpenAI)
- **Enable/Disable Toggle**: AI can be turned on/off in settings
- **Cost Tracking**: Real-time monitoring of daily and monthly costs
- **Budget Limits**: Default $10/day, $250/month (configurable)
- **Scheduling Recommendations**: AI suggests optimal slot and bed assignments
- **Usage Analytics**: Detailed logs of all AI requests and costs

### Architecture

```
Backend/
├── Controllers/
│   └── AIController.cs          # REST API endpoints
├── Services/AI/
│   ├── AIService.cs             # Core business logic
│   ├── AIRepository.cs          # Dapper-based data access
│   └── GeminiClient.cs          # HTTP client for Gemini API
├── Models/
│   ├── AISettings.cs            # Configuration model
│   └── AIUsageLog.cs            # Usage tracking model
└── Migrations/
    └── 001_AI_Integration.sql   # Database schema
```

### Database Tables

**AISettings** - Stores AI configuration:
- AIEnabled, AIProvider, DailyCostLimit, MonthlyCostLimit
- Feature toggles (scheduling, NLQ, analytics)
- Encrypted API key
- Current usage counters

**AIUsageLogs** - Tracks every AI request:
- Timestamp, Provider, RequestType
- Token counts, cost, processing time
- Success/error information

### API Endpoints

#### 1. Get AI Settings (Admin Only)
```http
GET /api/ai/settings
Authorization: Bearer {jwt-token}
```

Response:
```json
{
  "aiEnabled": false,
  "aiProvider": "Gemini",
  "dailyCostLimit": 10.00,
  "monthlyCostLimit": 250.00,
  "currentDailyCost": 0.00,
  "currentMonthlyCost": 0.00,
  "todayRequestCount": 0,
  "monthRequestCount": 0,
  "hasApiKey": false
}
```

#### 2. Update AI Settings (Admin Only)
```http
PUT /api/ai/settings
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "aiEnabled": true,
  "apiKey": "YOUR_GEMINI_API_KEY",
  "dailyCostLimit": 20.00,
  "monthlyCostLimit": 500.00,
  "enableSchedulingRecommendations": true
}
```

#### 3. Get Scheduling Recommendation
```http
POST /api/ai/schedule/recommend
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "patientId": 123,
  "preferredDate": "2025-01-15",
  "preferredSlotId": 2
}
```

Response:
```json
{
  "recommendedSlotId": 2,
  "recommendedBedNumber": 5,
  "recommendedDate": "2025-01-15",
  "confidence": 0.87,
  "reasoning": "Slot 2 has highest availability and matches patient HD cycle",
  "factors": ["Bed availability", "HD cycle compatibility", "Historical patterns"],
  "modelUsed": "Gemini Pro",
  "processingTimeMs": 1245,
  "cost": 0.002150
}
```

#### 4. Get Usage Statistics (Admin Only)
```http
GET /api/ai/usage/stats
Authorization: Bearer {jwt-token}
```

Response:
```json
{
  "todayCost": 1.25,
  "monthCost": 45.80,
  "todayRequests": 125,
  "monthRequests": 3420,
  "dailyUsagePercentage": 12.5,
  "monthlyUsagePercentage": 18.3,
  "usageBreakdown": [
    {
      "requestType": "Scheduling",
      "count": 3200,
      "totalCost": 42.50,
      "avgProcessingTimeMs": 1150
    }
  ]
}
```

#### 5. Check AI Status
```http
GET /api/ai/status
Authorization: Bearer {jwt-token}
```

Response:
```json
{
  "enabled": true,
  "provider": "Gemini",
  "underBudget": true,
  "dailyUsage": "12.5%",
  "monthlyUsage": "18.3%"
}
```

## Setup Instructions

### Step 1: Get Gemini API Key

1. Go to [Google AI Studio](https://makersuite.google.com/app/apikey)
2. Sign in with your Google account
3. Click "Create API Key"
4. Copy the generated key (starts with `AIza...`)

### Step 2: Configure in Application

1. Start the backend API
2. Login as an admin user
3. Navigate to Settings > AI Integration
4. Enter your Gemini API key
5. Set budget limits (default: $10/day, $250/month)
6. Enable "AI Scheduling Recommendations"
7. Click "Save Settings"

### Step 3: Test the Integration

```bash
# Check AI status
curl -X GET http://localhost:5000/api/ai/status \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Get scheduling recommendation
curl -X POST http://localhost:5000/api/ai/schedule/recommend \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"patientId": 1, "preferredDate": "2025-01-15"}'
```

## Cost Management

### Pricing (Gemini Pro)
- **Input**: $0.0005 per 1,000 characters
- **Output**: $0.0015 per 1,000 characters

### Example Costs
- Simple scheduling query: ~$0.002 per request
- 500 daily requests: ~$1.00/day
- 10,000 monthly requests: ~$20/month

### Cost Controls
1. **Daily Limit**: Automatic shutoff when daily budget reached
2. **Monthly Limit**: Automatic shutoff when monthly budget reached
3. **Enable/Disable**: Turn AI off completely in settings
4. **Usage Dashboard**: Real-time cost and request monitoring

### Budget Alerts
- System logs warning when 80% of budget used
- AI automatically disables when 100% of budget reached
- Counters reset daily (00:00 UTC) and monthly (1st of month)

## Security

- API keys encrypted in database using AES-256
- Admin-only access to settings and usage stats
- All requests logged with user ID for auditing
- Encrypted connection to Gemini API (HTTPS)

## Monitoring

### Usage Logs
All AI requests logged in `AIUsageLogs` table:
- Timestamp of request
- User who made request
- Input/output token counts
- Cost calculation
- Processing time
- Success/failure status

### Analytics Dashboard
View AI usage trends:
- Cost by day/week/month
- Request count trends
- Average processing times
- Success rate
- Cost per request type

## Troubleshooting

### AI Not Working
1. Check if AI is enabled in settings
2. Verify API key is correct
3. Check if under budget limits
4. Review error logs in `AIUsageLogs`

### Invalid API Key
```json
{
  "error": "Invalid Gemini API key"
}
```
**Solution**: Get new API key from Google AI Studio

### Budget Exceeded
```json
{
  "error": "AI cost limit reached"
}
```
**Solution**: Wait for daily/monthly reset or increase limits

### Slow Responses
- Normal processing: 1-2 seconds
- If > 5 seconds, check network connectivity
- Review processing times in usage stats

## Future Enhancements (Not Yet Implemented)

These features are planned but not included in the initial release:

- **Frontend UI**: Angular components for AI settings
- **Natural Language Queries**: Ask questions in plain English
- **Predictive Analytics**: Patient outcome predictions
- **Multi-Model Support**: Add OpenAI, Claude options
- **Smart Alerts**: AI-powered notifications
- **Auto-Optimization**: AI suggests workflow improvements

## Testing Checklist

- [x] Database migration applied
- [x] Backend API compiles
- [x] Backend API runs
- [x] AISettings table created
- [x] AIUsageLogs table created
- [ ] Admin can update settings
- [ ] API key validation works
- [ ] Scheduling recommendations return results
- [ ] Cost tracking increments correctly
- [ ] Budget limits enforced
- [ ] Usage stats display correctly

## Support

For issues or questions:
1. Check error logs in `AIUsageLogs` table
2. Review API endpoint responses
3. Verify Gemini API service status
4. Check authentication/authorization

## Production Deployment

### Before Going Live
1. Test with small budget limits first ($1/day)
2. Monitor costs for 1 week
3. Review recommendation accuracy
4. Get user feedback on AI suggestions
5. Gradually increase budget based on usage

### Recommended Settings for One Hospital
- Daily Limit: $5-10
- Monthly Limit: $100-250
- Enable scheduling recommendations only
- Disable experimental features (NLQ, analytics)
- Review usage weekly

### Scaling Considerations
- Cost scales linearly with request count
- 1,000 requests/day ≈ $2-3/day
- Monitor usage patterns to optimize prompts
- Consider caching for repeated queries

## Documentation Files
- `HD_Scheduler_AI_Integration_Spec_Gemini_OpenAi_Claude.md` - Full technical specification
- `AI_INTEGRATION_QUICKSTART.md` - This file
- `Backend/Migrations/001_AI_Integration.sql` - Database schema
- `apply-ai-migration.ps1` - Migration script

---

**Status**: Backend Complete ✅ | Frontend Pending ⏳
**Branch**: `feature/ai-integration`
**Last Updated**: January 2025
