using CMI.Access.Repository.Systems.Dir;
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
    private readonly string password;
    private readonly string serviceUrl;
    private readonly string username;
    private readonly string exportOption;


    public RosettaDataAccess()
    {
        username = Settings.Default.RepositoryUser;
        password = Settings.Default.RepositoryPassword;
        serviceUrl = Settings.Default.RepositoryServiceUrl;
        exportOption = Settings.Default.RepositoryExportOption;
    }
    

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
            using (var stream = response.GetResponseStream())
            {
                var sr = new StreamReader(stream);
                entityData = sr.ReadToEnd();
                sr.Close();
            }
            
            Log.Error(ex, $"Unexpected error while export Intellectual Entity {entityData}");
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