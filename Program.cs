
internal class Program
{
    private static void Main(string[] args)
    {
        var aids = new HashSet<byte>();

        foreach (var filePath in Directory.GetFiles("/home/fabii/Downloads/AtomicBombers/Worlds"))
        {
            var filename = Path.GetFileName(filePath);
            var bytes = File.ReadAllBytes(filePath);
            var header = ParseHeader(bytes, aids); //20 + 40*10 bytes

            //ÿ = delimiter between levels 357

            var levels = ParseLevels(filename, header, bytes);

            var world = new World(header, levels);

            //357 bytes + ÿ delimiter byte
            
        }

        Console.WriteLine(string.Join(", ", aids.Order().Select(x => x.ToString("X2"))));
    }

    static LevelData[] ParseLevels(string filename, Header header, byte[] bytes)
    {
        var levels = new LevelData[10];
        var levelData = bytes.Skip(20 + 40 * 10).Chunk(358).ToArray();


        for (int i = 0; i < levelData.Length; i++)
        {
            var layout = new Dictionary<string, LevelData.GridElement>();
            var hiddenLayout = new Dictionary<string, LevelData.GridElement>();

            for (int j = 0; j < levelData[i].Length - 1; j++)
            {
                int x = j % 21 + 1;
                int y = 17 - j / 21;

                var (visible, hidden) = ToGridElement(levelData[i][j]);
                
                if (visible is not null)
                    layout[$"{x},{y}"] = visible.Value;

                if (hidden is not null)
                    hiddenLayout[$"{x},{y}"] = hidden.Value;
            }

            var levelMeta = header.LevelMeta[i];
            levels[i] = new LevelData(header.WorldName, levelMeta.Name, "A34134246", "0000861", levelMeta.BlockStyle, levelMeta.Fill, (LevelData.During)levelMeta.TimeOfDay, layout, hiddenLayout);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(levels[i], Newtonsoft.Json.Formatting.Indented);
            var directory = $"/home/fabii/Projects/AtomicBombers/Assets/StreamingAssets/Worlds/{filename}";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText($"{directory}/Level{i}.json", json);
            
        }

        return levels;
    }

    static (LevelData.GridElement? visible, LevelData.GridElement? hidden) ToGridElement(byte b)
    {
        return b switch
        {
            //random tile
            0x20 or 0x01 => (null, null),

            0x00 or 0x58 => (LevelData.GridElement.IndestructibleBlock, null),
            0x02 => (LevelData.GridElement.WalkableBlock, null),
            0x03 => (LevelData.GridElement.SlightlyDamagedBlock, null),
            0x04 => (LevelData.GridElement.MediumDamagedBlock, null),
            0x05 => (LevelData.GridElement.HeavilyDamagedBlock, null),
            0x86 or 0x87 => (LevelData.GridElement.SolidBlock, null),

            //players
            0x34 => (LevelData.GridElement.Player4, null),
            0x33 => (LevelData.GridElement.Player3, null),
            0x32 => (LevelData.GridElement.Player2, null),
            0x31 => (LevelData.GridElement.Player1, null),

            //nothing
            0x56 or 0x4C => (LevelData.GridElement.Empty, null),

            //consumables
            0x08 => (LevelData.GridElement.Megabomb, null),
            0x09 => (LevelData.GridElement.Light, null),
            0x0A => (LevelData.GridElement.Powder, null),
            0x0B => (LevelData.GridElement.Bomb, null),
            0x0C => (LevelData.GridElement.Surprise, null),
            // 0x0D => LevelData.GridElement.DeadPlayer
            0x0E => (LevelData.GridElement.AtomicBomb, null),
            0x0F => (LevelData.GridElement.KeepForce, null),
            0x10 => (LevelData.GridElement.Mine, null),
            0x11 => (LevelData.GridElement.Pacman, null),
            0x12 => (LevelData.GridElement.Ghost, null),
            0x13 => (LevelData.GridElement.Immortal, null),

            0x80 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.IndestructibleBlock),
            0x81 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.SolidBlock),
            0x82 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.WalkableBlock),
            0x83 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.SlightlyDamagedBlock),
            0x84 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.MediumDamagedBlock),
            0x85 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.HeavilyDamagedBlock),
            0x88 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Megabomb),
            0x89 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Light),
            0x8A => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Powder),
            0x8B => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Bomb),
            0x8C => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Surprise),
            // 0x8D => (LevelData.GridElement.SolidBlock, LevelData.GridElement.DeadPlayer),
            0x8E => (LevelData.GridElement.SolidBlock, LevelData.GridElement.AtomicBomb),
            0x8F => (LevelData.GridElement.SolidBlock, LevelData.GridElement.KeepForce),
            0x90 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Mine),
            0x91 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Pacman),
            0x92 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Ghost),
            0x93 => (LevelData.GridElement.SolidBlock, LevelData.GridElement.Immortal),

            _ => throw new NotImplementedException(),
        };
    }

    static string AsString(byte[] bytes) => new string([.. bytes.Select(x => (char)x)]);

    static Header ParseHeader(byte[] bytes, HashSet<byte> aids)
    {
        //20 bytes for world name
        var worldbytes = bytes[0..20];
        var worldName = CutWhen(worldbytes, 0, x => x is 0);

        //40 bytes for level meta. bytes: 0,1,2 = ??, 3 = block style, 4-null = name, rest = ??
        var levelbytes = new byte[10][];
        for (int j = 0; j < 10; j++)
            levelbytes[j] = bytes[(20 + 40 * j)..(20 + 40 * j + 40)];

        var levelMetadata = new LevelMeta[10];
        for (int j = 0; j < levelbytes.Length; j++)
        {
            var name = CutWhen(levelbytes[j], 4, x => x is 0);
            levelMetadata[j] = new LevelMeta(AsString(name), levelbytes[j][3], levelbytes[j][26] is not 0, levelbytes[j][25]);
            aids.Add(levelbytes[j][26]);
        }

        var header = new Header(AsString(worldName), levelMetadata);
        return header;
    }

    static byte[] CutWhen(byte[] input, int start, Func<byte, bool> condition)
    {
        return CutWhen(input, ref start, condition);
    }

    static byte[] CutWhen(byte[] input, ref int start, Func<byte, bool> condition, bool skipEnd = true)
    {
        for (int i = start; i < input.Length; i++)
        {
            if (condition(input[i]))
            {
                var result = input[start..i];
                start = skipEnd ? i + 1 : i;
                return result;
            }
        }

        start = input.Length;
        return [];
    }
}

record Header(string WorldName, LevelMeta[] LevelMeta);
record LevelMeta(string Name, int BlockStyle, bool Fill, int TimeOfDay);
record World(Header Header, LevelData[] Levels);