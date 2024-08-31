using Core.CrossCuttingConcerns.Exceptions.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core.CrossCuttingConcerns.Exceptions.HttpProblemDetails;

public class ValidationProblemDetails : ProblemDetails
{
    public IEnumerable<ValidationExceptionModel> Errors { get; init; }
    public ValidationProblemDetails(IEnumerable<ValidationExceptionModel> errors)
	{
		Title = "Request Validation Error";
		Detail = "One or more validation errors occurred.";
		Status = StatusCodes.Status400BadRequest;
		Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
		Errors = errors;
	}
}
