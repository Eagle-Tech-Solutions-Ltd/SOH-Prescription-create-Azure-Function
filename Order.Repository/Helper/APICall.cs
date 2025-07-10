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

        public static async Task<string> GetCallBC(string APIEndPoint, string Token)
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
            }

            return retval;
        }
    }
}
