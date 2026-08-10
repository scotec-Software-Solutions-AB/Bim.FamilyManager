using System;
using WixSharp;
using ExitDialog = WixSharp.UI.WPF.ExitDialog;
using ProgressDialog = WixSharp.UI.WPF.ProgressDialog;
using WelcomeDialog = WixSharp.UI.WPF.WelcomeDialog;

namespace Bim.FamilyManager.Installer
{
    public class Script
    {
        public static void Main(string[] args)
        {
            var revitVersion = GetRevitVersion();

            var project = new ManagedProject($"BIM.FamilyManager {revitVersion}", GetDirectories(revitVersion))
            {
                GUID = Guid.NewGuid(),
                Platform = Platform.x64,
                UpgradeCode = GetUpgradeCode(),
                Version = GetProductVersion(),
                ControlPanelInfo = GetProductInfo(),
                MajorUpgrade = MajorUpgrade.Default,
                BackgroundImage = @"Resources\Icons\BackgroundImage.png",
                BannerImage = @"Resources\Icons\BannerImage.png",

                Properties = new[]
                {
                    new Property("REVIT_VERSION", revitVersion),
                    // Always install per-user. Revit 2027+ does not support ProgramData add-in discovery.
                    new Property("MSIINSTALLPERUSER", "1"),
                }
            };

            var ui = project.ManagedUI = new ManagedUI();
            ui.InstallDialogs.Add<WelcomeDialog>()
              .Add<ProgressDialog>()
              .Add<ExitDialog>();

            Compiler.BuildMsi(project);
        }

        private static Dir GetDirectories(string revitVersion)
        {
            // [AppDataFolder] resolves to %APPDATA%\ for per-user installs (MSIINSTALLPERUSER=1).
            // The .addin file sits directly in the Revit Addins folder; all binaries go in the subfolder
            // referenced by the <Assembly> path inside the .addin manifest.
            var revitAddinsPath = $@"[AppDataFolder]Autodesk\Revit\Addins\{revitVersion}";

            return new Dir(new Id("INSTALLDIR"), revitAddinsPath,
                // .addin manifest sits in the root of the Revit Addins folder
                new File(@"..\..\..\..\Publish\Bim.FamilyManager.addin"),
                // Binaries go in the subfolder matching the <Assembly> path in the .addin manifest
                new Dir("Bim.FamilyManager",
                    new Files(@"..\..\..\..\Publish\Bim.FamilyManager\*.*")));
        }

        private static Guid GetUpgradeCode()
        {
#if REVIT2027
            return new Guid("{72DC508A-1091-40F8-9632-DCE3F0C8F64A}");
#elif REVIT2026
            return new Guid("40FC4669-353A-4610-8F95-505FC8EFFBD2");
#else
            return new Guid("6B4E2EEC-E9E3-4AC1-9A1F-83F605B543BE");
#endif
        }

        private static ProductInfo GetProductInfo()
        {
            return new ProductInfo()
            {
                Comments = "Revit add-in for managing and standardizing Revit families.",
                HelpLink = "https://github.com/scotec-Software-Solutions-AB/Bim.FamilyManager/issues",
                UrlInfoAbout = "https://www.scotec.com/bimfamilymanager",
                UrlUpdateInfo = "https://github.com/scotec-Software-Solutions-AB/Bim.FamilyManager/releases",
                InstallLocation = "[INSTALLDIR]", 
                Manufacturer = "scotec",
                ProductIcon = @"Resources\Icons\Logo.ico"
            };
        }

        private static Version GetProductVersion()
        {
            var semver = Environment.GetEnvironmentVariable("PkgVersion") ?? "0.1.0-local"; // fallback if not set
            var version = semver.Split('-', '+')[0];
            
            if (!Version.TryParse(version, out var parsedVersion))
            {
                throw new InvalidOperationException("Invalid version format in PkgVersion environment variable. Expected format: Major.Minor.Build (e.g., 2025.0.0)");
            }

            // When using versions like 2025.0.0, Wix generates the following warning while building the .msi file:
            // WIX1148: Invalid MSI package version: '2025.0.0'. The Windows Installer SDK says that MSI package versions must have a
            //    major version less than 256, a minor version less than 256, and a build version less than 65536. The revision value is
            //    ignored but version labels and metadata are not allowed. Violating the MSI rules sometimes works as expected but
            //    the behavior is unpredictable and undefined. Future versions of WiX might treat invalid package versions as an error.
            // To avoid this warning, we use a version format that is compatible with MSI, such as 25.0.0.
            
            return parsedVersion.Major > 256 ? new Version(parsedVersion.Major % 100, parsedVersion.Minor, parsedVersion.Build) : parsedVersion;
        }

        private static string GetRevitVersion()
        {
            var revitVersion = Environment.GetEnvironmentVariable("RevitVersion") ?? "2025"; // fallback if not set
            return revitVersion;
        }
    }
}
