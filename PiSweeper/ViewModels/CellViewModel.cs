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
    private bool _isRevealed = false;

    public string Text
    {
        get => _text;
        private set => SetField(ref _text, value);
    }
    private string _text = "";

    public void RevealValue()
    {
        IsRevealed = true;
        Text = value.ToString();
    }
}
