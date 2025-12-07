import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Theme {
  name: string;
  displayName: string;
  primary: string;
  secondary: string;
  background: string;
  gradientStart: string;
  gradientEnd: string;
}

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private themes: Theme[] = [
    {
      name: 'dialyzeflow',
      displayName: 'DialyzeFlow',
      primary: '#0077B6',
      secondary: '#00A896',
      background: '#ffffff',
      gradientStart: '#0077B6',
      gradientEnd: '#02C39A'
    },
    {
      name: 'purple',
      displayName: 'Purple Dream',
      primary: '#667eea',
      secondary: '#764ba2',
      background: '#ffffff',
      gradientStart: '#667eea',
      gradientEnd: '#764ba2'
    }
  ];

  private currentThemeSubject: BehaviorSubject<Theme>;
  public currentTheme$: Observable<Theme>;

  constructor() {
    const savedTheme = localStorage.getItem('selectedTheme');
    const initialTheme = savedTheme 
      ? this.themes.find(t => t.name === savedTheme) || this.themes[0]
      : this.themes[0];
    
    this.currentThemeSubject = new BehaviorSubject<Theme>(initialTheme);
    this.currentTheme$ = this.currentThemeSubject.asObservable();
    this.applyTheme(initialTheme);
  }

  public get currentTheme(): Theme {
    return this.currentThemeSubject.value;
  }

  public getThemes(): Theme[] {
    return this.themes;
  }

  public setTheme(themeName: string): void {
    const theme = this.themes.find(t => t.name === themeName);
    if (theme) {
      this.currentThemeSubject.next(theme);
      localStorage.setItem('selectedTheme', themeName);
      this.applyTheme(theme);
    }
  }

  private applyTheme(theme: Theme): void {
    document.documentElement.style.setProperty('--theme-primary', theme.primary);
    document.documentElement.style.setProperty('--theme-secondary', theme.secondary);
    document.documentElement.style.setProperty('--theme-background', theme.background);
    document.documentElement.style.setProperty('--theme-gradient-start', theme.gradientStart);
    document.documentElement.style.setProperty('--theme-gradient-end', theme.gradientEnd);
  }
}
