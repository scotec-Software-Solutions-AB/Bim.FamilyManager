using System.Windows;
using System.Windows.Controls;
using System.Security.Principal;
using WixSharp;
using WixSharp.UI.WPF;
using WixToolset.Dtf.WindowsInstaller;

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
                // MsiRuntime is not available during Init. In registered-product
                // maintenance, restore the authoritative MSI feature state once loaded.
                Loaded += OnLoaded;
            }
            catch (System.Exception ex)
            {
                InstallerDiagnostics.TryWriteErrorLog(ex);
                MessageBox.Show(
                    "Failed to initialize version selector.\n\nDetails written to:\n" + InstallerDiagnostics.ErrorLogPath,
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
                var runtime = _parentDialog.MsiRuntime();
                if (runtime != null && !string.IsNullOrEmpty(runtime.Session["Installed"]))
                    ApplyCurrentFeatureState(runtime);

                UpdateNextButton();
            }
            catch (System.Exception ex)
            {
                InstallerDiagnostics.TryWriteErrorLog(ex);
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
                InstallerDiagnostics.TryWriteErrorLog(new System.InvalidOperationException("MsiRuntime is null in OnGoNext."));
                MessageBox.Show(
                    "Installer session is unavailable. Please restart the installer.",
                    "BIM.FamilyManager Installer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var addLocal = new System.Collections.Generic.List<string>();
            var remove = new System.Collections.Generic.List<string>();
            var reinstall = new System.Collections.Generic.List<string>();
            var isMaintenance = !string.IsNullOrEmpty(runtime.Session["Installed"]);
            var installedProduct = isMaintenance ? GetInstalledProduct(runtime) : null;

            foreach (var entry in _viewModel.Versions)
            {
                var featureId = string.Format("Feature_Revit{0}", entry.Year);
                // ADDLOCAL/REMOVE are the correct MSI properties for controlling feature install state
                // from an embedded UI. FeatureInfo.RequestState uses MsiSetFeatureState which requires
                // the Selection Manager and is unavailable in embedded UI context (causes error 2731).
                if (entry.IsSelectable && entry.IsSelected)
                {
                    addLocal.Add(featureId);
                    // DTF cannot query a registered feature state during first-time install.
                    if (isMaintenance)
                    {
                        var currentState = installedProduct.GetFeatureState(featureId);
                        if (currentState == InstallState.Local || currentState == InstallState.Source)
                            reinstall.Add(featureId);
                    }
                }
                else
                    remove.Add(featureId);
            }

            // Write to runtime.Session (WixSharp ISession / MsiSessionAdapter), which calls
            // MsiSetProperty on the underlying Dtf Session — safe in embedded UI.
            runtime.Session["ADDLOCAL"] = addLocal.Count > 0 ? string.Join(",", addLocal) : string.Empty;
            runtime.Session["REMOVE"] = remove.Count > 0 ? string.Join(",", remove) : string.Empty;
            // A selected feature may already be installed but have missing files. Its
            // per-user component key paths are registry values, so ADDLOCAL alone does
            // not detect that damage; REINSTALL explicitly repairs retained features.
            runtime.Session["REINSTALL"] = reinstall.Count > 0 ? string.Join(",", reinstall) : string.Empty;
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
            var installedProduct = GetInstalledProduct(runtime);

            foreach (var entry in _viewModel.Versions)
            {
                var featureId = string.Format("Feature_Revit{0}", entry.Year);
                var currentState = installedProduct.GetFeatureState(featureId);
                var isInstalled = currentState == InstallState.Local || currentState == InstallState.Source;

                entry.IsSelectable = entry.IsDetected || isInstalled;
                entry.IsSelected = isInstalled;
            }
        }

        private static ProductInstallation GetInstalledProduct(MsiRuntime runtime)
        {
            var currentIdentity = WindowsIdentity.GetCurrent();
            var userSid = currentIdentity.User?.Value;
            if (string.IsNullOrEmpty(userSid))
                throw new System.InvalidOperationException("Could not determine the current Windows user SID.");

            return new ProductInstallation(
                runtime.Session["ProductCode"],
                userSid,
                UserContexts.UserUnmanaged);
        }

    }
}