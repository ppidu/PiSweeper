using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PiSweeper.ViewModels;

public sealed class MainWindowViewModel : BaseViewModel
{
    private const int DefaultWidth = 10;
    private const int DefaultHeight = 10;

    private readonly int _width = DefaultWidth;
    private readonly int _height = DefaultHeight;
    private readonly int _minesCount = 10;
    private int[][] _field = null!;
    private ObservableCollection<CellViewModel> _gameField = [];

    public ObservableCollection<CellViewModel> GameField
    {
        get => _gameField;
        set => SetField(ref _gameField, value);
    }

    public MainWindowViewModel()
    {
        ResetGame();
        ClickCellCommand = new RelayCommand(parameter => OnClickCell((CellViewModel)parameter!));
        StartNewGameCommand = new RelayCommand(_ => OnStartNewGame());
    }

    private void ResetGame()
    {
        AdjustGameFieldSize();
        InitializeGameField();
        PrintGameField();
        RefreshUiGameField();
    }

    private void AdjustGameFieldSize()
    {
        _field = new int[_width][];
        for (var i = 0; i < _width; i++) _field[i] = new int[_height];
    }

    private void InitializeGameField()
    {
        // ScatterMines
        var minePositions = new HashSet<int>();
        while (minePositions.Count < _minesCount)
        {
            var xPosition = Random.Shared.Next(0, _width - 2) + 1;
            var yPosition = Random.Shared.Next(0, _height - 2) + 1;
            var position = xPosition + yPosition * _width;

            if (!minePositions.Add(position)) continue;

            _field[xPosition][yPosition] = -1;
        }
        
        // Calculate Field Values
        foreach (var minePosition in minePositions)
        {
            var xPosition = minePosition % _width;
            var yPosition = minePosition / _width;
            
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

    private void RefreshUiGameField()
    {
        var newGameField = new List<CellViewModel>();

        for (var y = 1; y < _height - 1; y++)
        {
            for (var x = 1; x < _width - 1; x++)
            {
                newGameField.Add(new CellViewModel(x, y,  _field[x][y]));
            }
        }

        GameField = new ObservableCollection<CellViewModel>(newGameField);
    }

    public ICommand ClickCellCommand { get; private set; }

    private void OnClickCell(CellViewModel cell)
    {
        Console.WriteLine("Clicked " + cell.X + "|" + cell.Y);
        cell.RevealValue();
    }

    public ICommand StartNewGameCommand { get; private set; }

    private void OnStartNewGame()
    {
        ResetGame();
    }
}