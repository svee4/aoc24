using System.Text.RegularExpressions;

const string TestInput = """
3   4
4   3
2   5
1   3
3   9
3   3
""";

var combined = File.ReadAllLines("input.txt")
    .Select(line => Regex.Matches(line, @"(\d+)\S+(\d+)"))
    .Select(match => new { Left = int.Parse(match[0].Value), Right = int.Parse(match[1].Value) });

var part1 = combined.Select(thing => thing.Left).Order()
    .Zip(combined.Select(thing => thing.Right).Order())
    .Select(tuple => Math.Abs(tuple.First - tuple.Second))
    .Sum();

Console.WriteLine($"Part 1: {part1}");

var occurrences = combined.Select(thing => thing.Right).GroupBy(v => v).ToDictionary(g => g.Key, g => g.Count());
var part2 = combined.Select(thing => thing.Left).Select(i => i * occurrences.GetValueOrDefault(i, 0)).Sum();

Console.WriteLine($"Part 2: {part2}");
