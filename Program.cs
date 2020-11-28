using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using endoimport;
using endoimport.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace endomondo_importer
{
    class Program
    {
        const string _clientId = "56860";
        private static bool _verbose;
        
        private static ServiceProvider ServiceProvider;

        static async Task<int> Main(string[] args)
        {
            ServiceProvider = BuildServiceProvider();
            
            Console.WriteLine("Importing stuff...");

            var token = await GetToken();
            
            var folder = GetFolder();
            await ImportAllFilesIn(folder);

            return 0;
        }

        private static async Task<OidcResponse> GetToken()
        {
            var client = ServiceProvider.GetRequiredService<OidcClient>();
            
            OidcResponse token = null;
            bool tokenIsValid = false;
            
            // Get token from file if it exists, and check if it is still valid.
            var tokenFile = Config.GetConfigFile("token.json");
            if (File.Exists(tokenFile))
            {
                var content = await File.ReadAllTextAsync(tokenFile);
                token = JsonSerializer.Deserialize<OidcResponse>(content);

                var now = DateTime.UtcNow;
                var epochSeconds = (now - DateTime.UnixEpoch).TotalSeconds;

                tokenIsValid = token?.expires_at > epochSeconds;
            }

            if (!tokenIsValid)
            {
                var code = await GetCode();
                token = await client.Login(code);

                var text = JsonSerializer.Serialize(token, new JsonSerializerOptions() { WriteIndented = true});
                await File.WriteAllTextAsync(tokenFile, text);
            }
            
            return token;
        }

        private static async Task ImportAllFilesIn(IEnumerable<FileInfo> folder)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        private static IEnumerable<FileInfo> GetFolder()
        {
            throw new NotImplementedException();
        }
        
        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            var dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");

            var logFile = Config.GetConfigFile($"{dateSuffix}.log");

            services.AddLogging(logging => 
                        logging
                            .AddFileLogger(options => options.File = logFile)
                            .SetMinimumLevel(LogLevel.Debug));
            
            services.AddHttpClient<OidcClient>(client => 
                    client.BaseAddress = new Uri("https://www.strava.com/oauth/token"))
                        .AddPolicyHandler(GetRetryPolicy());

            services.AddSingleton<UserAuthentication>();
            
            var clientConfigBuilder = new ConfigurationBuilder()
                .AddJsonFile(Config.GetConfigFile("client.config"), false);
            
            var cfg = clientConfigBuilder.Build();
            
            services.Configure<StravaAppConfig>(
                cfg.GetSection(nameof(StravaAppConfig)));
            
            return services.BuildServiceProvider();
        }

        
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static async Task<string> GetCode()
        {
            var userAuth = ServiceProvider.GetRequiredService<UserAuthentication>();
            return await userAuth.GetCode();
        }

    }
}
