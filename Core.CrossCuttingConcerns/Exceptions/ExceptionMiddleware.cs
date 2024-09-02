using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.SeriLog;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.CrossCuttingConcerns.Exceptions;

public class ExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly HttpExceptionHandler _httpExceptionHnadler;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly LoggerServiceBase _loggerService;

	public ExceptionMiddleware(RequestDelegate next, LoggerServiceBase loggerService, IHttpContextAccessor httpContextAccessor)
	{
		_next = next;
		_httpExceptionHnadler = new HttpExceptionHandler();
		_loggerService = loggerService;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception exception)
		{
			await LogException(context, exception);
			await HandleExceptionAsync(context.Response, exception);
		}
	}

	private Task LogException(HttpContext context, Exception exception)
	{
		var logParameters = new List<LogParameter>
		{
			new LogParameter { Type = context.GetType().Name, Value = exception.ToString() }
		};

		var logDetail = new LogDetailWithException
		{
			MethodName = _next.Method.Name,
			User = (context.User.Identity?.Name ?? "?"),
			LogParameters = logParameters,
			ExceptionMessage = exception.Message 
		};

		_loggerService.Error(JsonSerializer.Serialize(logDetail));
		return Task.CompletedTask;
	}

	private Task HandleExceptionAsync(HttpResponse response, Exception exception)
	{
		response.ContentType = "application/json";
		_httpExceptionHnadler.Response = response;
		return _httpExceptionHnadler.HandleExceptionAsync(exception);
	}
}
