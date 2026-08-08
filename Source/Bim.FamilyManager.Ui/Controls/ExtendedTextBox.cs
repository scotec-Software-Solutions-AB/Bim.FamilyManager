using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Bim.FamilyManager.Ui.Controls;

public class ExtendedTextBox : TextBox
{
    // Dependency property for Watermark
    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(ExtendedTextBox),
            new PropertyMetadata(string.Empty));

    // Dependency property for ClearCommand
    public static readonly DependencyProperty ClearCommandProperty =
        DependencyProperty.Register(
            nameof(ClearCommand),
            typeof(ICommand),
            typeof(ExtendedTextBox),
            new PropertyMetadata(null));

    // Dependency property for ButtonImageSource
    public static readonly DependencyProperty ButtonImageSourceProperty =
        DependencyProperty.Register(
            nameof(ButtonImageSource),
            typeof(ImageSource),
            typeof(ExtendedTextBox),
            new PropertyMetadata(null));

    // Dependency property for Button tooltip
    public static readonly DependencyProperty ButtonTooltipProperty =
        DependencyProperty.Register(
            nameof(ButtonTooltip),
            typeof(string),
            typeof(ExtendedTextBox),
            new PropertyMetadata(string.Empty));

    // Dependency property for ButtonImageSource
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(ExtendedTextBox),
            new PropertyMetadata(null));

    // Dependency property for Button tooltip
    public static readonly DependencyProperty ImageTooltipProperty =
        DependencyProperty.Register(
            nameof(ImageTooltip),
            typeof(string),
            typeof(ExtendedTextBox),
            new PropertyMetadata(string.Empty));

    // Dependency property for WatermarkForeground
    public static readonly DependencyProperty WatermarkForegroundProperty =
        DependencyProperty.Register(
            nameof(WatermarkForeground),
            typeof(Brush),
            typeof(ExtendedTextBox),
            new PropertyMetadata(Brushes.Gray));

    static ExtendedTextBox()
    {
        // Automatically apply the default style
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExtendedTextBox),
            new FrameworkPropertyMetadata(typeof(ExtendedTextBox)));
    }

    public ExtendedTextBox()
    {
        // Default ClearCommand implementation
        ClearCommand = new RelayCommand(_ => Clear(), _ => !string.IsNullOrEmpty(Text));
    }

    public ImageSource ButtonImageSource
    {
        get => (ImageSource)GetValue(ButtonImageSourceProperty);
        set => SetValue(ButtonImageSourceProperty, value);
    }

    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public Brush WatermarkForeground
    {
        get => (Brush)GetValue(WatermarkForegroundProperty);
        set => SetValue(WatermarkForegroundProperty, value);
    }

    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public string ButtonTooltip
    {
        get => (string)GetValue(ButtonTooltipProperty);
        set => SetValue(ButtonTooltipProperty, value);
    }

    public string ImageTooltip
    {
        get => (string)GetValue(ImageTooltipProperty);
        set => SetValue(ImageTooltipProperty, value);
    }

    public ICommand ClearCommand
    {
        get => (ICommand)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    private class RelayCommand : ICommand
    {
        private readonly Predicate<object?>? _canExecute;
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
