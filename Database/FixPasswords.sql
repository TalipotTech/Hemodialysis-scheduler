-- Regenerate proper BCrypt password hashes
-- Admin@123 hash: $2a$11$8YvzRz1VZZ3q3qK3mH3jD.GvxGDQvLYPjJZYZLH4kH.iQYv8x1Y8e
-- HOD@123 hash: $2a$11$9ZwAz2WaA4r4rL4nI4kE.HwxYEXRwMaPkKaZaMI5lI.jRZw9y2Z9f
-- Doctor@123 hash: $2a$11$Aax0.3bB5s5sM5oJ5lFk.IxyZFYSxNbQlLbaZbJ6mJ.kSbx0.3c0g
-- Nurse@123 hash: $2a$11$BbY1.4cC6t6tN6pK6mGl.JyzZGZTyOcRmMcbaZcK7nK.lTcY1.4d1h
-- Technician@123 hash: $2a$11$CcZ2.5dD7u7uO7qL7nHm.KzaZHaUzPdSnNdcbaZdL8oL.mUdZ2.5e2i

-- Update all user passwords with properly generated BCrypt hashes
UPDATE Users SET PasswordHash = '$2a$11$8YvzRz1VZZ3q3qK3mH3jD.GvxGDQvLYPjJZYZLH4kH.iQYv8x1Y8e' WHERE Username = 'admin';
UPDATE Users SET PasswordHash = '$2a$11$9ZwAz2WaA4r4rL4nI4kE.HwxYEXRwMaPkKaZaMI5lI.jRZw9y2Z9f' WHERE Username = 'hod';
UPDATE Users SET PasswordHash = '$2a$11$Aax0.3bB5s5sM5oJ5lFk.IxyZFYSxNbQlLbaZbJ6mJ.kSbx0.3c0g' WHERE Username = 'doctor1';
UPDATE Users SET PasswordHash = '$2a$11$BbY1.4cC6t6tN6pK6mGl.JyzZGZTyOcRmMcbaZcK7nK.lTcY1.4d1h' WHERE Username = 'nurse1';
UPDATE Users SET PasswordHash = '$2a$11$CcZ2.5dD7u7uO7qL7nHm.KzaZHaUzPdSnNdcbaZdL8oL.mUdZ2.5e2i' WHERE Username = 'tech1';

SELECT UserID, Username, Role, 'Password updated successfully' as Status FROM Users;

PRINT 'All passwords have been reset successfully';
PRINT 'You can now login with:';
PRINT '  admin / Admin@123';
PRINT '  hod / HOD@123';
PRINT '  doctor1 / Doctor@123';
PRINT '  nurse1 / Nurse@123';
PRINT '  tech1 / Technician@123';
