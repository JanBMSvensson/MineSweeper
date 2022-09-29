
using static System.Console;
using MineSweeperConsoleUI;
using MineSweeper;

Write("SPACE = Toggle flag, ENTER = Open, SHIFT+ENTER = Open neighbours, ESCAPE = Exit   Game time: ");
int timeRow = CursorLeft;
int timeCol = CursorTop;
WriteLine();

int gameWidth = 25;
int gameHeight = 12;
int bombCount = (int)(gameHeight * gameWidth * .1); // 10%

var UI = new CellVisualizer(CursorLeft, CursorTop, gameWidth, gameHeight);
UI.WriteBoxes();
UI.MoveToCell(0, 0);

var Game = new MineSweeperGame(gameWidth, gameHeight, bombCount);
Game.BlockOpen += Game_BlockOpen;
Game.BombExploded += Game_BombExploded;
Game.FlagChanged += Game_FlagChanged;
Game.GameWon += Game_GameWon;
Game.GameTime += Game_GameTime;


Game.StartGame();

bool exit = false;
while (!exit)
{
    var keyInfo = ReadKey(true);
    switch (keyInfo.Key)
    {
        case ConsoleKey.Enter:
            if ((keyInfo.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
                Game.OpenNeighbourBlocks(UI.X, UI.Y);
            else
                Game.OpenBlock(UI.X, UI.Y);
            break;

        case ConsoleKey.Spacebar:
            Game.ToggleFlag(UI.X, UI.Y);
            break;
        case ConsoleKey.LeftArrow:
            UI.MoveCursor(CellVisualizer.Direction.Left);
            break;
        case ConsoleKey.RightArrow:
            UI.MoveCursor(CellVisualizer.Direction.Right);
            break;
        case ConsoleKey.UpArrow:
            UI.MoveCursor(CellVisualizer.Direction.Up);
            break;
        case ConsoleKey.DownArrow:
            UI.MoveCursor(CellVisualizer.Direction.Down);
            break;
        case ConsoleKey.Escape:
            exit = true;
            break;
    }
}



// Handle game events ....

void Game_GameWon(object? sender, PassedTimEventArgs e)
{
    WriteLine($"GAME WON in {(int)e.Time.TotalMinutes} minutes and {e.Time.Seconds} seconds");
}

void Game_FlagChanged(object? sender, FlagEventArgs e)
{
    if (e.IsFlagged)
        Write(UI.FlagChar);
    else
        Write(UI.HiddenChar);

    UI.MoveToCell(e.X, e.Y);
}

void Game_BombExploded(object? sender, PositionEventArgs e)
{
    WriteLine("GAME LOST!");
}

void Game_BlockOpen(object? sender, OpenEventArgs e)
{
    UI.MoveToCell(e.X, e.Y);
    Write(e.BombsHint > 0 ? e.BombsHint : " ");
    UI.MoveToCell(UI.X, UI.Y);
}

void Game_GameTime(object? sender, PassedTimEventArgs e)
{
    SetCursorPosition(timeRow, timeCol);
    Write($"{((int)e.Time.Minutes).ToString().PadRight(2,'0')}:{e.Time.Seconds.ToString().PadLeft(2,'0')} ");
    UI.MoveToCell(UI.X, UI.Y);
}

