using System;
using System.Collections.Generic;
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

namespace Bim.FamilyManager.Installer
{
    /// <summary>
    /// Interaction logic for InstallScopeDialog.xaml
    /// </summary>
    public partial class InstallScopeDialog : UserControl, IWpfDialogContent
    {
        public InstallScopeDialog()
        {
            InitializeComponent();
        }

        public void Init(CustomDialogBase parentDialog)
        {
        }
    }
}
