import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { MatTooltipModule } from '@angular/material/tooltip';

// Equipment usage tracking for dialysis equipment
export interface EquipmentUsageStatus {
  equipmentType: string;
  currentUsageCount: number;
  maxUsageLimit: number;
  remainingUses: number;
  usagePercentage: number;
  status: 'OK' | 'Warning' | 'Critical' | 'Expired';
  message?: string;
  requiresReplacement: boolean;
}

@Component({
  selector: 'app-equipment-usage-alert',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule,
    MatTooltipModule
  ],
  templateUrl: './equipment-usage-alert.component.html',
  styleUrls: ['./equipment-usage-alert.component.scss']
})
export class EquipmentUsageAlertComponent implements OnInit, OnChanges {
  @Input() dialyserCount: number = 0;
  @Input() bloodTubingCount: number = 0;
  @Input() showDetails: boolean = true;

  // Equipment limits
  readonly DIALYSER_MAX = 7;
  readonly BLOOD_TUBING_MAX = 12;
  readonly DIALYSER_WARNING_THRESHOLD = 6;
  readonly BLOOD_TUBING_WARNING_THRESHOLD = 10;

  dialyserStatus: EquipmentUsageStatus | null = null;
  bloodTubingStatus: EquipmentUsageStatus | null = null;
  hasAlerts: boolean = false;

  ngOnInit(): void {
    this.updateStatuses();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['dialyserCount'] || changes['bloodTubingCount']) {
      this.updateStatuses();
    }
  }

  private updateStatuses(): void {
    this.dialyserStatus = this.calculateStatus('Dialyser', this.dialyserCount, this.DIALYSER_MAX, this.DIALYSER_WARNING_THRESHOLD);
    this.bloodTubingStatus = this.calculateStatus('Blood Tubing', this.bloodTubingCount, this.BLOOD_TUBING_MAX, this.BLOOD_TUBING_WARNING_THRESHOLD);
    
    this.hasAlerts = (this.dialyserStatus.status !== 'OK') || (this.bloodTubingStatus.status !== 'OK');
  }

  private calculateStatus(
    equipmentType: string,
    currentCount: number,
    maxLimit: number,
    warningThreshold: number
  ): EquipmentUsageStatus {
    const remainingUses = Math.max(0, maxLimit - currentCount);
    const usagePercentage = Math.round((currentCount / maxLimit) * 100);
    
    let status: 'OK' | 'Warning' | 'Critical' | 'Expired' = 'OK';
    let message = '';
    let requiresReplacement = false;

    if (currentCount >= maxLimit) {
      status = 'Expired';
      requiresReplacement = true;
      message = `⚠️ CRITICAL: ${equipmentType} has reached maximum usage limit (${maxLimit} times). MUST be replaced before next session!`;
    } else if (currentCount > maxLimit) {
      status = 'Expired';
      requiresReplacement = true;
      message = `⛔ DANGER: ${equipmentType} has EXCEEDED maximum usage limit! Current: ${currentCount}, Max: ${maxLimit}. Replace immediately!`;
    } else if (currentCount >= warningThreshold) {
      status = 'Critical';
      message = `⚠️ WARNING: ${equipmentType} is nearing maximum usage. Current: ${currentCount}/${maxLimit}. ${remainingUses} use(s) remaining. Please prepare replacement.`;
    } else if (currentCount >= (warningThreshold - 1)) {
      status = 'Warning';
      message = `ℹ️ NOTICE: ${equipmentType} usage at ${currentCount}/${maxLimit}. ${remainingUses} use(s) remaining.`;
    } else {
      message = `✓ ${equipmentType} usage is normal (${currentCount}/${maxLimit}).`;
    }

    return {
      equipmentType,
      currentUsageCount: currentCount,
      maxUsageLimit: maxLimit,
      remainingUses,
      usagePercentage,
      status,
      message,
      requiresReplacement
    };
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'OK': return 'primary';
      case 'Warning': return 'accent';
      case 'Critical': return 'warn';
      case 'Expired': return 'warn';
      default: return 'primary';
    }
  }

  getProgressBarColor(usagePercentage: number): string {
    if (usagePercentage >= 100) return 'warn';
    if (usagePercentage >= 85) return 'warn';
    if (usagePercentage >= 70) return 'accent';
    return 'primary';
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'OK': return 'check_circle';
      case 'Warning': return 'info';
      case 'Critical': return 'warning';
      case 'Expired': return 'error';
      default: return 'info';
    }
  }
}
