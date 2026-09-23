using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeExperimentLab.Models;

namespace RecipeExperimentLab.Data

{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            IServiceProvider services,
            IConfiguration configuration)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "Admin";
            const string userRole = "User";

            foreach (var roleName in new[] { adminRole, userRole })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(
                        new IdentityRole(roleName));

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Kunde inte skapa rolle {roleName}");
                    }
                }
            }

            var email = configuration["Admin:Email"];
            var password = configuration["Admin:Password"];

            if (string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null) 
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "Systemadministratör",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        ",",
                        createResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException($"Kunde inte skapa Admin {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(admin, adminRole);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Kunde inte tilldela adminrollen");
                }
            }
        }
    }
}
