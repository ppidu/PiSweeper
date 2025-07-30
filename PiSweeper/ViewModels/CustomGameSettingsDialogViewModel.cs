namespace PiSweeper.ViewModels;

public sealed class CustomGameSettingsDialogViewModel : BaseViewModel
{
    public int Rows
    {
        get => _rows;
        set => SetField(ref _rows, value);
    }
    private int _rows = 9;
    
    public int Columns
    {
        get => _columns;
        set => SetField(ref _columns, value);
    }
    private int _columns = 12;
    
    public int Mines
    {
        get => _mines;
        set => SetField(ref _mines, value);
    }
    private int _mines = 50;
}