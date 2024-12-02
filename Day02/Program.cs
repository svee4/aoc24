using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Text;

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
List<(int[] Input, bool Expected)> samples = [];

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

    samples.Add((levels.ToArray(), safe));
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

int SampleCount = samples.Count;
var sampleResults = new bool[SampleCount];
foreach (var (index, sample) in samples.Take(SampleCount).Index())
{

    List<string> log = [];
    var logger = log.Add;

    var result = IsSafeReportSpanParallel(sample.Input, logger);
    if (sample.Expected != result)
    {
        Console.WriteLine();
        Console.WriteLine($"Expected: {sample.Expected}, actual: {result}");
        Console.WriteLine(string.Join("\n", log));
    }

    sampleResults[index] = sample.Expected == result;
}

Console.WriteLine();
//Console.WriteLine($"results:\n\n{string.Join("\n", sampleResults)}");

static bool IsSafeReportSpanParallel(ReadOnlySpan<int> reportInput, Action<string> logger)
{
    // assumes that the maximum length of one report is 8 levels
    // assumes that there is no 0 or negative levels

    var length = reportInput.Length;

    var reportSpan = (Span<int>)stackalloc int[8];
    reportInput.CopyTo(reportSpan);

    var report = Vector256.Create<int>(reportSpan);
    var shiftedOneRight = Vector256.Shuffle(report, Vector256.Create([-1, 0, 1, 2, 3, 4, 5, 6, 7]));

    var conditionalSelectVector = Vector256.Negate(Vector256.Min(report, Vector256<int>.One));
    conditionalSelectVector = conditionalSelectVector.WithElement(0, 0);

    LogVec(conditionalSelectVector, "condsel");
    LogVec(report);
    LogVec(shiftedOneRight, "shifted");

    var diffs = Vector256.Subtract(shiftedOneRight, report);
    LogVec(diffs);

    // if any in Abs(diffs) is < 1 or > 3 its invalid

    var abs = Vector256.Abs(diffs);
    LogVec(abs, "absbfr");
    LogVec(conditionalSelectVector, "condsel");

    abs = Vector256.ConditionalSelect(conditionalSelectVector, abs, Vector256<int>.One);
    LogVec(abs);

    if (Vector256.LessThanAny(abs, Vector256.Create(1)))
    {
        logger("False due to less than 1");
        return false;
    }

    if (Vector256.GreaterThanAny(abs, Vector256.Create(3)))
    {
        logger("False due to greater than 3");
        return false;
    }

    // all diffs should be either negative or positive, indicating growth or decline.
    // that means either all high bits are set or all are unset.
    // if any value differs from the others then the direction is changing

    // but first we need to patch this up too
    // the value doesnt matter as long as its a valid diff, ie not at index 0 or index >= length
    var diffsPatched = Vector256.ConditionalSelect(conditionalSelectVector, diffs, Vector256.Create(diffs[1]));

    LogVec(diffsPatched, "diffsp");

    var highBits = (byte)Vector256.ExtractMostSignificantBits(diffsPatched);
    logger($"highBits: {highBits:b8}");

    if (highBits is not (byte.MinValue or byte.MaxValue))
    {
        logger("False due to highBits did not have all or nothing set");
        return false;
    }

    return true;

    void LogVec<T>(Vector256<T> vec, [CallerArgumentExpression(nameof(vec))] string name = "") where T : struct
    {
        logger($"{name,-8}: {Vec2Str(vec)}");
    }

    static string Vec2Str<T>(Vector256<T> vec) where T : struct
    {
        return string.Join(", ",
            VecNumerator(vec).Select(v => $"{v,3}"));

        static IEnumerable<T> VecNumerator(Vector256<T> source)
        {
            for (var i = 0; i < Vector256<T>.Count; i++)
            {
                yield return source[i];
            }
        }
    }
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

[InlineArray(8)]
struct Buffer8<T>
{
    private T _0;
}