using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ModelLib;
using IndivduellUppgiftAPI.Services;
using Microsoft.AspNetCore.Authorization;
using IndivduellUppgiftAPI.Authentication;
using System.Collections.Generic;

namespace IndivduellUppgiftAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthenticateController : ControllerBase
	{
		private readonly IUserService _userService;

		public AuthenticateController(IUserService userService)
		{
			_userService = userService;
		}

		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			var result = await _userService.Authenticate(model);

			if (result == null)
				return BadRequest(new Response() { Status = "Error", Message = "Invalid user or password" });

			return Ok(result);

		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			var result = await _userService.Register(model);

			if (result.Status == "Error")
				return StatusCode(StatusCodes.Status500InternalServerError, result);

			return Ok(result);
		}
		[HttpPost]
		[Route("refresh")]
		public async Task<IActionResult> Refresh([FromBody] RefreshRequest model)
		{
			var result = await _userService.RefreshJWT(model);
			if (result == null)
				return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "Unhelpful error message" });

			if (result.Status == "Error")
				return StatusCode(StatusCodes.Status500InternalServerError, result);

			return Ok(result);
		}

		[RoleAuthorize(Roles.Admin, Roles.Vd)]
		[HttpGet]
		[Route("GetAll")]
		public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
		{
			if (! _userService.CheckJti(User))
				return StatusCode(StatusCodes.Status401Unauthorized, new Response() { Status = "Unauthorized", Message = "Invalid Token" });
			var result = await _userService.GetUsers();
			if(result == null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "No users in DB" });
			}
			return Ok(result);
		}

		[Authorize]
		[HttpPut]
		[Route("Update")]
		public async Task<IActionResult> Update([FromQuery] string username, [FromBody] UpdateModel model)
		{
			if (! _userService.CheckJti(User))
				return StatusCode(StatusCodes.Status401Unauthorized, new Response() { Status = "Unauthorized", Message = "Invalid Token" });
			if (username != null)
			{
				if (!User.IsInRole(Roles.Admin))
					return StatusCode(StatusCodes.Status401Unauthorized, new Response { Status = "Unauthorized", Message = "Unauthorized to update specific user" });
			}
			else
			{
				username = User.Identity.Name;
			}
			var result = await _userService.UpdateUser(username, model);
			if(result == null)
				return StatusCode(StatusCodes.Status500InternalServerError, new Response() { Status = "Error", Message = "Something went wrong" });
			return Ok(result);
		}

		[RoleAuthorize(Roles.Admin)]
		[HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete([FromQuery] string username)
		{
			var result = await _userService.DeleteUser(username);

			if (result.Status == "Error")
				return StatusCode(StatusCodes.Status500InternalServerError, result);
			return Ok(result);
		}
	}
}
