using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Exceptions.Extensions;

public static class ProblemDetailsExtensions
{
	// This is a generic method that serializes the given ProblemDetails object to a JSON string.
	public static string AsJson<TProblemDetail>(this TProblemDetail problemDetail)
		where TProblemDetail : ProblemDetails => JsonSerializer.Serialize(problemDetail); 

}
