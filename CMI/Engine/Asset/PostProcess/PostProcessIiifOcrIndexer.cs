using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CMI.Contract.Common;
using CMI.Engine.Asset.Solr;
using CommonServiceLocator;
using Serilog;
using SolrNet;

namespace CMI.Engine.Asset.PostProcess;

public class PostProcessIiifOcrIndexer : ProcessAnalyzerBase
{
    private readonly SolrConnectionInfo solrConnectionInfo;
    private AddParameters addParameters;
    private int counter;
    private ISolrOperations<SolrRecord> solr;

    public string RootFolder { get; set; }
    public string ArchiveRecordId { get; set; }

    public PostProcessIiifOcrIndexer(SolrConnectionInfo solrConnectionInfo)
    {
        this.solrConnectionInfo = solrConnectionInfo;
        if (!InitializeSolr())
        {
            throw new InvalidOperationException("Could not initialize connection to solr database");
        }
    }

    protected override void AnalyzeFiles(string rootOrSubFolder, List<RepositoryFile> files)
    {
        CopyOcrFiles(rootOrSubFolder);
        IndexOcrFiles(rootOrSubFolder);
    }

    private bool InitializeSolr()
    {
        Startup.InitContainer();
        Startup.Init<SolrRecord>(solrConnectionInfo.SolrUrl + solrConnectionInfo.SolrCoreName);
        solr = ServiceLocator.Current.GetInstance<ISolrOperations<SolrRecord>>();
        if (!Directory.Exists(solrConnectionInfo.SolrInstallationPath + @"/hOcr"))
        {
            Directory.CreateDirectory(solrConnectionInfo.SolrInstallationPath + @"/hOcr");
        }

        addParameters = new AddParameters
        {
            CommitWithin = 1000,
            Overwrite = true
        };
        try
        {
            // Check if Core "iiif" is already present
            solr.Ping();
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Solr connection Error.");
            return false;
        }
    }

    private void CopyOcrFiles(string rootOrSubFolder)
    {
        var directory = new DirectoryInfo(rootOrSubFolder);
        if (directory.Exists)
        {
            var files = directory.GetFiles();
            foreach (var file in files.Where(f => f.Extension.EndsWith(".hOcr", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (file.Exists)
                {
                    var hOcrDirectory = GetHOcrDestinationDirectory(rootOrSubFolder);

                    var fi = new FileInfo(hOcrDirectory.FullName + @"/" + file.Name);
                    if (fi.Exists)
                    {
                        fi.Delete();
                    }

                    File.Copy(file.FullName, fi.FullName);
                }
                else
                {
                    Log.Warning("hOcr-File does not exist: {FullName}", file.FullName);
                }
            }
        }
    }


    private void IndexOcrFiles(string tempFolder)
    {
        var directory = new DirectoryInfo(tempFolder);
        if (directory.Exists)
        {
            var files = directory.GetFiles("*.hOcr");
            foreach (var file in files)
            {
                if (file.Exists)
                {
                    var hOcrDirectory = GetHOcrDestinationDirectory(tempFolder);
                    var ocrFilePath = hOcrDirectory.FullName.Substring(solrConnectionInfo.SolrInstallationPath.Length + 1).Replace("\\", @"/");

                    counter++;
                    if (!UploadSolr(new SolrRecord
                        {
                            Id = ArchiveRecordId + "_" + counter,
                            Source = ArchiveRecordId,
                            Title = file.Name,
                            OCRText = @$"/var/solr/{ocrFilePath}/{file.Name}"
                        }))
                    {
                        Log.Warning("Solr Update not correct work: {FullName}", file.FullName);
                    }
                }
                else
                {
                    Log.Warning("File does not exists: {FullName}", file.FullName);
                }
            }
        }
    }

    private DirectoryInfo GetHOcrDestinationDirectory(string sourceDirectory)
    {
        var hOcrDirectory = new DirectoryInfo(solrConnectionInfo.SolrInstallationPath + @$"\hOcr\{ArchiveRecordId}");
        var relativePath = sourceDirectory == RootFolder ? "" : sourceDirectory.Substring(RootFolder.Length + 1);

        var destinationDirectory = new DirectoryInfo(Path.Combine(hOcrDirectory.FullName, relativePath));

        if (!destinationDirectory.Exists)
        {
            destinationDirectory.Create();
        }

        return destinationDirectory;
    }

    private bool UploadSolr(SolrRecord document)
    {
        var result = solr.Add(document, addParameters);
        if (result.Status == 0)
        {
            var commitResult = solr.Commit();
            if (commitResult.Status == 0)
            {
                return true;
            }
        }

        return false;
    }
}