using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLib
{
	public class AuthenticateResponse
	{
		public string Token { get; set; }
		public DateTime ValidTo { get; set; }
	}
}
