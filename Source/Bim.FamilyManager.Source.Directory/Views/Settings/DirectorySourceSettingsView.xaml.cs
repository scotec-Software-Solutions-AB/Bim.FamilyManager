using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Bim.FamilyManager.Source.Directory.Views.Settings;

/// <summary>
///     Represents the view for configuring directory source settings in the application.
/// </summary>
/// <remarks>
///     This class is a WPF UserControl that provides the UI for managing directory source settings.
///     It is associated with the <see cref="FamilyManager.ViewModels.DirectorySourceSettingsViewModel" />
///     through the MVVM pattern.
/// </remarks>
public partial class DirectorySourceSettingsView : UserControl
{
    public DirectorySourceSettingsView()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Handles the folder selection process triggered by a user interaction.
    /// </summary>
    /// <param name="sender">The source of the event, typically the control that was clicked.</param>
    /// <param name="e">The event data associated with the folder selection action.</param>
    /// <remarks>
    ///     This method opens a folder browser dialog to allow the user to select a directory.
    ///     The selected directory path is then displayed in the associated text box.
    /// </remarks>
    private void OnSelectFolder(object sender, RoutedEventArgs e)
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Select family directory",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (folderDialog.ShowDialog() == true)
        {
            FolderTextBox.Text = folderDialog.FolderName;
        }
    }
}
