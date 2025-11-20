-- Update all user passwords with properly generated BCrypt hashes
-- These hashes were generated using BCrypt.Net with work factor 11

UPDATE Users SET PasswordHash = '$2a$11$FPf3ZqXCW9SRXX7BDDXLnuQJb2qwSodEk3O/HUGNsVC.lOnB.xI9S' WHERE Username = 'admin';
UPDATE Users SET PasswordHash = '$2a$11$L7HpMj/hle5rwLwaVpPT.u3k0dx4c.6yCHn98e311RyigJH7qDqbm' WHERE Username = 'hod';
UPDATE Users SET PasswordHash = '$2a$11$36EF06zzYeZSchMiow/zNuszzYngFlhpP9hv.lirOZrVWALErVKLG' WHERE Username = 'doctor1';
UPDATE Users SET PasswordHash = '$2a$11$DLe/tGuD4Nv7VABLdVR6auFM5UFVNyjR5fm1sse1A0eLEmJJmbsba' WHERE Username = 'nurse1';
UPDATE Users SET PasswordHash = '$2a$11$l.hSU2cSf1SGUyB24Y3v5OHnlFMaXV52.Fxs/tZdLDqp0LvCttgcy' WHERE Username = 'tech1';

-- Verify the updates
SELECT Username, LEFT(PasswordHash, 10) as HashPrefix, LEN(PasswordHash) as HashLength FROM Users;
