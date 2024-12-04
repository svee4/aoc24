
var input = File.ReadAllText("input.txt");
//input = """
//MMMSXXMASM
//MSAMXMSMSA
//AMXSXMAAMM
//MSAMASMSMX
//XMASAMXAMM
//XXAMMXXAMA
//SMSMSASXSS
//SAXAMASAAA
//MAMMMXMMMM
//MXMXAXMASX
//""";

input = input.ReplaceLineEndings("\n");

var x = 0;
var y = 0;
var letters = (List<Letter>)[];

for (var i = 0; i < input.Length; i++)
{
	var c = input[i];
	if (c == '\n')
	{
		x = 0;
		y++;
		continue;
	}

	letters.Add(new Letter(c, x, y));
	x++;
}

var map = letters.ToDictionary(letter => letter.Coord, letter => letter.Character);

Part1();

void Part1()
{
	var part1 = 0;

	foreach (var letter in letters.Where(letter => letter.Character == 'X'))
	{
		// right
		if (IsXMASAuto(map, letter.Coord, Extensions.EastBy))
		{
			part1++;
		}

		// left
		if (IsXMASAuto(map, letter.Coord, Extensions.WestBy))
		{
			part1++;
		}

		// up
		if (IsXMASAuto(map, letter.Coord, Extensions.NorthBy))
		{
			part1++;
		}

		// down
		if (IsXMASAuto(map, letter.Coord, Extensions.SouthBy))
		{
			part1++;
		}

		// up-right diagonal
		if (IsXMASAuto(map, letter.Coord, Extensions.NorthBy, Extensions.EastBy))
		{
			part1++;
		}

		// down-right diagonal
		if (IsXMASAuto(map, letter.Coord, Extensions.SouthBy, Extensions.EastBy))
		{
			part1++;
		}

		// up-left diagonal
		if (IsXMASAuto(map, letter.Coord, Extensions.NorthBy, Extensions.WestBy))
		{
			part1++;
		}

		// down-left diagonal
		if (IsXMASAuto(map, letter.Coord, Extensions.SouthBy, Extensions.WestBy))
		{
			part1++;
		}

		static bool IsXMASAuto(Dictionary<Coord, char> map, Coord start, params Func<Coord, int, Coord>[] byers)
		{
			Coord Byer(int by) => byers.Aggregate(start, (cur, func) => func(cur, by));

			return IsXMAS([
				map.GetValueOrDefault(start),
				map.GetValueOrDefault(Byer(1)),
				map.GetValueOrDefault(Byer(2)),
				map.GetValueOrDefault(Byer(3)),
			]);

			static bool IsXMAS(ReadOnlySpan<char> value) => value is "XMAS";
		}
	}

	Console.WriteLine($"Part1: {part1}");


}

record Letter(char Character, int X, int Y)
{
	public Coord Coord => new(X, Y);
}

readonly record struct Coord(int X, int Y);

static class Extensions
{
	public static Coord NorthBy(this Coord coord, int n) => coord with { Y = coord.Y + n };
	public static Coord SouthBy(this Coord coord, int n) => coord with { Y = coord.Y - n };
	public static Coord EastBy(this Coord coord, int n) => coord with { X = coord.X + n };
	public static Coord WestBy(this Coord coord, int n) => coord with { X = coord.X - n };
}
