import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-syncfusion-demo-hub',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="demo-hub-container">
      <div class="demo-hub-header">
        <h1>🎯 Syncfusion Essential® JS 2 Demo Center</h1>
        <p class="subtitle">Explore Syncfusion components integrated for Hemodialysis Scheduler</p>
        <div class="license-info">
          <span class="badge">Community Edition</span>
          <span class="badge">Version 31.x.x</span>
          <span class="badge success">Licensed ✓</span>
        </div>
      </div>

      <div class="demo-grid">
        <!-- Scheduler Demo Card -->
        <div class="demo-card scheduler-card">
          <div class="demo-icon">📅</div>
          <h2>Basic Scheduler</h2>
          <p class="demo-description">
            Simple calendar scheduler with drag-and-drop and multiple views 
            for managing dialysis sessions.
          </p>
          
          <div class="features-list">
            <div class="feature">✓ Multiple Views (Day/Week/Month)</div>
            <div class="feature">✓ Drag & Drop Sessions</div>
            <div class="feature">✓ Color-coded Appointments</div>
            <div class="feature">✓ Quick & Simple Setup</div>
          </div>

          <a routerLink="/syncfusion/scheduler" class="demo-button">
            View Basic Scheduler →
          </a>
        </div>

        <!-- Advanced Scheduler Demo Card -->
        <div class="demo-card advanced-scheduler-card">
          <div class="demo-icon">🏥</div>
          <h2>Advanced Scheduler</h2>
          <p class="demo-description">
            Multi-resource scheduler with bed management, shift grouping, 
            and timeline views - Perfect for dialysis centers.
          </p>
          
          <div class="features-list">
            <div class="feature">✓ Resource Grouping (Beds & Shifts)</div>
            <div class="feature">✓ Timeline Views</div>
            <div class="feature">✓ Recurring Sessions</div>
            <div class="feature">✓ Multi-day Scheduling</div>
          </div>

          <a routerLink="/syncfusion/advanced-scheduler" class="demo-button">
            View Advanced Scheduler →
          </a>
        </div>

        <!-- Grid Demo Card -->
        <div class="demo-card grid-card">
          <div class="demo-icon">📊</div>
          <h2>Data Grid Component</h2>
          <p class="demo-description">
            Powerful data grid with sorting, filtering, pagination, and 
            inline editing for patient management and data tables.
          </p>
          
          <div class="features-list">
            <div class="feature">✓ Advanced Filtering</div>
            <div class="feature">✓ Multi-column Sorting</div>
            <div class="feature">✓ Inline Editing</div>
            <div class="feature">✓ Excel/PDF Export</div>
            <div class="feature">✓ Pagination & Search</div>
          </div>

          <a routerLink="/syncfusion/grid" class="demo-button">
            View Grid Demo →
          </a>
        </div>
      </div>

      <!-- Getting Started Section -->
      <div class="getting-started">
        <h2>🚀 Getting Started</h2>
        <div class="steps">
          <div class="step">
            <div class="step-number">1</div>
            <div class="step-content">
              <h3>Explore Demos</h3>
              <p>Click on the cards above to see live demos of each component</p>
            </div>
          </div>
          <div class="step">
            <div class="step-number">2</div>
            <div class="step-content">
              <h3>Test Features</h3>
              <p>Interact with components: drag, drop, filter, sort, and edit</p>
            </div>
          </div>
          <div class="step">
            <div class="step-number">3</div>
            <div class="step-content">
              <h3>Evaluate & Decide</h3>
              <p>Compare with current solution and provide feedback</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Resources Section -->
      <div class="resources-section">
        <h2>📚 Resources & Documentation</h2>
        <div class="resource-links">
          <a href="https://ej2.syncfusion.com/angular/demos/" target="_blank" class="resource-link">
            🌐 Live Demos
          </a>
          <a href="https://ej2.syncfusion.com/angular/documentation/" target="_blank" class="resource-link">
            📖 Documentation
          </a>
          <a href="https://www.syncfusion.com/forums/angular-js2" target="_blank" class="resource-link">
            💬 Community Forums
          </a>
          <a href="https://github.com/syncfusion/ej2-angular-ui-components" target="_blank" class="resource-link">
            📦 GitHub Repository
          </a>
        </div>
      </div>

      <!-- Status Banner -->
      <div class="status-banner">
        <div class="status-icon">✅</div>
        <div class="status-content">
          <strong>Integration Status:</strong> Complete & Ready for Testing
          <br>
          <small>Branch: syncfusion-integration | Build: Successful | License: Active</small>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .demo-hub-container {
      max-width: 1400px;
      margin: 0 auto;
      padding: 40px 20px;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
    }

    .demo-hub-header {
      text-align: center;
      margin-bottom: 50px;
    }

    .demo-hub-header h1 {
      font-size: 2.5rem;
      color: #1976d2;
      margin-bottom: 10px;
    }

    .subtitle {
      font-size: 1.2rem;
      color: #666;
      margin-bottom: 20px;
    }

    .license-info {
      display: flex;
      gap: 10px;
      justify-content: center;
      margin-top: 20px;
    }

    .badge {
      padding: 6px 16px;
      border-radius: 20px;
      background: #e3f2fd;
      color: #1976d2;
      font-size: 0.9rem;
      font-weight: 500;
    }

    .badge.success {
      background: #e8f5e9;
      color: #2e7d32;
    }

    .demo-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 30px;
      margin-bottom: 50px;
    }

    .demo-card {
      background: white;
      border-radius: 12px;
      padding: 30px;
      box-shadow: 0 4px 6px rgba(0,0,0,0.1);
      transition: transform 0.3s, box-shadow 0.3s;
      border-top: 4px solid;
    }

    .scheduler-card {
      border-top-color: #1976d2;
    }

    .advanced-scheduler-card {
      border-top-color: #d32f2f;
    }

    .grid-card {
      border-top-color: #388e3c;
    }

    .demo-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 8px 12px rgba(0,0,0,0.15);
    }

    .demo-icon {
      font-size: 3rem;
      margin-bottom: 15px;
    }

    .demo-card h2 {
      color: #333;
      margin-bottom: 15px;
      font-size: 1.5rem;
    }

    .demo-description {
      color: #666;
      line-height: 1.6;
      margin-bottom: 20px;
    }

    .features-list {
      display: grid;
      gap: 8px;
      margin-bottom: 25px;
    }

    .feature {
      color: #555;
      font-size: 0.95rem;
      padding: 8px 0;
    }

    .demo-button {
      display: inline-block;
      background: linear-gradient(135deg, #1976d2 0%, #1565c0 100%);
      color: white;
      padding: 12px 30px;
      border-radius: 6px;
      text-decoration: none;
      font-weight: 600;
      transition: all 0.3s;
      text-align: center;
    }

    .demo-button:hover {
      background: linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
      transform: translateX(5px);
    }

    .getting-started {
      background: #f5f5f5;
      padding: 40px;
      border-radius: 12px;
      margin-bottom: 40px;
    }

    .getting-started h2 {
      text-align: center;
      color: #333;
      margin-bottom: 30px;
    }

    .steps {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 30px;
    }

    .step {
      display: flex;
      gap: 15px;
      align-items: start;
    }

    .step-number {
      width: 40px;
      height: 40px;
      background: #1976d2;
      color: white;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      font-size: 1.2rem;
      flex-shrink: 0;
    }

    .step-content h3 {
      color: #333;
      margin-bottom: 8px;
    }

    .step-content p {
      color: #666;
      line-height: 1.5;
    }

    .resources-section {
      background: white;
      padding: 40px;
      border-radius: 12px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      margin-bottom: 30px;
    }

    .resources-section h2 {
      text-align: center;
      color: #333;
      margin-bottom: 30px;
    }

    .resource-links {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 15px;
    }

    .resource-link {
      padding: 15px 20px;
      background: #f5f5f5;
      border-radius: 8px;
      text-decoration: none;
      color: #1976d2;
      font-weight: 500;
      transition: all 0.3s;
      text-align: center;
    }

    .resource-link:hover {
      background: #e3f2fd;
      transform: translateY(-2px);
    }

    .status-banner {
      display: flex;
      align-items: center;
      gap: 20px;
      padding: 20px 30px;
      background: linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%);
      border-radius: 12px;
      border-left: 5px solid #2e7d32;
    }

    .status-icon {
      font-size: 2rem;
    }

    .status-content {
      color: #1b5e20;
      line-height: 1.6;
    }

    .status-content strong {
      font-size: 1.1rem;
    }

    .status-content small {
      color: #2e7d32;
    }

    @media (max-width: 768px) {
      .demo-hub-header h1 {
        font-size: 1.8rem;
      }

      .demo-grid {
        grid-template-columns: 1fr;
      }

      .steps {
        grid-template-columns: 1fr;
      }

      .resource-links {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class SyncfusionDemoHubComponent {}
