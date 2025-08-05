namespace PiSweeper.ViewModels;

public enum RevealReason
{
    Unrevealed,
    PlayerClick,
    GameOver,
}

public sealed class CellViewModel(int x, int y, int value) : BaseViewModel
{
    public int X => x;

    public int Y => y;

    public int Value => value;

    public bool IsZero => value == 0;

    public bool IsBomb => value == -1;

    public bool IsRevealedDueToGameOver => RevealReason == RevealReason.GameOver;

    public bool IsFlaggedCorrectly => IsBomb && IsFlagged;

    public bool IsFlaggedIncorrectly => !IsBomb && IsFlagged;

    public RevealReason RevealReason
    {
        get => _revealReason;
        private set
        {
            if (!SetField(ref _revealReason, value)) return;
            OnPropertyChanged(nameof(IsRevealedDueToGameOver));
            OnPropertyChanged(nameof(IsFlaggedCorrectly));
            OnPropertyChanged(nameof(IsFlaggedIncorrectly));
        }
    }

    private RevealReason _revealReason = RevealReason.Unrevealed;

    public bool IsExploded
    {
        get => _isExploded;
        private set => SetField(ref _isExploded, value);
    }
    private bool _isExploded;

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
            if (IsFlagged && RevealReason == RevealReason.Unrevealed) return "?";
            if (!IsRevealed || value == 0) return "";
            if (IsBomb) return "*";
            return value.ToString();
        }
    }

    public void RevealValue(RevealReason reason)
    {
        if (reason == RevealReason.PlayerClick)
        {
            // Unflag when revealing by player click
            // but keep when revealing due to game over
            // so that the player can see which flags were correct
            IsFlagged = false;
        }
        RevealReason = reason;
        IsRevealed = true;
    }

    public void ToggleFlag()
    {
        if (IsRevealed) return;
        IsFlagged = !_isFlagged;
    }

    public void Explode()
    {
        RevealValue(RevealReason.PlayerClick);
        IsExploded = true;
    }
}
