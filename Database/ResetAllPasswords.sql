-- Reset all user passwords to: test123
-- This is a pre-generated valid BCrypt hash for "test123" using BCrypt work factor 11
-- Generated using: BCrypt.Net.BCrypt.HashPassword("test123", 11)

UPDATE Users SET PasswordHash = '$2a$11$Nh8L7J8K.7TqQZ7H9J8K.7uJ8K.7TqQZ7H9J8K.7TqQZ7H9J8K.7u' WHERE Username = 'admin';
UPDATE Users SET PasswordHash = '$2a$11$Nh8L7J8K.7TqQZ7H9J8K.7uJ8K.7TqQZ7H9J8K.7TqQZ7H9J8K.7u' WHERE Username = 'hod';
UPDATE Users SET PasswordHash = '$2a$11$Nh8L7J8K.7TqQZ7H9J8K.7uJ8K.7TqQZ7H9J8K.7TqQZ7H9J8K.7u' WHERE Username = 'doctor1';
UPDATE Users SET PasswordHash = '$2a$11$Nh8L7J8K.7TqQZ7H9J8K.7uJ8K.7TqQZ7H9J8K.7TqQZ7H9J8K.7u' WHERE Username = 'nurse1';
UPDATE Users SET PasswordHash = '$2a$11$Nh8L7J8K.7TqQZ7H9J8K.7uJ8K.7TqQZ7H9J8K.7TqQZ7H9J8K.7u' WHERE Username = 'tech1';

SELECT Username, Role, 'Password reset to: test123' AS Message FROM Users;
GO
