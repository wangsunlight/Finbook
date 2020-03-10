using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Polly.Wrap;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Resilience
{
    public class ResilienceHttpClient : IHttpClient
    {
        /// <summary>
        /// http对象
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 根据url orgin去创建 policy
        /// </summary>
        private readonly Func<string, IEnumerable<Policy>> _policyCreator;

        /// <summary>
        /// 把 policy 打包成组合 poliy wraps ,进行本地缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, PolicyWrap> _poliayWrappers;

        private readonly ILogger<ResilienceHttpClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(Func<string, IEnumerable<Policy>> policyCreator, ILogger<ResilienceHttpClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = new HttpClient();
            _poliayWrappers = new ConcurrentDictionary<string, PolicyWrap>();
            this._policyCreator = policyCreator;
            this._logger = logger;
            this._httpContextAccessor = httpContextAccessor;
        }


        public Task<HttpResponseMessage> PutAsync<T>(string url, T item, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Put, url, item);

            return DoPostPutAsync(HttpMethod.Put, url, func, authoriztionToken, requestId, authoriztionMethod);
        }

        public Task<string> GetStringAsync(string url, string authoriztionToken = null, string authoriztionMethod = "Bearer")
        {
            var origin = GetOriginFromUri(url);

            return HttpInvoker(origin, async () =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                SetAuthorizationHeader(requestMessage);

                if (authoriztionToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authoriztionMethod, authoriztionToken);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            });
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, url, item);
            return await DoPostPutAsync(HttpMethod.Post, url, func, authoriztionToken, requestId, authoriztionMethod);
        }
        public async Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> form, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer")
        {
            Func<HttpRequestMessage> func = () => CreateHttpRequestMessage(HttpMethod.Post, url, form);
            return await DoPostPutAsync(HttpMethod.Post, url, func, authoriztionToken, requestId, authoriztionMethod);
        }

        private Task<HttpResponseMessage> DoPostPutAsync(HttpMethod method, string url, Func<HttpRequestMessage> requestMessageFunc, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("必须是Post或者Put请求", nameof(method));
            }

            var origin = GetOriginFromUri(url);

            return HttpInvoker(origin, async () =>
            {

                HttpRequestMessage requestMessage = requestMessageFunc();

                SetAuthorizationHeader(requestMessage);

                if (authoriztionToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authoriztionMethod, authoriztionToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }

                return response;
            });
        }


        private async Task<T> HttpInvoker<T>(string origin, Func<Task<T>> action)
        {
            var normalizedOrigin = NormalizeOrigin(origin);

            if (!_poliayWrappers.TryGetValue(normalizedOrigin, out PolicyWrap policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreator(normalizedOrigin).ToArray());
                _poliayWrappers.TryAdd(normalizedOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync(action, new Context(normalizedOrigin));
        }

        private HttpRequestMessage CreateHttpRequestMessage<T>(HttpMethod method, string url, T item)
        {
            return new HttpRequestMessage(method, url) { Content = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json") };
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, Dictionary<string, string> form)
        {
            return new HttpRequestMessage(method, url) { Content = new FormUrlEncodedContent(form) };
        }

        private string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private static string GetOriginFromUri(string uri)
        {
            var url = new Uri(uri);

            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";

            return origin;
        }

        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizetionHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizetionHeader))
            {
                requestMessage.Headers.Add("Authorization", new List<string>() { authorizetionHeader });
            }
        }

    }
}
