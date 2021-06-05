using IndivduellUppgiftAPI.Authentication;
using IndivduellUppgiftAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndivduellUppgiftAPI.Services
{
	public interface INorthwindService
	{
		Task<IEnumerable<Order>> GetOrdersByEmployee(string username);
		Task<IEnumerable<Order>> GetOrdersByCountry(string Country);
		Task<IEnumerable<Order>> GetAllOrders();
	}
	public class NorthwindService : INorthwindService
	{
		private readonly NorthwindContext northwindContext;
		private readonly AppDbContext appDbContext;

		public NorthwindService(NorthwindContext northwindContext, AppDbContext appDbContext)
		{
			this.northwindContext = northwindContext;
			this.appDbContext = appDbContext;
		}

		public async Task<IEnumerable<Order>> GetAllOrders()
		{
			return await northwindContext.Orders.Select(x => x).ToListAsync();
		}

		public async Task<IEnumerable<Order>> GetOrdersByCountry(string country)
		{
			return await northwindContext.Orders.Where(x => x.ShipCountry == country).ToListAsync();
		}

		public async Task<IEnumerable<Order>> GetOrdersByEmployee(string username)
		{
			var employee = await appDbContext.Users.FirstOrDefaultAsync(x => x.UserName == username);

			if (employee == null)
				return null;

			return await northwindContext.Orders.Where(x => x.EmployeeId == employee.NorthwindLink).ToListAsync();
		}
	}
}
