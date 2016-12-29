using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTA.Laddon
{
	public class ValidationResult
	{
		public bool IsValid { get; set; }
		public string Message { get; set; }

		public ValidationResult()
		{
			Message = string.Empty;
		}
	}
}
