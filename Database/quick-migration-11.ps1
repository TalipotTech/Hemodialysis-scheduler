# Simple Equipment Alerts Migration Script
Write-Host "Adding EquipmentUsageAlerts table..." -ForegroundColor Cyan

$dbPath = ".\Backend\hd_scheduler.db"

# Check if db exists
if (-Not (Test-Path $dbPath)) {
    Write-Host "Database not found at $dbPath" -ForegroundColor Red
    exit 1
}

$sql = @"
CREATE TABLE IF NOT EXISTS EquipmentUsageAlerts (
    AlertID INTEGER PRIMARY KEY AUTOINCREMENT,
    PatientID INTEGER NOT NULL,
    ScheduleID INTEGER,
    EquipmentType TEXT NOT NULL,
    CurrentUsageCount INTEGER NOT NULL,
    MaxUsageLimit INTEGER NOT NULL,
    Severity TEXT NOT NULL,
    AlertMessage TEXT NOT NULL,
    IsAcknowledged INTEGER NOT NULL DEFAULT 0,
    AcknowledgedBy TEXT,
    AcknowledgedAt TEXT,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (ScheduleID) REFERENCES HDSchedules(ScheduleID)
);

CREATE INDEX IF NOT EXISTS idx_equipment_alerts_patient ON EquipmentUsageAlerts(PatientID);
CREATE INDEX IF NOT EXISTS idx_equipment_alerts_schedule ON EquipmentUsageAlerts(ScheduleID);
CREATE INDEX IF NOT EXISTS idx_equipment_alerts_unacknowledged ON EquipmentUsageAlerts(IsAcknowledged, PatientID);
CREATE INDEX IF NOT EXISTS idx_equipment_alerts_severity ON EquipmentUsageAlerts(Severity, IsAcknowledged);
"@

# Save SQL to temp file
$tempSql = ".\Database\temp_migration.sql"
$sql | Out-File -FilePath $tempSql -Encoding UTF8

Write-Host "Table will be created when the backend starts." -ForegroundColor Green
Write-Host "SQL saved to: $tempSql" -ForegroundColor Yellow
Write-Host "`nYou can manually run this SQL if needed." -ForegroundColor Cyan
