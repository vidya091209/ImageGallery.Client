using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ImageGallery.Client.Configuration;
using StackExchange.Redis;

using ConfigurationOptions = ImageGallery.Client.Configuration.ConfigurationOptions;


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

namespace ImageGallery.Client2
{
    public class Startup
    {
        /*
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        */
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        //public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddOptions();
            services.Configure<ConfigurationOptions>(Configuration);

            services.Configure<ConfigurationOptions>(Configuration.GetSection("applicationSettings"));
            services.Configure<Dataprotection>(Configuration.GetSection("dataprotection"));
            services.Configure<OpenIdConnectConfiguration>(Configuration.GetSection("openIdConnectConfiguration"));

            services.AddSingleton<IConfiguration>(Configuration);

            var config = Configuration.Get<ConfigurationOptions>();

            //config.OpenIdConnectConfiguration.ClientId = "";


            Console.WriteLine($"Dataprotection Enabled: {config.Dataprotection.Enabled}");
            Console.WriteLine($"DataprotectionRedis: {config.Dataprotection.RedisConnection}");
            Console.WriteLine($"RedisKey: {config.Dataprotection.RedisKey}");

            if (config.Dataprotection.Enabled)
            {
                var redis = ConnectionMultiplexer.Connect(config.Dataprotection.RedisConnection);
                services.AddDataProtection().PersistKeysToRedis(redis, config.Dataprotection.RedisKey);
            }

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "ImageGallery.Client",
                    Description = "ImageGallery.Client",
                    Version = "v1"
                });
            });

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                    "CanOrderFrame",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("country", "be");
                        policyBuilder.RequireClaim("subscriptionlevel", "PayingUser");
                        //policyBuilder.RequireRole("role", "PayingUser");
                    });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                options.Authority = config.OpenIdConnectConfiguration.Authority;
                options.ClientId = config.OpenIdConnectConfiguration.ClientId;
                
                var arr = new String[]{ "openid", "profile", "address", "roles", "imagegalleryapi", "subscriptionlevel", "country", "offline_access"};
                foreach (var scope in arr) {
                    options.Scope.Add(scope);
                }

                options.ResponseType = "code id_token";
                options.SaveTokens = true;
                options.ClientSecret = config.OpenIdConnectConfiguration.ClientSecret;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = tokenValidatedContext =>
                    {
                        var identity = tokenValidatedContext.Principal.Identity
                            as ClaimsIdentity;

                        var subjectClaim = identity.Claims.FirstOrDefault(z => z.Type == "sub");
                                                
                        var newClaimsIdentity = new ClaimsIdentity(
                            "oidc",
                            "given_name",
                            "role");

                        newClaimsIdentity.AddClaim(subjectClaim);

                        tokenValidatedContext.Principal = new ClaimsPrincipal(newClaimsIdentity);

                        return Task.FromResult(0);
                    },
                    OnUserInformationReceived = userInformationReceivedContext =>
                    {
                        userInformationReceivedContext.User.Remove("address");
                        return Task.FromResult(0);
                    }
                };
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //var options2 = new RewriteOptions().AddRedirectToHttps();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };

            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardOptions);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ImageGallery.Client V1");
                options.ConfigureOAuth2("swaggerui", "", "", "Swagger UI");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
