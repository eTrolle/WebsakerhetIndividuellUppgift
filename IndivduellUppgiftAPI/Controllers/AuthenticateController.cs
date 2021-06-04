using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ModelLib;
using IndivduellUppgiftAPI.Services;

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
	}
}
