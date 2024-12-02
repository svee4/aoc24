using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

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
}

Console.WriteLine($"Part 1: {part1 == 421}");

var part1simd = Part1Simd(input);
Console.WriteLine($"Part 1 simd: {part1 == 421}");

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

	samples.Add((report.ToArray(), safe));
}

if (part2 == 476)
{
	Console.WriteLine("Part2 ok");
}
else
{
	Console.WriteLine($"Part2 expected: 476 actual: {part2}");
}

Console.WriteLine("expect: 476 actual: " + Part2Simd(input));

var sampleCount = samples.Count;
var sampleResults = new bool[sampleCount];

foreach (var (index, sample) in samples.Take(sampleCount).Index())
{

	List<string> log = [];
	var logger = log.Add;

	#region impl

	var result = false;

	if (IsSafeReportSpanParallelDebug(sample.Input, -1, logger))
	{
		result = true;
		continue;
	}

	for (var i = 0; i < sample.Input.Length; i++)
	{
		var report = sample.Input.ToArray();
		if (IsSafeReportSpanParallelDebug(report, i, logger))
		{
			result = true;
			break;
		}
	}

	#endregion

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

static int Part1Simd(string input)
{
	var total = 0;
	foreach (var line in input.AsSpan().EnumerateLines())
	{
		var report = line.ToString().Split(' ').Select(int.Parse).ToArray();

		if (IsSafeReportSpanParallelOptimized(report, -1))
		{
			total++;
		}

	}

	return total;
}

static int Part2Simd(string input)
{
	var total = 0;
	var report = (Span<int>)stackalloc int[8];

	foreach (var line in input.AsSpan().EnumerateLines())
	{
		report.Clear();

		foreach (var (index, level) in
			line.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Index())
		{
			report[index] = level;
		}

		if (IsSafeReportSpanParallelOptimized(report, -1))
		{
			total++;
			continue;
		}

		for (var i = 0; i < report.Length; i++)
		{
			if (IsSafeReportSpanParallelOptimized(report, i))
			{
				total++;
				break;
			}
		}
	}

	return total;
}

static bool IsSafeReportSpanParallelOptimized(ReadOnlySpan<int> reportInput, int removeIndex)
{
	var reportSpan = (Span<int>)stackalloc int[8];

	if (removeIndex > -1)
	{
		reportInput[..removeIndex].CopyTo(reportSpan);
		reportInput[(removeIndex + 1)..].CopyTo(reportSpan[removeIndex..]);
	}
	else
	{
		reportInput.CopyTo(reportSpan);
	}

	var report = Vector256.Create<int>(reportSpan);
	var shiftedOneRight = Vector256.Shuffle(report, Vector256.Create([-1, 0, 1, 2, 3, 4, 5, 6, 7]));
	var diffs = Vector256.Subtract(shiftedOneRight, report);

	var conditionalSelectVector = Vector256.Negate(Vector256.Min(report, Vector256<int>.One));
	conditionalSelectVector = conditionalSelectVector.WithElement(0, 0);

	var ranges = Vector256.ConditionalSelect(conditionalSelectVector, Vector256.Abs(diffs), Vector256<int>.One);
	if (Vector256.LessThanAny(ranges, Vector256.Create(1))
		|| Vector256.GreaterThanAny(ranges, Vector256.Create(3)))
	{
		return false;
	}

	var diffsPatched = Vector256.ConditionalSelect(conditionalSelectVector, diffs, Vector256.Create(diffs[1]));
	var highBits = (byte)Vector256.ExtractMostSignificantBits(diffsPatched);

	if (highBits is not (byte.MinValue or byte.MaxValue))
	{
		return false;
	}

	return true;
}

static bool IsSafeReportSpanParallelDebug(ReadOnlySpan<int> reportInput, int removeIndex, Action<string> logger)
{
	// assumes that the maximum length of one report is 8 levels
	// assumes that there is no 0 or negative levels

	var reportSpan = (Span<int>)stackalloc int[8];

	if (removeIndex > -1)
	{
		reportInput[..removeIndex].CopyTo(reportSpan);
		reportInput[(removeIndex + 1)..].CopyTo(reportSpan[removeIndex..]);
	}
	else
	{
		reportInput.CopyTo(reportSpan);
	}

	logger($"removeindex: {removeIndex}");

	var report = Vector256.Create<int>(reportSpan);
	LogVec(report);

	var shiftVector = Vector256.Create([-1, 0, 1, 2, 3, 4, 5, 6, 7]);

	var shiftedOneRight = Vector256.Shuffle(report, shiftVector);
	LogVec(shiftedOneRight, "shifted");

	var diffs = Vector256.Subtract(shiftedOneRight, report);
	LogVec(diffs);

	var conditionalSelectVector = Vector256.Negate(Vector256.Min(report, Vector256<int>.One));
	conditionalSelectVector = conditionalSelectVector.WithElement(0, 0);

	LogVec(conditionalSelectVector, "condsel");

	var ranges = Vector256.ConditionalSelect(conditionalSelectVector, Vector256.Abs(diffs), Vector256<int>.One);
	LogVec(ranges);

	if (Vector256.LessThanAny(ranges, Vector256.Create(1))
		|| Vector256.GreaterThanAny(ranges, Vector256.Create(3)))
	{
		logger("Fail: range");
		return false;
	}

	var diffsPatched = Vector256.ConditionalSelect(
		conditionalSelectVector,
		diffs,
		Vector256.Create(diffs[1]));

	LogVec(diffsPatched, "highbits");
	var highBits = (byte)Vector256.ExtractMostSignificantBits(diffsPatched);

	if (highBits is not (byte.MinValue or byte.MaxValue))
	{
		logger("Fail: highbits");
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
