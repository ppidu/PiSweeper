using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using PiSweeper.Dialogs;

namespace PiSweeper.ViewModels;

public sealed class MainWindowViewModel : BaseViewModel
{
    private enum GameState
    {
        InGame,
        GameOver,
    }
    
    private const int DefaultWidth = 8;
    private const int DefaultHeight = 8;
    private const string FontFamily = "Segoe UI";

    private int _minesCount = 10;
    private int[][] _field = null!;
    private ObservableCollection<CellViewModel> _gameField = [];
    private readonly Dictionary<Point, CellViewModel> _gameFieldMap = [];

    private GameState _gameState = GameState.InGame;
    
    private readonly DispatcherTimer _timer;
    
    public ObservableCollection<CellViewModel> GameField
    {
        get => _gameField;
        set => SetField(ref _gameField, value);
    }

    public int LeftTags
    {
        get => _leftTags;
        set => SetField(ref _leftTags, value);
    }
    private int _leftTags;
    
    public TimeSpan Time
    {
        get => _time;
        private set => SetField(ref _time, value);
    }
    private TimeSpan _time = TimeSpan.Zero;
    
    public int Width
    {
        get => _width;
        private set => SetField(ref _width, value);
    }
    private int _width = DefaultWidth;
    
    public int Height
    {
        get => _height;
        private set => SetField(ref _height, value);
    }
    private int _height = DefaultHeight;

    public double MinWindowWidth
    {
        get => _minWindowWidth;
        private set => SetField(ref _minWindowWidth, value);
    }
    private double _minWindowWidth;

    public double MinWindowHeight
    {
        get => _minWindowHeight;
        private set => SetField(ref _minWindowHeight, value);
    }
    private double _minWindowHeight;
    
    public MainWindowViewModel()
    {
        ClickCellCommand = new RelayCommand(parameter => OnClickCell((CellViewModel)parameter!));
        StartNewGameCommand = new RelayCommand(_ => OnStartNewGame());
        ToggleFlagCommand = new RelayCommand(parameter => OnToggleFlag((CellViewModel)parameter!));
        SetGameFieldSizeCommand = new RelayCommand(parameter => OnSetGameFieldSizeAsync(parameter as string));
            
        _timer = new  DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
        
        ResetGame();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        Time += TimeSpan.FromSeconds(1);
    }

    private void ResetGame()
    {
        AdjustGameFieldSize();
        InitializeGameField();
        RefreshUiGameField();
        UpdateWindowMinSize();
        
        LeftTags = _minesCount;
        Time = TimeSpan.Zero;
        _timer.Start();
        _gameState = GameState.InGame;
    }

    private void AdjustGameFieldSize()
    {
        _field = new int[Width + 2][];
        for (var i = 0; i < Width + 2; i++)
        {
            _field[i] = new int[Height + 2];
        }
    }

    private void InitializeGameField()
    {
        // ScatterMines
        var minePositions = new HashSet<Point>();
        while (minePositions.Count < _minesCount)
        {
            var xPosition = Random.Shared.Next(0, Width) + 1;
            var yPosition = Random.Shared.Next(0, Height) + 1;

            if (!minePositions.Add(new Point(xPosition, yPosition))) continue;

            _field[xPosition][yPosition] = -1;
        }

        // Calculate Field Values
        foreach (var minePosition in minePositions)
        {
            var xPosition = minePosition.X;
            var yPosition = minePosition.Y;

            if (_field[xPosition - 1][yPosition - 1] != -1) _field[xPosition - 1][yPosition - 1] += 1;
            if (_field[xPosition][yPosition - 1] != -1) _field[xPosition][yPosition - 1] += 1;
            if (_field[xPosition + 1][yPosition - 1] != -1) _field[xPosition + 1][yPosition - 1] += 1;

            if (_field[xPosition - 1][yPosition] != -1) _field[xPosition - 1][yPosition] += 1;
            if (_field[xPosition + 1][yPosition] != -1) _field[xPosition + 1][yPosition] += 1;

            if (_field[xPosition - 1][yPosition + 1] != -1) _field[xPosition - 1][yPosition + 1] += 1;
            if (_field[xPosition][yPosition + 1] != -1) _field[xPosition][yPosition + 1] += 1;
            if (_field[xPosition + 1][yPosition + 1] != -1) _field[xPosition + 1][yPosition + 1] += 1;
        }
    }

    private void RefreshUiGameField()
    {
        var newGameField = new List<CellViewModel>();
        _gameFieldMap.Clear();

        for (var y = 1; y < Height + 1; y++)
        {
            for (var x = 1; x < Width + 1; x++)
            {
                var cellViewModel = new CellViewModel(x, y, _field[x][y]);
                newGameField.Add(cellViewModel);
                _gameFieldMap.Add(new Point(x, y), cellViewModel);
            }
        }

        GameField = new ObservableCollection<CellViewModel>(newGameField);
    }
        
    private void UpdateWindowMinSize()
    {
        var typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Bold, FontStretch.Expanded);
        var layout = new TextLayout(
            text: "W",
            typeface: typeface,
            fontSize: 32,
            foreground: Brushes.Black,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity,
            textAlignment: TextAlignment.Left,
            textWrapping: TextWrapping.NoWrap,
            lineHeight: double.NaN,
            letterSpacing: 0,
            textTrimming: TextTrimming.None
        );

        var buttonWidth = layout.Width;
        var buttonHeight = layout.Height;
        const int horizontalPadding = 16; 
        const int verticalPadding = 39; 

        MinWindowWidth = Width * buttonWidth + horizontalPadding;
        MinWindowHeight = Height * buttonHeight + verticalPadding;
    }
    
    public ICommand ClickCellCommand { get; private set; }

    private void OnClickCell(CellViewModel cell)
    {
        if (_gameState != GameState.InGame || cell.IsFlagged || cell.IsRevealed) return;
        
        if (_field[cell.X][cell.Y] == 0)
        {
            // Expand zero values
            var cellsToReveal = new List<CellViewModel>();
            var cellsToCheck = new Queue<Point>();
            var alreadyCheckedCells = new HashSet<Point>();
            
            cellsToCheck.Enqueue(new Point(cell.X, cell.Y));
            while (cellsToCheck.Count > 0)
            {
                var currentPosition = cellsToCheck.Dequeue();
                if (!alreadyCheckedCells.Add(currentPosition)) continue;
                if (!_gameFieldMap.TryGetValue(currentPosition, out var currentCell)) continue;
                cellsToReveal.Add(currentCell);
                
                // Add neighbours if not zero valued; else it is border
                if (!currentCell.IsZero) continue;
                cellsToCheck.Enqueue(new Point(currentPosition.X - 1, currentPosition.Y - 1));
                cellsToCheck.Enqueue(new Point(currentPosition.X, currentPosition.Y - 1));
                cellsToCheck.Enqueue(new Point(currentPosition.X + 1, currentPosition.Y - 1));
                
                cellsToCheck.Enqueue(new Point(currentPosition.X - 1, currentPosition.Y));
                cellsToCheck.Enqueue(new Point(currentPosition.X + 1, currentPosition.Y));
                
                cellsToCheck.Enqueue(new Point(currentPosition.X - 1, currentPosition.Y + 1));
                cellsToCheck.Enqueue(new Point(currentPosition.X, currentPosition.Y + 1));
                cellsToCheck.Enqueue(new Point(currentPosition.X + 1, currentPosition.Y + 1));
            }

            foreach (var cellToReveal in cellsToReveal)
            {
                var currentCell = _gameFieldMap[new Point(cellToReveal.X, cellToReveal.Y)]; 
                LeftTags += currentCell.IsFlagged ? 1 : 0;
                currentCell.RevealValue(RevealReason.PlayerClick);
            }
        }
        else if (cell.IsBomb)
        {
            // Bomb clicked -> game over; reveal whole field
            MessageBox.ShowDialog("You lost :-(");
            cell.Explode();
            _timer.Stop();
            _gameState = GameState.GameOver;
            RevealAll();
        }
        else
        {
            // "Normal" value cell; just reveal
            cell.RevealValue(RevealReason.PlayerClick);
        }
        
        // Check if player won
        if (_gameState == GameState.GameOver) return;
        var allBombUnrevealed = _gameField.Where(x => x.IsBomb).All(x => !x.IsRevealed);
        var allNormalCellsRevealed  = _gameField.Where(x => !x.IsBomb).All(x => x.IsRevealed);
        if (allBombUnrevealed && allNormalCellsRevealed)
        {
            MessageBox.ShowDialog("You won :-)");
            _timer.Stop();
            _gameState = GameState.GameOver;
        }
    }

    private void RevealAll()
    {
        foreach (var cellViewModel in _gameField)
        {
            if (cellViewModel.IsRevealed) continue;
            cellViewModel.RevealValue(RevealReason.GameOver);
        }
    }

    public ICommand StartNewGameCommand { get; private set; }

    private void OnStartNewGame()
    {
        ResetGame();
    }

    public ICommand ToggleFlagCommand { get; private set; }

    private void OnToggleFlag(CellViewModel cell)
    {
        if (_gameState != GameState.InGame) return;
        if (LeftTags == 0 && !cell.IsFlagged) return;
        cell.ToggleFlag();
        LeftTags += cell.IsFlagged ? -1 : 1;
    }

    public ICommand SetGameFieldSizeCommand { get; private set; }
    
    private async void OnSetGameFieldSizeAsync(string? size)
    {
        if (size == "S")
        {
            Width = 8;
            Height = 8;
            _minesCount = 10;
        }
        else if (size == "M")
        {
            Width = 12;
            Height = 12;
            _minesCount = 23;
        }
        else if (size == "L")
        {
            Width = 20;
            Height = 20;
            _minesCount = 63;
        }
        else
        {
            var dialog = new CustomGameSettingsDialog();
            var viewModel = new CustomGameSettingsDialogViewModel();
            dialog.DataContext = viewModel;
            await dialog.ShowDialog(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow);

            Width = (int)viewModel.Rows;
            Height = (int)viewModel.Columns;
            _minesCount = (int)viewModel.Mines;
        }
        ResetGame();
    }
}