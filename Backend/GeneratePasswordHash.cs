// Quick utility to generate BCrypt password hashes
// Run with: dotnet script GeneratePasswordHash.cs

using System;

public class PasswordHashGenerator
{
    public static void Main()
    {
        var passwords = new[] { "Admin@123", "HOD@123", "Doctor@123", "Nurse@123", "Tech@123" };
        
        foreach (var password in passwords)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Hash: {hash}");
            Console.WriteLine();
        }
    }
}

PasswordHashGenerator.Main();
