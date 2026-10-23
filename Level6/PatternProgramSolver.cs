using System.Numerics;

namespace CCC_42
{
    internal static class PatternProgramSolver
    {
        private sealed record TargetCell(int Id, BigInteger Mask, int[] Sources);

        private sealed record Candidate(
            int Pair,
            TargetCell Target,
            int MovementTicks,
            BigInteger FirstRay,
            BigInteger SecondRay);

        private sealed record Batch(
            Candidate[] Assignments,
            BigInteger Created,
            int CreatedCount,
            int MovementTicks,
            int PairUsage);

        // These disjoint masks record cells hit once, twice, or at least three times.
        private readonly record struct HitCounts(BigInteger Once, BigInteger Twice, BigInteger Many)
        {
            public HitCounts AddRay(BigInteger ray)
            {
                BigInteger occupied = Once | Twice | Many;
                return new HitCounts(
                    (Once & ~ray) | (ray & ~occupied),
                    (Twice & ~ray) | (Once & ray),
                    Many | (Twice & ray));
            }
        }

        public static void WriteProgram(
            int radius,
            string[,] pattern,
            int requiredSubmissions,
            TextWriter writer)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(radius, 1);
            ArgumentOutOfRangeException.ThrowIfNegative(requiredSubmissions);

            string[,] matrix = Helpers.GenerateMatrix(radius, out int rows, out int columns);
            if (pattern.GetLength(0) != rows || pattern.GetLength(1) != columns)
            {
                throw new ArgumentException("The pattern dimensions do not match the board radius.", nameof(pattern));
            }

            // Start at the left corner and walk clockwise around the six edges.
            var perimeter = new List<(int Row, int Column)>();
            (int Row, int Column)[] edgeSteps =
            {
                (-1, 1), (0, 2), (1, 1), (1, -1), (0, -2), (-1, -1)
            };
            int perimeterRow = radius;
            int perimeterColumn = 0;
            foreach (var step in edgeSteps)
            {
                for (int i = 0; i < radius; i++)
                {
                    perimeter.Add((perimeterRow, perimeterColumn));
                    perimeterRow += step.Row;
                    perimeterColumn += step.Column;
                }
            }

            var perimeterIndices = perimeter
                .Select((position, index) => (position, index))
                .ToDictionary(item => item.position, item => item.index);
            (int Row, int Column)[] directions =
            {
                (0, 2), (1, 1), (1, -1), (0, -2), (-1, -1), (-1, 1)
            };

            // Cache the cells crossed by every possible emitter ray.
            var cellMasks = new BigInteger[rows, columns];
            int interiorCellCount = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (matrix[row, column] == "O")
                    {
                        cellMasks[row, column] = BigInteger.One << interiorCellCount++;
                    }
                }
            }

            var rays = new BigInteger[6, perimeter.Count];
            for (int emitter = 0; emitter < 6; emitter++)
            {
                var step = directions[emitter];
                for (int position = 0; position < perimeter.Count; position++)
                {
                    int row = perimeter[position].Row + step.Row;
                    int column = perimeter[position].Column + step.Column;
                    while (IsInside(row, column) && matrix[row, column] != "*")
                    {
                        rays[emitter, position] |= cellMasks[row, column];
                        row += step.Row;
                        column += step.Column;
                    }
                }
            }

            var targets = new List<TargetCell>();
            BigInteger targetMask = BigInteger.Zero;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (pattern[row, column] != "M")
                    {
                        continue;
                    }
                    if (matrix[row, column] != "O")
                    {
                        throw new ArgumentException("Pattern M cells must be inside the board.", nameof(pattern));
                    }

                    int[] sources = directions
                        .Select(step => FindSource(row, column, step.Row, step.Column))
                        .ToArray();
                    var target = new TargetCell(targets.Count, cellMasks[row, column], sources);
                    targets.Add(target);
                    targetMask |= target.Mask;
                }
            }

            if (targets.Count == 0)
            {
                for (int i = 0; i < requiredSubmissions; i++)
                {
                    writer.WriteLine("- - - - - -");
                }
                return;
            }

            int[] positions = Enumerable.Range(0, 6).Select(emitter => emitter * radius).ToArray();
            int[] pairUsage = new int[3];
            int submissions = 0;
            BigInteger matter = BigInteger.Zero;
            var locations = new List<(int Row, int Column, int Direction, bool Active)>();
            for (int emitter = 0; emitter < 6; emitter++)
            {
                var position = perimeter[positions[emitter]];
                locations.Add((position.Row, position.Column, emitter + 1, false));
            }

            while (submissions < requiredSubmissions)
            {
                Batch batch = ChooseBatch();
                int[] destinations = (int[])positions.Clone();
                bool[] activeEmitters = new bool[6];
                foreach (var assignment in batch.Assignments)
                {
                    int first = assignment.Pair * 2;
                    destinations[first] = assignment.Target.Sources[first];
                    destinations[first + 1] = assignment.Target.Sources[first + 1];
                    activeEmitters[first] = true;
                    activeEmitters[first + 1] = true;
                    pairUsage[assignment.Pair]++;
                }

                // All selected pairs move together while inactive.
                while (!positions.SequenceEqual(destinations))
                {
                    var instructions = new string[6];
                    for (int emitter = 0; emitter < 6; emitter++)
                    {
                        instructions[emitter] = Move(ref positions[emitter], destinations[emitter]);
                    }
                    writer.WriteLine(string.Join(" ", instructions));
                }

                string toggleInstructions = string.Join(" ", activeEmitters.Select(active => active ? "T" : "-"));
                writer.WriteLine(toggleInstructions);
                for (int emitter = 0; emitter < 6; emitter++)
                {
                    var position = perimeter[positions[emitter]];
                    locations[emitter] = (position.Row, position.Column, emitter + 1, activeEmitters[emitter]);
                }

                Helpers.TraceLasers(matrix, rows, columns, locations);
                matter |= batch.Created;
                if (Helpers.CheckPatternAndRemoveMs(matrix, pattern))
                {
                    submissions++;
                    matter = BigInteger.Zero;
                }
                else if (matter == targetMask)
                {
                    throw new InvalidOperationException("The constructed program did not submit the expected pattern.");
                }

                if (submissions == requiredSubmissions)
                {
                    return;
                }

                // Switch every firing pair off before moving to the next batch.
                writer.WriteLine(toggleInstructions);
            }

            Batch ChooseBatch()
            {
                var candidates = new List<Candidate>[3];
                for (int pair = 0; pair < 3; pair++)
                {
                    int first = pair * 2;
                    candidates[pair] = targets
                        .Where(target => (target.Mask & matter).IsZero)
                        .Select(target => new Candidate(
                            pair,
                            target,
                            Math.Max(
                                Distance(positions[first], target.Sources[first]),
                                Distance(positions[first + 1], target.Sources[first + 1])),
                            rays[first, target.Sources[first]],
                            rays[first + 1, target.Sources[first + 1]]))
                        .OrderBy(candidate => candidate.MovementTicks)
                        .ThenBy(candidate => candidate.Target.Id)
                        .ToList();
                }

                Batch? best = null;
                // Try nearby seeds from every pair so all three pairs can contribute.
                foreach (var seed in candidates.SelectMany(pair => pair.Take(2)))
                {
                    Consider(seed);
                    int[] otherPairs = Enumerable.Range(0, 3).Where(pair => pair != seed.Pair).ToArray();
                    foreach (int pair in otherPairs)
                    {
                        foreach (var second in candidates[pair])
                        {
                            Consider(seed, second);
                        }
                    }

                    // Test three-pair batches together: a third pair can suppress an
                    // unwanted crossing that would make the two-pair batch unsafe.
                    foreach (var second in candidates[otherPairs[0]].Take(8))
                    {
                        foreach (var third in candidates[otherPairs[1]].Take(8))
                        {
                            Consider(seed, second, third);
                        }
                    }
                }

                // A single pair always creates just its target, guaranteeing progress.
                return best ?? throw new InvalidOperationException("No safe deposition was found.");

                void Consider(Candidate first, Candidate? second = null, Candidate? third = null)
                {
                    if (second != null && first.Target.Id == second.Target.Id ||
                        third != null && (first.Target.Id == third.Target.Id || second!.Target.Id == third.Target.Id))
                    {
                        return;
                    }

                    HitCounts hits = default;
                    BigInteger plannedTargets = BigInteger.Zero;
                    int movementTicks = 0;
                    int usage = 0;
                    Add(first);
                    if (second != null) Add(second);
                    if (third != null) Add(third);

                    // Reject erasures, off-pattern matter, and batches creating more
                    // than three cells. Every assigned pair must create its own target.
                    if (!(hits.Many & matter).IsZero)
                    {
                        return;
                    }
                    BigInteger created = hits.Twice & ~matter;
                    if (!(created & ~targetMask).IsZero ||
                        (created & plannedTargets) != plannedTargets)
                    {
                        return;
                    }
                    int createdCount = CountUpToThree(created);
                    if (createdCount == 0 || createdCount > 3)
                    {
                        return;
                    }

                    if (best != null &&
                        (createdCount < best.CreatedCount ||
                         createdCount == best.CreatedCount && movementTicks > best.MovementTicks ||
                         createdCount == best.CreatedCount && movementTicks == best.MovementTicks && usage >= best.PairUsage))
                    {
                        return;
                    }

                    Candidate[] assignments = third != null
                        ? new[] { first, second!, third }
                        : second != null ? new[] { first, second } : new[] { first };
                    best = new Batch(assignments, created, createdCount, movementTicks, usage);

                    void Add(Candidate candidate)
                    {
                        hits = hits.AddRay(candidate.FirstRay).AddRay(candidate.SecondRay);
                        plannedTargets |= candidate.Target.Mask;
                        movementTicks = Math.Max(movementTicks, candidate.MovementTicks);
                        usage += pairUsage[candidate.Pair];
                    }
                }
            }

            bool IsInside(int row, int column) =>
                row >= 0 && row < rows && column >= 0 && column < columns;

            int FindSource(int row, int column, int rowStep, int columnStep)
            {
                while (IsInside(row, column))
                {
                    if (matrix[row, column] == "*")
                    {
                        return perimeterIndices[(row, column)];
                    }
                    row -= rowStep;
                    column -= columnStep;
                }
                throw new InvalidOperationException("Could not find a perimeter source for a target cell.");
            }

            int Distance(int from, int to)
            {
                int difference = Math.Abs(to - from);
                return Math.Min(difference, perimeter.Count - difference);
            }

            string Move(ref int position, int destination)
            {
                if (position == destination)
                {
                    return "-";
                }
                int forward = (destination - position + perimeter.Count) % perimeter.Count;
                int backward = (position - destination + perimeter.Count) % perimeter.Count;
                bool clockwise = forward <= backward;
                position = (position + (clockwise ? 1 : -1) + perimeter.Count) % perimeter.Count;
                return clockwise ? "F" : "B";
            }
        }

        private static int CountUpToThree(BigInteger cells)
        {
            int count = 0;
            while (!cells.IsZero && count <= 3)
            {
                cells &= cells - BigInteger.One;
                count++;
            }
            return count;
        }
    }
}
