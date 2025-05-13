using CMI.Contract.Asset;
using CMI.Contract.Common;
using Serilog;
using System;
using System.Threading.Tasks;
using CMI.Utilities.License;

namespace CMI.Engine.Asset.PreProcess
{
    public class AssetPreparationEngine : IAssetPreparationEngine
    {
        private readonly PreProcessAnalyzerDetectAndFlagLargeDimensions analyzerDetectAndFlagLargeDimensions;
        private readonly PreProcessAnalyzerOptimizePdf analyzerOptimizePdf;

        public AssetPreparationEngine(PreProcessAnalyzerDetectAndFlagLargeDimensions analyzerDetectAndFlagLargeDimensions,
            PreProcessAnalyzerOptimizePdf analyzerOptimizePdf)
        {
            this.analyzerDetectAndFlagLargeDimensions = analyzerDetectAndFlagLargeDimensions;
            this.analyzerOptimizePdf = analyzerOptimizePdf;

            LicenseHelper.SetAsposeLicense();          

        }

        public Task<ProcessStepResult> DetectAndFlagLargeDimensions(RepositoryPackage package, string tempFolder, int primaerdatenAuftragId)
        {
            try
            {
                Log.Information("Starting to detect large dimensions in documents for primaerdatenAuftrag with id {PrimaerdatenAuftragId}",
                    primaerdatenAuftragId);
                analyzerDetectAndFlagLargeDimensions.AnalyzeRepositoryPackage(package, tempFolder);
                return Task.FromResult(new ProcessStepResult { Success = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex,
                    "Unexpected error while detecting large dimensions in documents for primaerdatenAuftrag with id {PrimaerdatenAuftragId}",
                    primaerdatenAuftragId);
                return Task.FromResult(new ProcessStepResult
                    { Success = false, ErrorMessage = "Unexpected error while detecting large dimensions in documents." });
            }
        }

        public Task<ProcessStepResult> OptimizePdfIfRequired(RepositoryPackage package, string tempFolder, int primaerdatenAuftragId)
        {
            try
            {
                Log.Information("Starting to detect and optimize PDF for primaerdatenAuftrag with id {PrimaerdatenAuftragId}", primaerdatenAuftragId);
                analyzerOptimizePdf.AnalyzeRepositoryPackage(package, tempFolder);
                return Task.FromResult(new ProcessStepResult() { Success = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while detect and optimize PDF for primaerdatenAuftrag with id {PrimaerdatenAuftragId}",
                    primaerdatenAuftragId);
                return Task.FromResult(new ProcessStepResult()
                    { Success = false, ErrorMessage = "Unexpected error while detect and optimize PDF." });
            }
        }
    }
}
