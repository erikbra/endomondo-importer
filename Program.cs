using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            if (args.Length != 1)
            {
                PrintUsage(Environment.GetCommandLineArgs()[0]);
                return 1;
            }

            var zipFile = args[0];
            
            ServiceProvider = BuildServiceProvider();
            
            Console.WriteLine("Importing stuff...");

            var token = await GetToken();
            
            await ImportAllFilesIn(zipFile, token);

            return 0;
        }

        private static void PrintUsage(string programName)
        {
            Console.Out.WriteLine($@"
Usage: {programName} <folder with export files from Endomondo>

Example: {programName} ~/endomondo-export
");
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
                if (token?.refresh_token != null)
                {
                    token = await client.Refresh(token.refresh_token);
                    await StoreToken(token, tokenFile);
                }
                else
                {
                    var code = await GetCode();
                    token = await client.Login(code);

                    await StoreToken(token, tokenFile);
                }
            }
            
            return token;
        }

        private static async Task StoreToken(OidcResponse token, string tokenFile)
        {
            var text = JsonSerializer.Serialize(token, new JsonSerializerOptions() {WriteIndented = true});
            await File.WriteAllTextAsync(tokenFile, text);
        }

        private static async Task ImportAllFilesIn(string file, OidcResponse token)
        {
            const string Workouts = nameof(Workouts);

            IList<string> jsonFiles = new List<string>();
            IList<string> tcxFiles = new List<string>();
            
            using ZipArchive archive = ZipFile.OpenRead(file);
            foreach (var entry in archive.Entries)
            {
                var fullName = entry.FullName;
                var directoryName = Path.GetDirectoryName(fullName);
                if (Workouts.Equals(directoryName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var lastDot = fullName.LastIndexOf('.');
                    var stem = fullName.Substring(0, lastDot);
                    var extension = fullName.Substring(lastDot);
                    switch (extension)
                    {
                        case ".tcx":
                            tcxFiles.Add(stem);
                            break;
                        case ".json":
                            jsonFiles.Add(stem);
                            break;
                        default:
                            throw new ApplicationException("Unknown file type: " + extension);
                    }
                }
                
                //await Console.Out.WriteLineAsync(directoryName);
                //await Console.Out.WriteLineAsync(entry.Name);
            }

            var missingTcx = jsonFiles.Except(tcxFiles);
            foreach (var stem in missingTcx)
            {
                Console.WriteLine($"Couldn't find TCX file for JSON file {stem}.json");
            }

            var missingJson = tcxFiles.Except(jsonFiles);
            foreach (var stem in missingJson)
            {
                Console.WriteLine($"Couldn't find JSON file for TCX file {stem}.tcx");
            }

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
