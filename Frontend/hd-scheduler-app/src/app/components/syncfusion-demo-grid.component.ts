import { Component } from '@angular/core';
import { GridModule, PageService, SortService, FilterService, ToolbarService, EditService } from '@syncfusion/ej2-angular-grids';

interface Patient {
  id: number;
  name: string;
  age: number;
  dialysisType: string;
  frequency: string;
  lastSession: Date;
  nextSession: Date;
  status: string;
}

@Component({
  selector: 'app-syncfusion-demo-grid',
  standalone: true,
  imports: [GridModule],
  providers: [PageService, SortService, FilterService, ToolbarService, EditService],
  template: `
    <div class="syncfusion-grid-wrapper">
      <h2>Patient Management - Syncfusion Grid Demo</h2>
      <ejs-grid 
        [dataSource]='patients' 
        [allowPaging]='true' 
        [allowSorting]='true'
        [allowFiltering]='true'
        [filterSettings]='filterSettings'
        [pageSettings]='pageSettings'
        [toolbar]='toolbar'
        [editSettings]='editSettings'>
        <e-columns>
          <e-column field='id' headerText='Patient ID' width='100' [isPrimaryKey]='true'></e-column>
          <e-column field='name' headerText='Patient Name' width='150'></e-column>
          <e-column field='age' headerText='Age' width='80' textAlign='Right'></e-column>
          <e-column field='dialysisType' headerText='Dialysis Type' width='130'></e-column>
          <e-column field='frequency' headerText='Frequency' width='120'></e-column>
          <e-column field='lastSession' headerText='Last Session' width='130' type='date' format='yMd'></e-column>
          <e-column field='nextSession' headerText='Next Session' width='130' type='date' format='yMd'></e-column>
          <e-column field='status' headerText='Status' width='100'></e-column>
        </e-columns>
      </ejs-grid>
    </div>
  `,
  styles: [`
    .syncfusion-grid-wrapper {
      padding: 20px;
    }
    
    h2 {
      margin-bottom: 20px;
      color: #333;
    }
  `]
})
export class SyncfusionDemoGridComponent {
  public patients: Patient[] = [
    {
      id: 1001,
      name: 'John Doe',
      age: 65,
      dialysisType: 'Hemodialysis',
      frequency: '3x per week',
      lastSession: new Date(2025, 0, 10),
      nextSession: new Date(2025, 0, 13),
      status: 'Active'
    },
    {
      id: 1002,
      name: 'Jane Smith',
      age: 58,
      dialysisType: 'Hemodialysis',
      frequency: '3x per week',
      lastSession: new Date(2025, 0, 11),
      nextSession: new Date(2025, 0, 14),
      status: 'Active'
    },
    {
      id: 1003,
      name: 'Bob Johnson',
      age: 72,
      dialysisType: 'Hemodialysis',
      frequency: '2x per week',
      lastSession: new Date(2025, 0, 9),
      nextSession: new Date(2025, 0, 16),
      status: 'Active'
    },
    {
      id: 1004,
      name: 'Alice Williams',
      age: 54,
      dialysisType: 'Hemodialysis',
      frequency: '3x per week',
      lastSession: new Date(2025, 0, 10),
      nextSession: new Date(2025, 0, 13),
      status: 'Active'
    },
    {
      id: 1005,
      name: 'Charlie Brown',
      age: 67,
      dialysisType: 'Hemodialysis',
      frequency: '3x per week',
      lastSession: new Date(2025, 0, 11),
      nextSession: new Date(2025, 0, 14),
      status: 'Active'
    },
    {
      id: 1006,
      name: 'Diana Prince',
      age: 61,
      dialysisType: 'Hemodialysis',
      frequency: '2x per week',
      lastSession: new Date(2025, 0, 8),
      nextSession: new Date(2025, 0, 15),
      status: 'Active'
    },
    {
      id: 1007,
      name: 'Ethan Hunt',
      age: 59,
      dialysisType: 'Hemodialysis',
      frequency: '3x per week',
      lastSession: new Date(2025, 0, 10),
      nextSession: new Date(2025, 0, 13),
      status: 'Inactive'
    },
    {
      id: 1008,
      name: 'Fiona Carter',
      age: 63,
      dialysisType: 'Hemodialysis',
      frequency: '3x per week',
      lastSession: new Date(2025, 0, 11),
      nextSession: new Date(2025, 0, 14),
      status: 'Active'
    }
  ];

  public pageSettings: Object = { pageSize: 10 };
  public filterSettings: Object = { type: 'Excel' };
  public toolbar: string[] = ['Search', 'ExcelExport', 'PdfExport'];
  public editSettings: Object = { 
    allowEditing: true, 
    allowAdding: true, 
    allowDeleting: true,
    mode: 'Dialog' 
  };
}
