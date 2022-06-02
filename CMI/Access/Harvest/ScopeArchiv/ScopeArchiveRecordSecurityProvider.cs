using System;
using System.Linq;
using System.Threading.Tasks;

using CMI.Contract.Common;
using Serilog;

namespace CMI.Access.Harvest.CMIAIS
{
    public class ScopeArchiveRecordSecurityProvider : IArchiveRecordSecurityProvider
    {
        private readonly IAISDataProvider dataProvider;
        public ScopeArchiveRecordSecurityProvider(IAISDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <summary>
        ///     Loads the security details.
        /// </summary>
        /// <param name="recordId">The archive record identifier.</param>
        /// <returns>ArchiveRecordSecurity.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<ArchiveRecordSecurity> GetArchiveRecordSecurity(string archiveRecordId)
        {
            try
            {
                var tMetadataSecurityTokens = this.dataProvider.LoadMetadataSecurityTokens(archiveRecordId);
                var tPrimaryDataSecurityTokens = this.dataProvider.LoadPrimaryDataSecurityTokens(archiveRecordId);

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
                Log.Error(ex, "Failed to load the security information for record {RecordId}", archiveRecordId);
                throw;
            }
        }
    }
}
