namespace CCC_42
{
    internal class Level6
    {
        public static void Solve()
        {
            string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
            string fullPath = Path.Combine(basePath, nameof(Level6));

            foreach (string inPath in Directory.GetFiles(fullPath, "*.txt"))
            {
                using var reader = new StreamReader(inPath);
                using var writer = new StreamWriter(Path.ChangeExtension(inPath, ".out"));

                int cases = int.Parse(ReadNonEmptyLine(reader));
                for (int caseIndex = 0; caseIndex < cases; caseIndex++)
                {
                    string[] values = ReadNonEmptyLine(reader)
                        .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                    int radius = int.Parse(values[0]);
                    int rows = int.Parse(values[1]);
                    int columns = int.Parse(values[2]);
                    int requiredSubmissions = int.Parse(values[3]);

                    var pattern = new string[rows, columns];
                    for (int row = 0; row < rows; row++)
                    {
                        string[] cells = ReadNonEmptyLine(reader)
                            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                        for (int column = 0; column < columns; column++)
                        {
                            pattern[row, column] = cells.Length == columns
                                ? cells[column]
                                : cells[0][column].ToString();
                        }
                    }

                    PatternProgramSolver.WriteProgram(radius, pattern, requiredSubmissions, writer);
                    writer.WriteLine();
                }
            }
        }

        private static string ReadNonEmptyLine(StreamReader reader)
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    return line;
                }
            }

            throw new EndOfStreamException("Expected another input line.");
        }
    }
}
