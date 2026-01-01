using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Defines how a level is structured.
/// </summary>
/// <param name="Consumables">Each 0 or 1 entry defines if a consumable is available or not. TODO: allow 0-9 to define likeliness of each item?</param>
/// <param name="BlockType">Controls the visual appearance of the level.</param>
/// <param name="Fill">Wether or not the level grid should be randomly filled with blocks for grid points not defined in the <see cref="Layout"/>.</param>
/// <param name="Layout">A coordinate+item pair. Example: { "1,1": "Player1", "17,3": "Explosive" }</param>
public record LevelData(long Consumables, int BlockType, bool Fill, Dictionary<string, LevelData.GridElement> Layout)
{
    static readonly System.Random random = new();

    //slight bias towards certain blocks
    static readonly GridElement[] BiasedBlocks = new GridElement[]
    {GridElement.Walkable, GridElement.Empty, GridElement.Solid, GridElement.Walkable, GridElement.Solid, GridElement.Walkable, GridElement.Solid, GridElement.Walkable, GridElement.Walkable};

    public GridElement GetItemAtCoordinate(int x, int y)
    {
        if (Layout.TryGetValue($"{x},{y}", out var itemType))
            return itemType;

        if (Fill)
            return BiasedBlocks[random.Next(BiasedBlocks.Length)];

        return GridElement.Empty;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GridElement
    {
        Player4 = -4,
        Player3 = -3,
        Player2 = -2,
        Player1 = -1,
        Empty = 0,
        Explosive,
        Persistent,
        Solid,
        Walkable,
        PacMan,
        Disease,
    }
}