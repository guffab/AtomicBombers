using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

/// <summary>
/// Defines how a level is structured.
/// </summary>
/// <param name="Consumables">Each 0 or 1 entry defines if a consumable is available or not. TODO: allow 0-9 to define likeliness of each item?</param>
/// <param name="BlockStyle">Controls the visual appearance of the level.</param>
/// <param name="Fill">Wether or not the level grid should be randomly filled with blocks for grid points not defined in the <see cref="Layout"/>.</param>
/// <param name="Layout">A coordinate+item pair. Example: { "1,1": "Player1", "17,3": "Explosive" }</param>
/// <param name="HiddenItems">A coordinate+item pair for items that are only revealed after exploding a block.</param>
public record LevelData(string WorldName, string Name, string ItemProbabilities, string BlockProbabilities, int BlockStyle, bool Fill, LevelData.During TimeOfDay, Dictionary<string, LevelData.GridElement> Layout, Dictionary<string, LevelData.GridElement> HiddenItems)
{
    static readonly System.Random random = new();
    List<GridElement> AvailableItems;
    List<GridElement> AvailableBlocks;

    /// <summary>
    /// Call at level start.
    /// </summary>
    public GridElement GetItemAt(Vector2Int v)
    {
        if (Layout.TryGetValue($"{v.x},{v.y}", out var itemType))
            return itemType;

        if (HiddenItems.ContainsKey($"{v.x},{v.y}"))
            return GridElement.SolidBlock;

        if (Fill)
        {
            AvailableBlocks ??= GetAvailableBlocks();
            return AvailableBlocks[random.Next(AvailableBlocks.Count)];
        }

        return GridElement.Empty;
    }

    /// <summary>
    /// Call after a block exploded.
    /// </summary>
    public GridElement GetNewItemAt(Vector2Int v)
    {
        if (HiddenItems.TryGetValue($"{v.x},{v.y}", out var hiddenItem))
            return hiddenItem;

        AvailableItems ??= GetAvailableItems();
        return AvailableItems[random.Next(AvailableItems.Count)];
    }

    private List<GridElement> GetAvailableItems()
    {
        var items = new GridElement[] { GridElement.Empty, GridElement.Mine, GridElement.Light, GridElement.KeepForce, GridElement.Powder,
                                        GridElement.Bomb, GridElement.AtomicBomb, GridElement.Pacman, GridElement.Surprise };
        return ParseProbabilities(ItemProbabilities, items);
    }

    private List<GridElement> GetAvailableBlocks()
    {
        var blocks = new GridElement[] { GridElement.IndestructibleBlock, GridElement.SlightlyDamagedBlock, GridElement.MediumDamagedBlock, GridElement.HeavilyDamagedBlock, GridElement.SolidBlock, GridElement.WalkableBlock, GridElement.Empty };
        return ParseProbabilities(BlockProbabilities, blocks);
    }

    private static List<GridElement> ParseProbabilities(string encodedProbabilities, GridElement[] availableElements)
    {
        var chances = new List<GridElement>();

        //treat each entry as a hex probability
        for (int i = 0; i < Math.Min(encodedProbabilities.Length, availableElements.Length); i++)
        {
            if (int.TryParse(encodedProbabilities[i].ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int multiplier))
                chances.AddRange(Enumerable.Repeat(availableElements[i], multiplier));
        }
        return chances;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GridElement
    {
        //blocks
        IndestructibleBlock = -11,
        SlightlyDamagedBlock,
        MediumDamagedBlock,
        HeavilyDamagedBlock,
        SolidBlock,
        WalkableBlock,

        //explosive
        Mine = -5,

        //players
        Player4 = -4,
        Player3 = -3,
        Player2 = -2,
        Player1 = -1,

        //nothing
        Empty = 0,

        //consumables
        Light,
        KeepForce,
        Powder,
        Bomb,
        AtomicBomb,
        Megabomb,
        Pacman,
        Immortal,
        Ghost,
        Surprise,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum During
    {
        Day = 0,

        Night = 1,

        Dawn = 2,
    }
}