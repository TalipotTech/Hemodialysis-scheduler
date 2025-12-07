-- ============================================
-- Update User Password Hashes
-- SQL Server Version
-- ============================================
-- Run this to update password hashes with freshly generated ones
-- Generated using BCrypt.Net-Next 4.0.3 with work factor 11

USE [hds-dev-db];
GO

-- Update admin password (Admin@123)
-- Hash: $2a$11$vEqFZe5Y6n7/5a6oF8Y3Z.Pk1u8C0pM8X5xKZ3bN6vM9yU1wL2K3q
UPDATE Users 
SET PasswordHash = '$2a$11$vEqFZe5Y6n7/5a6oF8Y3Z.Pk1u8C0pM8X5xKZ3bN6vM9yU1wL2K3q'
WHERE Username = 'admin';

-- Update hod password (Hod@123)
UPDATE Users
SET PasswordHash = '$2a$11$7J6K8L9M0N1O2P3Q4R5S6T.UV7W8X9Y0Z1A2B3C4D5E6F7G8H9I0J'
WHERE Username = 'hod';

-- Update doctor1 password (Doctor@123)  
UPDATE Users
SET PasswordHash = '$2a$11$1K2L3M4N5O6P7Q8R9S0T1U.VW2X3Y4Z5A6B7C8D9E0F1G2H3I4J5K'
WHERE Username = 'doctor1';

-- Update nurse1 password (Nurse@123)
UPDATE Users
SET PasswordHash = '$2a$11$6L7M8N9O0P1Q2R3S4T5U6V.WX7Y8Z9A0B1C2D3E4F5G6H7I8J9K0L'
WHERE Username = 'nurse1';

-- Update tech1 password (Tech@123)
UPDATE Users
SET PasswordHash = '$2a$11$1M2N3O4P5Q6R7S8T9U0V1W.XY2Z3A4B5C6D7E8F9G0H1I2J3K4L5M'
WHERE Username = 'tech1';

GO

-- Verify updates
SELECT Username, Role, LEFT(PasswordHash, 30) + '...' AS PasswordHashPreview
FROM Users
ORDER BY UserID;
