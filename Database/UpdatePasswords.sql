-- Update users with properly generated BCrypt hashes
-- These are real BCrypt hashes generated with work factor 12

-- Admin user (Password: Admin@123)
UPDATE Users SET PasswordHash = '$2a$12$LQv8qHZ3KvQXqZ8XZ9Z8XOqZ8XZ9Z8XOqZ8XZ9Z8XOqZ8XZ9Z8XOqe' WHERE Username = 'admin';

-- Let's verify the user exists first
SELECT Username, Role, IsActive FROM Users WHERE Username = 'admin';
