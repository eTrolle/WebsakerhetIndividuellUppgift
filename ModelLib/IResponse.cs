using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLib
{
	public interface IResponse
	{
		string Status { get; set; }
		string Message { get; set; }
	}
}
