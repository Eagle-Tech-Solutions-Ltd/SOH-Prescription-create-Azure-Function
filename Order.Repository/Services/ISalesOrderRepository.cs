using Microsoft.Extensions.Logging;
using Order.Repository.Entities;
using Order.Repository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Services
{
    public interface ISalesOrderRepository
    {
        Task<BCCustomerResponse> GetCustomDetailAsync(ILogger log, string customerId);
        //Task<BCCustomerResponse> GetCustomerByID(ILogger log,string customerId);
        //Task<List<BCCustomerResponse>> GetCustomListAsync(ILogger log);

        Task<List<AppConfigurationSetting>> GetSettingsAsync();
        Task<PendingPrescriptionResponse> GetPrescriptionOrdersByIdAsync(int id);
        //Task<PendingPrescriptionResponse> GetPrescriptionOrdersAsync();
        Task<bool> UpdatePrescriptionOrdersAsync(ILogger log, bool isSent, int id, string blobFilePath, string fileName);
        Task<string> GetForgeAccessTokenAsync();
        Task<string> GetPrescriptionDetails(ILogger log, string forgeOrderId, string forgeToken);
    }
}
