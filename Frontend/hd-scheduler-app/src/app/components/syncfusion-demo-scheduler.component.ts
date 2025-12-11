import { Component } from '@angular/core';
import { ScheduleModule, DayService, WeekService, WorkWeekService, MonthService, AgendaService, EventSettingsModel } from '@syncfusion/ej2-angular-schedule';

@Component({
  selector: 'app-syncfusion-demo-scheduler',
  standalone: true,
  imports: [ScheduleModule],
  providers: [DayService, WeekService, WorkWeekService, MonthService, AgendaService],
  template: `
    <div class="syncfusion-scheduler-wrapper">
      <h2>Hemodialysis Scheduler - Syncfusion Demo</h2>
      <ejs-schedule 
        width='100%' 
        height='650px' 
        [selectedDate]="selectedDate"
        [eventSettings]="eventSettings"
        [views]="views"
        [currentView]="currentView">
      </ejs-schedule>
    </div>
  `,
  styles: [`
    .syncfusion-scheduler-wrapper {
      padding: 20px;
    }
    
    h2 {
      margin-bottom: 20px;
      color: #333;
    }
  `]
})
export class SyncfusionDemoSchedulerComponent {
  public selectedDate: Date = new Date(2025, 0, 15);
  public currentView: string = 'Week';
  public views: Array<string> = ['Day', 'Week', 'WorkWeek', 'Month', 'Agenda'];
  
  // Sample dialysis session data
  public eventSettings: EventSettingsModel = {
    dataSource: [
      {
        Id: 1,
        Subject: 'HD Session - John Doe (Bed 1)',
        StartTime: new Date(2025, 0, 15, 8, 0),
        EndTime: new Date(2025, 0, 15, 12, 0),
        IsAllDay: false,
        CategoryColor: '#1e88e5'
      },
      {
        Id: 2,
        Subject: 'HD Session - Jane Smith (Bed 2)',
        StartTime: new Date(2025, 0, 15, 8, 30),
        EndTime: new Date(2025, 0, 15, 12, 30),
        IsAllDay: false,
        CategoryColor: '#7cb342'
      },
      {
        Id: 3,
        Subject: 'HD Session - Bob Johnson (Bed 3)',
        StartTime: new Date(2025, 0, 15, 13, 0),
        EndTime: new Date(2025, 0, 15, 17, 0),
        IsAllDay: false,
        CategoryColor: '#e53935'
      },
      {
        Id: 4,
        Subject: 'HD Session - Alice Williams (Bed 1)',
        StartTime: new Date(2025, 0, 16, 8, 0),
        EndTime: new Date(2025, 0, 16, 12, 0),
        IsAllDay: false,
        CategoryColor: '#1e88e5'
      },
      {
        Id: 5,
        Subject: 'HD Session - Charlie Brown (Bed 4)',
        StartTime: new Date(2025, 0, 16, 14, 0),
        EndTime: new Date(2025, 0, 16, 18, 0),
        IsAllDay: false,
        CategoryColor: '#fb8c00'
      }
    ]
  };
}
