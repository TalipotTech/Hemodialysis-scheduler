-- AI Feature Suggestions Table (for developer use)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AIFeatureSuggestions')
BEGIN
    CREATE TABLE AIFeatureSuggestions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        FeatureTitle NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(50) NOT NULL, -- Autocomplete, Workflow, Analytics, UI/UX, Integration
        Priority NVARCHAR(20) NOT NULL, -- High, Medium, Low
        Context NVARCHAR(MAX) NULL,
        Reasoning NVARCHAR(MAX) NULL,
        ImpactScore INT NOT NULL DEFAULT 5, -- 1-10
        ImplementationComplexity INT NOT NULL DEFAULT 5, -- 1-10
        EstimatedEffort NVARCHAR(50) NULL, -- Hours, Days, Weeks
        GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        GeneratedBy NVARCHAR(100) NOT NULL, -- AI model version
        IsReviewed BIT NOT NULL DEFAULT 0,
        IsImplemented BIT NOT NULL DEFAULT 0,
        ImplementedAt DATETIME2 NULL,
        DeveloperNotes NVARCHAR(MAX) NULL,
        UpvoteCount INT NOT NULL DEFAULT 0,
        
        INDEX IX_AIFeatureSuggestions_Category (Category),
        INDEX IX_AIFeatureSuggestions_Priority (Priority),
        INDEX IX_AIFeatureSuggestions_GeneratedAt (GeneratedAt DESC),
        INDEX IX_AIFeatureSuggestions_IsImplemented (IsImplemented)
    );
    
    PRINT 'AIFeatureSuggestions table created successfully';
END
ELSE
BEGIN
    PRINT 'AIFeatureSuggestions table already exists';
END
GO

-- Autocomplete Cache Table (for performance optimization)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AutocompleteCache')
BEGIN
    CREATE TABLE AutocompleteCache (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PatientID INT NOT NULL,
        FieldName NVARCHAR(100) NOT NULL,
        PredictedValue NVARCHAR(MAX) NOT NULL,
        Confidence DECIMAL(5,2) NOT NULL,
        UsageCount INT NOT NULL DEFAULT 0,
        LastUsed DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        
        INDEX IX_AutocompleteCache_PatientField (PatientID, FieldName),
        INDEX IX_AutocompleteCache_ExpiresAt (ExpiresAt)
    );
    
    PRINT 'AutocompleteCache table created successfully';
END
ELSE
BEGIN
    PRINT 'AutocompleteCache table already exists';
END
GO

PRINT 'AI Feature Suggestion migration completed successfully';
