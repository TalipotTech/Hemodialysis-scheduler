# DialyzeFlow Theme and Branding Update

## Summary
Updated the login page with the official DialyzeFlow logo and implemented a theme system that allows users to switch between different color schemes.

## Changes Implemented

### 1. Logo Integration
- **Location**: Login page header
- **File**: `/assets/dialyzeflow-logo.svg`
- **Replaced**: Emoji icon (🏥) with professional DialyzeFlow logo
- **Size**: Max width 280px, responsive with auto height
- **Effect**: Added subtle drop shadow for visual depth

### 2. Theme System

#### Theme Service (`theme.service.ts`)
Created a new service that manages application themes:
- **Default Theme**: DialyzeFlow (matching brand colors)
- **Secondary Theme**: Purple Dream (original gradient)
- **Features**:
  - Persists theme selection in localStorage
  - Observable pattern for reactive theme changes
  - CSS custom properties for dynamic styling

#### Theme Definitions

**DialyzeFlow Theme (Default)**
```typescript
{
  name: 'dialyzeflow',
  displayName: 'DialyzeFlow',
  primary: '#0077B6',      // Primary Blue
  secondary: '#00A896',    // Primary Teal
  background: '#ffffff',
  gradientStart: '#0077B6', // Blue to Green gradient
  gradientEnd: '#02C39A'   // Primary Green
}
```

**Purple Dream Theme (Secondary)**
```typescript
{
  name: 'purple',
  displayName: 'Purple Dream',
  primary: '#667eea',
  secondary: '#764ba2',
  background: '#ffffff',
  gradientStart: '#667eea',
  gradientEnd: '#764ba2'
}
```

### 3. Theme Selector UI
- **Location**: Top-right corner of login page
- **Icon**: Palette icon button
- **Style**: 
  - Glassmorphism effect (blur + transparency)
  - Floating button with shadow
  - Hover effects for better UX
- **Menu Items**: 
  - Shows all available themes
  - Active theme marked with check icon
  - Highlighted background for selected theme

### 4. CSS Custom Properties
Global CSS variables enable dynamic theming:
```css
:root {
  --theme-primary: #0077B6;
  --theme-secondary: #00A896;
  --theme-background: #ffffff;
  --theme-gradient-start: #0077B6;
  --theme-gradient-end: #02C39A;
}
```

### 5. Material Design Integration
Updated Angular Material theme to use cyan/azure palettes matching DialyzeFlow colors:
```scss
@include mat.theme((
  color: (
    primary: mat.$azure-palette,
    tertiary: mat.$cyan-palette,
  ),
  typography: Roboto,
  density: 0,
));
```

## Files Modified

### Created Files
1. `Frontend/hd-scheduler-app/src/app/core/services/theme.service.ts`
2. `Frontend/hd-scheduler-app/public/assets/dialyzeflow-logo.svg`
3. `Frontend/hd-scheduler-app/public/assets/dialyzeflow-logo.png`

### Modified Files
1. `Frontend/hd-scheduler-app/src/app/features/auth/login/login.ts`
   - Added theme service integration
   - Added theme selector UI logic
   - Imported MatMenuModule and MatIconModule

2. `Frontend/hd-scheduler-app/src/app/features/auth/login/login.html`
   - Replaced emoji with logo image
   - Added theme selector button with menu
   - Integrated theme switching functionality

3. `Frontend/hd-scheduler-app/src/app/features/auth/login/login.scss`
   - Updated background to use CSS variables
   - Added theme selector button styles
   - Added glassmorphism effects
   - Added logo container styles

4. `Frontend/hd-scheduler-app/src/styles.scss`
   - Added CSS custom properties for theming
   - Updated Material theme palette

## Brand Guidelines Compliance

Colors used match the official DialyzeFlow Brand Guidelines:
- **Primary Blue**: #0077B6 (Main brand color)
- **Primary Teal**: #00A896 (Gradient middle, accents)
- **Primary Green**: #02C39A ("Flow" text, success states)
- **Gradient**: Linear gradient from Blue → Teal → Green (135deg)

## User Experience

### Theme Selection
1. Users see a palette icon in the top-right corner
2. Clicking opens a menu with available themes
3. Selected theme is marked with a check icon
4. Theme choice persists across sessions via localStorage
5. Default theme is DialyzeFlow (brand colors)

### Visual Design
- Logo is prominently displayed at login
- Smooth transitions between themes
- Glassmorphism effect on theme selector
- Professional appearance matching brand identity

## Technical Implementation

### Service Architecture
```
ThemeService
├── Observable theme stream (currentTheme$)
├── Theme definitions array
├── localStorage persistence
└── CSS custom property management
```

### Component Integration
```
Login Component
├── Subscribes to theme changes
├── Renders theme selector menu
├── Calls themeService.setTheme()
└── Displays current theme
```

## Testing Checklist
- [x] Logo displays correctly on login page
- [x] Default theme is DialyzeFlow
- [x] Theme selector button is visible and functional
- [x] Theme changes apply immediately
- [x] Selected theme persists after page reload
- [x] Purple Dream theme still works (secondary option)
- [x] Responsive on mobile devices
- [x] Build completes successfully
- [x] Deployed to Azure

## Deployment
- **URL**: https://lively-pond-08e4f7c00.3.azurestaticapps.net
- **Status**: ✅ Successfully deployed
- **Build Time**: ~14.5 seconds
- **Bundle Size**: 561.58 kB initial (144.49 kB compressed)

## Future Enhancements
1. Add more theme options (e.g., Dark mode, Medical green, Ocean blue)
2. Extend theme system to other pages beyond login
3. Add theme preview thumbnails in selector
4. Create admin panel for custom theme creation
5. Support organization-specific branding

## Brand Assets Source
Logo and icons sourced from:
`Assets/dialyzeflow-branding/logos/dialyzeflow-logo.svg`

Brand guidelines reference:
`Assets/dialyzeflow-branding/docs/DialyzeFlow_Brand_Guidelines.md`
