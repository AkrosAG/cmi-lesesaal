using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CMI.Access.Repository.Properties;
using Serilog;

namespace CMI.Access.Repository.Systems.Rosetta
{
    public class RosettaConnector
    {
        private readonly string password = Settings.Default.RepositoryPassword;
        private readonly string username = Settings.Default.RepositoryUser;

        public async Task<string> InitExport(string entityId)
        {
            // e. g.: https://app.data-archive-test.ethz.ch/rest/v0/ies/IE7731039?op=export

            var url = $"{Settings.Default.RepositoryServiceUrl}{entityId}?op=export";
            var xmlString = await PostAsync(url, new StringContent(string.Empty, Encoding.UTF8, "application/xml"));

            var processUrl = GetProcessUrl(xmlString);
            if (string.IsNullOrEmpty(processUrl))
            {
                Log.Error("Failed to get process URL for entity {entityId}", entityId);
                return null;
            }

            return await WaitUntilReady(processUrl);    
        }

        public async Task<string> WaitUntilReady(string path)
        {
            return await Task.FromResult("");
        }

        public async Task<string> PostAsync(string url, HttpContent content)
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

        private string GetProcessUrl(string xml)
        {
            var xmlDoc = XDocument.Parse(xml);
            var node = xmlDoc.Descendants("info")
                        .FirstOrDefault(e => e.Attribute("desc")?.Value == "process_instance_id_link");

            return node?.Value;
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
