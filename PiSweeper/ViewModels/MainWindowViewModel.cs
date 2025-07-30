using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
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
    
    private const int DefaultWidth = 10;
    private const int DefaultHeight = 10;

    private readonly int _width = DefaultWidth;
    private readonly int _height = DefaultHeight;
    private readonly int _minesCount = 10;
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
    
    public MainWindowViewModel()
    {
        ClickCellCommand = new RelayCommand(parameter => OnClickCell((CellViewModel)parameter!));
        StartNewGameCommand = new RelayCommand(_ => OnStartNewGame());
        ToggleFlagCommand = new RelayCommand(parameter => OnToggleFlag((CellViewModel)parameter!));
        
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
#if DEBUG
        PrintGameField();
#endif
        RefreshUiGameField();
        LeftTags = _minesCount;
        Time = TimeSpan.Zero;
        _timer.Start();
        _gameState = GameState.InGame;
    }

    private void AdjustGameFieldSize()
    {
        _field = new int[_width][];
        for (var i = 0; i < _width; i++)
        {
            _field[i] = new int[_height];
        }
    }

    private void InitializeGameField()
    {
        // ScatterMines
        var minePositions = new HashSet<Point>();
        while (minePositions.Count < _minesCount)
        {
            var xPosition = Random.Shared.Next(0, _width - 2) + 1;
            var yPosition = Random.Shared.Next(0, _height - 2) + 1;

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

#if DEBUG
    private void PrintGameField()
    {
        // Display Grid for DEBUG
        for (var y = 0; y < _height; y++)
        {
            for (var x = 0; x < _width; x++)
            {
                Console.Write(_field[x][y].ToString().PadLeft(2) + "|");
            }

            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
            Console.WriteLine(" ");
        }

        Console.WriteLine();
    }
#endif

    private void RefreshUiGameField()
    {
        var newGameField = new List<CellViewModel>();
        _gameFieldMap.Clear();

        for (var y = 1; y < _height - 1; y++)
        {
            for (var x = 1; x < _width - 1; x++)
            {
                var cellViewModel = new CellViewModel(x, y, _field[x][y]);
                newGameField.Add(cellViewModel);
                _gameFieldMap.Add(new Point(x, y), cellViewModel);
            }
        }

        GameField = new ObservableCollection<CellViewModel>(newGameField);
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
                currentCell.RevealValue();
            }
        }
        else if (cell.IsBomb)
        {
            // Bomb clicked -> game over; reveal whole field
            MessageBox.ShowDialog("You lost :-(");
            cell.RevealValue();
            _timer.Stop();
            _gameState = GameState.GameOver;
            foreach (var cellViewModel in _gameField)
            {
                cellViewModel.RevealValue();
            }
        }
        else
        {
            // "Normal" value cell; just reveal
            cell.RevealValue();
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
}