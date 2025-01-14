using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse>(IDistributedCache cache,
                                                  ILogger<CachingBehavior<TRequest, TResponse>> logger,
                                                  IConfiguration configuration) : IPipelineBehavior<TRequest, TResponse>
                                                  where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly CacheSettings _cacheSetting = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? throw new InvalidOperationException();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        if (request.BypassCache)
            return await next();

        TResponse response;
        byte[]? cachedResponse = await cache.GetAsync(request.CacheKey);
        if (cachedResponse != null)
        {
            response = JsonSerializer.Deserialize<TResponse>(Encoding.Default.GetString(cachedResponse));
            logger.LogInformation("Fetched from Cache {CacheKey}", request.CacheKey);
        }
        else
        {
            response = await GetResponseAndAddToCache(request, next, cancellationToken);
        }
        return response;
    }

    private async Task<TResponse?> GetResponseAndAddToCache(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var slidingExpiration = request.SlidingExpiration ?? TimeSpan.FromDays(_cacheSetting.SlidingExpiration);
        var options = new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration };

        var serilizedResponse = Encoding.Default.GetBytes(JsonSerializer.Serialize(response));

        await cache.SetAsync(request.CacheKey, serilizedResponse, options, cancellationToken);
        logger.LogInformation("Added to Cache {CacheKey}", request.CacheKey);

        if (request.CacheGroupKey is not null)
        {
            await AddCacheKeyToGroup(request, slidingExpiration, cancellationToken);
        }

        return response;
    }

    private async Task AddCacheKeyToGroup(TRequest request, TimeSpan slidingExpiration, CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;
        var cacheGroupKey = request.CacheGroupKey;
        var cacheGroupKeyList = await cache.GetAsync(cacheGroupKey!, cancellationToken);
        var cacheGroupKeyListString = Encoding.Default.GetString(cacheGroupKeyList ?? Encoding.Default.GetBytes("[]"));
        var cacheGroupKeyListDeserialized = JsonSerializer.Deserialize<HashSet<string>>(cacheGroupKeyListString);
        if (cacheGroupKeyListDeserialized is null)
        {
            cacheGroupKeyListDeserialized = new HashSet<string>();
        }
        cacheGroupKeyListDeserialized.Add(cacheKey);
        var cacheGroupKeyListSerialized = JsonSerializer.Serialize(cacheGroupKeyListDeserialized);
        await cache.SetAsync(cacheGroupKey, Encoding.Default.GetBytes(cacheGroupKeyListSerialized), new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration }, cancellationToken);
        logger.LogInformation("Added to Cache Group {CacheGroupKey}", cacheGroupKey);
        return;
    }
}
