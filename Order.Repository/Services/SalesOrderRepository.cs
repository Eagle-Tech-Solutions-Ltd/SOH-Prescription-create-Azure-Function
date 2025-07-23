using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Order.Repository.DataContexts;
using Order.Repository.Entities;
using Order.Repository.Helper;
using Order.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Services
{
    public class SalesOrderRepository : ISalesOrderRepository
    {
        private readonly SOHOrderDBContext _context;

        private static string _cachedToken;
        private static DateTime _tokenExpiry = DateTime.MinValue;
        private static readonly object _tokenLock = new();

        //private static List<BCCustomerResponse> _cachedCustomer;
        //private static DateTime _customerExpiry = DateTime.MinValue;
        //private static readonly object _customerLock = new();

        public SalesOrderRepository(SOHOrderDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BCCustomerResponse> GetCustomDetailAsync(ILogger log, string id)
        {
            try
            {
                string token = await GetAccessTokenAsync(log);
                var DefaultCompanyID = Environment.GetEnvironmentVariable("DefaultCompanyID");
                var bcCustomerDetailResponse = await APICall.GetCallBC(log,$"v2.0/companies({DefaultCompanyID})/customers({id})", token);

                if (string.IsNullOrEmpty(bcCustomerDetailResponse))
                {
                    log.LogError("Customer Empty from BC api");
                    return null;
                }

                var retval = JsonConvert.DeserializeObject<BCCustomerResponse>(bcCustomerDetailResponse);

                if (retval != null && !string.IsNullOrEmpty(retval.id))
                {
                    return retval;
                }
                else
                {
                    log.LogError("Customer Detail Empty: "+ bcCustomerDetailResponse);
                }
            }
            catch (Exception ex)
            {
                log.LogError("Customer Detail Error: " + ex.ToString());
            }

            return null;
        }

        //public async Task<List<BCCustomerResponse>> GetCustomListAsync(ILogger log)
        //{
        //    try
        //    {
        //        string token = await GetAccessTokenAsync();
        //        var DefaultCompanyID = Environment.GetEnvironmentVariable("DefaultCompanyID");
        //        var bcCustomerDetailResponse = await APICall.GetCallBC($"v2.0/companies({DefaultCompanyID})/customers", token);

        //        if (string.IsNullOrEmpty(bcCustomerDetailResponse))
        //        {
        //            log.LogError("Customer List Empty from BC api");
        //            return null;
        //        }

        //        var retval = JsonConvert.DeserializeObject<BCCustomerListResponse>(bcCustomerDetailResponse);

        //        if (retval.value != null && retval.value.Count > 0)
        //        {
        //            // Filter out customers with empty IDs
        //            var value = retval.value.Where(c => !string.IsNullOrEmpty(c.id)).ToList();
        //            _cachedCustomer = value;
        //            return value;
        //        }
        //        else
        //        {
        //            log.LogError("Customer List Empty");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError("Customer List Error: " + ex.ToString());
        //    }

        //    return null;
        //}

        //public async Task<BCCustomerResponse> GetCustomerByID(ILogger log,string customerId)
        //{
        //    try
        //    {
        //        if (_cachedCustomer != null)
        //        {
        //            var retval = _cachedCustomer.FirstOrDefault(o => o.id == customerId);

        //            if (retval != null)
        //                return retval;
        //            else
        //            {
        //                await GetCustomListAsync(log);
        //                return _cachedCustomer.FirstOrDefault(o => o.id == customerId);
        //            }
        //        }
        //        else
        //        {
        //            await GetCustomListAsync(log);
        //            return _cachedCustomer.FirstOrDefault(o => o.id == customerId);
        //        }
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        public async Task<string> GetAccessTokenAsync(ILogger log)
        {
            // Thread-safe cache check
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                    return _cachedToken;
            }

            try
            {
                var TokenURL = Environment.GetEnvironmentVariable("TokenURL");
                var ClientID = Environment.GetEnvironmentVariable("ClientID");
                var Tenant = Environment.GetEnvironmentVariable("Tenant");
                var ClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
                var Scope = Environment.GetEnvironmentVariable("Scope");

                using (HttpClient client = new HttpClient())
                {
                    var requestData = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", ClientID),
                        new KeyValuePair<string, string>("client_secret", ClientSecret),
                        new KeyValuePair<string, string>("scope", Scope),
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    });

                    HttpResponseMessage response = await client.PostAsync(TokenURL, requestData);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(responseBody);
                        var token = json["access_token"]?.ToString();
                        var expiresIn = json["expires_in"]?.ToObject<int?>() ?? 3599;

                        if (!string.IsNullOrEmpty(token))
                        {
                            lock (_tokenLock)
                            {
                                _cachedToken = token;
                                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // Renew 1 min before expiry
                            }
                            return token;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // Optionally log the exception
                log.LogError("Acc Token error: " + ex.ToString());
            }

            return string.Empty;
        }

        public async Task<string> GetForgeAccessTokenAsync()
        {
            var retval = "";

            try
            {
                var APIUrl = Environment.GetEnvironmentVariable("ForgeAPIUrl");
                var ClientID = Environment.GetEnvironmentVariable("ForgeClientID");
                var ClientSecret = Environment.GetEnvironmentVariable("ForgeClientSecret");

                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, APIUrl + "generate-token/");
                    request.Headers.Add("X-Client-ID", ClientID);
                    request.Headers.Add("X-Client-Secret", ClientSecret);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var tokenResponse = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(tokenResponse))
                    {
                        var mainModel = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse);

                        if (mainModel != null && !string.IsNullOrEmpty(mainModel.token))
                            retval = mainModel.token;
                    }

                }
            }
            catch (Exception ex)
            {
            }

            return retval;
        }

        public async Task<List<AppConfigurationSetting>> GetSettingsAsync()
        {
            return await _context.AppConfigurationSettings
                .Where(o => o.IsDelete == false)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<PendingPrescriptionResponse> GetPrescriptionOrdersByIdAsync(int id)
        {
            var retval = new PendingPrescriptionResponse();

            try
            {
                var prescriptionDownloadRequests = await _context.PrescriptionDownloadRequests
                .Where(o => o.IsWorkDone == false && o.ID == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

                if (prescriptionDownloadRequests == null)
                    return retval;

                retval.PrescriptionRequestID = prescriptionDownloadRequests.ID;
                var orderIds = prescriptionDownloadRequests.OrderIds.Replace(" ", "").Split(',').Select(int.Parse).ToList();
                retval.orders = await _context.OrderDetails
                    .Where(o => orderIds.Contains(o.id) && o.status == "shipped")
                    .Include(o => o.OrderItemDetails)
                    .ToListAsync()
                    .ConfigureAwait(false);

                return retval;
            }
            catch (Exception ex)
            {
            }

            return null;
        }

        //public async Task<PendingPrescriptionResponse> GetPrescriptionOrdersAsync()
        //{
        //    var retval = new PendingPrescriptionResponse();

        //    try
        //    {
        //        var prescriptionDownloadRequests = await _context.PrescriptionDownloadRequests
        //        .Where(o => o.IsWorkDone == false)
        //        .FirstOrDefaultAsync()
        //        .ConfigureAwait(false);

        //        if (prescriptionDownloadRequests == null)
        //            return retval;

        //        retval.PrescriptionRequestID = prescriptionDownloadRequests.ID;
        //        var orderIds = prescriptionDownloadRequests.OrderIds.Replace(" ", "").Split(',').Select(int.Parse).ToList();
        //        retval.orders = await _context.OrderDetails
        //            .Where(o => orderIds.Contains(o.id) && o.status == "shipped")
        //            .Include(o => o.OrderItemDetails)
        //            .ToListAsync()
        //            .ConfigureAwait(false);

        //        return retval;
        //    }
        //    catch (Exception ex)
        //    {
        //    }

        //    return null;
        //}

        public async Task<bool> UpdatePrescriptionOrdersAsync(ILogger log, bool IsSent, int id, string blobFilePath, string fileName)
        {
            var retval = false;

            try
            {
                var prescriptionDownloadRequest = await _context.PrescriptionDownloadRequests
                .Where(o => o.ID == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

                if (prescriptionDownloadRequest != null)
                {
                    prescriptionDownloadRequest.IsWorkDone = true;
                    prescriptionDownloadRequest.IsSent = IsSent;
                    prescriptionDownloadRequest.TryCount = 1;
                    prescriptionDownloadRequest.BlobFileName = fileName;
                    prescriptionDownloadRequest.BlobFilePath = blobFilePath;
                    prescriptionDownloadRequest.UpdatedDate = TimeZoneHelper.GetCurrentAustraliaTime();
                    _context.PrescriptionDownloadRequests.Update(prescriptionDownloadRequest);
                    await _context.SaveChangesAsync();
                    log.LogInformation($"Update Prescription Successfully.");
                    retval = true;
                }
            }
            catch (Exception ex)
            {
                log.LogError("Update Prescription Error: " + ex.ToString());
                retval = false;
            }

            return retval;
        }

        public async Task<string> GetPrescriptionDetails(ILogger log, string forgeOrderId, string forgeToken)
        {
            var retval = string.Empty;

            try
            {
                var prescriptionDetail = await APICall.ForgeGetCall("order-prescriber-data/" + forgeOrderId + "/", forgeToken);
                log.LogError("Prescription Details: " + prescriptionDetail);
                return prescriptionDetail;
            }
            catch (Exception ex)
            {
            }

            return retval;
        }
    }
}
