using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using User.API.Data;
using Microsoft.EntityFrameworkCore;
using User.API.Filters;
using Microsoft.Extensions.Options;
using User.API.Dtos;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Consul;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DotNetCore.CAP;
using DotNetCore.CAP.Dashboard.NodeDiscovery;

namespace User.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<UserContext>(option =>
            {
                option.UseMySql(Configuration.GetConnectionString("MysqlUser"));
            });

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDisvovery"));

            services.AddSingleton<IConsulClient>(p =>
                new ConsulClient(cfg =>
                {
                    {
                        var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                        if (!string.IsNullOrEmpty(serviceConfiguration.Cousul.HttpEndpoint))
                        {
                            cfg.Address = new Uri(serviceConfiguration.Cousul.HttpEndpoint);
                        }
                    }
                }));

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Authority = "http://localhost:5000";
                    options.Audience = "user_api";
                });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(GlobalExceptionFilter));
            }).AddNewtonsoftJson();


            services.AddCap(option =>
            {
                option.UseEntityFramework<UserContext>();

                option.UseRabbitMQ("114.55.105.181");

                // Register Dashboard
                option.UseDashboard();

                // Register to Consul
                option.UseDiscovery(d =>
                {
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 8500;
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 5001;
                    d.NodeId = "1";
                    d.NodeName = "CAP User.API Node";
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime applicationLifetime, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consul)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseAuthentication();

            applicationLifetime.ApplicationStarted.Register(() =>
            {
                RegisterService(app, applicationLifetime, loggerFactory, serviceOptions, consul);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            UserContextSeed.SeedAsync(app, loggerFactory).Wait();
        }

        private void RegisterService(IApplicationBuilder app, IHostApplicationLifetime appLife, ILoggerFactory loggerFactory, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck()
                {
                    // 注册超时
                    Timeout = TimeSpan.FromSeconds(5),
                    // 服务停止多久后注销服务
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    // 健康检查时间间隔
                    Interval = TimeSpan.FromSeconds(10),
                    // 健康检查地址
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port
                };

                consul.Agent.ServiceRegister(registration).Wait();

                appLife.ApplicationStopping.Register(() =>
                {
                    consul.Agent.ServiceDeregister(serviceId).Wait();
                });
            }
        }
    }
}
