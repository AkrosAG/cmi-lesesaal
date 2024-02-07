using System.Threading.Tasks;
using CMI.Access.Repository.Properties;
using System.IO;
using System.Net;
using System;
using Serilog;
using System.Text;

namespace CMI.Access.Repository.Systems.Rosetta;

public class RosettaDataAccess : IRosettaDataAccess
{
    private readonly string password = Settings.Default.RepositoryPassword;
    private readonly string serviceUrl = Settings.Default.RepositoryServiceUrl;
    private readonly string username = Settings.Default.RepositoryUser;


    public Task<string> ExportIntellectualEntity(string entityId)
    {
        var entityData = string.Empty;
        try
        {
            // e. g.: https://app.data-archive-test.ethz.ch/rest/v0/ies/IE7731039?op=export

            // Call a web rest service to export the intellectual entity
            // Use post method to call the service and use basic authentication
            var url = string.Format(serviceUrl, entityId);
            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";
            request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{username}:{password}")));
            
            var response = request.GetResponse();
            using var stream = response.GetResponseStream();
            if (stream != null)
            {
                var sr = new StreamReader(stream);
                entityData = sr.ReadToEnd();
                sr.Close();
            }

            Log.Information($"Intellectual Entity {entityId} exported successfully.");

        }
        catch (WebException ex)
        {
            var response = (HttpWebResponse)ex.Response;
            var responseErrorData = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    var sr = new StreamReader(stream);
                    responseErrorData = sr.ReadToEnd();
                    sr.Close();
                }
            }

            Log.Error(ex, $"Known errors when exporting Intellectual Entity {responseErrorData}");
            throw;
        }
        catch (Exception ex)
        {

            Log.Error(ex, "Unexpected error while export Intellectual Entity.");
            throw;
        }

        return Task.FromResult(entityData);
    }
}