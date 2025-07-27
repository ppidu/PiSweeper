using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using PiSweeper.ViewModels;

namespace PiSweeper.Dialogs;

public sealed partial class MessageBox : Window
{
    private class MessageBoxViewModel : BaseViewModel
    {
        public string? Message
        {
            get => _message;
            set => SetField(ref _message, value);
        }

        private string? _message;
    }

    public static Task ShowDialog(string? message)
    {
        var dialog = new MessageBox();
        dialog.ViewModel.Message = message;
        return dialog.ShowDialog(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime)
            .MainWindow);
    }

    public MessageBox()
    {
        InitializeComponent();
        DataContext = new MessageBoxViewModel();
    }

    private MessageBoxViewModel ViewModel => (MessageBoxViewModel)DataContext!;

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Window.Close();
    }
}