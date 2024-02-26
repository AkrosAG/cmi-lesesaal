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
        private const int maxCommandExecutionTime = 60000;
        private const int maxCommandTimeout = 1000;
        private readonly string password = Settings.Default.RepositoryPassword;
        private readonly string username = Settings.Default.RepositoryUser;

        public async Task<string> ExportEntityAsync(string entityId)
        {
            // e. g.: https://app.data-archive-test.ethz.ch/rest/v0/ies/IE444295?op=export
            var url = string.Format(Settings.Default.RepositoryExportIEUrl, entityId);
            var exportXml = await PostAsync(url, new StringContent(string.Empty, Encoding.UTF8, "application/xml"));

            var entityExportResult = new EntityExportResult(exportXml);

            if (string.IsNullOrEmpty(entityExportResult.ProcessUrl))
            {
                Log.Error("Failed to get process URL for entity {entityId}", entityId);
                return null;
            }

            var timeoutCancellation = new CancellationTokenSource(maxCommandExecutionTime);
            try
            {
                var success =  await WaitForCompletitionAsync(entityExportResult.ProcessUrl, timeoutCancellation.Token);
                return success ? entityExportResult.ExportPath : null;
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
                        Log.Information("Process status: {status}", statusElement.Value);
                        break;
                    default:
                        Log.Error("Failed to get status for process {processUrl}: {status}", processUrl, statusElement?.Value ?? "NULL");
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
