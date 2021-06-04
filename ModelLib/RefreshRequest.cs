using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLib
{
	public class RefreshRequest
	{
		public string Token { get; set; }
		public string RefreshToken { get; set; }
	}
}
