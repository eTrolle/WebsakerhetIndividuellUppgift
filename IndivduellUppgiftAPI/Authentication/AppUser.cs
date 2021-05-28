using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IndivduellUppgiftAPI.Authentication
{
	public class AppUser : IdentityUser
	{
		[Required]
		public int NorthwindLink { get; set; }
	}
}
