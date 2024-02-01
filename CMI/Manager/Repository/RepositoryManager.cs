using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMI.Contract.Common;
using CMI.Contract.Parameter;
using CMI.Manager.Repository.ParameterSettings;
using CMI.Manager.Repository.Systems;
using Serilog;
using Serilog.Context;

namespace CMI.Manager.Repository;

public class RepositoryManager : IRepositoryManager
{
    private readonly IRepositoryProvider repositoryProvider;
    private readonly RepositorySyncSettings syncSettings;


    /// <summary>
    ///     Initializes a new instance of the <see cref="RepositoryManager" /> class.
    /// </summary>
    public RepositoryManager(IRepositoryProvider repositoryProvider,
        IParameterHelper parameterHelper)
    {
        this.repositoryProvider = repositoryProvider;
        syncSettings = parameterHelper.GetSetting<RepositorySyncSettings>();
    }

    #region IRepositoryManager Members

    /// <summary>
    ///     Gets the package.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="archiveRecordId">The archive record identifier.</param>
    /// <returns>RepositoryPackageResult.</returns>
    public async Task<RepositoryPackageResult> GetPackage(string packageId, string archiveRecordId, int primaerdatenAuftragId)
    {
        var startTime = DateTime.Now;

        // Getting the package including the metadata.xml 
        var packageResult = await repositoryProvider.GetPackage(packageId, archiveRecordId, true, new List<string>(), primaerdatenAuftragId);

        // Output duration
        var timespan = new TimeSpan(DateTime.Now.Ticks - startTime.Ticks);
        Log.Information("Package {packageId} with {SizeInBytes} bytes fetched in {TotalSeconds} seconds. Valid status is: {Valid}", packageId,
            packageResult.PackageDetails?.SizeInBytes, timespan.TotalSeconds, packageResult.Valid);

        return packageResult;
    }

    public async Task<RepositoryPackageResult> AppendPackageToArchiveRecord(ArchiveRecord archiveRecord, long mutationId, int primaerdatenId)
    {
        var startTime = DateTime.Now;

        var packageId = archiveRecord.Metadata.PrimaryDataLink;
        var archiveRecordId = archiveRecord.ArchiveRecordId;

        using (LogContext.PushProperty("packageId", packageId))
        {
            if (!string.IsNullOrEmpty(packageId) && !string.IsNullOrEmpty(archiveRecordId))
            {
                var fileTypesToIgnore = syncSettings.IgnorierteDateitypenFuerSynchronisierung.Split(',');
                // Getting the package, but for syncing we don't need the overhead of creating the metadata stuff
                var packageResult = await repositoryProvider.GetPackage(packageId, archiveRecordId, false,
                    fileTypesToIgnore.Select(f => f.Trim()).ToList(),
                    primaerdatenId);

                // Output duration
                var timespan = new TimeSpan(DateTime.Now.Ticks - startTime.Ticks);
                Log.Information("Package {packageId} with {SizeInBytes} bytes fetched in {TotalSeconds} seconds. Valid status is: {Valid}",
                    packageId,
                    packageResult.PackageDetails.SizeInBytes, timespan.TotalSeconds, packageResult.Valid);

                if (packageResult.Success && packageResult.Valid)
                {
                    // Append the package to the archive record
                    archiveRecord.PrimaryData.Add(packageResult.PackageDetails);
                    return packageResult;
                }

                Log.Warning(
                    "Package {packageId} for Archiverecord {archiveRecordId} not appended, because package could not be created or was invalid. ({ErrorMessage})",
                    packageId, archiveRecordId,
                    packageResult.ErrorMessage);

                packageResult.ErrorMessage +=
                    $"{(!string.IsNullOrEmpty(packageResult.ErrorMessage) ? Environment.NewLine : string.Empty)}Package successfull status: {packageResult.Success}. Package valid status: {packageResult.Valid}";
                return packageResult;
            }

            return new RepositoryPackageResult {ErrorMessage = "Invalid arguments for appending package"};
        }
    }

    public RepositoryPackageInfoResult ReadPackageMetadata(string packageId, string archiveRecordId)
    {
        // Init the return value
        var retVal = new RepositoryPackageInfoResult
        {
            Success = false,
            Valid = false,
            PackageDetails = new RepositoryPackage {ArchiveRecordId = archiveRecordId}
        };

        try
        {
            return repositoryProvider.ReadPackageMetadata(packageId, archiveRecordId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get package metadata with id {packageId} from repository", packageId);
            retVal.ErrorMessage = "Failed to get package metadata from repository";
            throw;
        }
    }

    #endregion
}