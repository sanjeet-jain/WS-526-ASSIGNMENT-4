using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using ImageSharingWithCloudStorage.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithCloudStorage.DAL
{
    public  class ApplicationDbInitializer
    {
        private ApplicationDbContext db;
        private ILogContext logs;
        private ILogger<ApplicationDbInitializer> logger;

        public ApplicationDbInitializer(ApplicationDbContext db, ILogContext logs, ILogger<ApplicationDbInitializer> logger)
        {
            this.db = db;
            this.logs = logs;
            this.logger = logger;
        }

        public async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            /*
             * Create image views log it doesn't already exist
             */
            await logs.CreateTableAsync();

            await db.Database.MigrateAsync();

            db.RemoveRange(db.Images);
            db.RemoveRange(db.Tags);
            db.RemoveRange(db.Users);
            await db.SaveChangesAsync();

            logger.LogInformation("Adding role: User");
            var idResult = await CreateRole(serviceProvider, "User");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create User role!");
            }

            // TODO-DONE add other roles
            idResult = await CreateRole(serviceProvider, "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create Admin role!");
            }

            idResult = await CreateRole(serviceProvider, "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create Approver role!");
            }

            idResult = await CreateRole(serviceProvider, "Supervisor");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create Supervisor role!");
            }
            
            //TODO end

            logger.LogInformation("Adding user: jfk");
            idResult = await CreateAccount(serviceProvider, "jfk@example.org", "jfk123", "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create jfk user!");
            }

            logger.LogInformation("Adding user: nixon");
            idResult = await CreateAccount(serviceProvider, "nixon@example.org", "nixon123", "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create nixon user!");
            }

            // TODO-DONE add other users and assign more roles
            logger.LogInformation("Adding user: sanjeet");
            idResult = await CreateAccount(serviceProvider, "sanjeet@sit.edu", "sit123", "User");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create sanjeet user!");
            }

            logger.LogInformation("Adding user: admin");
            idResult = await CreateAccount(serviceProvider, "admin@sit.edu", "sit123", "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create admin user!");
            }

            logger.LogInformation("Adding user: app");
            idResult = await CreateAccount(serviceProvider, "app@sit.edu", "sit123", "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create app user!");
            }
            
            logger.LogInformation("Adding user: sup");
            idResult = await CreateAccount(serviceProvider, "sup@sit.edu", "sit123", "Supervisor");
            if (!idResult.Succeeded)
            {
                logger.LogError("Failed to create sup user!");
            }

            Tag portrait = new Tag { Name = "portrait" };
            db.Tags.Add(portrait);
            Tag architecture = new Tag { Name = "architecture" };
            db.Tags.Add(architecture);

            // TODO-DONE add other tags
            Tag custom = new Tag { Name = "custom" };
            db.Tags.Add(custom);


            await db.SaveChangesAsync();


        }

        public static async Task<IdentityResult> CreateRole(IServiceProvider provider,
                                                            string role)
        {
            RoleManager<IdentityRole> roleManager = provider
                .GetRequiredService
                       <RoleManager<IdentityRole>>();
            var idResult = IdentityResult.Success;
            if (await roleManager.FindByNameAsync(role) == null)
            {
                idResult = await roleManager.CreateAsync(new IdentityRole(role));
            }
            return idResult;
        }

        public static async Task<IdentityResult> CreateAccount(IServiceProvider provider,
                                                               string email, 
                                                               string password,
                                                               string role)
        {
            UserManager<ApplicationUser> userManager = provider
                .GetRequiredService
                       <UserManager<ApplicationUser>>();
            var idResult = IdentityResult.Success;

            if (await userManager.FindByNameAsync(email) == null)
            {
                ApplicationUser user = new ApplicationUser { UserName = email, Email = email };
                idResult = await userManager.CreateAsync(user, password);

                if (idResult.Succeeded)
                {
                    idResult = await userManager.AddToRoleAsync(user, role);
                }
            }

            return idResult;
        }

    }
}