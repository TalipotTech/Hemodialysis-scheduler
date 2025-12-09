import { Component, OnInit, HostListener, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService, Theme } from '../../../core/services/theme.service';

interface MenuItem {
  label: string;
  icon: string;
  route?: string;
  children?: MenuItem[];
  roles?: string[];
}

@Component({
  selector: 'app-sidebar-nav',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatToolbarModule,
    MatExpansionModule,
    MatTooltipModule,
    MatMenuModule
  ],
  templateUrl: './sidebar-nav.component.html',
  styleUrls: ['./sidebar-nav.component.scss']
})
export class SidebarNavComponent implements OnInit {
  @ViewChild('sidenav') sidenav!: MatSidenav;
  
  isExpanded = true;
  userRole = '';
  userName = '';
  isMobile = false;
  sidenavMode: 'side' | 'over' = 'side';
  themes: Theme[] = [];
  currentTheme: Theme;
  
  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      route: '/admin',
      roles: ['Admin']
    },
    {
      label: 'Patients',
      icon: 'people',
      roles: ['Admin', 'Doctor', 'Nurse'],
      children: [
        { label: 'Patient List', icon: 'list', route: '/patients' },
        { label: 'New Patient', icon: 'person_add', route: '/patients/new' }
      ]
    },
    {
      label: 'HD Schedule',
      icon: 'event',
      roles: ['Admin', 'Doctor', 'Nurse', 'HOD', 'Technician'],
      children: [
        { label: 'Schedule Log', icon: 'calendar_view_month', route: '/schedule' },
        { label: 'Shift Schedule', icon: 'schedule', route: '/shift-schedule' }
      ]
    },
    {
      label: 'Staff Management',
      icon: 'badge',
      roles: ['Admin', 'HOD'],
      children: [
        { label: 'Staff List', icon: 'group', route: '/admin/staff-management' },
        { label: 'Staffing Status', icon: 'assignment_ind', route: '/admin/staffing-status' }
      ]
    },
    {
      label: 'Reports',
      icon: 'assessment',
      route: '/admin/reports',
      roles: ['Admin', 'HOD']
    },
    {
      label: 'User Management',
      icon: 'manage_accounts',
      route: '/admin/user-management',
      roles: ['Admin']
    },
    {
      label: 'Audit Logs',
      icon: 'history',
      route: '/admin/audit-logs',
      roles: ['Admin']
    },
    {
      label: 'System Settings',
      icon: 'settings',
      route: '/admin/system-settings',
      roles: ['Admin']
    },
    {
      label: 'AI Tools',
      icon: 'auto_awesome',
      roles: ['Admin', 'HOD', 'Doctor', 'Nurse'],
      children: [
        { label: 'AI Assistant', icon: 'smart_toy', route: '/ai-chat', roles: ['Admin', 'HOD', 'Doctor', 'Nurse'] },
        { label: 'Risk Assessment', icon: 'health_and_safety', route: '/risk-assessment', roles: ['Admin', 'HOD', 'Doctor'] },
        { label: 'Generate Reports', icon: 'description', route: '/report-generation', roles: ['Admin', 'HOD'] },
        { label: 'AI Analytics', icon: 'analytics', route: '/analytics-dashboard', roles: ['Admin'] },
        { label: 'AI Settings', icon: 'settings', route: '/admin/ai-settings', roles: ['Admin'] }
      ]
    }
  ];

  filteredMenuItems: MenuItem[] = [];

  constructor(
    private authService: AuthService,
    private router: Router,
    public themeService: ThemeService
  ) {
    this.themes = this.themeService.getThemes();
    this.currentTheme = this.themeService.currentTheme;
  }

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole() || '';
    this.userName = this.authService.getUsername();
    this.filterMenuByRole();
    this.checkScreenSize();
    
    this.themeService.currentTheme$.subscribe(theme => {
      this.currentTheme = theme;
    });

    // Listen for toggle sidebar event from breadcrumb
    window.addEventListener('toggleSidebar', () => {
      this.toggleSidebar();
    });
  }

  @HostListener('window:resize')
  onResize() {
    this.checkScreenSize();
  }

  checkScreenSize(): void {
    const width = window.innerWidth;
    this.isMobile = width <= 768;
    
    if (this.isMobile) {
      this.sidenavMode = 'over';
      this.isExpanded = false; // Close by default on mobile
    } else {
      this.sidenavMode = 'side';
      this.isExpanded = true; // Open by default on desktop
    }
  }

  filterMenuByRole(): void {
    this.filteredMenuItems = this.menuItems.filter(item => {
      if (!item.roles || item.roles.length === 0) return true;
      return item.roles.includes(this.userRole);
    });
  }

  toggleSidebar(): void {
    if (this.isMobile) {
      // On mobile, toggle the drawer open/close
      this.sidenav.toggle();
    } else {
      // On desktop, expand/collapse
      this.isExpanded = !this.isExpanded;
    }
  }

  navigate(route: string): void {
    this.router.navigate([route]);
    
    // Close drawer on mobile after navigation
    if (this.isMobile && this.sidenav) {
      this.sidenav.close();
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  isActive(route: string): boolean {
    return this.router.url === route;
  }

  hasChildren(item: MenuItem): boolean {
    return !!(item.children && item.children.length > 0);
  }

  changeTheme(themeName: string): void {
    this.themeService.setTheme(themeName);
  }
}
