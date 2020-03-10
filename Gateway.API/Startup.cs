using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using IdentityServer4.AccessTokenValidation;
using Microsoft.IdentityModel.Logging;

namespace Gateway.API
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var authenticationProviderKey = "finbook";

            services.AddAuthentication()
                .AddIdentityServerAuthentication(authenticationProviderKey, option => {
                    option.Authority = "http://localhost:5000";
                    option.ApiName = "gateway_api";
                    option.SupportedTokens = SupportedTokens.Both;
                    option.ApiSecret = "secret";
                    option.RequireHttpsMetadata = false;
                });

            services.AddOcelot();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            IdentityModelEventSource.ShowPII = true;

            app.UseAuthentication();

            app.UseOcelot().Wait();
        }
    }
}
