using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Core.CrossCuttingConcerns.Exceptions.Extensions;

public static class ProblemDetailsExtensions
{
	// This is a generic method that serializes the given ProblemDetails object to a JSON string.
	public static string AsJson<TProblemDetail>(this TProblemDetail problemDetail)
		where TProblemDetail : ProblemDetails => JsonSerializer.Serialize(problemDetail); 

}
