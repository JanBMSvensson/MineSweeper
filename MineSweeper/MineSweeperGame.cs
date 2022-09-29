using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace MineSweeper
{
    public class PositionEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }

        internal PositionEventArgs(int x, int y)
        {
            X = x; Y = y; 
        }
    }
    public class PassedTimEventArgs : EventArgs
    {
        public TimeSpan Time { get; }
        internal PassedTimEventArgs (TimeSpan time)
        {
            Time = time;
        }
    }
    public class OpenEventArgs : PositionEventArgs
    {
        public int BombsHint { get; }
        internal OpenEventArgs(int x, int y, int bombsHint) : base(x, y)
        {
            BombsHint = bombsHint;
        }
    }
    public class FlagEventArgs : PositionEventArgs
    {
        public bool IsFlagged { get; }
        internal FlagEventArgs(int x, int y, bool isFlagged) : base(x, y)
        {
            IsFlagged = isFlagged;
        }
    }
    public class LostGameEventArgs : PositionEventArgs
    {
        public (int x, int y)[] BombLocations { get; }

        internal LostGameEventArgs(int x, int y, (int x, int y)[] bombLocaltions) : base(x,y)
        {
            BombLocations = bombLocaltions;
        }
    }

    public class MineSweeperGame
    {
        private System.Timers.Timer? timer;
        private readonly bool[,] Opened;
        private readonly bool[,] Flagged;
        private readonly bool[,] Bombs;
        private DateTime GameStarted;
        private DateTime GameFinished;

        public event EventHandler<LostGameEventArgs>? BombExploded;
        public event EventHandler<OpenEventArgs>? BlockOpen;
        public event EventHandler<FlagEventArgs>? FlagChanged;
        public event EventHandler<PassedTimEventArgs>? GameTime;
        public event EventHandler<PassedTimEventArgs>? GameWon;


        public int Width { get; }
        public int Height { get; }
        public int BombCount { get; }

        public MineSweeperGame(int width, int height, int bombCount)
        {
            Width = width;
            Height = height;
            BombCount = bombCount;

            Opened = new bool[width, height];
            Flagged = new bool[width, height];
            Bombs = new bool[width, height];

            ResetArea();
            AddBombs(bombCount);
        }




        public void StartGame()
        {
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            GameStarted = DateTime.Now;
        }

        public void StopGame()
        {
            if (timer is not null)
            {
                timer.Stop();
                GameFinished = DateTime.Now;
                timer.Elapsed -= Timer_Elapsed;
                timer = null;
            }
        }

        public void ToggleFlag(int x, int y)
        {
            if(!Opened[x, y])
            {
                Flagged[x, y] = !Flagged[x, y];
                FlagChanged?.Invoke(this, new FlagEventArgs(x, y, Flagged[x, y]));
                if (IsGameWon())
                {
                    StopGame();
                    GameWon?.Invoke(this, new PassedTimEventArgs(new TimeSpan(DateTime.Now.Ticks)));
                }
            }
        }
        
        public void OpenBlock(int x, int y)
        {
            if (!Flagged[x, y] && !Opened[x, y])
            {
                Opened[x, y] = true;
                //Debug.WriteLine($"Opened X={x}, Y={y}");
                if (Bombs[x, y])
                {
                    StopGame();
                    
                    BombExploded?.Invoke(this, new LostGameEventArgs(x, y, GetUnmarkedBombs()));
                }
                else
                {
                    int hint = CountNeighbouringBombs(x, y);
                    BlockOpen?.Invoke(this, new OpenEventArgs(x, y, hint));

                    if (hint == 0)
                        OpenNeighbourBlocks(x, y);
                }

                if (IsGameWon())
                {
                    StopGame();
                    GameWon?.Invoke(this, new PassedTimEventArgs(new TimeSpan(GameFinished.Ticks - GameStarted.Ticks)));
                }
            }
        }


        public void OpenNeighbourBlocks(int x, int y)
        {
            if (Opened[x, y])
            {
                var (x1, y1, x2, y2) = GetNeighbours(x, y);
                for (int rx = x1; rx <= x2; rx++)
                    for (int ry = y1; ry <= y2; ry++)
                        if (!Opened[rx, ry]) { OpenBlock(rx, ry); }
            }
        }

        private (int x, int y)[] GetUnmarkedBombs()
        {
            List<(int x, int y)> list = new();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Bombs[x, y] && !Flagged[x, y]) list.Add((x, y));

            return list.ToArray();
        }

        private int CountNeighbouringBombs(int x, int y)
        {
            int count = 0;
            var (x1, y1, x2, y2) = GetNeighbours(x, y);
            for (int px = x1; px <= x2; px++)
                for (int py = y1; py <= y2; py++)
                    if (Bombs[px, py]) { count++; }

            return count;
        }
        private (int x1, int y1, int x2, int y2) GetNeighbours(int x, int y)
        {
            int x1 = x - 1;
            int y1 = y - 1;
            int x2 = x + 1;
            int y2 = y + 1;
            if (x1 < 0) x1 = 0;
            if (y1 < 0) y1 = 0;
            if (x2 >= Width) x2 = Width - 1;
            if (y2 >= Height) y2 = Height - 1;
            return (x1, y1, x2, y2);
        }

        private bool IsGameWon()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (!Opened[x, y] && !Bombs[x, y])
                        return false;

            return true;
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            GameTime?.Invoke(this, new PassedTimEventArgs(new TimeSpan(DateTime.Now.Ticks - GameStarted.Ticks)));
        }



        private void ResetArea()
        {
            for(int column = 0; column < Width; column++)
            {
                for (int row = 0; row < Height; row++)
                {
                    Opened[column, row] = false;
                    Flagged[column, row] = false;
                    Bombs[column, row] = false;
                }
            }
        }

        private void AddBombs(int bombCount)
        {
            Random random = new();

            int x, y;
            while (bombCount > 0)
            {
                x = random.Next(Width);
                y = random.Next(Height);
                if (!Bombs[x,y])
                {
                    Bombs[x, y] = true;
                    bombCount--;
                    //Debug.WriteLine($"X={x}, Y={y}");
                }
            }
        }

    }
}