namespace PiSweeper.ViewModels;

public sealed class CellViewModel(int x, int y, int value) : BaseViewModel
{
    public int X => x;

    public int Y => y;
    
    public int Value => value;

    public bool IsBomb => value == -1;

    public bool IsRevealed 
    {
        get => _isRevealed;
        private set => SetField(ref _isRevealed, value);
    }
    private bool _isRevealed;

    public bool IsFlagged 
    {
        get => _isFlagged;
        private set => SetField(ref _isFlagged, value);
    }
    private bool _isFlagged;

    public string Text
    {
        get => _text;
        private set => SetField(ref _text, value);
    }
    private string _text = "";

    public void RevealValue()
    {
        if (IsFlagged) return;
        IsRevealed = true;
        Text = value == 0 ? "" : value == -1 ? "*" : value.ToString();
    }

    public void ToggleFlag()
    {
        if (IsRevealed) return;
        IsFlagged = !_isFlagged;
        Text = _isFlagged ? "?" :  string.Empty;
    }
}
