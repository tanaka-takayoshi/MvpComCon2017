using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;
using ChatApp.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.SignalR.Redis;

namespace ChatApp
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
            var conn = Configuration["REDIS_CONNECTION_STRING"];

            var redis = ConnectionMultiplexer.Connect(conn);

            services.AddDataProtection()
                .PersistKeysToRedis(redis, "Dataprotection-Keys");

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = conn;
                option.InstanceName = "master";
            });

            services.AddSignalR().AddRedis(option =>
            {
                option.Options.AbortOnConnectFail = false;
                option.Options.Ssl = true;
                option.Options.EndPoints.Add("mvpcomcon2017.redis.cache.windows.net", 6380);
                option.Options.Password = "p9NLOpjI2bzGRSvXUQlogYTgwHpAksqG55OVvqkxklA=";
            });

            services.AddSingleton(typeof(RedisHubLifetimeManager<>), typeof(RedisHubLifetimeManager<>));
            //services.AddSingleton(typeof(HubLifetimeManager<>), typeof(RedisPresenceHublifetimeManager<>));
            //services.AddSingleton(typeof(IUserTracker<>), typeof(RedisUserTracker<>));

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddAzureAdB2C(options => Configuration.Bind("AzureAdB2C", options))
            .AddCookie();

            services.AddMvc();
        }

        private const string XForwardedPathBase = "X-Forwarded-PathBase";
        private const string XForwardedProto = "X-Forwarded-Proto";

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //not enough
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            //workaround https://github.com/aspnet/Docs/issues/2384#issuecomment-286146843
            app.Use((context, next) =>
            {
                if (context.Request.Headers.TryGetValue(XForwardedPathBase, out StringValues pathBase))
                {
                    context.Request.PathBase = new PathString(pathBase);

                }

                if (context.Request.Headers.TryGetValue(XForwardedProto, out StringValues proto))
                {
                    context.Request.Protocol = proto;
                }

                return next();
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("chat");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
