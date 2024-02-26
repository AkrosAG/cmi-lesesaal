using System.Threading.Tasks;
using CMI.Access.Repository.Properties;
using System.IO;
using System.Net;
using System;
using Serilog;
using System.Text;
using System.Net.Http;

namespace CMI.Access.Repository.Systems.Rosetta;

public class RosettaDataAccess : IRosettaDataAccess
{
    private readonly string serviceUrl = Settings.Default.RepositoryServiceUrl;
    private readonly RosettaConnector rosettaConnector;

    public RosettaDataAccess(RosettaConnector connector)
    {
        rosettaConnector = connector;
    }

    public async Task<string> ExportIntellectualEntity(string entityId)
    {
        entityId = "IE444295";

        var path = await rosettaConnector.StartExportAsync(entityId);
        if(!string.IsNullOrEmpty(path))
        {
            Log.Information($"Intellectual Entity {entityId} exported successfully to {path}.");
            return path;
        }
        
        return null;
    }
}