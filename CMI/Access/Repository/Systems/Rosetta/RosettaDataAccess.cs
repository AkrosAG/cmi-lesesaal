using System.Threading.Tasks;
using CMI.Access.Repository.Properties;
using System.IO;
using System.Net;
using System;
using Serilog;
using System.Text;

namespace CMI.Access.Repository.Systems.Rosetta;

public class RosettaDataAccess: IRosettaDataAccess
{
    private readonly string password = Settings.Default.RepositoryPassword;
    private readonly string serviceUrl = Settings.Default.RepositoryServiceUrl;
    private readonly string username = Settings.Default.RepositoryUser;
    private readonly string exportOption = Settings.Default.RepositoryExportOption;


    public Task<string> ExportIntellectualEntity(string entityId)
    {
        var entityData  = string.Empty;
        try
        {
            // e. g.: https://app.data-archive-test.ethz.ch/rest/v0/ies/IE7731039?op=export
            var url = serviceUrl + entityId + exportOption;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "post";
            httpWebRequest.PreAuthenticate = true;
            // Das funktioniert nicht NetworkCredential(username, password);
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            httpWebRequest.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            var responseObj = httpWebRequest.GetResponse();

            using (var stream = responseObj.GetResponseStream())
            {
                var sr = new StreamReader(stream);
                entityData = sr.ReadToEnd();
                sr.Close();
            }
        }
        catch (WebException ex)
        {
            var response = (HttpWebResponse)ex.Response;
            var responseErrorData = string.Empty;
            using (var stream = response.GetResponseStream())
            {
                var sr = new StreamReader(stream);
                responseErrorData = sr.ReadToEnd();
                sr.Close();
            }
            
            Log.Error(ex, $"Unexpected error while export Intellectual Entity {responseErrorData}");
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