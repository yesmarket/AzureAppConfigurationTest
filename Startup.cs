using System;
using AppConfigTest;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AppConfigTest
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var config =
                new ConfigurationBuilder()
                    .AddAzureAppConfiguration(options =>
                    {
                        var env = Environment.GetEnvironmentVariable("AZURE_FUNCTION_ENVIRONMENT");
                        options
                            .Connect(Environment.GetEnvironmentVariable("AppConfigurationConnectionString"))
                            .Select("*", env)
                            .ConfigureKeyVault(kv =>
                            {
                                kv.SetCredential(TokenCredential);
                            })
                            .ConfigureRefresh(refreshOptions =>
                            {
                                refreshOptions
                                    .Register("Test:Message", env, false)
                                    .SetCacheExpiration(TimeSpan.FromSeconds(1));
                            });
                        services.AddSingleton(options.GetRefresher());
                    })
                    .Build();

            services.AddSingleton<IConfiguration>(config);
            services.Configure<Test>(config.GetSection("Test"));
        }

        private static TokenCredential TokenCredential =>
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"))
                ? (TokenCredential)new DefaultAzureCredential()
                : new ManagedIdentityCredential();
    }
}
