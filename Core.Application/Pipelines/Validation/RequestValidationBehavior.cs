using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationException = Core.CrossCuttingConcerns.Exceptions.Types.ValidationException; // Kendi yazdığımız ValidationException.

namespace Core.Application.Pipelines.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var context = new ValidationContext<object>(request);
		var errors = _validators
			.Select(v => v.Validate(context))
			.SelectMany(result => result.Errors)
			.Where(failure => failure != null)
			.GroupBy(
			keySelector: x => x.PropertyName,
			resultSelector: (propertyName, errors) =>
			new ValidationExceptionModel { Errors = errors.Select(e => e.ErrorMessage), Property = propertyName }).ToList();

		if (errors.Any())
			throw new ValidationException(errors);

		TResponse response = await next();
		return response;
	}
}

