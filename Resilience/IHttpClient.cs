using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

namespace Resilience
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> PostAsync<T>(string url, T item, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer");

        Task<HttpResponseMessage> PostAsync(string url, Dictionary<string, string> form, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer");

        Task<HttpResponseMessage> PutAsync<T>(string url, T item, string authoriztionToken = null, string requestId = null, string authoriztionMethod = "Bearer");

        Task<string> GetStringAsync(string url, string authoriztionToken = null, string authoriztionMethod = "Bearer");
    }
}
