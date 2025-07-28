namespace PiSweeper.ViewModels;

public sealed class CellViewModel(int x, int y, int value) : BaseViewModel
{
    public int X => x;

    public int Y => y;

    public int Value => value;

    public bool IsZero => value == 0;

    public bool IsBomb => value == -1;

    public bool IsRevealed
    {
        get => _isRevealed;
        private set
        {
            if (!SetField(ref _isRevealed, value)) return;
            OnPropertyChanged(nameof(Text));
        }
    }
    private bool _isRevealed;

    public bool IsFlagged
    {
        get => _isFlagged;
        private set
        {
            if (!SetField(ref _isFlagged, value)) return;
            OnPropertyChanged(nameof(Text));
        }
    }
    private bool _isFlagged;

    public string Text
    {
        get
        {
            if (IsFlagged) return "?";
            if (!IsRevealed || value == 0) return "";
            if (IsBomb) return "*";
            return value.ToString();
        }
    }

    public void RevealValue()
    {
        IsRevealed = true;
        IsFlagged = false;
    }

    public void ToggleFlag()
    {
        if (IsRevealed) return;
        IsFlagged = !_isFlagged;
    }
}