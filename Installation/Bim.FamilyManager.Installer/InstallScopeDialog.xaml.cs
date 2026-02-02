using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using WixSharp;
using WixSharp.UI.WPF;
using WixToolset.Dtf.WindowsInstaller;

namespace Bim.FamilyManager.Installer
{
    /// <summary>
    /// Interaction logic for InstallScopeDialog.xaml
    /// </summary>
    public partial class InstallScopeDialog : UserControl, IWpfDialogContent
    {
        public InstallScopeDialog()
        {
            Debugger.Launch();
            InitializeComponent();
        }

        public void Init(CustomDialogBase parentDialog)
        {
            var scopeViewModel = new ScopeViewModel() { IsMachineScope = true };
            DataContext = scopeViewModel;


            parentDialog.GoNextButton.Click += (sender, e) =>
            {
                var revitVersion = Environment.GetEnvironmentVariable("REVIT_VERSION") ?? "2025"; // fallback if not set
                var runtime = parentDialog.MsiRuntime();
                if (scopeViewModel.IsUserScope)
                {
                    runtime.Session["MSIINSTALLPERUSER"] = "1";
                    runtime.Session["INSTALLDIR"] = Environment.ExpandEnvironmentVariables($@"%appdata%\Autodesk\Revit\Addins\{revitVersion}");
                }

                if (scopeViewModel.IsMachineScope)
                {
                    runtime.Session["MSIINSTALLPERUSER"] = "0";
                    runtime.Session["INSTALLDIR"] = Environment.ExpandEnvironmentVariables($@"%programdata%\Autodesk\Revit\Addins\{revitVersion}");
                }

            };
        }
    }
}
