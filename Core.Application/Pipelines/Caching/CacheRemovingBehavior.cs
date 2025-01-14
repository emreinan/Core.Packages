using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Core.Application.Pipelines.Caching;

public class CacheRemovingBehavior<TRequest, TResponse>(IDistributedCache cache,
                                                        ILogger<CacheRemovingBehavior<TRequest, 
														TResponse>> logger)
														: IPipelineBehavior<TRequest, TResponse>
														where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
    public async Task<TResponse> Handle(TRequest request, 
										RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
	{
		if (request.BypassCache)
			return await next();

		var response = await next();

		if (request.CacheGroupKey is not null)
		{
			var cacheKeys = await cache.GetAsync(request.CacheGroupKey, cancellationToken);

			if (cacheKeys is not null)
			{
				var keysInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cacheKeys))!;

				foreach (var cacheKey in keysInGroup)
				{
					await cache.RemoveAsync(cacheKey, cancellationToken);
					logger.LogInformation($"Removed from Cache {cacheKey}");
				}
				await cache.RemoveAsync(request.CacheGroupKey, cancellationToken);
				logger.LogInformation($"Removed from Cache {request.CacheGroupKey}");
			}
		}

		if (request.CacheKey is not null)
		{
			await cache.RemoveAsync(request.CacheKey, cancellationToken);
			logger.LogInformation($"Removed from Cache {request.CacheKey}");
		}

		return response;
	}
}
