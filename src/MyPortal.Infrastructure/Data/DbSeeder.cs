using Microsoft.EntityFrameworkCore;
using MyPortal.Core.Entities;
using MyPortal.Infrastructure.Data;

namespace MyPortal.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Statuses
        if (!await context.Statuses.AnyAsync())
        {
            var statuses = new[]
            {
                new Status { Name = "Active", Description = "Active status" },
                new Status { Name = "Inactive", Description = "Inactive status" },
                new Status { Name = "Pending", Description = "Pending status" },
                new Status { Name = "Deleted", Description = "Deleted status" }
            };
            
            context.Statuses.AddRange(statuses);
            await context.SaveChangesAsync();
        }
        
        // Seed UserTypes
        if (!await context.UserTypes.AnyAsync())
        {
            var userTypes = new[]
            {
                new UserType { Name = "Admin", Description = "System Administrator" },
                new UserType { Name = "Manager", Description = "Network Manager" },
                new UserType { Name = "User", Description = "Regular User" },
                new UserType { Name = "Affiliate", Description = "Affiliate Marketer" }
            };
            
            context.UserTypes.AddRange(userTypes);
            await context.SaveChangesAsync();
        }
        
        // Seed default admin user if no users exist
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Email = "admin@myportal.com",
                Password = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes("Admin@123"))),
                UserTypeId = 1, // Admin
                StatusId = 1 // Active
            };
            
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            
            var adminProfile = new UserProfile
            {
                UserId = adminUser.Id,
                FirstName = "System",
                LastName = "Administrator",
                StatusId = 1
            };
            
            context.UserProfiles.Add(adminProfile);
            await context.SaveChangesAsync();
        }
    }
}
