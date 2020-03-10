using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using User.Identity.Dtos;
using Newtonsoft.Json;
using Consul;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private IHttpClient _httpClient;
        private readonly string _userServiceUrl;
        private ILogger<UserService> _logger;

        public UserService(IHttpClient httpClient, IOptions<ServiceDiscoveryOptions> serviceDiscoveryOptions, ConsulClient consulClient, ILogger<UserService> logger)
        {
            this._httpClient = httpClient;
            this._logger = logger;
            var address = consulClient.Health.Service(serviceDiscoveryOptions.Value.UserServiceName, "", true).Result;
            //.ResolveService("service.consul", serviceDiscoveryOptions.Value.UserServiceName)

            if (address.Response.Count() != 0)
            {
                var host = address.Response.First().Service.Address;
                var port = address.Response.First().Service.Port;

                _userServiceUrl = $"http://{host}:{port}";
            }
        }
        public async Task<UserInfo> CheckOrCreate(string phone)
        {
            var form = new Dictionary<string, string> { { "phone", phone } };//这个不行，post请求phone不过去
            try
            {
                var respose = await _httpClient.PostAsync(_userServiceUrl + "/api/users/check-or-create/" + phone, form);
                if (respose.StatusCode == HttpStatusCode.OK)
                {
                    var user = await respose.Content.ReadAsStringAsync();

                    var userinfo = JsonConvert.DeserializeObject<UserInfo>(user);

                    _logger.LogError("CheckOrCreate userid" + userinfo.id);
                    return userinfo;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("CheckOrCreate  在重试之后失败" + e.Message + e.StackTrace);
                throw e;
            }

            return null;
        }
    }
}
