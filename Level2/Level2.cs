using System.Reflection;

namespace CCC_42
{
    internal class Level2
    {
        public static void Solve()
        {
            string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            string className = MethodBase.GetCurrentMethod()!.DeclaringType!.Name;
            string fullPath = Path.Combine(basePath, className);

            try
            {
                foreach (var inPath in Directory.GetFiles(fullPath))
                {
                    if (Path.GetExtension(inPath).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        string outPath = inPath.Replace(".txt", ".out");
                        File.Create(outPath).Close();
                        StreamReader sr = new StreamReader(inPath);
                        StreamWriter sw = new StreamWriter(outPath);

                        int.TryParse(sr.ReadLine(), out int lines);
                        while (lines != 0)
                        {
                            string[] values = sr.ReadLine()!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                            int.TryParse(values[0], out int radius);
                            int.TryParse(values[1], out int rows);
                            int.TryParse(values[2], out int columns);

                            var matrix = new string[rows, columns];
                            int numberRow = -1;
                            int numberColumn = -1;
                            int direction = 0;

                            for (int row = 0; row < rows; row++)
                            {
                                string[] cells = sr.ReadLine()!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                                for (int column = 0; column < columns; column++)
                                {
                                    string cell = cells.Length == columns
                                        ? cells[column]
                                        : cells[0][column].ToString();
                                    matrix[row, column] = cell;

                                    if (int.TryParse(cell, out int number) && number is >= 1 and <= 6)
                                    {
                                        numberRow = row;
                                        numberColumn = column;
                                        direction = number;
                                    }
                                }
                            }

                            var dirRow = direction == 2 || direction == 3
                                ? 1
                                : direction == 5 || direction == 6
                                    ? -1
                                    : 0;
                            var dirColumn = direction == 1 || direction == 2 || direction == 6 ? 1 : -1;

                            numberRow += dirRow;
                            numberColumn += dirColumn;

                            while (!matrix[numberRow, numberColumn].Equals("*"))
                            {
                                if (!matrix[numberRow, numberColumn].Equals("-"))
                                {
                                    matrix[numberRow, numberColumn] = "M";
                                }
                                numberRow += dirRow;
                                numberColumn += dirColumn;
                            }

                            for (int row = 0; row < rows; row++)
                            {
                                for (int column = 0; column < columns; column++)
                                {
                                    sw.Write(matrix[row, column]);
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine();
                            sr.ReadLine();

                            lines--;
                        }

                        sr.Close();
                        sw.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

    }
}
