using System;
using Avalonia.Controls;
using Avalonia.Input;
using PiSweeper.ViewModels;

namespace PiSweeper.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Button { DataContext: CellViewModel cellViewModel } cell) return;
        var point = e.GetCurrentPoint(cell);
        if (point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            ViewModel.ToggleFlagCommand.Execute(cellViewModel);
        }
    }
}