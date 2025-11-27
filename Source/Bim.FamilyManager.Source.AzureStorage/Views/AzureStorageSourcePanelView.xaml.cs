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

namespace Bim.FamilyManager.Source.AzureStorage.Views
{
    /// <summary>
    /// Represents the view for the Azure Storage source panel in the application.
    /// </summary>
    /// <remarks>
    /// This class is a partial class that inherits from <see cref="UserControl"/> and is used to define the user interface
    /// for interacting with Azure Storage as a family source. It is associated with the corresponding XAML file for layout
    /// and design.
    /// </remarks>
    public partial class AzureStorageSourcePanelView : UserControl
    {
        public AzureStorageSourcePanelView()
        {
            InitializeComponent();
        }
    }
}
