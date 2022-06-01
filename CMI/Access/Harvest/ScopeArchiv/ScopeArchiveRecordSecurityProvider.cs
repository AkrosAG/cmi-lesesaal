using System;
using System.Threading.Tasks;

using CMI.Contract.Common;

namespace CMI.Access.Harvest.CMIAIS
{
    public class ScopeArchiveRecordSecurityProvider : IArchiveRecordSecurityProvider
    {
        private readonly IAISDataProvider _provider;
        public ScopeArchiveRecordSecurityProvider(IAISDataProvider dataProvider)
        {
            _provider = dataProvider;
        }

        /// <summary>
        ///     Loads the security details.
        /// </summary>
        /// <param name="recordId">The archive record identifier.</param>
        /// <returns>ArchiveRecordSecurity.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<ArchiveRecordSecurity> GetArchiveRecordSecurity(int archiveRecordId)
        {
            try
            {
                var tMetadataSecurityTokens = _dataProvider.LoadMetadataSecurityTokens(recordId);
                var tPrimaryDataSecurityTokens = _dataProvider.LoadPrimaryDataSecurityTokens(recordId);

                await Task.WhenAll(tMetadataSecurityTokens, tPrimaryDataSecurityTokens);
                return new ArchiveRecordSecurity
                {
                    MetadataAccessToken = tMetadataSecurityTokens.Result,
                    PrimaryDataDownloadAccessToken = tPrimaryDataSecurityTokens.Result.DownloadAccessTokens.Any() ? tPrimaryDataSecurityTokens.Result.DownloadAccessTokens : null,
                    PrimaryDataFulltextAccessToken = tPrimaryDataSecurityTokens.Result.FulltextAccessTokens.Any() ? tPrimaryDataSecurityTokens.Result.FulltextAccessTokens : null,
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load the security information for record {RecordId}", recordId);
                throw;
            }
        }
    }
}
