using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Resilience;
using Polly;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace Contact.API.Infrastructure
{
    public class ResilienceClientFactory
    {
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// 重试次数
        /// </summary>
        private int _retryCount;
        /// <summary>
        /// 熔断之前允许的异常次数
        /// </summary>
        private int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor, int retryCount, int exceptionCountAllowedBeforeBreaking)
        {
            this._logger = logger;
            this._httpContextAccessor = httpContextAccessor;
            this._retryCount = retryCount;
            this._exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }
        public ResilienceHttpClient GetResilienceHttpClient() => new ResilienceHttpClient(origin => CreatePolicy(origin), _logger, _httpContextAccessor);

        private Policy[] CreatePolicy(string origin)
        {
            return new Policy[] {
            Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(
                _retryCount,
                retryAttempt =>TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    var msg = $"重试 {retryCount} 次" +
                    $"PolicyKey {context.PolicyKey} " +
                    $"ExecutionKey {context.ExecutionKey} " +
                    $"exception : {exception}";

                    _logger.LogWarning(msg);
                    _logger.LogDebug(msg);
                }),

            Policy.Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                _exceptionCountAllowedBeforeBreaking,
                TimeSpan.FromMinutes(1),
                (exception,duration)=>{
                    _logger.LogTrace("熔断器开启");
                },
                ()=>{
                _logger.LogTrace("熔断器重置（关闭）");
                })
            };
        }
    }
}
