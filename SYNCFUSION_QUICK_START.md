# Syncfusion Quick Start Guide

## How to Use the Demo Components

### Step 1: Verify Installation
Ensure all Syncfusion packages are installed:
```bash
cd Frontend/hd-scheduler-app
npm install --legacy-peer-deps
```

### Step 2: Add Demo Components to Your App

You have two demo components ready to use:
- `SyncfusionDemoSchedulerComponent` - Scheduler with sample dialysis sessions
- `SyncfusionDemoGridComponent` - Patient management grid

#### Option A: Add to Routing (Recommended)

Update your `app.routes.ts` or routing configuration:

```typescript
import { Routes } from '@angular/router';
import { SyncfusionDemoSchedulerComponent } from './components/syncfusion-demo-scheduler.component';
import { SyncfusionDemoGridComponent } from './components/syncfusion-demo-grid.component';

export const routes: Routes = [
  {
    path: 'syncfusion-scheduler',
    component: SyncfusionDemoSchedulerComponent
  },
  {
    path: 'syncfusion-grid',
    component: SyncfusionDemoGridComponent
  },
  // ... your other routes
];
```

#### Option B: Add Directly to App Component

Update your `app.ts` or main component:

```typescript
import { Component } from '@angular/core';
import { SyncfusionDemoSchedulerComponent } from './components/syncfusion-demo-scheduler.component';
import { SyncfusionDemoGridComponent } from './components/syncfusion-demo-grid.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    SyncfusionDemoSchedulerComponent,
    SyncfusionDemoGridComponent,
    // ... other imports
  ],
  template: `
    <div class="app-container">
      <h1>Hemodialysis Scheduler - Syncfusion Demo</h1>
      
      <!-- Scheduler Demo -->
      <app-syncfusion-demo-scheduler></app-syncfusion-demo-scheduler>
      
      <hr style="margin: 40px 0;">
      
      <!-- Grid Demo -->
      <app-syncfusion-demo-grid></app-syncfusion-demo-grid>
    </div>
  `,
  styles: [`
    .app-container {
      padding: 20px;
      max-width: 1400px;
      margin: 0 auto;
    }
    
    h1 {
      text-align: center;
      margin-bottom: 30px;
    }
  `]
})
export class App {
  // ...
}
```

### Step 3: Run the Application

```bash
cd Frontend/hd-scheduler-app
npm start
```

Navigate to:
- `http://localhost:4200` - Main app
- `http://localhost:4200/syncfusion-scheduler` - Scheduler demo (if using routing)
- `http://localhost:4200/syncfusion-grid` - Grid demo (if using routing)

### Step 4: Test the Components

#### Scheduler Features to Test:
- ✅ View different calendar views (Day, Week, Month, Agenda)
- ✅ Click and drag to create new appointments
- ✅ Click on existing appointments to view/edit
- ✅ Drag appointments to reschedule
- ✅ Resize appointments to change duration

#### Grid Features to Test:
- ✅ Sort columns by clicking headers
- ✅ Filter data using the filter row
- ✅ Search across all columns
- ✅ Page through records
- ✅ Edit patient records (click edit icon)

## Customization Examples

### Change Scheduler Theme
The scheduler uses the Material theme by default. To use a different theme, update `angular.json`:

Replace:
```json
"node_modules/@syncfusion/ej2-base/styles/material.css"
```

With one of:
```json
"node_modules/@syncfusion/ej2-base/styles/bootstrap5.css"
"node_modules/@syncfusion/ej2-base/styles/tailwind.css"
"node_modules/@syncfusion/ej2-base/styles/fluent.css"
```

Apply the same change to all Syncfusion CSS imports.

### Add More Scheduler Features

Update the scheduler component to add resources (beds):

```typescript
import { ResourcesModel } from '@syncfusion/ej2-angular-schedule';

export class SyncfusionDemoSchedulerComponent {
  public resourceDataSource: Object[] = [
    { BedId: 1, BedName: 'Bed 1', BedColor: '#1e88e5' },
    { BedId: 2, BedName: 'Bed 2', BedColor: '#7cb342' },
    { BedId: 3, BedName: 'Bed 3', BedColor: '#e53935' },
    { BedId: 4, BedName: 'Bed 4', BedColor: '#fb8c00' }
  ];

  public group: GroupModel = {
    resources: ['Beds']
  };

  public resources: ResourcesModel[] = [{
    field: 'BedId',
    title: 'Bed',
    name: 'Beds',
    dataSource: this.resourceDataSource,
    textField: 'BedName',
    idField: 'BedId',
    colorField: 'BedColor'
  }];
}
```

Then update the template:
```html
<ejs-schedule 
  [group]="group"
  [resources]="resources"
  ...>
</ejs-schedule>
```

### Connect to Backend API

Replace the static data with API calls:

```typescript
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';

export class SyncfusionDemoSchedulerComponent {
  private http = inject(HttpClient);
  
  ngOnInit() {
    this.loadScheduleData();
  }
  
  loadScheduleData() {
    this.http.get<any[]>('api/hdschedule')
      .subscribe(data => {
        // Transform backend data to Syncfusion format
        this.eventSettings = {
          dataSource: data.map(item => ({
            Id: item.scheduleId,
            Subject: `HD Session - ${item.patientName}`,
            StartTime: new Date(item.sessionStartTime),
            EndTime: new Date(item.sessionEndTime),
            // ... map other fields
          }))
        };
      });
  }
}
```

## Troubleshooting

### License Banner Appears
If you see "This application was built using a trial version" banner:
1. Check that license is registered in `main.ts`
2. Clear browser cache
3. Restart dev server

### Styles Not Loading
If components look unstyled:
1. Verify CSS imports in `angular.json`
2. Check browser console for 404 errors
3. Run `npm install --legacy-peer-deps` again

### Build Errors
```bash
# Clear everything and reinstall
rm -rf node_modules package-lock.json
npm install --legacy-peer-deps
ng build
```

### TypeScript Errors
Make sure your `tsconfig.json` includes:
```json
{
  "compilerOptions": {
    "skipLibCheck": true
  }
}
```

## Next Steps

1. ✅ Test the demo components
2. ✅ Explore Syncfusion documentation for more features
3. ✅ Plan migration of existing components
4. ✅ Get team feedback
5. ✅ Decide on full integration vs staying with current solution

## Resources

- 📚 [Full Integration Guide](./SYNCFUSION_INTEGRATION_GUIDE.md)
- 🌐 [Syncfusion Angular Demos](https://ej2.syncfusion.com/angular/demos/)
- 📖 [Scheduler Documentation](https://ej2.syncfusion.com/angular/documentation/schedule/getting-started/)
- 📖 [Grid Documentation](https://ej2.syncfusion.com/angular/documentation/grid/getting-started/)

---
**Ready to test!** Run `npm start` in the Frontend/hd-scheduler-app directory and check out the demo components.
