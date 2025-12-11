import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { 
  ScheduleModule, 
  DayService, 
  WeekService, 
  WorkWeekService, 
  MonthService, 
  AgendaService,
  TimelineViewsService,
  TimelineMonthService,
  EventSettingsModel,
  GroupModel,
  ResourcesModel,
  ScheduleComponent,
  PopupOpenEventArgs,
  View
} from '@syncfusion/ej2-angular-schedule';

interface DialysisSession {
  Id: number;
  Subject: string;
  PatientName: string;
  PatientId: string;
  BedId: number;
  ShiftId: number;
  StartTime: Date;
  EndTime: Date;
  Description?: string;
  SessionType: string;
  IsAllDay: boolean;
  RecurrenceRule?: string;
}

@Component({
  selector: 'app-syncfusion-advanced-scheduler',
  standalone: true,
  imports: [ScheduleModule, CommonModule],
  providers: [
    DayService, 
    WeekService, 
    WorkWeekService, 
    MonthService, 
    AgendaService,
    TimelineViewsService,
    TimelineMonthService
  ],
  template: `
    <div class="advanced-scheduler-container">
      <div class="scheduler-header">
        <h2>🏥 Advanced Dialysis Scheduler - Resource Management</h2>
        <p class="subtitle">Multi-bed scheduling with shift grouping and drag-drop capabilities</p>
      </div>

      <div class="scheduler-controls">
        <div class="view-selector">
          <button 
            *ngFor="let view of availableViews" 
            (click)="changeView(view)"
            [class.active]="currentView === view"
            class="view-btn">
            {{ view }}
          </button>
        </div>
        <div class="info-badges">
          <span class="info-badge">📅 {{ totalSessions }} Sessions</span>
          <span class="info-badge">🛏️ {{ bedResources.length }} Beds</span>
          <span class="info-badge">⏰ {{ shiftResources.length }} Shifts</span>
        </div>
      </div>

      <ejs-schedule 
        #scheduleObj
        width='100%' 
        height='700px' 
        [selectedDate]="selectedDate"
        [currentView]="currentView"
        [eventSettings]="eventSettings"
        [group]="group"
        [resources]="resources"
        [views]="views"
        [workHours]="workHours"
        [timeScale]="timeScale"
        (popupOpen)="onPopupOpen($event)">
      </ejs-schedule>

      <div class="scheduler-legend">
        <h3>Session Types</h3>
        <div class="legend-items">
          <div class="legend-item">
            <span class="legend-color" style="background: #1e88e5"></span>
            <span>Standard HD</span>
          </div>
          <div class="legend-item">
            <span class="legend-color" style="background: #7cb342"></span>
            <span>Emergency HD</span>
          </div>
          <div class="legend-item">
            <span class="legend-color" style="background: #e53935"></span>
            <span>CRRT</span>
          </div>
          <div class="legend-item">
            <span class="legend-color" style="background: #fb8c00"></span>
            <span>Peritoneal Dialysis</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .advanced-scheduler-container {
      padding: 20px;
      max-width: 1600px;
      margin: 0 auto;
    }

    .scheduler-header {
      margin-bottom: 25px;
    }

    .scheduler-header h2 {
      color: #1976d2;
      margin-bottom: 8px;
      font-size: 1.8rem;
    }

    .subtitle {
      color: #666;
      font-size: 1.1rem;
    }

    .scheduler-controls {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      flex-wrap: wrap;
      gap: 15px;
    }

    .view-selector {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .view-btn {
      padding: 10px 20px;
      border: 2px solid #1976d2;
      background: white;
      color: #1976d2;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 600;
      transition: all 0.3s;
    }

    .view-btn:hover {
      background: #e3f2fd;
    }

    .view-btn.active {
      background: #1976d2;
      color: white;
    }

    .info-badges {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }

    .info-badge {
      padding: 8px 16px;
      background: #f5f5f5;
      border-radius: 20px;
      font-size: 0.9rem;
      font-weight: 500;
      color: #555;
    }

    .scheduler-legend {
      margin-top: 30px;
      padding: 20px;
      background: #f5f5f5;
      border-radius: 8px;
    }

    .scheduler-legend h3 {
      margin-bottom: 15px;
      color: #333;
    }

    .legend-items {
      display: flex;
      gap: 25px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .legend-color {
      width: 20px;
      height: 20px;
      border-radius: 4px;
      display: inline-block;
    }

    @media (max-width: 768px) {
      .scheduler-controls {
        flex-direction: column;
        align-items: stretch;
      }

      .view-selector {
        justify-content: center;
      }

      .info-badges {
        justify-content: center;
      }
    }

    /* Syncfusion customizations */
    ::ng-deep .e-schedule .e-timeline-view .e-resource-left-td {
      width: 150px;
    }

    ::ng-deep .e-schedule .e-timeline-view .e-resource-cells {
      font-weight: 600;
    }

    ::ng-deep .e-schedule .e-appointment {
      border-radius: 4px;
      font-weight: 500;
    }
  `]
})
export class SyncfusionAdvancedSchedulerComponent {
  @ViewChild('scheduleObj') scheduleObj!: ScheduleComponent;

  public selectedDate: Date = new Date(2025, 0, 15);
  public currentView: View = 'TimelineDay';
  public totalSessions = 0;

  public views: View[] = ['Day', 'Week', 'TimelineDay', 'TimelineWeek', 'Month'];
  public availableViews = ['Day', 'Week', 'Timeline Day', 'Timeline Week', 'Month'];

  public workHours = {
    highlight: true,
    start: '06:00',
    end: '22:00'
  };

  public timeScale = {
    enable: true,
    interval: 60,
    slotCount: 2
  };

  // Bed Resources
  public bedResources: Object[] = [
    { BedId: 1, BedName: 'Bed 1', BedColor: '#1e88e5', Capacity: 1 },
    { BedId: 2, BedName: 'Bed 2', BedColor: '#1e88e5', Capacity: 1 },
    { BedId: 3, BedName: 'Bed 3', BedColor: '#7cb342', Capacity: 1 },
    { BedId: 4, BedName: 'Bed 4', BedColor: '#7cb342', Capacity: 1 },
    { BedId: 5, BedName: 'Bed 5', BedColor: '#e53935', Capacity: 1 },
    { BedId: 6, BedName: 'Bed 6', BedColor: '#fb8c00', Capacity: 1 }
  ];

  // Shift Resources
  public shiftResources: Object[] = [
    { ShiftId: 1, ShiftName: 'Morning Shift (6 AM - 2 PM)', ShiftColor: '#FFE082' },
    { ShiftId: 2, ShiftName: 'Afternoon Shift (2 PM - 10 PM)', ShiftColor: '#FFAB91' },
    { ShiftId: 3, ShiftName: 'Night Shift (10 PM - 6 AM)', ShiftColor: '#CE93D8' }
  ];

  public group: GroupModel = {
    byGroupID: false,
    resources: ['Shifts', 'Beds']
  };

  public resources: ResourcesModel[] = [
    {
      field: 'ShiftId',
      title: 'Shift',
      name: 'Shifts',
      dataSource: this.shiftResources,
      textField: 'ShiftName',
      idField: 'ShiftId',
      colorField: 'ShiftColor'
    },
    {
      field: 'BedId',
      title: 'Bed',
      name: 'Beds',
      dataSource: this.bedResources,
      textField: 'BedName',
      idField: 'BedId',
      colorField: 'BedColor'
    }
  ];

  // Sample dialysis sessions with realistic data
  private dialysisSessions: DialysisSession[] = [
    // Morning Shift - Bed 1
    {
      Id: 1,
      Subject: 'HD Session',
      PatientName: 'John Doe',
      PatientId: 'P1001',
      BedId: 1,
      ShiftId: 1,
      StartTime: new Date(2025, 0, 15, 6, 0),
      EndTime: new Date(2025, 0, 15, 10, 0),
      Description: 'Standard hemodialysis - 4 hours',
      SessionType: 'Standard HD',
      IsAllDay: false
    },
    // Morning Shift - Bed 2
    {
      Id: 2,
      Subject: 'HD Session',
      PatientName: 'Jane Smith',
      PatientId: 'P1002',
      BedId: 2,
      ShiftId: 1,
      StartTime: new Date(2025, 0, 15, 6, 30),
      EndTime: new Date(2025, 0, 15, 10, 30),
      Description: 'Standard hemodialysis - 4 hours',
      SessionType: 'Standard HD',
      IsAllDay: false
    },
    // Morning Shift - Bed 3
    {
      Id: 3,
      Subject: 'Emergency HD',
      PatientName: 'Bob Johnson',
      PatientId: 'P1003',
      BedId: 3,
      ShiftId: 1,
      StartTime: new Date(2025, 0, 15, 7, 0),
      EndTime: new Date(2025, 0, 15, 11, 0),
      Description: 'Emergency hemodialysis',
      SessionType: 'Emergency HD',
      IsAllDay: false
    },
    // Afternoon Shift - Bed 1
    {
      Id: 4,
      Subject: 'HD Session',
      PatientName: 'Alice Williams',
      PatientId: 'P1004',
      BedId: 1,
      ShiftId: 2,
      StartTime: new Date(2025, 0, 15, 14, 0),
      EndTime: new Date(2025, 0, 15, 18, 0),
      Description: 'Standard hemodialysis - 4 hours',
      SessionType: 'Standard HD',
      IsAllDay: false
    },
    // Afternoon Shift - Bed 4
    {
      Id: 5,
      Subject: 'CRRT Session',
      PatientName: 'Charlie Brown',
      PatientId: 'P1005',
      BedId: 4,
      ShiftId: 2,
      StartTime: new Date(2025, 0, 15, 14, 0),
      EndTime: new Date(2025, 0, 15, 22, 0),
      Description: 'Continuous Renal Replacement Therapy',
      SessionType: 'CRRT',
      IsAllDay: false
    },
    // Multiple days - Recurring pattern
    {
      Id: 6,
      Subject: 'HD Session',
      PatientName: 'Diana Prince',
      PatientId: 'P1006',
      BedId: 2,
      ShiftId: 1,
      StartTime: new Date(2025, 0, 16, 6, 0),
      EndTime: new Date(2025, 0, 16, 10, 0),
      Description: 'Mon-Wed-Fri schedule',
      SessionType: 'Standard HD',
      IsAllDay: false,
      RecurrenceRule: 'FREQ=WEEKLY;BYDAY=MO,WE,FR;COUNT=12'
    },
    // Afternoon - Bed 5
    {
      Id: 7,
      Subject: 'PD Session',
      PatientName: 'Ethan Hunt',
      PatientId: 'P1007',
      BedId: 5,
      ShiftId: 2,
      StartTime: new Date(2025, 0, 15, 15, 0),
      EndTime: new Date(2025, 0, 15, 17, 0),
      Description: 'Peritoneal Dialysis training',
      SessionType: 'Peritoneal Dialysis',
      IsAllDay: false
    }
  ];

  public eventSettings: EventSettingsModel = {
    dataSource: this.dialysisSessions,
    fields: {
      id: 'Id',
      subject: { name: 'Subject', title: 'Session Type' },
      startTime: { name: 'StartTime' },
      endTime: { name: 'EndTime' },
      description: { name: 'Description' },
      recurrenceRule: { name: 'RecurrenceRule' }
    }
  };

  ngOnInit() {
    this.totalSessions = this.dialysisSessions.length;
  }

  changeView(viewName: string) {
    const viewMap: { [key: string]: View } = {
      'Day': 'Day',
      'Week': 'Week',
      'Timeline Day': 'TimelineDay',
      'Timeline Week': 'TimelineWeek',
      'Month': 'Month'
    };
    this.currentView = viewMap[viewName];
  }

  onPopupOpen(args: PopupOpenEventArgs): void {
    if (args.type === 'Editor') {
      // Customize the editor popup
      args.cancel = false;
    }
  }
}
