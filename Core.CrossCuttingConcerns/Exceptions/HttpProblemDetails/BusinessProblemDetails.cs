using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class BusinessProblemDetails : ProblemDetails
{
	public BusinessProblemDetails(string detail)
	{
		Title = "Rule Violation";
		Detail = detail;
		Status = StatusCodes.Status400BadRequest;
		Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
	}
}
