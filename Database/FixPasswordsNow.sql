-- Fix password hashes with proper BCrypt format
-- Admin@123, HOD@123, Doctor@123, Nurse@123, Technician@123

UPDATE Users SET PasswordHash = '$2a$11$lWu/DN/p2oZPIwWNVOanCeSdccbzv8J61rrg74H1I28eWuViTfj9W' WHERE Username = 'admin';
UPDATE Users SET PasswordHash = '$2a$11$IEfI4s31fm/r.Gj.VlUFLOez5psIAsjtR3Mk9yY8CUBlarMFtUmYC' WHERE Username = 'hod';
UPDATE Users SET PasswordHash = '$2a$11$FVWMvaw4v/CXOl.9LPand.VSpHixPqbJAfI66bfvCpNZtsL0vXJmi' WHERE Username = 'doctor1';
UPDATE Users SET PasswordHash = '$2a$11$BMwzwCFmNUEe.PkT6XntIOfZ.7SiNiDgbDanOWvK31DamxKnLONE6' WHERE Username = 'nurse1';
UPDATE Users SET PasswordHash = '$2a$11$E3dWVB93ucD3K8jJWBMAOeMDQs4QPsVwbyHkzVlkCROjDcKa0APma' WHERE Username = 'tech1';

SELECT Username, Role, LEFT(PasswordHash, 20) + '...' AS PasswordHashPreview FROM Users;
PRINT 'Passwords updated successfully!';
