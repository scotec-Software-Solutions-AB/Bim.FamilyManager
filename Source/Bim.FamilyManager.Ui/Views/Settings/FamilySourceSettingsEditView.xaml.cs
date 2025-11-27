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

namespace Bim.FamilyManager.Ui.Views.Settings
{
    /// <summary>
    /// Represents the WPF user control for editing a family source in the settings UI.
    /// </summary>
    /// <remarks>
    /// This control provides the interaction logic for FamilySourceSettingsEditView.xaml, allowing users to modify family source settings.
    /// </remarks>
    public partial class FamilySourceSettingsEditView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FamilySourceSettingsEditView"/> class.
        /// </summary>
        /// <remarks>
        /// Calls <see cref="InitializeComponent"/> to set up the control and its bindings.
        /// </remarks>
        public FamilySourceSettingsEditView()
        {
            InitializeComponent();
        }
    }
}
