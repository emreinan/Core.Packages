using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class InternalServerErrorProblemDetails : ProblemDetails
{
	public InternalServerErrorProblemDetails(string detail)
	{
		Title = "Internal Server Error";
		Detail = detail;
		Status = StatusCodes.Status500InternalServerError;
		Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
	}
}
