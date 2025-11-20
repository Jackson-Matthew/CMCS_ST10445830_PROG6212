using CMCS_ST10445830.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS_ST10445830.Data
{
    public static class SeedData
    {
        public static async Task Initialize(AuthDbContext context)
        {
            // Only seed if no users exist (to avoid conflicts with your SQL data)
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "Lecturer01",
                        PasswordHash = "Lecturerpass123",
                        Role = "Lecturer"
                    },
                    new User
                    {
                        Username = "AC01",
                        PasswordHash = "AC123",
                        Role = "Academic coordinator"  // Match your SQL role
                    },
                    new User
                    {
                        Username = "HR01",
                        PasswordHash = "HR123",
                        Role = "HR"  // Match your SQL role
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
    }
}