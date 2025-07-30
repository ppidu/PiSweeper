using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PiSweeper.Dialogs;

public partial class CustomGameSettingsDialog : Window
{
    public CustomGameSettingsDialog()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Window.Close();
    }
}