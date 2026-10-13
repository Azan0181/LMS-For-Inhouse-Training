using Microsoft.AspNetCore.Identity;
using VoyageFreightLMS.Models;

namespace VoyageFreightLMS.Data;

/// <summary>
/// Seeds the database with the four application roles and one default Admin user
/// the first time the app runs.  Called from Program.cs.
/// </summary>
public static class SeedData
{
    public static readonly string[] AllRoles = { "Admin", "Trainer", "Employee", "HR", "Manager" };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // ── Create roles ───────────────────────────────────────────────
        foreach (var role in AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Create default Admin user ──────────────────────────────────
        const string adminEmail = "admin@voyagefreight.com";
        const string adminPassword = "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                Department = "IT",
                JobTitle = "System Administrator",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
