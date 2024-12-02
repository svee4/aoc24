using System.Reflection.Metadata.Ecma335;

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
        //Console.WriteLine($"{string.Join(", ", levels)}: SAFE");
        part1++;
    }
    else
    {
        //Console.WriteLine($"{string.Join(", ", levels)}: UNSAFE");
    }
}

Console.WriteLine($"Part 1: {part1 == 421}");


var part2 = 0;

foreach (var line in input.Split(Environment.NewLine))
{
    var report = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToArray();

    var report2 = report.ToList();

    var safe = IsSafeReportSpan(report);
    var safe2 = IsSafeReport(report2);

    if (safe != safe2)
    {
        Console.WriteLine($"EXPECTED: {safe2}, ACTUAL: {safe} FOR INPUT {string.Join(", ", report)}");
    }

    for (var i = 0; !safe && i < report.Length; i++)
    {
        safe = IsSafeReportSpan(report, removeIndex: i);

        var report22 = report.ToList();
        report22.RemoveAt(i);
        safe2 = IsSafeReport(report22);

        if (safe != safe2)
        {
            Console.WriteLine($"EXPECTED: {safe2}, ACTUAL: {safe} FOR INPUT {string.Join(", ", report)} WITH REMOVEINDEX {i}");
        }
    }

    if (safe)
    {
        part2++;
    }
}

if (part2 == 476)
{
    Console.WriteLine("Part2 ok");
}
else
{
    Console.WriteLine($"Part2 expected: 476 actual: {part2}");
}

static bool IsSafeReportSpan(ReadOnlySpan<int> report, int removeIndex = -1)
{
    var prev = removeIndex is 0 ? report[1] : report[0];

    var isIncreasing = removeIndex switch
    {
        0 => report[1] < report[2],
        1 => report[0] < report[2],
        _ => report[0] < report[1],
    };

    for (var i = removeIndex is 0 ? 2 : 1; i < report.Length; i++)
    {
        if (i == removeIndex)
        {
            continue;
        }

        var cur = report[i];
        var diff = cur - prev;
        prev = cur;

        if (Math.Abs(diff) is < 1 or > 3)
        {
            return false;
        }

        if (isIncreasing)
        {
            if (diff < 0)
            {
                return false;
            }
        }
        else if (diff > 0)
        {
            return false;
        }
    }

    return true;
}

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