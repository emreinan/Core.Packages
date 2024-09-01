﻿using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest,TResponse> : IPipelineBehavior<TRequest, TResponse> 
	where TRequest :IRequest<TResponse>, ICachableRequest
{
	private readonly CacheSettings _cacheSetting;
	private readonly IDistributedCache _cache;
	private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

	public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger,IConfiguration configuration)
	{
		_cacheSetting = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? throw new InvalidOperationException();
		_cache = cache;
		_logger = logger;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (request.BypassCache)
			return await next();

		TResponse response;
		byte[]? cachedResponse = await _cache.GetAsync(request.CacheKey);
		if (cachedResponse != null)
		{
			response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse));
			_logger.LogInformation($"Fecthed from Cache {request.CacheKey}");
		}
		else
		{
			response = await getResponseAndAddToCache(request, next,cancellationToken);
		}
		return response;
	}

	private async Task<TResponse?> getResponseAndAddToCache(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		var response = await next();

		var slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSetting.SlidingExpiration);
		var options = new DistributedCacheEntryOptions{SlidingExpiration = slidingExpiration};

		var serilizedResponse = Encoding.Default.GetBytes(JsonSerializer.Serialize(response));

		await _cache.SetAsync(request.CacheKey,serilizedResponse, options, cancellationToken);
		_logger.LogInformation($"Added to Cache {request.CacheKey}");

		return response;
	}
}
