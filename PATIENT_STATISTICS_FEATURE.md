# Patient Statistics Feature - Implementation Complete ✅

## Overview
The system now provides comprehensive patient statistics showing **total patients (active + discharged)** broken down by **Day, Week, Month, and Year**. This gives complete visibility into dialysis session metrics.

---

## API Endpoint

### Get Patient Statistics
**Endpoint:** `GET /api/schedule/patient-statistics`

**Authorization:** Admin, HOD, Doctor, Nurse, Technician (all medical staff)

**Query Parameters:**
- `date` (optional): Reference date in format `yyyy-MM-dd` (defaults to today)

**Example Requests:**
```bash
# Today's statistics
GET /api/schedule/patient-statistics

# Statistics for specific date
GET /api/schedule/patient-statistics?date=2025-11-15
```

---

## Response Structure

### Complete Response Example
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "referenceDate": "2025-11-26",
    "generatedAt": "2025-11-26 09:18:05",
    
    "day": {
      "date": "2025-11-26",
      "dayOfWeek": "Wednesday",
      "active": 5,
      "discharged": 0,
      "total": 5,
      "uniquePatients": 5,
      "dischargeRate": 0
    },
    
    "week": {
      "startDate": "2025-11-23",
      "endDate": "2025-11-29",
      "weekNumber": 48,
      "active": 15,
      "discharged": 11,
      "total": 26,
      "uniquePatients": 10,
      "dischargeRate": 42.31,
      "averageSessionsPerDay": 3.71
    },
    
    "month": {
      "month": "November 2025",
      "startDate": "2025-11-01",
      "endDate": "2025-11-30",
      "active": 15,
      "discharged": 43,
      "total": 58,
      "uniquePatients": 36,
      "dischargeRate": 74.14,
      "averageSessionsPerDay": 1.93
    },
    
    "year": {
      "year": 2025,
      "startDate": "2025-01-01",
      "endDate": "2025-12-31",
      "active": 22,
      "discharged": 43,
      "total": 65,
      "uniquePatients": 36,
      "dischargeRate": 66.15,
      "averageSessionsPerMonth": 5.42,
      "averageSessionsPerDay": 0.18
    }
  }
}
```

---

## Data Metrics Explained

### Day Statistics
| Metric | Description | Example |
|--------|-------------|---------|
| `date` | The specific date | "2025-11-26" |
| `dayOfWeek` | Day name | "Wednesday" |
| `active` | Sessions not yet discharged | 5 |
| `discharged` | Sessions already discharged | 0 |
| `total` | Total sessions (active + discharged) | 5 |
| `uniquePatients` | Number of different patients | 5 |
| `dischargeRate` | Percentage discharged (%) | 0.00 |

**Use Cases:**
- Monitor today's workload
- Track current active sessions
- Daily performance metrics

---

### Week Statistics
| Metric | Description | Example |
|--------|-------------|---------|
| `startDate` | Week start (Sunday) | "2025-11-23" |
| `endDate` | Week end (Saturday) | "2025-11-29" |
| `weekNumber` | ISO week number | 48 |
| `active` | Sessions not discharged this week | 15 |
| `discharged` | Sessions discharged this week | 11 |
| `total` | Total sessions this week | 26 |
| `uniquePatients` | Different patients this week | 10 |
| `dischargeRate` | Percentage discharged (%) | 42.31 |
| `averageSessionsPerDay` | Average sessions per day | 3.71 |

**Use Cases:**
- Weekly workload planning
- Staff scheduling
- Compare week-over-week performance

---

### Month Statistics
| Metric | Description | Example |
|--------|-------------|---------|
| `month` | Month name and year | "November 2025" |
| `startDate` | First day of month | "2025-11-01" |
| `endDate` | Last day of month | "2025-11-30" |
| `active` | Sessions not discharged this month | 15 |
| `discharged` | Sessions discharged this month | 43 |
| `total` | Total sessions this month | 58 |
| `uniquePatients` | Different patients this month | 36 |
| `dischargeRate` | Percentage discharged (%) | 74.14 |
| `averageSessionsPerDay` | Average sessions per day | 1.93 |

**Use Cases:**
- Monthly billing and reporting
- Performance reviews
- Capacity planning

---

### Year Statistics
| Metric | Description | Example |
|--------|-------------|---------|
| `year` | Calendar year | 2025 |
| `startDate` | January 1st | "2025-01-01" |
| `endDate` | December 31st | "2025-12-31" |
| `active` | Sessions not discharged this year | 22 |
| `discharged` | Sessions discharged this year | 43 |
| `total` | Total sessions this year | 65 |
| `uniquePatients` | Different patients this year | 36 |
| `dischargeRate` | Percentage discharged (%) | 66.15 |
| `averageSessionsPerMonth` | Average per month | 5.42 |
| `averageSessionsPerDay` | Average per day | 0.18 |

**Use Cases:**
- Annual reports
- Long-term trend analysis
- Budget planning

---

## Calculation Details

### Date Range Calculations

**Day:**
```
Start: referenceDate at 00:00:00
End: referenceDate at 23:59:59
```

**Week:**
```
Start: Previous Sunday from reference date
End: Following Saturday (7 days later)
Week Number: ISO 8601 week number
```

**Month:**
```
Start: 1st day of the month
End: Last day of the month
```

**Year:**
```
Start: January 1st
End: December 31st
```

### Status Classification

**Active Sessions:**
```sql
WHERE IsDischarged = 0 AND IsMovedToHistory = 0
```

**Discharged Sessions:**
```sql
WHERE IsDischarged = 1 OR IsMovedToHistory = 1
```

**Total Sessions:**
```
Total = Active + Discharged
```

**Discharge Rate:**
```
DischargeRate = (Discharged / Total) * 100
```

---

## Frontend Integration

### Angular Service Method
The method is already added to `schedule.service.ts`:

```typescript
getPatientStatistics(date?: string): Observable<ApiResponse<any>> {
  let params = new HttpParams();
  if (date) {
    params = params.set('date', date);
  }
  return this.http.get<ApiResponse<any>>(`${this.apiUrl}/patient-statistics`, { params });
}
```

### Usage Example in Component
```typescript
import { ScheduleService } from './core/services/schedule.service';

export class DashboardComponent implements OnInit {
  statistics: any;

  constructor(private scheduleService: ScheduleService) {}

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.scheduleService.getPatientStatistics().subscribe({
      next: (response) => {
        if (response.success) {
          this.statistics = response.data;
          console.log('Today:', this.statistics.day);
          console.log('This Week:', this.statistics.week);
          console.log('This Month:', this.statistics.month);
          console.log('This Year:', this.statistics.year);
        }
      },
      error: (error) => {
        console.error('Error loading statistics:', error);
      }
    });
  }

  loadStatisticsForDate(date: string): void {
    this.scheduleService.getPatientStatistics(date).subscribe({
      next: (response) => {
        if (response.success) {
          this.statistics = response.data;
        }
      }
    });
  }
}
```

### Display in Template
```html
<div class="statistics-container">
  <mat-card class="stat-card">
    <mat-card-header>
      <mat-card-title>Today's Statistics</mat-card-title>
      <mat-card-subtitle>{{ statistics?.day.date }} ({{ statistics?.day.dayOfWeek }})</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <div class="stat-row">
        <span class="label">Active:</span>
        <span class="value">{{ statistics?.day.active }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Discharged:</span>
        <span class="value">{{ statistics?.day.discharged }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Total:</span>
        <span class="value">{{ statistics?.day.total }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Unique Patients:</span>
        <span class="value">{{ statistics?.day.uniquePatients }}</span>
      </div>
    </mat-card-content>
  </mat-card>

  <mat-card class="stat-card">
    <mat-card-header>
      <mat-card-title>This Week</mat-card-title>
      <mat-card-subtitle>Week {{ statistics?.week.weekNumber }}</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <div class="stat-row">
        <span class="label">Active:</span>
        <span class="value">{{ statistics?.week.active }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Discharged:</span>
        <span class="value">{{ statistics?.week.discharged }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Total:</span>
        <span class="value">{{ statistics?.week.total }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Discharge Rate:</span>
        <span class="value">{{ statistics?.week.dischargeRate }}%</span>
      </div>
    </mat-card-content>
  </mat-card>

  <mat-card class="stat-card">
    <mat-card-header>
      <mat-card-title>This Month</mat-card-title>
      <mat-card-subtitle>{{ statistics?.month.month }}</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <div class="stat-row">
        <span class="label">Active:</span>
        <span class="value">{{ statistics?.month.active }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Discharged:</span>
        <span class="value">{{ statistics?.month.discharged }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Total:</span>
        <span class="value">{{ statistics?.month.total }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Avg/Day:</span>
        <span class="value">{{ statistics?.month.averageSessionsPerDay }}</span>
      </div>
    </mat-card-content>
  </mat-card>

  <mat-card class="stat-card">
    <mat-card-header>
      <mat-card-title>This Year</mat-card-title>
      <mat-card-subtitle>{{ statistics?.year.year }}</mat-card-subtitle>
    </mat-card-header>
    <mat-card-content>
      <div class="stat-row">
        <span class="label">Active:</span>
        <span class="value">{{ statistics?.year.active }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Discharged:</span>
        <span class="value">{{ statistics?.year.discharged }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Total:</span>
        <span class="value">{{ statistics?.year.total }}</span>
      </div>
      <div class="stat-row">
        <span class="label">Unique Patients:</span>
        <span class="value">{{ statistics?.year.uniquePatients }}</span>
      </div>
    </mat-card-content>
  </mat-card>
</div>
```

---

## Testing

### PowerShell Test Script
```powershell
# Login
$loginBody = @{
    username = "admin"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/auth/login" `
    -Method Post -Body $loginBody -ContentType "application/json"

$token = $loginResponse.data.token

# Get today's statistics
$stats = Invoke-RestMethod -Uri "http://localhost:5001/api/schedule/patient-statistics" `
    -Method Get -Headers @{Authorization="Bearer $token"}

# Display results
Write-Host "Day: Active=$($stats.data.day.active), Discharged=$($stats.data.day.discharged), Total=$($stats.data.day.total)"
Write-Host "Week: Active=$($stats.data.week.active), Discharged=$($stats.data.week.discharged), Total=$($stats.data.week.total)"
Write-Host "Month: Active=$($stats.data.month.active), Discharged=$($stats.data.month.discharged), Total=$($stats.data.month.total)"
Write-Host "Year: Active=$($stats.data.year.active), Discharged=$($stats.data.year.discharged), Total=$($stats.data.year.total)"

# Get statistics for specific date
$dateStats = Invoke-RestMethod -Uri "http://localhost:5001/api/schedule/patient-statistics?date=2025-11-01" `
    -Method Get -Headers @{Authorization="Bearer $token"}
```

### cURL Test
```bash
# Login
TOKEN=$(curl -X POST "http://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}' \
  | jq -r '.data.token')

# Get statistics
curl -X GET "http://localhost:5001/api/schedule/patient-statistics" \
  -H "Authorization: Bearer $TOKEN" | jq

# Get statistics for specific date
curl -X GET "http://localhost:5001/api/schedule/patient-statistics?date=2025-11-01" \
  -H "Authorization: Bearer $TOKEN" | jq
```

---

## Use Cases

### 1. Admin Dashboard
Display real-time statistics showing:
- Today's active vs discharged patients
- Weekly trends and discharge rates
- Monthly performance metrics
- Year-to-date totals

### 2. HOD Reporting
Generate reports showing:
- Weekly capacity utilization
- Monthly discharge efficiency
- Staff workload distribution
- Long-term trend analysis

### 3. Performance Monitoring
Track key metrics:
- Discharge rate trends
- Average sessions per day/month
- Patient volume patterns
- Seasonal variations

### 4. Capacity Planning
Analyze data for:
- Peak hours/days identification
- Resource allocation decisions
- Future expansion planning
- Staff scheduling optimization

---

## Benefits

### 1. **Comprehensive Visibility**
- See complete picture: active + discharged
- Multiple time perspectives (day/week/month/year)
- Unique patient counts

### 2. **Real-Time Metrics**
- Current active sessions
- Today's completed sessions
- Running totals

### 3. **Historical Analysis**
- Week-over-week trends
- Monthly comparisons
- Yearly patterns

### 4. **Performance Insights**
- Discharge efficiency rates
- Average session volumes
- Capacity utilization

### 5. **Easy Integration**
- RESTful API endpoint
- Angular service method ready
- Standard JSON response format

---

## Implementation Files

| File | Purpose |
|------|---------|
| `Backend/Controllers/ScheduleController.cs` | Statistics endpoint implementation |
| `Frontend/.../schedule.service.ts` | Angular service method |

---

## Example Output Analysis

### Current Statistics (2025-11-26):
```
Day (Wed):    5 active + 0 discharged = 5 total (0% discharged)
Week 48:      15 active + 11 discharged = 26 total (42% discharged)
November:     15 active + 43 discharged = 58 total (74% discharged)
Year 2025:    22 active + 43 discharged = 65 total (66% discharged)
```

**Insights:**
- ✅ Today has 5 active sessions (not yet completed)
- ✅ This week: 26 total sessions with 42% completion rate
- ✅ This month: 58 sessions with high 74% discharge rate
- ✅ Year to date: 65 total sessions across 36 unique patients
- ✅ Monthly average: ~2 sessions per day
- ✅ Weekly average: ~4 sessions per day

---

## FAQ

### Q: What counts as "active"?
**A:** Sessions where `IsDischarged = 0` AND `IsMovedToHistory = 0`

### Q: What counts as "discharged"?
**A:** Sessions where `IsDischarged = 1` OR `IsMovedToHistory = 1`

### Q: Why does uniquePatients differ from total?
**A:** One patient can have multiple sessions (e.g., 3 sessions per week for dialysis treatment)

### Q: How is the week calculated?
**A:** Sunday to Saturday based on the reference date, using ISO 8601 week numbering

### Q: Can I get statistics for past dates?
**A:** Yes, use the `?date=yyyy-MM-dd` parameter for any date

### Q: Is this real-time?
**A:** Yes, it queries the database directly and reflects current state

### Q: Does it include future scheduled sessions?
**A:** Yes, "active" includes both current and future pre-scheduled sessions

---

## Future Enhancements

Potential additions:
- [ ] Date range comparison (compare two periods)
- [ ] Breakdown by slot (morning/afternoon/evening)
- [ ] Patient demographics statistics
- [ ] Equipment usage statistics
- [ ] Staff workload distribution
- [ ] Export to CSV/Excel
- [ ] Visual charts and graphs
- [ ] Automated scheduled reports

---

**Feature Status:** ✅ **LIVE AND READY TO USE**

*The patient statistics endpoint is now available and can be integrated into dashboards and reports.*
