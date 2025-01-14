using Core.CrossCuttingConcerns.Exceptions.Handlers;
using Core.CrossCuttingConcerns.Logging;
using Core.CrossCuttingConcerns.SeriLog;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Core.CrossCuttingConcerns.Exceptions;

public class ExceptionMiddleware(RequestDelegate next, LoggerServiceBase loggerService)
{
    private readonly HttpExceptionHandler _httpExceptionHnadler = new HttpExceptionHandler();

    public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await next(context);
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
			new() { Type = context.GetType().Name, Value = exception.ToString() }
		};

		var logDetail = new LogDetailWithException
		{
			MethodName = next.Method.Name,
			User = (context.User.Identity?.Name ?? "?"),
			LogParameters = logParameters,
			ExceptionMessage = exception.Message 
		};

		loggerService.Error(JsonSerializer.Serialize(logDetail));
		return Task.CompletedTask;
	}

	private Task HandleExceptionAsync(HttpResponse response, Exception exception)
	{
		response.ContentType = "application/json";
		_httpExceptionHnadler.Response = response;
		return _httpExceptionHnadler.HandleExceptionAsync(exception);
	}
}
