using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.SeriLog;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.Application.Pipelines.Logging;

public class LoggingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor,
                                                  LoggerServiceBase loggerService) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, ILoggableRequest
{
    public async Task<TResponse> Handle(TRequest request,
										RequestHandlerDelegate<TResponse> next, 
										CancellationToken cancellationToken)
	{
		var logParameters = new List<LogParameter>
		{
			new LogParameter{Type=request.GetType().Name, Value= request}
		};

		var logDetail = new LogDetail
		{
			MethodName = next.Method.Name,
			User = (httpContextAccessor.HttpContext.User.Identity?.Name ?? "?"),
			LogParameters = logParameters
		};

		loggerService.Information(JsonSerializer.Serialize(logDetail));
		return await next();
	}
}
