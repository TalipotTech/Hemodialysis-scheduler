import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { Location } from '@angular/common';
import { SystemSettingsService, BedCapacity, SystemParameters } from '../../services/system-settings.service';
import { BedFormatterService, BedNamingConfig, BedPatternOption } from '../../services/bed-formatter.service';

@Component({
  selector: 'app-system-settings',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule,
    MatCardModule, 
    MatButtonModule, 
    MatIconModule, 
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatSelectModule,
    MatChipsModule
  ],
  templateUrl: './system-settings.component.html',
  styleUrls: ['./system-settings.component.scss']
})
export class SystemSettingsComponent implements OnInit {
  loading = false;
  bedCapacities: BedCapacity[] = [];
  systemParameters: SystemParameters | null = null;
  editingSlotId: number | null = null;
  tempMaxBeds: { [key: number]: number } = {};

  // Bed Naming Configuration
  bedNamingConfig: BedNamingConfig | null = null;
  availablePatterns: BedPatternOption[] = [];
  previewBeds: string[] = [];
  bedNamingLoading = false;
  selectedPattern: string = 'NUMERIC';
  bedPrefix: string = 'Bed';
  bedsPerGroup: number = 5;
  customFormat: string = 'Bed {n}';

  constructor(
    private settingsService: SystemSettingsService,
    private bedFormatterService: BedFormatterService,
    private location: Location,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadBedCapacity();
    this.loadSystemParameters();
    this.loadBedNamingConfiguration();
  }

  loadBedCapacity(): void {
    this.loading = true;
    this.settingsService.getBedCapacity().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.bedCapacities = response.data;
          // Initialize temp values
          this.bedCapacities.forEach(slot => {
            this.tempMaxBeds[slot.slotID] = slot.maxBeds;
          });
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading bed capacity:', error);
        this.snackBar.open('Failed to load bed capacity', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  loadSystemParameters(): void {
    this.settingsService.getSystemParameters().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.systemParameters = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading system parameters:', error);
      }
    });
  }

  startEdit(slotId: number): void {
    this.editingSlotId = slotId;
  }

  cancelEdit(slotId: number): void {
    // Reset to original value
    const slot = this.bedCapacities.find(s => s.slotID === slotId);
    if (slot) {
      this.tempMaxBeds[slotId] = slot.maxBeds;
    }
    this.editingSlotId = null;
  }

  incrementBeds(slotId: number): void {
    if (this.tempMaxBeds[slotId] < 100) {
      this.tempMaxBeds[slotId]++;
    }
  }

  decrementBeds(slotId: number): void {
    const slot = this.bedCapacities.find(s => s.slotID === slotId);
    if (slot && this.tempMaxBeds[slotId] > slot.usedBeds) {
      this.tempMaxBeds[slotId]--;
    }
  }

  saveBedCapacity(slotId: number): void {
    const newCapacity = this.tempMaxBeds[slotId];
    const slot = this.bedCapacities.find(s => s.slotID === slotId);

    if (!slot) return;

    if (newCapacity < slot.usedBeds) {
      this.snackBar.open(
        `Cannot set capacity below current usage (${slot.usedBeds} beds in use)`,
        'Close',
        { duration: 4000 }
      );
      return;
    }

    if (newCapacity < 1 || newCapacity > 100) {
      this.snackBar.open('Bed capacity must be between 1 and 100', 'Close', { duration: 3000 });
      return;
    }

    this.loading = true;
    this.settingsService.updateBedCapacity(slotId, { maxBeds: newCapacity }).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Bed capacity updated successfully', 'Close', { duration: 3000 });
          this.editingSlotId = null;
          this.loadBedCapacity();
          this.loadSystemParameters();
        } else {
          this.snackBar.open(response.message || 'Failed to update bed capacity', 'Close', { duration: 3000 });
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error updating bed capacity:', error);
        this.snackBar.open(
          error.error?.message || 'Failed to update bed capacity',
          'Close',
          { duration: 3000 }
        );
        this.loading = false;
      }
    });
  }
  goBack(): void {
    this.location.back();
  }

  // Bed Naming Configuration Methods
  loadBedNamingConfiguration(): void {
    this.bedNamingLoading = true;
    this.bedFormatterService.getBedNamingConfiguration().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.bedNamingConfig = response.data;
          this.availablePatterns = response.data.availablePatterns || [];
          this.selectedPattern = response.data.pattern;
          this.bedPrefix = response.data.prefix;
          this.bedsPerGroup = response.data.bedsPerGroup;
          this.customFormat = response.data.customFormat;
          this.updatePreview();
        }
        this.bedNamingLoading = false;
      },
      error: (error) => {
        console.error('Error loading bed naming configuration:', error);
        this.snackBar.open('Failed to load bed naming configuration', 'Close', { duration: 3000 });
        this.bedNamingLoading = false;
      }
    });
  }

  onPatternChange(): void {
    this.updatePreview();
  }

  onConfigChange(): void {
    this.updatePreview();
  }

  updatePreview(): void {
    const config: BedNamingConfig = {
      pattern: this.selectedPattern,
      prefix: this.bedPrefix,
      bedsPerGroup: this.bedsPerGroup,
      customFormat: this.customFormat
    };
    this.previewBeds = this.bedFormatterService.generatePreview(config, 10);
  }

  saveBedNamingConfiguration(): void {
    this.bedNamingLoading = true;
    const request = {
      pattern: this.selectedPattern,
      prefix: this.bedPrefix,
      bedsPerGroup: this.bedsPerGroup,
      customFormat: this.customFormat
    };

    this.bedFormatterService.updateBedNamingConfiguration(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('✓ Bed naming configuration updated successfully', 'Close', { duration: 3000 });
          this.loadBedNamingConfiguration();
        } else {
          this.snackBar.open(response.message || 'Failed to update configuration', 'Close', { duration: 3000 });
        }
        this.bedNamingLoading = false;
      },
      error: (error) => {
        console.error('Error updating bed naming configuration:', error);
        this.snackBar.open('Failed to update configuration', 'Close', { duration: 3000 });
        this.bedNamingLoading = false;
      }
    });
  }

  resetToDefault(): void {
    this.selectedPattern = 'NUMERIC';
    this.bedPrefix = 'Bed';
    this.bedsPerGroup = 5;
    this.customFormat = 'Bed {n}';
    this.updatePreview();
  }

  getPatternDescription(pattern: string): string {
    const option = this.availablePatterns.find(p => p.value === pattern);
    return option?.description || '';
  }
}
