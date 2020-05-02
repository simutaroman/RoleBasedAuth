using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace RoleBasedAuth.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleName = "Administrator";
            IdentityResult result;

            var roleExist = roleManager.RoleExistsAsync(roleName).Result;
            if (!roleExist)
            {
                result = roleManager.CreateAsync(new IdentityRole(roleName)).Result;
                if (result.Succeeded)
                {
                    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var config = serviceProvider.GetRequiredService<IConfiguration>();
                    var admin = userManager.FindByEmailAsync(config["AdminCredentials:Email"]).Result;
                    
                    if (admin == null)
                    {
                        admin = new IdentityUser()
                        {
                            UserName = config["AdminCredentials:Email"],
                            Email = config["AdminCredentials:Email"],
                            EmailConfirmed = true
                        };

                        result = userManager.CreateAsync(admin, config["AdminCredentials:Password"]).Result;
                        if (result.Succeeded)
                        {
                            result = userManager.AddToRoleAsync(admin, roleName).Result;
                            if (!result.Succeeded)
                            {
                                // todo: process errors
                            }
                        }
                    }
                }
            }
        }
    }
}
