using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using CMI.Access.Repository.Properties;
using Serilog;

namespace CMI.Access.Repository.Systems.Rosetta
{
    public class RosettaConnector
    {
        private readonly int maxCommandExecutionTime = Settings.Default.RepositoryTimeout;
        private readonly int maxCommandTimeout = Settings.Default.RepositoryCommandTimeout;
        private readonly string password = Settings.Default.RepositoryPassword;
        private readonly string username = Settings.Default.RepositoryUser;
        private readonly string exportIeUrl = Settings.Default.RepositoryExportIEUrl;

        public async Task<bool> StartExportAsync(string entityId)
        {
            // e. g.: "https://app.data-archive-test.ethz.ch/rest/v0/ies/{0}?op=export&export_path=/transdata/eth_vls&representation_packaging=tar",
            var url = string.Format(exportIeUrl, entityId);
            var exportXml = await PostAsync(url, new StringContent(string.Empty, Encoding.UTF8, "application/xml"));
            if (string.IsNullOrEmpty(exportXml))
            {
                Log.Error($"Failed to get process URL for entity {entityId}");
                return false;
            }
            var entityExportResult = new EntityExportResult(exportXml);

            if (string.IsNullOrEmpty(entityExportResult.ProcessUrl))
            {
                Log.Error($"Failed to get process URL for entity {entityId} ErrorMessage {entityExportResult.ErrorMessage}");
                return false;
            }

            var timeoutCancellation = new CancellationTokenSource(maxCommandExecutionTime);
            try
            {
                var success =  await WaitForCompletitionAsync(entityExportResult.ProcessUrl, timeoutCancellation.Token);
                return success;
            }
            catch (OperationCanceledException)
            {
                if (timeoutCancellation.IsCancellationRequested)
                {
                    Log.Error("Export process for entity {entityId} timed out", entityId);
                }
                throw;
            }
        }

        private async Task<bool> WaitForCompletitionAsync(string processUrl, CancellationToken stopToken)
        {
            processUrl = $"{Settings.Default.RepositoryServiceUrl}/{processUrl}";
            Log.Information("Getting status for export process {processUrl}", processUrl);

            using var httpClient = GetHttpClient();
            while (true)
            {   
                stopToken.ThrowIfCancellationRequested();
                var response = await httpClient.GetAsync(processUrl,HttpCompletionOption.ResponseContentRead, stopToken);
                response.EnsureSuccessStatusCode();
                
                var statusXml = await response.Content.ReadAsStringAsync();
                var statusElement = XDocument.Parse(statusXml).Root.Element("status");
                
                switch (statusElement?.Value)
                {
                    case "COMPLETED_SUCCESS":
                        return true;
                    case "RUNNING":
                        Log.Debug("Process status: RUNNING");
                        break;
                    case "PENDING":
                        Log.Debug("Process status: PENDING");
                        break;
                    default:
                        Log.Error($"Failed to get status for process {processUrl}: {statusElement?.Value}");
                        return false;
                }
               
                await Task.Delay(maxCommandTimeout, stopToken);
            }
        }

        private async Task<string> PostAsync(string url, HttpContent content)
        {
            using var httpClient = GetHttpClient();
            try
            {
                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "{source} - HTTP request failed: {message}", this, ex.Message);
            }

            return null;
        }

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient();

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            return httpClient;
        }
    }
}
