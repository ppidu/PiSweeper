namespace PiSweeper.ViewModels;

public sealed class CellViewModel(int x, int y, int value) : BaseViewModel
{
    public int X => x;

    public int Y => y;

    public bool IsBomb => value == -1;

    public string Text
    {
        get => _text;
        private set => SetField(ref _text, value);
    }
    private string _text = "";

    public void RevealValue()
    {
        Text = value.ToString();
    }
}
