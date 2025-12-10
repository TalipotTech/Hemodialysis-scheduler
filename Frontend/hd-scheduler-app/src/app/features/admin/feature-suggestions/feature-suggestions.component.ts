import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { AIService } from '../../../services/ai.service';

@Component({
  selector: 'app-feature-suggestions',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    FormsModule
  ],
  templateUrl: './feature-suggestions.component.html',
  styleUrl: './feature-suggestions.component.scss'
})
export class FeatureSuggestionsComponent implements OnInit {
  loading = false;
  generating = false;
  suggestions: any[] = [];
  filteredSuggestions: any[] = [];
  stats: any = null;
  selectedCategory = 'All';
  selectedPriority = 'All';
  
  categories = ['All', 'Autocomplete', 'Workflow', 'Analytics', 'UI/UX', 'Integration', 'Other'];
  priorities = ['All', 'High', 'Medium', 'Low'];
  
  displayedColumns = ['title', 'category', 'priority', 'impact', 'complexity', 'effort', 'generated', 'status', 'actions'];

  constructor(
    private aiService: AIService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadSuggestions();
    this.loadStats();
  }

  loadSuggestions(): void {
    this.loading = true;
    this.aiService.getPendingFeatureSuggestions(this.selectedCategory === 'All' ? undefined : this.selectedCategory).subscribe({
      next: (response: any) => {
        this.suggestions = response;
        this.applyFilters();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading suggestions:', error);
        this.snackBar.open('⚠️ Could not load feature suggestions', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  loadStats(): void {
    this.aiService.getFeatureSuggestionStats().subscribe({
      next: (response: any) => {
        this.stats = response;
      },
      error: (error) => {
        console.error('Error loading stats:', error);
      }
    });
  }

  generateNewSuggestions(): void {
    this.generating = true;
    this.aiService.analyzeAndSuggestFeatures().subscribe({
      next: (response: any) => {
        this.snackBar.open(`✨ Generated ${response.length} new feature suggestions!`, 'View', { duration: 5000 })
          .onAction().subscribe(() => {
            this.loadSuggestions();
            this.loadStats();
          });
        this.generating = false;
      },
      error: (error) => {
        console.error('Error generating suggestions:', error);
        this.snackBar.open('⚠️ Could not generate suggestions', 'Close', { duration: 3000 });
        this.generating = false;
      }
    });
  }

  applyFilters(): void {
    this.filteredSuggestions = this.suggestions.filter(s => {
      const categoryMatch = this.selectedCategory === 'All' || s.category === this.selectedCategory;
      const priorityMatch = this.selectedPriority === 'All' || s.priority === this.selectedPriority;
      return categoryMatch && priorityMatch;
    });
  }

  onCategoryChange(): void {
    this.applyFilters();
  }

  onPriorityChange(): void {
    this.applyFilters();
  }

  markAsImplemented(suggestion: any): void {
    const notes = prompt('Enter implementation notes (optional):');
    if (notes === null) return; // User cancelled

    this.aiService.updateFeatureSuggestionStatus(suggestion.id, 'Implemented', notes || '').subscribe({
      next: () => {
        this.snackBar.open('✅ Marked as implemented', 'Close', { duration: 2000 });
        this.loadSuggestions();
        this.loadStats();
      },
      error: (error) => {
        console.error('Error updating status:', error);
        this.snackBar.open('⚠️ Could not update status', 'Close', { duration: 3000 });
      }
    });
  }

  viewDetails(suggestion: any): void {
    alert(`Feature: ${suggestion.featureTitle}\n\nDescription:\n${suggestion.description}\n\nReasoning:\n${suggestion.reasoning}\n\nContext:\n${suggestion.context || 'N/A'}`);
  }

  getPriorityColor(priority: string): string {
    switch (priority) {
      case 'High': return 'warn';
      case 'Medium': return 'accent';
      case 'Low': return 'primary';
      default: return '';
    }
  }

  getCategoryIcon(category: string): string {
    switch (category) {
      case 'Autocomplete': return 'auto_awesome';
      case 'Workflow': return 'account_tree';
      case 'Analytics': return 'analytics';
      case 'UI/UX': return 'brush';
      case 'Integration': return 'integration_instructions';
      default: return 'lightbulb';
    }
  }
}
