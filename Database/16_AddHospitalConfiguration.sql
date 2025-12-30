-- Add Hospital Configuration Table for Bed Naming Patterns
-- This allows hospitals to choose their preferred bed naming format

CREATE TABLE IF NOT EXISTS HospitalConfiguration (
    ConfigID INTEGER PRIMARY KEY AUTOINCREMENT,
    ConfigKey TEXT NOT NULL UNIQUE,
    ConfigValue TEXT NOT NULL,
    Description TEXT,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT (datetime('now')),
    UpdatedAt TEXT DEFAULT (datetime('now'))
);

-- Insert default bed naming configuration
INSERT OR REPLACE INTO HospitalConfiguration (ConfigKey, ConfigValue, Description) VALUES
('BedNamingPattern', 'NUMERIC', 'Bed naming format: NUMERIC, PREFIXED_NUMERIC, ALPHA_NUMERIC, ALPHABETIC, CUSTOM'),
('BedPrefix', 'Bed', 'Prefix for bed names (used with PREFIXED_NUMERIC or CUSTOM)'),
('BedsPerGroup', '5', 'Number of beds per letter group (used with ALPHA_NUMERIC pattern)'),
('BedCustomFormat', 'Bed {n}', 'Custom format string: {n}=number, {a}=letter, {g}=group');

-- Verify
SELECT * FROM HospitalConfiguration WHERE ConfigKey LIKE 'Bed%';
