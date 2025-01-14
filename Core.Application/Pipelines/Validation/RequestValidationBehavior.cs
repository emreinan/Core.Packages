using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentValidation;
using MediatR;

using ValidationException = Core.CrossCuttingConcerns.Exceptions.Types.ValidationException; // Kendi yazdığımız ValidationException.

namespace Core.Application.Pipelines.Validation;

public class RequestValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var context = new ValidationContext<object>(request);
		var errors = validators
			.Select(v => v.Validate(context))
			.SelectMany(result => result.Errors)
			.Where(failure => failure != null)
			.GroupBy(
			keySelector: x => x.PropertyName,
			resultSelector: (propertyName, errors) =>
			new ValidationExceptionModel { Errors = errors.Select(e => e.ErrorMessage), Property = propertyName }).ToList();

		if (errors.Count != 0)
			throw new ValidationException(errors);

		TResponse response = await next();
		return response;
	}
}

