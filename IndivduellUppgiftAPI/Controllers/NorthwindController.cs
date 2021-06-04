using IndivduellUppgiftAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndivduellUppgiftAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NorthwindController : ControllerBase
	{
		private readonly NorthwindContext _context;
		private readonly IConfiguration _configuration;

		public NorthwindController(NorthwindContext context, IConfiguration configuration)
		{
			this._context = context;
			this._configuration = configuration;
		}

		[HttpGet("Employees")]
		public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
		{
			var employees = await _context.Employees.ToListAsync();
			return employees;
		}
	}
}
