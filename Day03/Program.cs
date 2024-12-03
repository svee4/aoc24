#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

using System.Text.RegularExpressions;

var input = File.ReadAllText("input.txt");
//input = "xmul(2,4)%&mul[3,7]!@^do_not_mul(5,5)+mul(32,64]then(mul(11,8)mul(8,5))";
//input = "xmul(2,4)&mul[3,7]!^don't()_mul(5,5)+mul(32,64](mul(11,8)undo()?mul(8,5))";

// part 1
{
	var regex = new Regex(@"mul\((\d{1,3}),(\d{1,3})\)");

	var matches = regex.Matches(input);

	var sum = matches
		.Select(match => int.Parse(match.Groups[1].Value) * int.Parse(match.Groups[2].Value))
		.Sum();

	Console.WriteLine($"Part1: {sum}");
}

// part 2
{
	var sum = new Regex(@"(?<mul>mul\((?<n1>\d{1,3}),(?<n2>\d{1,3})\))|(?<dont>don't\(\))|(?<do>do\(\))")
		.Matches(input)
		.Aggregate(
			new Alligator(Sum: 0, Enabled: true),
			(cum, match) =>
				match.Groups["mul"].Success && cum.Enabled
					? cum with { Sum = cum.Sum + int.Parse(match.Groups["n1"].Value) * int.Parse(match.Groups["n2"].Value) }
					: match.Groups["dont"].Success
					? cum with { Enabled = false }
					: match.Groups["do"].Success
					? cum with { Enabled = true }
					: cum)
		.Sum;

	Console.WriteLine($"Part2: {sum}");
}

record Alligator(int Sum, bool Enabled);
