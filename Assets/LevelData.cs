using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

/// <summary>
/// Defines how a level is structured.
/// </summary>
/// <param name="Consumables">Each 0 or 1 entry defines if a consumable is available or not. TODO: allow 0-9 to define likeliness of each item?</param>
/// <param name="BlockType">Controls the visual appearance of the level.</param>
/// <param name="Fill">Wether or not the level grid should be randomly filled with blocks for grid points not defined in the <see cref="Layout"/>.</param>
/// <param name="Layout">A coordinate+item pair. Example: { "1,1": "Player1", "17,3": "Explosive" }</param>
/// <param name="HiddenItems">A coordinate+item pair for items that are only revealed after exploding a block.</param>
public record LevelData(long ItemProbabilities, int BlockType, bool Fill, Dictionary<string, LevelData.GridElement> Layout, Dictionary<string, LevelData.GridElement> HiddenItems)
{
    static readonly System.Random random = new();
    List<GridElement> AvailableItems;

    //slight bias towards certain blocks
    static readonly GridElement[] BiasedBlocks = new GridElement[]
    {
        GridElement.WalkableBlock, GridElement.Empty, GridElement.SolidBlock, GridElement.WalkableBlock, GridElement.SolidBlock, GridElement.WalkableBlock,
        GridElement.SolidBlock, GridElement.SolidBlock, GridElement.WalkableBlock, GridElement.SolidBlock, GridElement.WalkableBlock, GridElement.SolidBlock,
    };

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
            return BiasedBlocks[random.Next(BiasedBlocks.Length)];

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
        var regularItems = new GridElement[] { GridElement.Mine, GridElement.Empty, GridElement.Light, GridElement.KeepForce, GridElement.Powder, GridElement.Bomb, GridElement.Pacman, GridElement.Surprise };
        var availableItems = new List<GridElement>();

        //treat each entry as a probability
        var c = ItemProbabilities.ToString();
        for (int i = 0; i < Math.Min(c.Length, regularItems.Length); i++)
        {
            var multiplier = int.Parse(c[i].ToString());
            availableItems.AddRange(Enumerable.Repeat(regularItems[i], multiplier));
        }

        return availableItems;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GridElement
    {
        //blocks
        PersistentBlock = -9,
        DamagedBlock = -8,
        SolidBlock = -7,
        WalkableBlock = -6,
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
}