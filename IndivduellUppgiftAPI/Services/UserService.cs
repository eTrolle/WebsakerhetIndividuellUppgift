using IndivduellUppgiftAPI.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelLib;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IndivduellUppgiftAPI.Data;
using IndivduellUppgiftAPI.Services;

namespace IndivduellUppgiftAPI.Services
{
	public interface IUserService
	{
		Task<AuthenticateResponse> Authenticate(LoginModel model);
		Task<Response> Register(RegisterModel model);
	}
	public class UserService : IUserService
	{
		private readonly UserManager<AppUser> userManager;
		private readonly NorthwindContext northwindContext;
		private readonly IConfiguration _configuration;

		public UserService(UserManager<AppUser> userManager, NorthwindContext northwindContext, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.northwindContext = northwindContext;
			this._configuration = configuration;
		}

		public async Task<AuthenticateResponse> Authenticate(LoginModel model)
		{
			var user = await userManager.FindByNameAsync(model.Username);

			if (!await userManager.CheckPasswordAsync(user, model.Password))
			{
				return null;
			}

			var token = await GenerateJWT(user);

			return new AuthenticateResponse()
			{
				Token = new JwtSecurityTokenHandler().WriteToken(token),
				ValidTo = token.ValidTo
			};
		}

		public async Task<Response> Register(RegisterModel model)
		{
			var userExists = await userManager.FindByNameAsync(model.Username);
			var emailExists = await userManager.FindByEmailAsync(model.Email);

			if (userExists != null && emailExists != null)
				return new Response { Status = "Error", Message = "User already exists or the Email is already registered." };

			var employee = northwindContext.Employees.FirstOrDefault(x => x.FirstName == model.FirstName && x.LastName == model.LastName);
			var oldEmployeeLink = userManager.Users.FirstOrDefault(x => x.NorthwindLink == employee.EmployeeId);

			if (employee == null && oldEmployeeLink != null)
				return new Response { Status = "Error", Message = "Unable to link Employee" };

			AppUser user = new AppUser()
			{
				Email = model.Email,
				SecurityStamp = Guid.NewGuid().ToString(),
				UserName = model.Username,
				NorthwindLink = employee.EmployeeId
			};

			var result = await userManager.CreateAsync(user, model.Password);
			if (!result.Succeeded)
				return new Response { Status = "Error", Message = "Something went wrong. Try again." };

			var admins = await userManager.GetUsersInRoleAsync("Admin");
			if (admins.Count == 0)
				await userManager.AddToRoleAsync(user, "Admin");
			await userManager.AddToRoleAsync(user, "Employee");

			return new Response { Status = "Success", Message = "User registered successfully" };
		}

		private async Task<JwtSecurityToken> GenerateJWT(AppUser user)
		{
			var userRoles = await userManager.GetRolesAsync(user);
			var country = northwindContext.Employees.FirstOrDefault(x => x.EmployeeId == user.NorthwindLink).Country;

			var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(ClaimTypes.Country, country)
				};
			foreach (var role in userRoles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"],
				audience: _configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddHours(1),
				claims: claims,
				signingCredentials: new SigningCredentials(
					new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
					SecurityAlgorithms.HmacSha256)
				);

			return token;
		}
	}
}
