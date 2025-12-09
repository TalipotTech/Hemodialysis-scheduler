import { Component, OnInit, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AIService } from '../../services/ai.service';
import { FormControl } from '@angular/forms';
import { Observable, map, startWith } from 'rxjs';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
  data?: any;
}

interface SavedPrompt {
  id: number;
  promptText: string;
  category: string;
  usageCount: number;
  lastUsedAt?: Date;
  createdAt: Date;
}

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.scss'
})
export class AIChatComponent implements OnInit {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  messages: ChatMessage[] = [];
  userInput = '';
  isAITyping = false;
  showSuggestions = false;
  savedPrompts: SavedPrompt[] = [];
  frequentPrompts: SavedPrompt[] = [];
  promptControl = new FormControl('');
  filteredPrompts!: Observable<SavedPrompt[]>;

  constructor(
    private aiService: AIService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.addWelcomeMessage();
    this.loadSavedPrompts();
    
    this.filteredPrompts = this.promptControl.valueChanges.pipe(
      startWith(''),
      map(value => {
        const filterValue = typeof value === 'string' ? value : '';
        return this._filterPrompts(filterValue);
      })
    );
  }

  private addWelcomeMessage(): void {
    this.messages.push({
      role: 'assistant',
      content: 'Hello! I\'m your HD Scheduler AI Assistant. I can help you with scheduling queries, patient information, and availability checks. Try asking me a question!',
      timestamp: new Date()
    });
  }

  async sendMessage(): Promise<void> {
    if (!this.userInput.trim()) return;

    const userMessage = this.userInput;
    this.userInput = '';
    this.showSuggestions = false;

    // Add user message
    this.messages.push({
      role: 'user',
      content: userMessage,
      timestamp: new Date()
    });

    this.isAITyping = true;
    this.scrollToBottom();

    try {
      // Process with AI
      const response = await this.aiService.processNaturalQuery(userMessage).toPromise();

      if (response && response.success) {
        this.messages.push({
          role: 'assistant',
          content: response.answer,
          timestamp: new Date(),
          data: response.data
        });
      } else {
        throw new Error(response?.error || 'Failed to process query');
      }
    } catch (error: any) {
      console.error('AI query error:', error);
      this.messages.push({
        role: 'assistant',
        content: 'I encountered an error processing your question. Please try rephrasing it or contact support if the problem persists.',
        timestamp: new Date()
      });
    } finally {
      this.isAITyping = false;
      this.scrollToBottom();
    }
  }

  useSuggestion(suggestion: string): void {
    this.userInput = suggestion;
    this.sendMessage();
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop =
          this.scrollContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }

  formatTime(timestamp: Date): string {
    return new Date(timestamp).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatMessageContent(content: string): SafeHtml {
    // Convert markdown-style formatting to HTML
    let formatted = content
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>') // Bold
      .replace(/\*(.*?)\*/g, '<em>$1</em>') // Italic
      .replace(/\n/g, '<br>'); // Line breaks
    
    return this.sanitizer.sanitize(1, formatted) || content;
  }

  isArray(value: any): boolean {
    return Array.isArray(value);
  }

  getObjectKeys(obj: any): string[] {
    if (!obj) return [];
    return Object.keys(obj);
  }

  formatFieldName(key: string): string {
    // Convert camelCase or snake_case to Title Case
    return key
      .replace(/([A-Z])/g, ' $1') // Add space before capital letters
      .replace(/_/g, ' ') // Replace underscores with spaces
      .replace(/^./, str => str.toUpperCase()) // Capitalize first letter
      .trim();
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) return '-';
    if (typeof value === 'boolean') return value ? 'Yes' : 'No';
    if (typeof value === 'number') return value.toLocaleString();
    
    // Check if it's a date string
    if (typeof value === 'string') {
      const date = new Date(value);
      if (!isNaN(date.getTime()) && value.includes('-') && value.length >= 10) {
        return date.toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
          hour: value.includes('T') ? '2-digit' : undefined,
          minute: value.includes('T') ? '2-digit' : undefined
        });
      }
    }
    
    return String(value);
  }

  private async loadSavedPrompts(): Promise<void> {
    try {
      const prompts = await this.aiService.getSavedPrompts().toPromise();
      this.savedPrompts = prompts || [];
      // Get top 6 most frequently used prompts
      this.frequentPrompts = this.savedPrompts
        .sort((a, b) => b.usageCount - a.usageCount)
        .slice(0, 6);
    } catch (error) {
      console.error('Error loading saved prompts:', error);
    }
  }

  private _filterPrompts(value: string): SavedPrompt[] {
    const filterValue = value.toLowerCase();
    return this.savedPrompts.filter(prompt =>
      prompt.promptText.toLowerCase().includes(filterValue) ||
      (prompt.category && prompt.category.toLowerCase().includes(filterValue))
    );
  }

  async usePrompt(prompt: SavedPrompt): Promise<void> {
    this.userInput = prompt.promptText;
    this.promptControl.setValue(prompt.promptText);
    
    // Record usage
    try {
      await this.aiService.recordPromptUsage(prompt.id).toPromise();
      await this.loadSavedPrompts(); // Refresh to update frequent prompts
    } catch (error) {
      console.error('Error recording prompt usage:', error);
    }
    
    // Send message after a brief delay to ensure UI updates
    setTimeout(() => {
      this.sendMessage();
    }, 100);
  }

  async saveCurrentPrompt(): Promise<void> {
    if (!this.userInput.trim()) return;
    
    try {
      await this.aiService.savePrompt(this.userInput, null).toPromise();
      await this.loadSavedPrompts();
      // Don't clear the input, let user send it
    } catch (error) {
      console.error('Error saving prompt:', error);
    }
  }

  async deletePrompt(promptId: number, event: Event): Promise<void> {
    event.stopPropagation();
    try {
      await this.aiService.deletePrompt(promptId).toPromise();
      await this.loadSavedPrompts();
    } catch (error) {
      console.error('Error deleting prompt:', error);
    }
  }

  onPromptSelected(event: any): void {
    const selectedValue = event.option.value;
    if (typeof selectedValue === 'string') {
      this.userInput = selectedValue;
      // Find the prompt object to record usage
      const prompt = this.savedPrompts.find(p => p.promptText === selectedValue);
      if (prompt) {
        this.aiService.recordPromptUsage(prompt.id).subscribe({
          next: () => {
            this.loadSavedPrompts();
          },
          error: (err) => console.error('Error recording usage:', err)
        });
      }
    }
  }
}
