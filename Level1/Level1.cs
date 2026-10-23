using System.Reflection;

namespace CCC_42
{
    internal class Level1
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
                            int.TryParse(sr.ReadLine(), out int radius);

                            string[,] matrix = Helpers.GenerateMatrix(radius, out int rows, out int columns);
                            for (int row = 0; row < rows; row++)
                            {
                                for (int column = 0; column < columns; column++)
                                {
                                    sw.Write(matrix[row, column]);
                                }
                                sw.WriteLine();
                            }
                            sw.WriteLine();

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
