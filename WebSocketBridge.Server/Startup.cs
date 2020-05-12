using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebSocketBridge.Server.Authentication;
using WebSocketBridge.Server.IotHub;
using WebSocketBridge.Server.MultiNode;
using WebSocketBridge.Server.Services;
using WebSocketBridge.Server.SingleNode;

namespace WebSocketBridge.Server
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
            var aiConfig = Configuration.GetSection("AppInsights");
            services
                .AddApplicationInsightsTelemetry()
                .ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, options) =>
                {
                    module.AuthenticationApiKey = aiConfig.GetValue<string>("AuthenticationApiKey");
                })
                .AddSingleton<ITelemetryChannel>(ctx => new ServerTelemetryChannel
                {
                    StorageFolder = aiConfig?.GetValue<string>("StorageFolder") ?? Path.GetTempPath()
                });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            services
                .Configure<IotSettings>(Configuration.GetSection("Iot"))
                .AddSingleton<IServiceClientProvider, ServiceClientProvider>()
                .AddSingleton<IRequestBridgeNotifier, IotHubRequestBridgeNotifier>()
                .AddSingleton(ctx => UrlEncoder.Default);

            var webBridgeConfiguration = Configuration.GetSection("WebBridge");
            services.Configure<WebBridgeOptions>(webBridgeConfiguration);

            // Check if the operational store has been configured
            var dataProtectionConfig = Configuration.GetSection("DataProtection");
            if (dataProtectionConfig != null)
            {
                var dataProtectionOptions = new DataProtectionOptions();
                dataProtectionConfig.Bind(dataProtectionOptions);

                var directory = new DirectoryInfo(dataProtectionOptions.Directory);
                directory.Create();

                services
                    .AddDataProtection()
                    .PersistKeysToFileSystem(directory);
            }

            // Registration of the single-server stream bridge
            var multiNodeConfiguration = Configuration.GetSection("MultiNode");
            if (multiNodeConfiguration != null)
                services.AddMultiNode(multiNodeConfiguration);
            else
                services.AddSingleNode();

            services
                .AddControllers();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                    options.DefaultChallengeScheme = ApiKeyAuthenticationOptions.DefaultScheme;
                })
                .AddApiKeySupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
