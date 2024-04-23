using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CMI.Contract.DocumentConverter;
using CMI.Manager.DocumentConverter.Abbyy;
using CMI.Manager.DocumentConverter.Extraction.Interfaces;
using CMI.Manager.DocumentConverter.Properties;
using Serilog;

namespace CMI.Manager.DocumentConverter.Extraction
{
    public class AbbyyTextExtractor : TextExtractorBase, INeedsAbbyyInstallation
    {
        private static readonly string[] extensions = { "tif", "tiff", "pdf", "jp2" };
        private readonly IAbbyyWorker abbyyWorker;

        private bool abbyPathExistsInSettings;
        private string pathToAbbyInstallation;

        public override IEnumerable<string> AllowedExtensions => extensions;

        public override int Rank => 1;

        public override bool IsAvailable => true;

        public string PathToAbbyyFrEngineDll
        {
            get => pathToAbbyInstallation;
            set
            {
                pathToAbbyInstallation = value;
                Log.Verbose($"Path to Abbyy FrEngine.dll has been set to '{pathToAbbyInstallation}'");
                abbyPathExistsInSettings = CheckForAbbyyInstallation();
            }
        }

        public bool PathToAbbyFrEngineDllHasBeenSet { get; set; }
        public string MissingAbbyyPathInstallationMessage { get; set; }
        public bool MissingAbbyyPathInstallationMessageHasBeenSet { get; set; }


        public AbbyyTextExtractor(IAbbyyWorker abbyyWorker)
        {
            this.abbyyWorker = abbyyWorker;
        }


        public override ExtractionResult ExtractText(IDoc doc, ITextExtractorSettings settings)
        {
            // Die ETH hat keine Abbyy Lizenz Stand 2024
          
            if (!abbyPathExistsInSettings)
            {
                var result = new ExtractionResult(settings.MaxExtractionSize)
                {
                    HasError = true, 
                    ErrorMessage = $"{MissingAbbyyPathInstallationMessage} - {doc.FileName}"
                };
                return result;
            }

            try
            {
                var fs = doc.Stream as FileStream;
                var result = abbyyWorker.ExtractTextFromDocument(fs?.Name, settings);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Abbyy text extraction failed unexpectedly: {ex.Message}");
                throw;
            }
        }



        private bool CheckForAbbyyInstallation()
        {
            if (string.IsNullOrWhiteSpace(PathToAbbyyFrEngineDll))
            {
                var exception = new ArgumentException("Path to FrEngine.dll not set");
                Log.Warning(exception, exception.Message);
                return false;
            }

            if (!File.Exists(PathToAbbyyFrEngineDll))
            {
                throw new FileNotFoundException($"Path/file '{PathToAbbyyFrEngineDll}' does not exist");
            }

            return true; // es ist noch keine Prüfung auf die Lizenz
        }
    }
}