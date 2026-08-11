using System.Windows;
using System.Windows.Controls;
using WixSharp;
using WixSharp.UI.WPF;

namespace Bim.FamilyManager.Installer
{
    public partial class RevitVersionSelectorControl : UserControl, IWpfDialogContent
    {
        private CustomDialogBase _parentDialog;
        private RevitVersionSelectorViewModel _viewModel;
        private TextBlock _validationHint;

        public RevitVersionSelectorControl()
        {
            InitializeComponent();
            _validationHint = (TextBlock)FindName("ValidationHint");
        }

        public void Init(CustomDialogBase parentDialog)
        {
            try
            {
                _parentDialog = parentDialog;
                _viewModel = new RevitVersionSelectorViewModel();
                DataContext = _viewModel;
                UpdateNextButton();
                parentDialog.GoNextButton.Click += OnGoNext;
                // MsiRuntime is not available during Init - defer feature state restore to Loaded.
                Loaded += OnLoaded;
            }
            catch (System.Exception ex)
            {
                WriteErrorLog(ex);
                MessageBox.Show(
                    "Failed to initialize version selector.\n\nDetails written to:\n" + ErrorLogPath(),
                    "BIM.FamilyManager Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            try
            {
                ApplyCurrentFeatureState(_parentDialog.MsiRuntime());
                UpdateNextButton();
            }
            catch
            {
                // Not critical on fresh install.
            }
        }

        private void OnGoNext(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsSelectionValid)
            {
                if (_validationHint != null) _validationHint.Visibility = Visibility.Visible;
                _parentDialog.GoNextButton.IsEnabled = false;
                return;
            }

            var runtime = _parentDialog.MsiRuntime();
            if (runtime == null)
            {
                WriteErrorLog(new System.InvalidOperationException("MsiRuntime is null in OnGoNext."));
                MessageBox.Show(
                    "Installer session is unavailable. Please restart the installer.",
                    "BIM.FamilyManager Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var addLocal = new System.Collections.Generic.List<string>();
            var remove = new System.Collections.Generic.List<string>();

            foreach (var entry in _viewModel.Versions)
            {
                var featureId = string.Format("Feature_Revit{0}", entry.Year);
                // ADDLOCAL/REMOVE are the correct MSI properties for controlling feature install state
                // from an embedded UI. FeatureInfo.RequestState uses MsiSetFeatureState which requires
                // the Selection Manager and is unavailable in embedded UI context (causes error 2731).
                if (entry.IsDetected && entry.IsSelected)
                    addLocal.Add(featureId);
                else
                    remove.Add(featureId);
            }

            // Write to runtime.Session (WixSharp ISession / MsiSessionAdapter), which calls
            // MsiSetProperty on the underlying Dtf Session — safe in embedded UI.
            runtime.Session["ADDLOCAL"] = addLocal.Count > 0 ? string.Join(",", addLocal) : string.Empty;
            runtime.Session["REMOVE"] = remove.Count > 0 ? string.Join(",", remove) : string.Empty;
        }

        private void CheckBoxOnClick(object sender, RoutedEventArgs e)
        {
            if (_validationHint != null) _validationHint.Visibility = Visibility.Collapsed;
            UpdateNextButton();
        }

        private void UpdateNextButton()
        {
            _parentDialog.GoNextButton.IsEnabled = _viewModel.IsSelectionValid;
        }

        private void ApplyCurrentFeatureState(MsiRuntime runtime)
        {
            foreach (var entry in _viewModel.Versions)
            {
                var featureId = string.Format("Feature_Revit{0}", entry.Year);
                try
                {
                    // Read current install state from session property set by ADDLOCAL/REMOVE.
                    var stateStr = runtime.Session[featureId];
                    entry.IsSelected = stateStr == "Local";
                }
                catch
                {
                    // Leave at default on fresh install.
                }
            }
        }

        private static string ErrorLogPath()
        {
            return System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                "BimFamilyManager_InstallError.txt");
        }

        private static void WriteErrorLog(System.Exception ex)
        {
            System.IO.File.WriteAllText(ErrorLogPath(),
                ex.GetType().FullName + "\r\n"
                + ex.Message + "\r\n\r\n"
                + ex.StackTrace + "\r\n\r\n"
                + (ex.InnerException != null ? ex.InnerException.ToString() : string.Empty));
        }
    }
}