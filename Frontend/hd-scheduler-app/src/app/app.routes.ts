import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/dashboard/admin-dashboard/admin-dashboard').then(m => m.AdminDashboard),
    canActivate: [authGuard],
    data: { roles: ['Admin'], breadcrumb: 'Admin Dashboard' }
  },
  {
    path: 'hod',
    loadComponent: () => import('./features/dashboard/hod-dashboard/hod-dashboard').then(m => m.HodDashboard),
    canActivate: [authGuard],
    data: { roles: ['HOD'] }
  },
  {
    path: 'doctor',
    loadComponent: () => import('./features/dashboard/doctor-dashboard/doctor-dashboard').then(m => m.DoctorDashboard),
    canActivate: [authGuard],
    data: { roles: ['Doctor'] }
  },
  {
    path: 'nurse',
    loadComponent: () => import('./features/dashboard/nurse-dashboard/nurse-dashboard').then(m => m.NurseDashboard),
    canActivate: [authGuard],
    data: { roles: ['Nurse'] }
  },
  {
    path: 'technician',
    loadComponent: () => import('./features/dashboard/technician-view/technician-view').then(m => m.TechnicianView),
    canActivate: [authGuard],
    data: { roles: ['Technician'] }
  },
  {
    path: 'staff',
    loadComponent: () => import('./features/dashboard/staff-entry/staff-entry').then(m => m.StaffEntry),
    canActivate: [authGuard],
    data: { roles: ['Doctor', 'Nurse'] }
  },
  {
    path: 'patients',
    loadComponent: () => import('./features/patients/patient-list/patient-list').then(m => m.PatientList),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'], breadcrumb: 'Patients' }
  },
  {
    path: 'patients/new',
    loadComponent: () => import('./features/patients/patient-form/patient-form').then(m => m.PatientForm),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'], breadcrumb: 'New Patient' }
  },
  {
    path: 'patients/:id',
    loadComponent: () => import('./features/patients/patient-form/patient-form').then(m => m.PatientForm),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'], breadcrumb: 'Edit Patient' }
  },
  {
    path: 'patients/:id/hd-session',
    loadComponent: () => import('./features/patients/hd-session-form/hd-session-form.component').then(m => m.HdSessionFormComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'], breadcrumb: 'Schedule HD Session' }
  },
  {
    path: 'patients/:id/history',
    loadComponent: () => import('./features/patients/patient-history/patient-history.component').then(m => m.PatientHistoryComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse', 'HOD', 'Technician'], breadcrumb: 'Patient History' }
  },
  {
    path: 'patients/:id/session/:scheduleId',
    loadComponent: () => import('./features/patients/session-details/session-details.component').then(m => m.SessionDetailsComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse', 'HOD', 'Technician'], breadcrumb: 'Session Details' }
  },
  {
    path: 'patients/:patientId/post-schedule/:scheduleId',
    loadComponent: () => import('./features/patients/post-schedule/post-schedule.component').then(m => m.PostScheduleComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'], breadcrumb: 'Post-Dialysis Data Entry' }
  },
  {
    path: 'patients/:id/monitoring/:scheduleId',
    loadComponent: () => import('./features/schedule/vital-monitoring/vital-monitoring.component').then(m => m.VitalMonitoringComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse', 'Technician'], breadcrumb: 'Vital Monitoring' }
  },
  {
    path: 'patients/:id/workflow/:scheduleId',
    loadComponent: () => import('./features/schedule/dialysis-workflow/dialysis-workflow.component').then(m => m.DialysisWorkflowComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'], breadcrumb: 'Dialysis Workflow' }
  },
  {
    path: 'schedule/hd-session/new/:patientId',
    loadComponent: () => import('./features/schedule/hd-session-schedule/hd-session-schedule.component').then(m => m.HdSessionScheduleComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse'] } // Admin, Doctor and Nurse can schedule HD sessions
  },
  {
    path: 'schedule/hd-session/edit/:scheduleId',
    loadComponent: () => import('./features/schedule/hd-session-schedule/hd-session-schedule.component').then(m => m.HdSessionScheduleComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'Doctor', 'Nurse', 'Technician'] } // All staff can view and edit active HD sessions
  },
  {
    path: 'schedule',
    loadComponent: () => import('./features/schedule/schedule-grid/schedule-grid').then(m => m.ScheduleGrid),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD', 'Doctor', 'Nurse', 'Technician'], breadcrumb: 'HD Schedule' }
  },
  {
    path: 'admin/user-management',
    loadComponent: () => import('./components/user-management/user-management.component').then(m => m.UserManagementComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin'], breadcrumb: 'User Management' }
  },
  {
    path: 'admin/staff-management',
    loadComponent: () => import('./components/staff-management/staff-management.component').then(m => m.StaffManagementComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD'], breadcrumb: 'Staff Management' }
  },
  {
    path: 'shift-schedule',
    loadComponent: () => import('./components/shift-schedule/shift-schedule.component').then(m => m.ShiftScheduleComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD', 'Doctor', 'Nurse', 'Technician'], breadcrumb: 'Shift Schedule' }
  },
  {
    path: 'admin/system-settings',
    loadComponent: () => import('./components/system-settings/system-settings.component').then(m => m.SystemSettingsComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin'], breadcrumb: 'System Settings' }
  },
  {
    path: 'admin/ai-settings',
    loadComponent: () => import('./components/ai-settings/ai-settings.component').then(m => m.AISettingsComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin'], breadcrumb: 'AI Integration' }
  },
  {
    path: 'ai-chat',
    loadComponent: () => import('./components/ai-chat/ai-chat.component').then(m => m.AIChatComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD', 'Doctor', 'Nurse'], breadcrumb: 'AI Assistant' }
  },
  {
    path: 'risk-assessment',
    loadComponent: () => import('./components/risk-assessment/risk-assessment.component').then(m => m.RiskAssessmentComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD', 'Doctor'], breadcrumb: 'Risk Assessment' }
  },
  {
    path: 'report-generation',
    loadComponent: () => import('./components/report-generation/report-generation.component').then(m => m.ReportGenerationComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD'], breadcrumb: 'Report Generation' }
  },
  {
    path: 'analytics-dashboard',
    loadComponent: () => import('./components/analytics-dashboard/analytics-dashboard.component').then(m => m.AnalyticsDashboardComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin'], breadcrumb: 'AI Analytics' }
  },
  {
    path: 'admin/reports',
    loadComponent: () => import('./components/reports/reports.component').then(m => m.ReportsComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin', 'HOD'], breadcrumb: 'Reports' }
  },
  {
    path: 'admin/audit-logs',
    loadComponent: () => import('./components/audit-logs/audit-logs.component').then(m => m.AuditLogsComponent),
    canActivate: [authGuard],
    data: { roles: ['Admin'], breadcrumb: 'Audit Logs' }
  },
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
