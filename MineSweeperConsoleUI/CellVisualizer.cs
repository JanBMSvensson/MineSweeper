using static System.Console;

namespace MineSweeperConsoleUI
{
    class CellVisualizer
    {
        int height, width;
        int screenX, screenY;
        int activeX, activeY;

        char[,] BorderChars = { { '┌', '┬', '┐' },
                            { '├', '┼', '┤' },
                            { '└', '┴', '┘' } };
        char HorizontalBorder = '─';
        char VerticalBorder = '│';
        public readonly char FlagChar = 'X';
        public readonly char HiddenChar = '░';

        public int X => activeX;
        public int Y => activeY;

        public CellVisualizer(int screenX, int screenY, int width, int height)
        {
            this.height = height;
            this.width = width;
            this.screenX = screenX;
            this.screenY = screenY;
            activeX = 0;
            activeY = 0;
        }


        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        public void MoveCursor(Direction d)
        {
            Move(d);
            MoveToCell(this.activeX, this.activeY);
        }

        public void MoveToCell(int x, int y)
        {
            int px = this.screenX + 2 + x * 4;
            int py = this.screenY + 1 + y * 2;
            SetCursorPosition(px, py);
            //Debug.WriteLine($"X={px}, Y={py}");
        }

        private bool Move(Direction d)
        {
            switch (d)
            {
                case Direction.Left:
                    if (activeX == 0)
                        return false;
                    else
                        activeX--;
                    break;

                case Direction.Right:
                    if (activeX == width - 1)
                        return false;
                    else
                        activeX++;
                    break;

                case Direction.Up:
                    if (activeY == 0)
                        return false;
                    else
                        activeY--;
                    break;

                case Direction.Down:
                    if (activeY == height - 1)
                        return false;
                    else
                        activeY++;
                    break;

                default:
                    throw new Exception("8627394876234");

            }

            return true;
        }

        public void WriteBoxes()
        {
            ForegroundColor = ConsoleColor.DarkGray;
            for (int y = 0; y <= height; y++)
            {
                for (int x = 0; x <= width; x++)
                {
                    int cx = y == 0 ? 0 : y == height ? 2 : 1;
                    int cy = x == 0 ? 0 : x == width ? 2 : 1;

                    Write(BorderChars[cx, cy]);
                    if (x < width)
                    {
                        Write(HorizontalBorder);
                        Write(HorizontalBorder);
                        Write(HorizontalBorder);
                    }
                }
                WriteLine();
                if (y < height)
                {
                    for (int x = 0; x <= width; x++)
                    {
                        Write(VerticalBorder);
                        if (x < width)
                        {
                            Write(' ');
                            Write(this.HiddenChar);
                            Write(' ');
                        }
                    }
                    WriteLine();
                }
            }
        }

    }
}
