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
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using Microsoft.Data.SqlClient;

namespace IndivduellUppgiftAPI.Services
{
	public interface IUserService
	{
		Task<AuthenticateResponse> Authenticate(LoginModel model);
		Task<Response> Register(RegisterModel model);
		Task<IResponse> RefreshJWT(RefreshRequest model);
		Task<IEnumerable<AppUser>> GetUsers();

		Task<IResponse> DeleteUser(string username);
		Task<IResponse> UpdateUser(string username, UpdateModel model);

		bool CheckJti(ClaimsPrincipal claimsPrincipal);
	}
	public class UserService : IUserService
	{
		private readonly UserManager<AppUser> userManager;
		private readonly TokenValidationParameters tokenValidationParameters;
		private readonly AppDbContext appDbContext;
		private readonly NorthwindContext northwindContext;
		private readonly IConfiguration _configuration;

		public UserService(UserManager<AppUser> userManager, TokenValidationParameters tokenValidationParameters, AppDbContext appDbContext, NorthwindContext northwindContext, IConfiguration configuration)
		{
			this.userManager = userManager;
			this.tokenValidationParameters = tokenValidationParameters;
			this.appDbContext = appDbContext;
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

			return token;
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

		public async Task<IResponse> RefreshJWT(RefreshRequest refreshRequest)
		{
			var jwtTokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var principal = jwtTokenHandler.ValidateToken(refreshRequest.Token, tokenValidationParameters, out var validatedToken);

				if (validatedToken is JwtSecurityToken jwtSecurityToken)
				{
					var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

					if (result == false)
						return null;

				}

				//checking if JWT token has expired
				var utcExpiryDate = long.Parse(principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
				var unixDateTimeNow = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

				if (utcExpiryDate > unixDateTimeNow)
				{
					return new Response()
					{
						Status = "Error",
						Message = "Token has not Expired"
					};
				}

				var storedRefreshToken = appDbContext.RefreshTokens.FirstOrDefault(x => x.Token == refreshRequest.RefreshToken);

				//checking if RefreshToken is still valid
				if (storedRefreshToken == null)
				{
					return new Response()
					{
						Status = "Error",
						Message = "Invalid RefreshToken"
					};
				}
				if (DateTime.Now > storedRefreshToken.ExpiryDate)
				{
					return new Response()
					{
						Status = "Error",
						Message = "RefreshToken is expired. Login again"
					};
				}

				if (storedRefreshToken.IsUsed)
				{
					return new Response()
					{
						Status = "Error",
						Message = "RefreshToken is used"
					};
				}

				if (storedRefreshToken.IsRevoked)
				{
					return new Response()
					{
						Status = "Error",
						Message = "RefreshToken is Revoked"
					};
				}

				var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

				//check if JWT is the correct
				if (storedRefreshToken.JwtId != jti)
				{
					return new Response()
					{
						Status = "Error",
						Message = "JWT does not match"
					};
				}

				storedRefreshToken.IsUsed = true;
				appDbContext.RefreshTokens.Update(storedRefreshToken);
				await appDbContext.SaveChangesAsync();

				var user = await userManager.FindByIdAsync(storedRefreshToken.UserId);
				return await GenerateJWT(user);
			}
			catch (Exception e)
			{
				return null;
			}

		}

		private async Task<AuthenticateResponse> GenerateJWT(AppUser user)
		{
			var userRoles = await userManager.GetRolesAsync(user);
			var country = northwindContext.Employees.FirstOrDefault(x => x.EmployeeId == user.NorthwindLink).Country;

			var jti = Guid.NewGuid().ToString();

			var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, user.UserName),
					new Claim(JwtRegisteredClaimNames.Jti, jti),
					new Claim(ClaimTypes.Country, country)
				};
			foreach (var role in userRoles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:ValidIssuer"],
				audience: _configuration["JWT:ValidAudience"],
				expires: DateTime.Now.AddMinutes(10),
				claims: claims,
				signingCredentials: new SigningCredentials(
					new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
					SecurityAlgorithms.HmacSha256)
				);

			var refreshToken = new RefreshToken()
			{
				JwtId = token.Id,
				IsUsed = false,
				UserId = user.Id,
				ExpiryDate = DateTime.Now.AddDays(1),
				IsRevoked = false,
				Token = Guid.NewGuid().ToString()
			};

			await appDbContext.RefreshTokens.AddAsync(refreshToken);
			await appDbContext.SaveChangesAsync();

			user.jti = jti;
			appDbContext.Users.Update(user);
			await appDbContext.SaveChangesAsync();

			return new AuthenticateResponse()
			{
				Status = "Success",
				Message = "Token successfully generated",
				JWT = new JwtSecurityTokenHandler().WriteToken(token),
				JWTValidTo = token.ValidTo,
				RefreshToken = refreshToken.Token
			};
		}

		public async Task<IEnumerable<AppUser>> GetUsers()
		{
			return await appDbContext.Users.Select(x => x).ToListAsync();
		}

		public async Task<IResponse> UpdateUser(string username, UpdateModel model)
		{
			var user = await userManager.FindByNameAsync(username);

			if (user == null)
				return null;

			var mailExists = await userManager.FindByEmailAsync(model.NewEmail);
			if (mailExists != null)
				return null;

			//add more for each field that should be updated.
			if (model.NewEmail != null)
				user.Email = model.NewEmail;

			var result = await userManager.UpdateAsync(user);

			if (!result.Succeeded)
				return null;
			return new Response { Status = "Success", Message = "User updated successfully" };

		}

		public async Task<IResponse> DeleteUser(string username)
		{

			var user = await userManager.FindByNameAsync(username);
			if (user == null)
				return new Response() { Status = "Error", Message = "User does not exist in db" };

			var result = appDbContext.Database.ExecuteSqlRawAsync("DELETE FROM AspNetUsers WHERE UserName=@username", new SqlParameter("@username", username));



			return new Response() { Status = "Success", Message = "User successfully deleted" };
		}

		public bool CheckJti(ClaimsPrincipal claimsPrincipal)
		{
			var claimJTI = ((ClaimsIdentity)claimsPrincipal.Identity).Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
			var user = appDbContext.Users.FirstOrDefault(x => x.UserName == claimsPrincipal.Identity.Name);

			if (claimJTI.Value == user.jti)
				return true;
			return false;
		}
	}
}
