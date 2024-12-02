const string TestInput = """
7 6 4 2 1
1 2 7 8 9
9 7 6 2 1
1 3 2 4 5
8 6 4 4 1
1 3 6 7 9
""";

var input = TestInput;
input = File.ReadAllText("input.txt");

var part1 = 0;

foreach (var line in input.Split(Environment.NewLine))
{
    List<int> levels = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToList();

    var prev = levels.First();
    var safe = true;
    var isIncreasing = levels[0] < levels[1];

    foreach (var level in levels.Skip(1))
    {
        var diff = level - prev;

        if (Math.Abs(diff) is < 1 or > 3)
        {
            safe = false;
            break;
        }

        if (isIncreasing)
        {
            if (diff < 0)
            {
                safe = false;
                break;
            }
        }
        else if (diff > 0)
        {
            safe = false;
            break;
        }

        prev = level;
    }

    if (safe)
    {
        Console.WriteLine($"{string.Join(", ", levels)}: SAFE");
        part1++;
    }
    else
    {
        Console.WriteLine($"{string.Join(", ", levels)}: UNSAFE");
    }
}

Console.WriteLine($"Part 1: {part1}");


var part2 = 0;

foreach (var line in input.Split(Environment.NewLine))
{
    var levels = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToList();

    var safe = IsSafeReport(levels);
    for (int i = 0; i < levels.Count && !safe; i++)
    {
        var temp = levels.ToList();
        temp.RemoveAt(i);
        safe = IsSafeReport(temp);
    }

    if (safe)
    {
        part2++;
    }
}

Console.WriteLine($"Part 2: {part2}");

static bool IsSafeReport(List<int> report)
{
    var index = -1;
    var prev = report[0];
    var isIncreasing = report[0] < report[1];

    for (var i = 1; i < report.Count; i++)
    {
        var level = report[i];
        var diff = level - prev;

        if (Math.Abs(diff) is < 1 or > 3)
        {
            index = i;
            break;
        }

        if (isIncreasing)
        {
            if (diff < 0)
            {
                index = i;
                break;
            }
        }
        else if (diff > 0)
        {
            index = i;
            break;
        }

        prev = level;
    }

    return index == -1;
}