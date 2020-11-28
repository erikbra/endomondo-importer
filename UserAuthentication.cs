using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace endoimport
{
    public class UserAuthentication
    {
        private readonly StravaAppConfig _config;
        private readonly ILogger<UserAuthentication> _logger;
        private readonly Uri _redirectUrl = new Uri("https://www.strava.com");

        public UserAuthentication(IOptions<StravaAppConfig> config, ILogger<UserAuthentication> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public async Task<string> GetCode()
        {
            var issuer = new Uri("https://www.strava.com/");
            var authorizeUrl = new Uri(issuer, "oauth/authorize" );

            var callbackUrl = new Uri("http://localhost:26004/callback");

            var state = "any state";

            IDictionary<string, string> param = new Dictionary<string, string>
            {
                { "client_id", _config.ClientId },
                { "scope", "activity:read_all,activity:write" },
                { "redirect_uri", callbackUrl.ToString() },
                { "response_type", "code" },
                { "approval_prompt", "auto"},
                { "state", state },
            };

            var authUrl = new Uri(QueryHelpers.AddQueryString(authorizeUrl.ToString(), param));

            TryOpenBrowser(authUrl);

            return await WaitForHttpResponseFromOidc(callbackUrl, _redirectUrl);
        }

        private void TryOpenBrowser(Uri url)
        {
            var urlString = url.ToString();

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(urlString) {UseShellExecute = true});
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", urlString);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", urlString);
                }
                else
                {
                    //throw 
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Unable to open browser for OS {rid}: {errorMessage}", RuntimeInformation.OSDescription, e.Message);
                
                Console.WriteLine($"Unable to open browser automatically. Please open the following URL in your browser: \n\n\n{urlString}");
            }
        }
        
        private static async Task<string> WaitForHttpResponseFromOidc(Uri callbackUrl, Uri redirectUrl)
        {
            using HttpListener listener = new HttpListener();
            listener.Prefixes.Add(callbackUrl + "/");
            listener.Start();
            
            Console.WriteLine($"Awaiting login response on {callbackUrl}");

            // Will wait here until we hear from a connection
            HttpListenerContext ctx = await listener.GetContextAsync();

            // Peel out the requests and response objects
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;

            var query = req.QueryString;
            resp.Redirect(redirectUrl.ToString());
            resp.OutputStream.Close();

            return query["code"];
        }
        
      
    }
}