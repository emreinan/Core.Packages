using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.SeriLog;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Pipelines.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, ILoggableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly LoggerServiceBase _loggerService;

	public LoggingBehavior(IHttpContextAccessor httpContextAccessor, LoggerServiceBase loggerService)
	{
		_httpContextAccessor = httpContextAccessor;
		_loggerService = loggerService;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var logParameters = new List<LogParameter>
		{
			new LogParameter{Type=request.GetType().Name, Value= request}
		};

		var logDetail = new LogDetail
		{
			MethodName = next.Method.Name,
			User = (_httpContextAccessor.HttpContext.User.Identity?.Name ?? "?"),
			LogParameters = logParameters
		};

		_loggerService.Information(JsonSerializer.Serialize(logDetail));
		return await next();
	}
}
