using System.Windows;
using Scotec.Wpf.ViewModels;

namespace Bim.FamilyManager.Ui.Views.Settings;

/// <summary>
///     Represents a dialog control that is specialized for a specific type of window.
/// </summary>
/// <remarks>
///     This class inherits from <see cref="Dialog{TWindow}" /> and provides additional functionality
///     or customization for managing dialog windows. It is designed to simplify the creation and handling
///     of dialogs in WPF applications.
/// </remarks>
public class Dialog<TWindow> : FrameworkElement where TWindow : Window, new()
{
    private static readonly DependencyProperty IsDialogVisibleProperty =
        DependencyProperty.Register(nameof(IsDialogVisible), typeof(bool), typeof(Dialog<TWindow>),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDialogVisibleChanged));

    private static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(ViewModel), typeof(Dialog<TWindow>),
            new FrameworkPropertyMetadata(null));

    private TWindow? _dialog;

    /// <summary>
    ///     Gets or sets a value indicating whether the dialog is currently visible.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the dialog is visible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     When set to <c>true</c>, the dialog window is displayed by invoking <see cref="ShowWindow" />.
    ///     When set to <c>false</c>, the dialog window is closed by invoking <see cref="CloseWindow" />.
    ///     This property supports two-way data binding.
    /// </remarks>
    public bool IsDialogVisible
    {
        get => (bool)GetValue(IsDialogVisibleProperty);
        set => SetValue(IsDialogVisibleProperty, value);
    }

    /// <summary>
    ///     Gets or sets the content associated with the dialog.
    /// </summary>
    /// <remarks>
    ///     The content is represented by a <see cref="ViewModel" /> instance, which provides the data context
    ///     and template selector for the dialog. This property is used to bind the dialog's data context
    ///     and customize its appearance through the associated data templates.
    /// </remarks>
    public ViewModel? Content
    {
        get => (ViewModel?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    ///     Called when the value of the <see cref="IsDialogVisible" /> dependency property changes.
    /// </summary>
    /// <param name="dependencyObject">
    ///     The <see cref="DependencyObject" /> on which the property value has changed.
    ///     This is expected to be an instance of <see cref="Dialog{TWindow}" />.
    /// </param>
    /// <param name="args">
    ///     The event data containing information about the property change,
    ///     including the old and new values.
    /// </param>
    /// <remarks>
    ///     This method is triggered automatically whenever the <see cref="IsDialogVisible" /> property changes.
    ///     If the new value is <c>true</c>, it initiates the display of the dialog window by invoking
    ///     <see cref="ShowWindow" />.
    ///     If the new value is <c>false</c>, it closes the dialog window by invoking <see cref="CloseWindow" />.
    /// </remarks>
    private static void OnIsDialogVisibleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        var control = (Dialog<TWindow>)dependencyObject;
        var newValue = (bool)args.NewValue;

        if (newValue)
        {
            control.Dispatcher.BeginInvoke(() =>
            {
                control.ShowWindow();
                control.SetCurrentValue(IsDialogVisibleProperty, false);
            });
        }
        else
        {
            control.Dispatcher.BeginInvoke(() => { control.CloseWindow(); });
        }
    }

    /// <summary>
    ///     Closes the currently displayed dialog window, if any, and releases its resources.
    /// </summary>
    /// <remarks>
    ///     This method is responsible for ensuring that the dialog window is properly closed and its resources are released.
    ///     It is typically invoked when the <see cref="IsDialogVisible" /> property changes to <c>false</c>.
    ///     After closing the dialog, the associated instance is set to <c>null</c>.
    /// </remarks>
    private void CloseWindow()
    {
        _dialog?.Close();
        _dialog = null;
    }

    /// <summary>
    ///     Displays the dialog window associated with this <see cref="Dialog{TWindow}" />.
    /// </summary>
    /// <remarks>
    ///     This method initializes a new instance of the dialog window of type <typeparamref name="TWindow" />.
    ///     The dialog's owner is set to the window containing this control, and its data context is set to the value of the
    ///     <see cref="Content" /> property. Additionally, the <see cref="ViewModel.DataTemplateSelector" /> is applied to the
    ///     dialog.
    ///     The dialog is displayed modally using <see cref="Window.ShowDialog" />.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the dialog window cannot be created or displayed.
    /// </exception>
    private void ShowWindow()
    {
        _dialog = new TWindow
        {
            Owner = Window.GetWindow(this),
            DataContext = Content,
            ContentTemplateSelector = Content?.DataTemplateSelector
        };

        _dialog.ShowDialog();
    }
}
