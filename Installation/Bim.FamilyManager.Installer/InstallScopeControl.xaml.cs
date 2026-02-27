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
    public partial class InstallScopeControl : UserControl, IWpfDialogContent
    {
        private CustomDialogBase _parentDialog;
        
        public InstallScopeControl()
        {
            InitializeComponent();
        }

        public void Init(CustomDialogBase parentDialog)
        {
            _parentDialog = parentDialog;
            var scopeViewModel = new ScopeViewModel
            {
                IsUserScope = true
            };
            DataContext = scopeViewModel;

            parentDialog.GoNextButton.IsEnabled = true;
                
            
            parentDialog.GoNextButton.Click += (sender, e) =>
            {
                var runtime = parentDialog.MsiRuntime();
                var revitVersion = runtime.Session["REVIT_VERSION"];
                if (string.IsNullOrWhiteSpace(revitVersion))
                {
                    throw new InvalidOperationException("REVIT_VERSION MSI property is not set.");
                }

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

        private void RadioButtonOnClick(object sender, RoutedEventArgs e)
        {
            _parentDialog.GoNextButton.IsEnabled = true;
        }
    }
}
