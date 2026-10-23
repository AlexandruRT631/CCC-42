using System.Reflection;

namespace CCC_42
{
    internal class Level5
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
                            int submissions = 0;

                            string[] values = sr.ReadLine()!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                            int.TryParse(values[0], out int radius);
                            int.TryParse(values[1], out int rows);
                            int.TryParse(values[2], out int columns);

                            var pattern = new string[rows, columns];

                            for (int row = 0; row < rows; row++)
                            {
                                string[] cells = sr.ReadLine()!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                                for (int column = 0; column < columns; column++)
                                {
                                    string cell = cells.Length == columns
                                        ? cells[column]
                                        : cells[0][column].ToString();
                                    pattern[row, column] = cell;
                                }
                            }

                            int.TryParse(sr.ReadLine(), out int instructionsCount);

                            string[,] matrix = Helpers.GenerateMatrix(radius, out int _, out int _);
                            var numberLocations = new List<(int Row, int Column, int Direction, bool Active)>();
                            numberLocations.Add((radius, 0, 1, false));
                            numberLocations.Add((0, radius, 2, false));
                            numberLocations.Add((0, columns - radius - 1, 3, false));
                            numberLocations.Add((radius, columns - 1, 4, false));
                            numberLocations.Add((rows - 1, columns - radius - 1, 5, false));
                            numberLocations.Add((rows - 1, radius, 6, false));

                            for (int i = 0; i < instructionsCount; i++)
                            {
                                string[] instructions = sr.ReadLine()!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

                                for (int locationIndex = 0; locationIndex < numberLocations.Count; locationIndex++)
                                {
                                    var (Row, Column, Direction, Active) = numberLocations[locationIndex];
                                    if (instructions[locationIndex].Equals("T"))
                                    {
                                        numberLocations[locationIndex] = (Row, Column, Direction, !Active);
                                    }
                                    else if (instructions[locationIndex].Equals("F"))
                                    {
                                        var nextPosition = Helpers.MoveToNextStar(matrix, Row, Column, clockwise: true);
                                        numberLocations[locationIndex] =
                                            (nextPosition.Row, nextPosition.Column, Direction, Active);
                                    }
                                    else if (instructions[locationIndex].Equals("B"))
                                    {
                                        var nextPosition = Helpers.MoveToNextStar(matrix, Row, Column, clockwise: false);
                                        numberLocations[locationIndex] =
                                            (nextPosition.Row, nextPosition.Column, Direction, Active);
                                    }
                                }

                                Helpers.TraceLasers(matrix, rows, columns, numberLocations);
                                if (Helpers.CheckPatternAndRemoveMs(matrix, pattern))
                                {
                                    submissions++;
                                }

                            }

                            sw.WriteLine(submissions);
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
