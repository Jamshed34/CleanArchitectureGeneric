using Domain.Entities;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            // Seed roles
            var adminRole = new Role { Name = "Admin" };
            var userRole = new Role { Name = "User" };

            if (!await roleManager.RoleExistsAsync(adminRole.Name))
                await roleManager.CreateAsync(adminRole);

            if (!await roleManager.RoleExistsAsync(userRole.Name))
                await roleManager.CreateAsync(userRole);

            // Seed admin user
            var adminUser = new User
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(adminUser.Email) == null)
            {
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, adminRole.Name);
            }

            // Seed configuration data for IdentityServer
            var configContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            if (!configContext.Clients.Any())
            {
                foreach (var client in IdentityServerConfig.GetClients(scope.ServiceProvider.GetRequiredService<IConfiguration>()))
                {
                    configContext.Clients.Add(client.ToEntity());
                }
                await configContext.SaveChangesAsync();
            }

            if (!configContext.IdentityResources.Any())
            {
                foreach (var resource in IdentityServerConfig.GetIdentityResources())
                {
                    configContext.IdentityResources.Add(resource.ToEntity());
                }
                await configContext.SaveChangesAsync();
            }

            if (!configContext.ApiResources.Any())
            {
                foreach (var resource in IdentityServerConfig.GetApiResources())
                {
                    configContext.ApiResources.Add(resource.ToEntity());
                }
                await configContext.SaveChangesAsync();
            }

            if (!configContext.ApiScopes.Any())
            {
                foreach (var scopes in IdentityServerConfig.GetApiScopes())
                {
                    configContext.ApiScopes.Add(scopes.ToEntity());
                }
                await configContext.SaveChangesAsync();
            }
        }
    }
}
