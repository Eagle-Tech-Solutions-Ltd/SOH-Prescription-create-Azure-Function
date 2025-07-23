using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Helper
{
    public class APICall
    {
        public static async Task<string> PostCallBCOData(string APIEndPoint, string Token, string data)
        {
            var retval = string.Empty;

            try
            {
                var Tenant = Environment.GetEnvironmentVariable("Tenant");
                var SandboxName = Environment.GetEnvironmentVariable("SandboxName");
                APIEndPoint = "https://api.businesscentral.dynamics.com/v2.0/" + Tenant + "/" + SandboxName + "/ODataV4/" + APIEndPoint;
                var jsonContent = new StringContent(data);
                jsonContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                var httpClient = new HttpClient { BaseAddress = new Uri(APIEndPoint) };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                var task = httpClient.PostAsync(APIEndPoint, jsonContent);
                var httpResponseMessage = task.Result;

                if (httpResponseMessage.IsSuccessStatusCode)
                    retval = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
            }

            return retval;
        }

        public static async Task<string> GetCallBC(ILogger log,string APIEndPoint, string Token)
        {
            var retval = string.Empty;

            try
            {
                var Tenant = Environment.GetEnvironmentVariable("Tenant");
                var SandboxName = Environment.GetEnvironmentVariable("SandboxName");

                APIEndPoint = "https://api.businesscentral.dynamics.com/v2.0/" + Tenant + "/" + SandboxName + "/api/" + APIEndPoint;
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(APIEndPoint);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                HttpResponseMessage response = client.GetAsync(APIEndPoint).Result;
                if (response.IsSuccessStatusCode)
                    retval = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                log.LogError("Customer BC api response error: " + ex.ToString());
            }

            return retval;
        }

        public static async Task<string> ForgePostCall(string Data)
        {
            var retval = string.Empty;

            try
            {
                var forgeAPIKey = Environment.GetEnvironmentVariable("ForgeAPIKey");
                var finalAPIURL = Environment.GetEnvironmentVariable("ForgeSignatureAPIURL");
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, finalAPIURL);
                request.Headers.Add("x-api-key", forgeAPIKey);
                var jsonContent = new StringContent(Data);
                jsonContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                request.Content = jsonContent;
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    retval = await response.Content.ReadAsStringAsync();
                else
                    retval = await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
            }

            return retval;
        }

        public static async Task<string> ForgeGetCall(string APIEndPoint, string Token)
        {
            var retval = string.Empty;

            try
            {
                var APIURL = Environment.GetEnvironmentVariable("ForgeAPIUrl");
                var finalAPIURL = APIURL + "" + APIEndPoint;
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, finalAPIURL);
                request.Headers.Add("Authorization", "Bearer " + Token);
                var content = new StringContent(string.Empty);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                retval = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
            }

            return retval;
        }
    }
}
