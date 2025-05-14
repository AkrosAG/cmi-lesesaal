using System;
using System.IO;
using System.Text;
using CMI.Utilities.License.Properties;
using Serilog;

namespace CMI.Utilities.License
{
    public static class LicenseHelper
    {
        private static bool isLicenseSet = false; // Prevents redundant license setting

        public static void SetAsposeLicense()
        {
            if (isLicenseSet)
                return;

            try
            {
                var licenseContent = Settings.Default.AsposeLicense;

                if (string.IsNullOrEmpty(licenseContent) || licenseContent.Contains("@@"))
                {
                    Log.Warning("Aspose license is missing or contains a placeholder. Running in evaluation mode.");
                    return; // Run without a license (Aspose will operate in trial mode)
                }

                using (var licenseStream = new MemoryStream(Encoding.UTF8.GetBytes(licenseContent)))
                {
                    var license = new Aspose.Pdf.License();
                    license.SetLicense(licenseStream);
                    isLicenseSet = true;
                    Log.Information("Aspose license successfully applied.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while setting Aspose license.");
            }
        }
    }
}
