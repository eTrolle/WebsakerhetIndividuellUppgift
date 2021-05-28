using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndivduellUppgiftAPI.Authentication
{
	public static class RoleSetup
	{
		public static async Task CreateInitialRoles(IServiceProvider serviceProvider)
		{
			using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
				if (!context.UserRoles.Any())
				{
					var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
					string[] roles = new string[] { "Admin", "Vd", "CountryManager", "Employee" };
					foreach (string r in roles)
					{
						if (!await roleManager.RoleExistsAsync(r))
							await roleManager.CreateAsync(new IdentityRole(r));
					}
				}
			}
		}
	}
}
