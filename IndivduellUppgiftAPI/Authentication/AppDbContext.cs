using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IndivduellUppgiftAPI.Authentication
{
	public class AppDbContext : IdentityDbContext<AppUser>
	{
		public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
	}
}
