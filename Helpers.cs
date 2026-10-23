namespace CCC_42
{
    internal static class Helpers
    {
        public static bool CheckPatternAndRemoveMs(
            string[,] matrix,
            string[,] pattern)
        {
            bool hasPatternMs = false;
            for (int row = 0; row < pattern.GetLength(0); row++)
            {
                for (int column = 0; column < pattern.GetLength(1); column++)
                {
                    bool patternHasM = pattern[row, column] == "M";
                    bool matrixHasM = matrix[row, column] == "M";
                    hasPatternMs |= patternHasM;

                    if (patternHasM != matrixHasM)
                    {
                        return false;
                    }
                }
            }

            if (!hasPatternMs)
            {
                return false;
            }

            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                for (int column = 0; column < matrix.GetLength(1); column++)
                {
                    if (matrix[row, column] == "M")
                    {
                        matrix[row, column] = "O";
                    }
                }
            }

            return true;
        }

        public static (int Row, int Column) MoveToNextStar(
            string[,] matrix,
            int currentRow,
            int currentColumn,
            bool clockwise)
        {
            var perimeterStars = new List<(int Row, int Column)>();
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            for (int column = 0; column < columns; column++)
            {
                if (matrix[0, column] == "*")
                {
                    perimeterStars.Add((0, column));
                }
            }

            for (int row = 1; row < rows - 1; row++)
            {
                for (int column = columns - 1; column >= 0; column--)
                {
                    if (matrix[row, column] == "*")
                    {
                        perimeterStars.Add((row, column));
                        break;
                    }
                }
            }

            for (int column = columns - 1; column >= 0; column--)
            {
                if (matrix[rows - 1, column] == "*")
                {
                    perimeterStars.Add((rows - 1, column));
                }
            }

            for (int row = rows - 2; row > 0; row--)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (matrix[row, column] == "*")
                    {
                        perimeterStars.Add((row, column));
                        break;
                    }
                }
            }

            int currentIndex = perimeterStars.FindIndex(
                position => position.Row == currentRow && position.Column == currentColumn);
            if (currentIndex < 0 || perimeterStars.Count == 0)
            {
                return (currentRow, currentColumn);
            }

            int step = clockwise ? 1 : -1;
            int nextIndex = (currentIndex + step + perimeterStars.Count) % perimeterStars.Count;
            return perimeterStars[nextIndex];
        }

        public static void TraceLasers(
            string[,] matrix,
            int rows,
            int columns,
            List<(int Row, int Column, int Direction, bool Active)> numberLocations)
        {
            var laserMatrix = new int[rows, columns];

            for (int row = 0; row < laserMatrix.GetLength(0); row++)
            {
                for (int column = 0; column < laserMatrix.GetLength(1); column++)
                {
                    laserMatrix[row, column] = 0;
                }
            }

            foreach (var location in numberLocations)
            {
                if (!location.Active) { continue; }

                int dirRow = location.Direction == 2 || location.Direction == 3
                    ? 1
                    : location.Direction == 5 || location.Direction == 6
                        ? -1
                        : 0;
                int dirColumn = location.Direction == 1 || location.Direction == 2 || location.Direction == 6 ? 1 : -1;

                int numberRow = location.Row + dirRow;
                int numberColumn = location.Column + dirColumn;

                while (numberRow >= 0 && numberRow < rows &&
                    numberColumn >= 0 && numberColumn < columns &&
                    numberRow < matrix.GetLength(0) && numberColumn < matrix.GetLength(1) &&
                    numberRow < laserMatrix.GetLength(0) && numberColumn < laserMatrix.GetLength(1) &&
                    !matrix[numberRow, numberColumn].Equals("*") &&
                    !(int.TryParse(matrix[numberRow, numberColumn], out int number) && number is >= 1 and <= 6))
                {
                    if (!matrix[numberRow, numberColumn].Equals("-"))
                    {
                        laserMatrix[numberRow, numberColumn]++;
                    }
                    numberRow += dirRow;
                    numberColumn += dirColumn;
                }
            }

            for (int row = 0; row < matrix.GetLength(0); row++)
            {
                for (int column = 0; column < matrix.GetLength(1); column++)
                {
                    if (matrix[row, column].Equals("M") && laserMatrix[row, column] >= 3)
                    {
                        matrix[row, column] = "O";
                    }
                    else if (matrix[row, column].Equals("O") && laserMatrix[row, column] == 2)
                    {
                        matrix[row, column] = "M";
                    }
                }
            }
        }

        public static string[,] GenerateMatrix(int radius, out int rows, out int columns)
        {
            var matrixRows = new List<string>();
            var topRow = new System.Text.StringBuilder();
            topRow.Append('-', radius);
            topRow.Append(string.Concat(Enumerable.Repeat("*-", radius + 1)));
            topRow.Append('-', Math.Max(radius - 1, 0));
            matrixRows.Add(topRow.ToString());

            for (int i = 0; i < radius - 1; i++)
            {
                var row = new System.Text.StringBuilder();
                row.Append('-', Math.Max(radius - i - 1, 0));
                row.Append("*-");
                row.Append(string.Concat(Enumerable.Repeat("O-", radius + i)));
                row.Append('*');
                row.Append('-', Math.Max(radius - i - 1, 0));
                matrixRows.Add(row.ToString());
            }

            var middleRow = new System.Text.StringBuilder();
            middleRow.Append("*-");
            middleRow.Append(string.Concat(Enumerable.Repeat("O-", radius * 2 - 1)));
            middleRow.Append('*');
            matrixRows.Add(middleRow.ToString());

            for (int i = radius - 2; i >= 0; i--)
            {
                var row = new System.Text.StringBuilder();
                row.Append('-', Math.Max(radius - i - 1, 0));
                row.Append("*-");
                row.Append(string.Concat(Enumerable.Repeat("O-", radius + i)));
                row.Append('*');
                row.Append('-', Math.Max(radius - i - 1, 0));
                matrixRows.Add(row.ToString());
            }

            var bottomRow = new System.Text.StringBuilder();
            bottomRow.Append('-', radius);
            bottomRow.Append(string.Concat(Enumerable.Repeat("*-", radius + 1)));
            bottomRow.Append('-', Math.Max(radius - 1, 0));
            matrixRows.Add(bottomRow.ToString());

            rows = matrixRows.Count;
            columns = matrixRows[0].Length;
            var matrix = new string[rows, columns];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    matrix[row, column] = matrixRows[row][column].ToString();
                }
            }

            return matrix;
        }
    }
}
