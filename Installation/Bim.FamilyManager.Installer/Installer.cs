using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WixSharp;
using ExitDialog = WixSharp.UI.WPF.ExitDialog;
using InstallDirDialog = WixSharp.UI.WPF.InstallDirDialog;
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
                new Dir(new Id("INSTALLDIR"), @"%ProgramFiles%\Bim.FamilyManager",
                    // Special file in the root of INSTALLDIR
                    new File(@"..\..\..\..\Source\Bim.FamilyManager\Bim.FamilyManager.addin"),
                    // Subfolder for all other files
                    new Dir("Bim.FamilyManager",
                        new Files(@"..\..\..\..\Source\Bim.FamilyManager\bin\x64\Debug\net8.0-windows\*.*")
                    )
                )
            );
            project.UpgradeCode = new Guid("6B4E2EEC-E9E3-4AC1-9A1F-83F605B543BE");
            //project.GUID = new Guid("91E63B96-01C1-481E-9334-50EF5C48DEDF");
            project.GUID =Guid.NewGuid();
 
            project.Platform = Platform.x64;

            //TODO: Set version dynamically
            project.Version = new Version("1.0.0");

            var ui = project.ManagedUI = new ManagedUI();
            ui.InstallDialogs.Add<WelcomeDialog>()
                //.Add<InstallDirDialog>()
                .Add<CustomDialogWith<InstallScopeDialog>>()
                .Add<ProgressDialog>()
                .Add<ExitDialog>();



            project.UIInitialized += e =>
            {
                // Since the default MSI localization data has no entry for 'CustomDlgTitle' (and other custom labels) we
                // need to add this new content dynamically. Alternatively, you can use WiX localization files (wxl).

                MsiRuntime runtime = e.ManagedUI.Shell.MsiRuntime();

                runtime.UIText["CustomDlgTitle"] = "Select Installation Scope";
                runtime.UIText["CustomDlgTitleDescription"] = "Please select the installation scope.";
            };


            Compiler.BuildMsi(project);
        }
    }
}