using System;
using System.CodeDom;
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
            var project = new ManagedProject("Bim.FamilyManager",
                // Root install directory, can be set at runtime
                //new Dir(new Id("INSTALLDIR"), @"%ProgramFiles%\Bim.FamilyManager",
                //    // Special file in the root of INSTALLDIR
                //    new File(@"..\..\..\..\Source\Bim.FamilyManager\Bim.FamilyManager.addin"),
                //    // Subfolder for all other files
                //    new Dir("Bim.FamilyManager",
                //        new Files(@"..\..\..\..\Source\Bim.FamilyManager\bin\x64\Debug\net8.0-windows\*.*")
                //    )
                new Dir(new Id("INSTALLDIR"), @"%ProgramFiles%\Bim.FamilyManager",
                    // Special file in the root of INSTALLDIR
                    new File(@"..\..\..\..\Publish\Bim.FamilyManager.addin"),
                    // Subfolder for all other files
                    new Dir("Bim.FamilyManager",
                        new Files(@"..\..\..\..\Publish\Bim.FamilyManager\*.*")))
            );
#if REVIT2026
            project.UpgradeCode = new Guid("40FC4669-353A-4610-8F95-505FC8EFFBD2");
#else
            project.UpgradeCode = new Guid("6B4E2EEC-E9E3-4AC1-9A1F-83F605B543BE");
#endif
            //project.GUID = new Guid("91E63B96-01C1-481E-9334-50EF5C48DEDF");
            project.GUID = Guid.NewGuid();

            project.Platform = Platform.x64;

            var revitVersion = Environment.GetEnvironmentVariable("RevitVersion") ?? "2025"; // fallback if not set
            var semver = Environment.GetEnvironmentVariable("PkgVersion") ?? "2025.0.0"; // fallback if not set
            var version = semver.Split('-', '+')[0];
            
            if (!Version.TryParse(version, out var parsedVersion))
            {
                throw new InvalidOperationException("Invalid version format in PkgVersion environment variable. Expected format: Major.Minor.Build (e.g., 2025.0.0)");
            }
            
            project.Version = parsedVersion;
            project.Properties = new[]
            {
                new Property("REVIT_VERSION", revitVersion)
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
    }
}
