using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace endoimport
{
    public class StravaApi
    {
        private HttpClient _httpClient;
        private StravaAppConfig _config;

        public StravaApi(HttpClient httpClient, IOptions<StravaAppConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }
        
        public async Task<UploadResponse> UploadActivity(Activity activity, string accessToken)
        {
            var url = "uploads";

            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer " + accessToken);

            UploadResponse result;

            using (var ms = new MemoryStream())
            {
                await activity.File.CopyToAsync(ms);
                var len = ms.Length;
                ms.Position = 0;
            
                using var form = new MultipartFormDataContent();
                //using (var streamContent = new StreamContent(activity.File))
                using (var streamContent = new StreamContent(ms))
                {
                    streamContent.Headers.Add("Content-Type", "application/octet-stream");
                    streamContent.Headers.Add("Content-Length", len.ToString());

                    //request.Content = streamContent;

                    //var response = new HttpClient().SendAsync(request).Result;

                    form.Add(streamContent, "file");
                    // using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                    // {
                    //     fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    //
                    //     // "file" parameter name should be the same as the server side input parameter name
                    //     form.Add(fileContent, "file", Path.GetFileName(filePath));
                    //     HttpResponseMessage response = await httpClient.PostAsync(url, form);
                    // }


                    form.Add(new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["name"] = activity.Name,
                        ["description"] = activity.Description,
                        ["data_type"] = activity.DataType,
                        ["external_id"] = activity.ExternalId,
                        ["commute"] = activity.Commute ? "1" : "0",
                        ["trainer"] = activity.Trainer ? "1" : "0"
                    }));

                    request.Content = form;
                    var response = await _httpClient.SendAsync(request);
                    result = await response.Content.ReadFromJsonAsync<UploadResponse>();
                }
            }

            return result;
        }
    }
}