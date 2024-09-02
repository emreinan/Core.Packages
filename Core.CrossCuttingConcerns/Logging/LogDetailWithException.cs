using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Logging;

public class LogDetailWithException : LogDetail
{
	public string ExceptionMessage { get; set; }
	// ihtiyaca göre ekstra propertyler eklenebilir.

	public LogDetailWithException()
	{
		ExceptionMessage = string.Empty;
	}

	public LogDetailWithException(
		string fullName,
		string methodName,
		string user, 
		List<LogParameter> logParameters, 
		string exceptionMessage) : base(fullName, methodName, user, logParameters)
	{
		ExceptionMessage = exceptionMessage;
	}
}
