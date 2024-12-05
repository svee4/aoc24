
#pragma warning disable CA1305 // Specify IFormatProvider

using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

var input = """
47|53
97|13
97|61
97|47
75|29
61|13
75|53
29|13
97|29
53|29
61|53
97|53
61|29
47|13
75|47
97|75
47|61
75|61
47|29
75|13
53|13

75,47,61,53,29
97,61,53,29,13
75,29,13
75,97,47,61,53
61,13,29
97,13,75,29,47
""";
input = File.ReadAllText("input.txt");
input = input.ReplaceLineEndings("\n");

var (ruleLines, updateLines) = input.Split("\n\n").FirstTwo();

var rulemap = (Dictionary<int, List<int>>)[];

foreach (var ruleLine in ruleLines.Split("\n"))
{
	var matches = Regex.Matches(ruleLine, @"(\d+)|(\d+)");
	var key = int.Parse(matches[0].Value);
	ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(rulemap, key, out var exists);
	list ??= [];
	list.Add(int.Parse(matches[1].Value));
}

var part1 = 0;
//var part2 = 0;

foreach (var line in updateLines.Split("\n"))
{
	var pages = line.Split(",", StringSplitOptions.RemoveEmptyEntries)
		.Select(int.Parse)
		.ToArray();

	var valid = true;
	foreach (var (index, page) in pages.Index().TakeWhile((_, _) => valid))
	{
		var rules = rulemap[page];

		foreach (var pageBefore in pages[..index])
		{
			if (rules.Contains(pageBefore))
			{
				valid = false;
				break;
			}
		}
	}

	if (valid)
	{
		part1 += pages[pages.Length / 2];
	}
}

Console.WriteLine($"Part1: {part1}");
//Console.WriteLine($"Part2: {part2}");

return;

static class Extensions
{
	public static (T, T) FirstTwo<T>(this IList<T> source) =>
		(source[0], source[1]);
}
