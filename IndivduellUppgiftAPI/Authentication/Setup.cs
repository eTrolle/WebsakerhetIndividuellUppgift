using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndivduellUppgiftAPI.Authentication
{
	public static class Setup
	{
		public static async Task CreateInitialRoles(IServiceProvider serviceProvider)
		{
			using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
				if (!context.UserRoles.Any())
				{
					var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
					string[] roles = new string[] { Roles.Admin, Roles.Vd, Roles.CountryManager, Roles.Employee};
					foreach (string r in roles)
					{
						if (!await roleManager.RoleExistsAsync(r))
							await roleManager.CreateAsync(new IdentityRole(r));
					}
				}
			}
		}

		public static async Task CreateInitialUsers(IServiceProvider serviceProvider)
		{
			using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
				if (!context.Users.Any())
				{
					var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
					string password = "Password123!";
					AppUser[] users = new AppUser[]
					{
						new AppUser()
						{
							UserName = "AdminUser",
							Email = "mail1@something.com",
							SecurityStamp = Guid.NewGuid().ToString(),
							NorthwindLink = 1
						},
						new AppUser()
						{
							UserName = "VdUser",
							Email = "mail2@something.com",
							SecurityStamp = Guid.NewGuid().ToString(),
							NorthwindLink = 2
						},
						new AppUser()
						{
							UserName = "ManagerUser",
							Email = "mail3@something.com",
							SecurityStamp = Guid.NewGuid().ToString(),
							NorthwindLink = 5
						},
						new AppUser()
						{
							UserName = "EmployeeUser",
							Email = "mail4@something.com",
							SecurityStamp = Guid.NewGuid().ToString(),
							NorthwindLink = 3
						}
					};
					foreach(AppUser u in users)
					{
						await userManager.CreateAsync(u, password);
						await userManager.AddToRoleAsync(u, Roles.Employee);
					}
					await userManager.AddToRoleAsync(users[0], Roles.Admin);
					await userManager.AddToRoleAsync(users[1], Roles.Vd);
					await userManager.AddToRoleAsync(users[2], Roles.CountryManager);
				}
			}
		}
	}
}
