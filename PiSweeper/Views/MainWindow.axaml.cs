using Avalonia.Controls;
using PiSweeper.ViewModels;

namespace PiSweeper.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;
}