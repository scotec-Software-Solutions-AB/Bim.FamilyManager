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
            var project = new ManagedProject($"BIM.FamilyManager {GetRevitVersion()}", GetDirectories())
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
                    new Property("REVIT_VERSION", GetRevitVersion())
                }
            };

            var ui = project.ManagedUI = new ManagedUI();
            ui.InstallDialogs.Add<WelcomeDialog>()
              //.Add<InstallDirDialog>()
              .Add<CustomDialogWith<InstallScopeControl>>()
              .Add<ProgressDialog>()
              .Add<ExitDialog>();

            project.UIInitialized += e =>
            {
                // Since the default MSI localization data has no entry for 'CustomDlgTitle' (and other custom labels) we
                // need to add this new content dynamically. Alternatively, you can use WiX localization files (wxl).

                var runtime = e.ManagedUI.Shell.MsiRuntime();

                runtime.UIText["CustomDlgTitle"] = "Select Installation Scope";
                runtime.UIText["CustomDlgTitleDescription"] = "Choose whether you want to install this application for all users on this computer or only for your current Windows account";

            };

            Compiler.BuildMsi(project);
        }

        private static Dir GetDirectories()
        {
            return new Dir(new Id("INSTALLDIR"), @"%ProgramFiles%\Bim.FamilyManager",
                // Special file in the root of INSTALLDIR
                new File(@"..\..\..\..\Publish\Bim.FamilyManager.addin"),
                // Subfolder for all other files
                new Dir("Bim.FamilyManager",
                    new Files(@"..\..\..\..\Publish\Bim.FamilyManager\*.*")));
        }

        private static Guid GetUpgradeCode()
        {
#if REVIT2026
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
