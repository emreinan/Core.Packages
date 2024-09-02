using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Pipelines.Caching;

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<CacheRemovingBehavior<TRequest, TResponse>> _logger;

	public CacheRemovingBehavior(IDistributedCache cache, ILogger<CacheRemovingBehavior<TRequest, TResponse>> logger)
	{
		_cache = cache;
		_logger = logger;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (request.BypassCache)
			return await next();

		var response = await next();

		if (request.CacheGroupKey is not null)
		{
			var cacheKeys = await _cache.GetAsync(request.CacheGroupKey, cancellationToken);

			if (cacheKeys is not null)
			{
				var keysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cacheKeys))!;

				foreach (var cacheKey in keysInGroup)
				{
					await _cache.RemoveAsync(cacheKey, cancellationToken);
					_logger.LogInformation($"Removed from Cache {cacheKey}");
				}
				await _cache.RemoveAsync(request.CacheGroupKey, cancellationToken);
				_logger.LogInformation($"Removed from Cache {request.CacheGroupKey}");
			}
		}

		if (request.CacheKey is not null)
		{
			await _cache.RemoveAsync(request.CacheKey, cancellationToken);
			_logger.LogInformation($"Removed from Cache {request.CacheKey}");
		}

		return response;
	}
}
