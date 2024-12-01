const string TestInput = """
3   4
4   3
2   5
1   3
3   9
3   3
""";

List<int> left = [];
List<int> right = [];

var input = File.ReadAllText("input.txt");
//var input = TestInput;

foreach (var line in input.Split(Environment.NewLine))
{
    var splits = line.Split(' ');
    left.Add(int.Parse(splits.First()));
    right.Add(int.Parse(splits.Last()));
}

left.Sort();
right.Sort();

var total = left.Zip(right).Select(tuple => Math.Max(tuple.First, tuple.Second) - Math.Min(tuple.First, tuple.Second)).Sum();
Console.WriteLine($"Part 1: {total}");

var occurrences = right.GroupBy(v => v).ToDictionary(g => g.Key, g => g.Count());

var part2 = left.Select(i => i * occurrences.GetValueOrDefault(i, 0)).Sum();

Console.WriteLine($"Part 2: {part2}");

