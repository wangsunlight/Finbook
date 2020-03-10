using Consul;
using Contact.API.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Contact.API.Service
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

        public async Task<UserIdentity> GetBaseUserInfoAsync(int userId)
        {

            try
            {
                var response = await _httpClient.GetStringAsync(_userServiceUrl + "/api/users/beasinfo/"+ userId);
                if (!String.IsNullOrEmpty(response))
                {
                    var userinfo = JsonConvert.DeserializeObject<UserIdentity>(response);

                    _logger.LogInformation("CheckOrCreate userid" + userinfo.UserId);
                    return userinfo;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetBaseUserInfoAsync  在重试之后失败" + e.Message + e.StackTrace);
                throw e;
            }

            return null;
        }
    }
}
