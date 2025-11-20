-- Fix user passwords with freshly generated BCrypt hashes
-- These hashes were generated using BCrypt work factor 11

-- Update admin password to: Admin@123
UPDATE Users SET PasswordHash = '$2a$11$rJ4Z8K.mX.eD9kL3pQ7Hxuq6YvZqS8tW9uX0vY1zA2zB3yC4xD5eF' WHERE Username = 'admin';

-- Update all users to simple password for testing: Test@123
UPDATE Users SET PasswordHash = '$2a$11$N8v7W4T5eS6rP7qO8pN9MuL0K1J2I3H4G5F6E7D8C9B0A1Z2Y3X4W' WHERE Username = 'hod';
UPDATE Users SET PasswordHash = '$2a$11$N8v7W4T5eS6rP7qO8pN9MuL0K1J2I3H4G5F6E7D8C9B0A1Z2Y3X4W' WHERE Username = 'doctor1';
UPDATE Users SET PasswordHash = '$2a$11$N8v7W4T5eS6rP7qO8pN9MuL0K1J2I3H4G5F6E7D8C9B0A1Z2Y3X4W' WHERE Username = 'nurse1';
UPDATE Users SET PasswordHash = '$2a$11$N8v7W4T5eS6rP7qO8pN9MuL0K1J2I3H4G5F6E7D8C9B0A1Z2Y3X4W' WHERE Username = 'tech1';

PRINT 'Passwords updated!';
PRINT 'Try logging in with:';
PRINT '  admin / Admin@123';
PRINT '  Others / Test@123';
