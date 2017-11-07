using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ImageGallery.Client.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using ConfigurationOptions = ImageGallery.Client.Configuration.ConfigurationOptions;

namespace ImageGallery.Client
{
    public class Startup
    {
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

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
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

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies",
                AccessDeniedPath = "/Authorization/AccessDenied"
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var config = Configuration.Get<ConfigurationOptions>();
            Console.WriteLine("Authority" + config.OpenIdConnectConfiguration.Authority);

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            {
                RequireHttpsMetadata = false,

                AuthenticationScheme = "oidc",
                Authority = config.OpenIdConnectConfiguration.Authority, 

                ClientId = "imagegalleryclient",
                Scope = { "openid", "profile", "address", "roles", "imagegalleryapi", "subscriptionlevel", "country", "offline_access" },
                ResponseType = "code id_token",
                // CallbackPath = new PathString("...")
                //SignedOutCallbackPath = new PathString("")
                SignInScheme = "Cookies",
                SaveTokens = true,
                ClientSecret = config.OpenIdConnectConfiguration.ClientSecret,
                GetClaimsFromUserInfoEndpoint = true,
                Events = new OpenIdConnectEvents()
                {
                    OnTokenValidated = tokenValidatedContext =>
                    {
                        var identity = tokenValidatedContext.Ticket.Principal.Identity
                            as ClaimsIdentity;

                        var subjectClaim = identity.Claims.FirstOrDefault(z => z.Type == "sub");

                        var newClaimsIdentity = new ClaimsIdentity(
                            tokenValidatedContext.Ticket.AuthenticationScheme,
                            "given_name",
                            "role");

                        newClaimsIdentity.AddClaim(subjectClaim);

                        tokenValidatedContext.Ticket = new AuthenticationTicket(
                            new ClaimsPrincipal(newClaimsIdentity),
                            tokenValidatedContext.Ticket.Properties,
                            tokenValidatedContext.Ticket.AuthenticationScheme);

                        return Task.FromResult(0);
                    },
                    OnUserInformationReceived = userInformationReceivedContext =>
                    {
                        userInformationReceivedContext.User.Remove("address");
                        return Task.FromResult(0);
                    }
                }
            });

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
            });
        }         
    }
}
