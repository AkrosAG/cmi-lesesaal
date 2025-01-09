using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using CMI.Contract.Asset;
using CMI.Contract.Common;
using Serilog;

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

            try
            {
                // Retrieve the license content from application settings
                string licenseContent = Properties.Settings.Default.AsposeLicense;
                var licensePdf = new License();
                if (string.IsNullOrWhiteSpace(licenseContent) || licenseContent.Contains("@@"))
                {
                    throw new Exception("License content is missing or placeholder is still present in application settings.");
                }

                // Convert the license content to a stream
                using (var licenseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(licenseContent)))
                {
                   licensePdf.SetLicense(licenseStream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while setting Aspose license.");
                throw;
            }

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
