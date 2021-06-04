using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLib
{
	public class AuthenticateResponse : IResponse
	{
		public string Status { get; set; }
		public string Message { get; set; }
		public string JWT { get; set; }
		public DateTime JWTValidTo { get; set; }
		public string RefreshToken { get; set; }
	}
}
