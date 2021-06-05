using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndivduellUppgiftAPI.Authentication
{
	public class RoleAuthorizeAttribute : AuthorizeAttribute
	{
		public RoleAuthorizeAttribute(params string[] roles) : base()
		{
			Roles = string.Join(",", roles);
		}
	}
}
