using IndivduellUppgiftAPI.Authentication;
using IndivduellUppgiftAPI.Data;
using IndivduellUppgiftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModelLib;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IndivduellUppgiftAPI.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly INorthwindService _northwindService;
		private readonly IUserService _userService;

		public OrderController(INorthwindService northwindService, IUserService userService)
		{
			_northwindService = northwindService;
			_userService = userService;
		}

		[RoleAuthorize(Roles.Admin, Roles.Vd, Roles.CountryManager)]
		[HttpGet("GetAllOrders")]
		public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
		{
			IEnumerable<Order> orders;
			if (User.IsInRole("CountryManager"))
			{
				var userCountry = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Country).Value;

				if (userCountry == null)
					return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "User has no country" });

				orders = await _northwindService.GetOrdersByCountry(userCountry);
			}
			else
			{
				orders = await _northwindService.GetAllOrders();
			}

			return Ok(orders);
		}

		[Authorize(Roles = "Admin,Vd,CountryManager")] //old
		[RoleAuthorize(Roles.Admin, Roles.Vd, Roles.CountryManager)]//new
		[HttpGet("GetCountryOrders")]
		public async Task<ActionResult<IEnumerable<Order>>> GetCountryOrders([FromQuery] string country)
		{
			IEnumerable<Order> orders;
			if (User.IsInRole("CountryManager"))
			{
				var userCountry = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Country).Value;

				if (userCountry == null)
					return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "User has no country" });

				if (userCountry != country)
					return StatusCode(StatusCodes.Status401Unauthorized, new Response() { Status = "Unauthorized", Message = "User is not allowed outside countries" });

				orders = await _northwindService.GetOrdersByCountry(userCountry);
			}
			else
			{
				orders = await _northwindService.GetOrdersByCountry(country);
			}

			return Ok(orders);
		}

		[HttpGet("GetMyOrders")]
		public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders([FromQuery] string username)
		{
			if (username != null)
			{
				if (!(User.IsInRole(Roles.Vd) || User.IsInRole(Roles.Admin)))
				{
					return StatusCode(StatusCodes.Status401Unauthorized, new Response() { Status = "Unauthorized", Message = "Regular users may not search by username" });
				}
			}
			else
			{
				username = User.Identity.Name;
			}
			var orders = await _northwindService.GetOrdersByEmployee(username);
			return Ok(orders);
		}
	}
}
