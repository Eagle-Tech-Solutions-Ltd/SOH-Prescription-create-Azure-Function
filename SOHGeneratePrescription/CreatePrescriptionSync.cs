using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Repository;
using Order.Repository.Helper;
using Order.Repository.Model;
using Order.Repository.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace SOHGeneratePrescription
{
    public class CreatePrescriptionSync
    {
        private readonly ILogger _logger;
        private readonly ISalesOrderRepository _salesOrderRepo;

        public CreatePrescriptionSync(ILoggerFactory loggerFactory)
        {
            //_emailHelper = emailHelper;
            _logger = loggerFactory.CreateLogger<SOHOrderContextFactory>();
            var context = new SOHOrderContextFactory().CreateDbContext();
            _salesOrderRepo = new SalesOrderRepository(context);
        }

        [FunctionName("CreatePrescriptionSync")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var response = false;
            string map = req.Query["map"];

            if (!string.IsNullOrEmpty(map))
                response = await CreateOrderPrescriptions(log, Convert.ToInt32(map));

            return response ? new OkObjectResult("success") : new OkObjectResult("error");
        }

        public async Task<bool> CreateOrderPrescriptions(ILogger log, int id)
        {
            var retval = false;
            QuestPDF.Settings.License = LicenseType.Community;
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            //var zipFilePath = Path.Combine(tempDir, "prescriptions.zip");
            var parentTemp = Path.GetTempPath();
            var zipFilePath = Path.Combine(parentTemp, $"prescriptions_{Guid.NewGuid()}.zip");

            try
            {
                var allFilesList = new List<string>();
                var settings = await _salesOrderRepo.GetSettingsAsync().ConfigureAwait(false);

                if (settings == null || settings.Count == 0)
                {
                    log.LogInformation("No settings found for the application.");
                    return false;
                }

                var allOrders = await _salesOrderRepo.GetPrescriptionOrdersByIdAsync(id).ConfigureAwait(false);

                if (allOrders == null || allOrders.orders == null || allOrders.orders.Count == 0)
                {
                    log.LogInformation("No prescription orders found.");
                    return false;
                }

                int fileIndex = 1;
                var blobConnectionString = settings.Where(m => m.SystemName == "blobconnectionstring").Select(m => m.DisplayValue).FirstOrDefault();
                var containerName = settings.Where(m => m.SystemName == "containername").Select(m => m.DisplayValue).FirstOrDefault();
                var blobStorageEndpoint = settings.Where(m => m.SystemName == "blobstorageendpoint").Select(m => m.DisplayValue).FirstOrDefault();
                var fromEmail = settings.Where(m => m.SystemName == "fromemail").Select(m => m.DisplayValue).FirstOrDefault();
                var port = settings.Where(m => m.SystemName == "port").Select(m => m.DisplayValue).FirstOrDefault();
                var host = settings.Where(m => m.SystemName == "host").Select(m => m.DisplayValue).FirstOrDefault();
                var enableSsl = settings.Where(m => m.SystemName == "enablessl").Select(m => m.DisplayValue).FirstOrDefault() == "true";
                var smtpUsername = settings.Where(m => m.SystemName == "smtpusername").Select(m => m.DisplayValue).FirstOrDefault();
                var smtpPassword = settings.Where(m => m.SystemName == "smtppassword").Select(m => m.DisplayValue).FirstOrDefault();
                var userdefault = settings.Where(m => m.SystemName == "userdefaultcredentials").Select(m => m.DisplayValue).FirstOrDefault() == "false";
                var toEmails = settings.Where(m => m.SystemName == "prescriptiondistributionlist").Select(m => m.DisplayValue).FirstOrDefault();
                var forgeToken = await _salesOrderRepo.GetForgeAccessTokenAsync().ConfigureAwait(false);

                foreach (var item in allOrders.orders)
                {
                    if (item == null)
                    {
                        log.LogError("Invalid order data found for order ID: " + item?.id);
                        continue;
                    }

                    var order = item;
                    var orderItemList = item.OrderItemDetails.ToList();
                    var finalPrescriptionDetail = new PrescriptionDetailResponse();
                    var prescriptionDetails = await _salesOrderRepo.GetPrescriptionDetails(log, order.forge_order_id, forgeToken).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(prescriptionDetails))
                        finalPrescriptionDetail = JsonConvert.DeserializeObject<PrescriptionDetailResponse>(prescriptionDetails);

                    var customer = await _salesOrderRepo.GetCustomDetailAsync(log, order.bc_customer_id).ConfigureAwait(false);
                    if (customer != null)
                    {
                        var signature = await GetPrescriberSignature(log, finalPrescriptionDetail?.prescriber_signature_file, finalPrescriptionDetail?.prescriber_signature_folder).ConfigureAwait(false);
                        using var stream = new MemoryStream();
                        byte[] logoBytesSignature;
                        //https://onlinepngtools.com/images/examples-onlinepngtools/george-walker-bush-signature.png

                        using (var httpClient = new HttpClient())
                            logoBytesSignature = await httpClient.GetByteArrayAsync(!string.IsNullOrEmpty(signature) ? signature : "https://onlinepngtools.com/images/examples-onlinepngtools/george-walker-bush-signature.png");

                        var document = QuestPDF.Fluent.Document.Create(container =>
                        {
                            container.Page(page =>
                            {
                                page.Size(PageSizes.A4);
                                page.Margin(30);
                                page.DefaultTextStyle(x => x.FontSize(10));
                                page.Header().Column(header =>
                                {
                                    // Row for centered company name
                                    header.Item().AlignLeft().Column(col =>
                                    {
                                        col.Item().Text("Pharmacy & Doctor").FontSize(10).Bold().FontColor("#E0E0E2");
                                        col.Item().Text("https://www.simpleonlinedoctor.com.au/").FontSize(10).Bold();
                                        col.Item().Text("119 RACECOURSE RD").FontSize(10);
                                        col.Item().Text("ASCOT QLD").FontSize(10);
                                        col.Item().Text("4007").FontSize(10);
                                    });
                                });

                                page.Content().PaddingTop(20).Column(col =>
                                {
                                    col.Item().Text("Patient").FontSize(10).Bold().FontColor("#E0E0E2");
                                    col.Item().Text(item.CustomerName).FontSize(10).Bold();
                                    col.Item().Text(item.AddressLine1 + " " + item.AddressLine2).FontSize(10);
                                    col.Item().Text(item.City + " " + item.State).FontSize(10);
                                    col.Item().Text(item.Country).FontSize(10);
                                    col.Item().Text(item.Postcode).FontSize(10);

                                    col.Item().AlignLeft().PaddingLeft(50).PaddingTop(20).Column(customerArea =>
                                    {
                                        customerArea.Item().PaddingBottom(20).Row(row =>
                                        {
                                            col.Item().Text("Email: " + customer.email).FontSize(10).Bold();
                                            col.Item().Text("Phone: " + customer.phoneNumber).FontSize(10);
                                        });
                                    });

                                    //col.Spacing(10);

                                    col.Item().PaddingTop(20).AlignLeft().Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.RelativeColumn(50); // Item.
                                            cols.RelativeColumn(25);  // Quantity
                                            cols.RelativeColumn(25);  // Price
                                        });

                                        // Header with borders
                                        table.Header(header =>
                                        {
                                            header.Cell().AlignLeft().Element(CellStyle).Text("Order Date:").Bold();
                                            header.Cell().AlignRight().Element(CellStyle).Text("Prescription").Bold();
                                            header.Cell().AlignRight().Element(CellStyle).Text(item.forge_order_id);
                                        });

                                        table.Cell().AlignLeft().Element(CellStyle).Text(item.order_date.ToString("dd/MM/yyyy"));
                                        table.Cell().AlignRight().Element(CellStyle).Text("Prescription Date").Bold();

                                        if (finalPrescriptionDetail.prescription_writing_date.HasValue)
                                            table.Cell().AlignRight().Element(CellStyle).Text(finalPrescriptionDetail.prescription_writing_date.Value.Date.ToString("dd/MM/yyyy"));
                                        else
                                            table.Cell().AlignRight().Element(CellStyle).Text("");

                                        table.Cell().AlignLeft().Element(CellStyle).Text(item.forge_order_id);
                                        table.Cell().AlignRight().Element(CellStyle).Text("Order Amount (AUD)").Bold();
                                        table.Cell().AlignRight().Element(CellStyle).Text("$" + item.total_amount.ToString(""));

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container
                                                .Padding(2);
                                        }
                                    });

                                    col.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Black);

                                    col.Item().PaddingTop(20).AlignLeft().Table(table =>
                                    {
                                        table.ColumnsDefinition(cols =>
                                        {
                                            cols.RelativeColumn(40); // Product
                                            cols.RelativeColumn(14);  // Strength
                                            cols.RelativeColumn(12);  // Qty
                                            cols.RelativeColumn(15);  // Unit Cost
                                            cols.RelativeColumn(19);  // Sub Total
                                        });

                                        // Header with borders
                                        table.Header(header =>
                                        {
                                            header.Cell().AlignLeft().Element(CellStyle).Text("Product").Bold();
                                            header.Cell().AlignCenter().Element(CellStyle).Text("Strength").Bold();
                                            header.Cell().AlignCenter().Element(CellStyle).Text("Qty").Bold();
                                            header.Cell().AlignCenter().Element(CellStyle).Text("Unit Cost").Bold();
                                            header.Cell().AlignCenter().Element(CellStyle).Text("Sub Total").Bold();
                                        });

                                        if (orderItemList != null && orderItemList.Count > 0)
                                        {
                                            foreach (var item in orderItemList)
                                            {
                                                table.Cell().AlignLeft().Element(CellStyle).Text(item.name);
                                                table.Cell().AlignCenter().Element(CellStyle).Text("0.5mg");
                                                table.Cell().AlignCenter().Element(CellStyle).Text(item.quantity.ToString());
                                                table.Cell().AlignCenter().Element(CellStyle).Text("$" + item.unit_price);
                                                table.Cell().AlignCenter().Element(CellStyle).Text("$" + item.total_price);

                                                table.Cell().AlignLeft().Element(CellStyleNoPadd).Text("Dosage:").FontSize(5).Bold();
                                                table.Cell().ColumnSpan(4).Element(CellStyleNoPadd).Text("");

                                                table.Cell().AlignLeft().Element(CellStyleNoPadd).Text(item.dosage_label).FontSize(5);
                                                table.Cell().ColumnSpan(4).Element(CellStyleNoPadd).Text("");
                                            }
                                        }

                                        table.Cell().ColumnSpan(3).Element(CellStyleNoPadd).Text("");
                                        table.Cell().AlignRight().Element(CellStyle).Text("Sub Total").FontSize(13).Bold();
                                        table.Cell().AlignRight().Element(CellStyle).Text("$" + order.total_amount).FontSize(13).Bold();

                                        table.Cell().ColumnSpan(3).Element(CellStyleNoPadd).Text("");
                                        table.Cell().AlignRight().Element(CellStyle).Text("Total").FontSize(15).Bold();
                                        table.Cell().AlignRight().Element(CellStyle).Text("$" + order.total_amount).FontSize(15).Bold();

                                        static IContainer CellStyle(IContainer container)
                                        {
                                            return container
                                                .Padding(2);
                                        }

                                        static IContainer CellStyleNoPadd(IContainer container)
                                        {
                                            return container
                                                .Padding(0);
                                        }
                                    });

                                    col.Item().AlignLeft().PaddingLeft(50).PaddingTop(20).Column(customer =>
                                    {
                                        customer.Item().PaddingBottom(20).Row(row =>
                                        {
                                            col.Item().Text("Repeat(s): As required during the approved period within reasonable use guidelines for the treatment. More repeats may be dispensed as required in the event the patient does not receive the treatment.").FontSize(10);
                                        });
                                    });

                                    if (!string.IsNullOrEmpty(signature) && logoBytesSignature != null && logoBytesSignature.Length > 0)
                                    {
                                        col.Item().PaddingTop(30).Row(row =>
                                        {

                                            row.ConstantItem(90)
                                            .Height(60)
                                            .AlignLeft()
                                            .Image(logoBytesSignature, ImageScaling.FitArea);

                                            row.RelativeItem(); // Spacer to fill row
                                        });
                                    }
                                    else
                                    {
                                        col.Item().PaddingTop(30).Text("").FontSize(10).Bold();
                                    }

                                    string PrescriberName = string.Empty;

                                    if (!string.IsNullOrWhiteSpace(finalPrescriptionDetail?.prescriber_name))
                                    {
                                        if (!string.IsNullOrEmpty(finalPrescriptionDetail?.prescriber_title))
                                            PrescriberName = finalPrescriptionDetail?.prescriber_title + " " + finalPrescriptionDetail?.prescriber_name;
                                        else
                                            PrescriberName = finalPrescriptionDetail?.prescriber_name;

                                        if (!string.IsNullOrWhiteSpace(finalPrescriptionDetail?.prescriber_number))
                                            PrescriberName += $" (Prescriber No. {finalPrescriptionDetail?.prescriber_number})";
                                    }

                                    if (!string.IsNullOrEmpty(PrescriberName))
                                        col.Item().PaddingTop(5).Text(PrescriberName).FontSize(10).Bold();
                                    else
                                        col.Item().PaddingTop(5).Text("").FontSize(10).Bold();

                                    if (!string.IsNullOrEmpty(PrescriberName) && !string.IsNullOrEmpty(finalPrescriptionDetail?.prescriber_professional_qualitifcation))
                                        col.Item().PaddingTop(5).Text(finalPrescriptionDetail?.prescriber_professional_qualitifcation).FontSize(10).Bold();
                                    else
                                        col.Item().PaddingTop(5).Text("").FontSize(10).Bold();
                                });

                                page.Footer().AlignCenter().Column(column =>
                                {
                                    column.Item().Text("RACECOURSE ROAD PHARMACY, M.LEUNG & H.YOO").FontSize(10).AlignCenter();
                                    column.Item().Text("119 RACECOURSE RD, ASCOT 4007 Ph: 32683222").FontSize(10).AlignCenter();
                                });
                            });
                        });

                        // Generate PDF into the stream
                        document.GeneratePdf(stream);
                        stream.Position = 0;
                        var base64String = Convert.ToBase64String(stream.ToArray());
                        allFilesList.Add(base64String);
                        var forgeID = !string.IsNullOrEmpty(order.forge_order_id) ? order.forge_order_id : fileIndex.ToString();

                        // Save file from base64
                        var filePath = Path.Combine(tempDir, $"prescription_{forgeID.Replace(" ", "")}.pdf");
                        File.WriteAllBytes(filePath, Convert.FromBase64String(base64String));
                        fileIndex++;
                        retval = true;
                    }
                    else
                    {
                        log.LogError("Customer Details not found for: " + order.bc_customer_id);
                    }
                }

                // Create zip file
                ZipFile.CreateFromDirectory(tempDir, zipFilePath);

                // Upload zip to Azure Blob Storage
                string blobName = $"prescriptions_{DateTime.UtcNow:yyyyMMddHHmmss}.zip";
                var blobServiceClient = new BlobServiceClient(blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(blobName);

                using (var fileStream = File.OpenRead(zipFilePath))
                    await blobClient.UploadAsync(fileStream, overwrite: true);

                // Clean up temp files
                Directory.Delete(tempDir, true);

                if (retval)
                {
                    var ZipBlobFileLink = blobStorageEndpoint + "" + containerName + "/" + blobName;

                    retval = await _salesOrderRepo.UpdatePrescriptionOrdersAsync(log, false, allOrders.PrescriptionRequestID, ZipBlobFileLink, blobName).ConfigureAwait(false);
                    log.LogInformation("Prescription orders processed successfully. " + retval.ToString());

                    //var isSent = await SendEmail(log, host, Convert.ToInt32(port), enableSsl, userdefault, smtpUsername, smtpPassword, fromEmail, ZipBlobFileLink, toEmails);
                    //retval = await _salesOrderRepo.UpdatePrescriptionOrdersAsync(log, isSent, allOrders.PrescriptionRequestID, ZipBlobFileLink, blobName).ConfigureAwait(false);

                    //if (isSent)
                    //    log.LogInformation("Prescription orders processed successfully. " + retval.ToString());
                    //else
                    //    log.LogError("Prescription created but failed to send in email.");
                }
                else
                {
                    log.LogError("Failed to process prescription orders.");
                }
            }
            catch (Exception ex)
            {
                retval = false;
            }

            return retval;
        }

        public async Task<bool> SendEmail(ILogger log, string host, int port, bool ssl, bool defaultCredentials, string userName, string password, string fromEmail, string body, string toEmails)
        {
            var retval = false;
            try
            {
                var smtpClient = new SmtpClient(host)
                {
                    Port = Convert.ToInt32(port),
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = Convert.ToBoolean(ssl),
                    //UseDefaultCredentials = true
                };

                MailMessage mail = new MailMessage();
                mail.Subject = "Prescription Email";
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.To.Add(toEmails);

                mail.From = new MailAddress(userName, password);
                smtpClient.Send(mail);
                retval = true;
            }
            catch (Exception ex)
            {
                log.LogError($"Sending email error: {ex.ToString()}");
                log.LogInformation($"Sending email error: {ex.ToString()}");
            }

            return retval;
        }

        public async Task<string> GetPrescriberSignature(ILogger log, string fileName, string folderName)
        {
            var signature = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(folderName))
                {
                    log.LogError("File name or path is empty.");
                    return signature;
                }

                var request = new ForgeSignatureRequest
                {
                    file_name = fileName,
                    folder_name = folderName
                };

                var jsonString = JsonConvert.SerializeObject(request);
                var signatureResponse = await APICall.ForgePostCall(jsonString);

                if (!string.IsNullOrEmpty(signatureResponse))
                {
                    var signatureData = JsonConvert.DeserializeObject<ForgeSignatureResponse>(signatureResponse);
                    if (signatureData != null && !string.IsNullOrEmpty(signatureData.data?.download_url) && signatureData.status?.success == true)
                        signature = signatureData.data?.download_url;
                    else
                        log.LogError("Signature data is null or empty.");
                }
                else
                    log.LogError("Signature response is null or empty.");
            }
            catch (Exception ex)
            {
                log.LogError($"Error fetching prescriber signature: {ex.Message}");
            }
            return signature;
        }
    }
}
