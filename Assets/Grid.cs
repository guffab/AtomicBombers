using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid
{
    private Dictionary<Vector2Int, List<GameObject>> GridToObject = new();
    private Dictionary<GameObject, Vector2Int> ObjectToGrid = new();

    [HideInInspector] public static Grid Current;
    [HideInInspector] public Vector2Int Dimensions { get; }
    [HideInInspector] public Vector2Int CellSize { get; }

    static Vector3 centerOffset;
    static float scale;

    public Grid(Vector2Int cellSize)
    {
        Current = this;
        CellSize = cellSize;
        Dimensions = new Vector2Int(MaximizeGrid(cellSize.x, 640), MaximizeGrid(cellSize.y, 480));

        if (scale is 0) //not initialized
        {
            scale = Camera.main.pixelHeight / 480;

            var gridMax = Camera.main.ScreenToWorldPoint(ToScreen(Dimensions));
            var screenMax = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 10));
            centerOffset = (screenMax - gridMax) / 2;
        }

        static int MaximizeGrid(int cellSizeDim, int maxScreenSize)
        {
            int dim = 0;

            while (dim + cellSizeDim < maxScreenSize)
                dim += cellSizeDim;

            //for some reason we want an odd number of grid points
            if ((dim / cellSizeDim) % 2 == 0)
                dim -= cellSizeDim;

            return dim / cellSizeDim;
        }
    }

    public void Add(GameObject g, Vector2Int v)
    {
        v = Wrap(v);
        ObjectToGrid[g] = v;
        if (GridToObject.TryGetValue(v, out var list))
            list.Add(g);
        else
            GridToObject[v] = new List<GameObject> { g };
    }

    public List<GameObject> GetObjects(Vector2Int v)
    {
        if (GridToObject.TryGetValue(Wrap(v), out var objects))
            return objects;

        return new();
    }

    public Vector2Int GetPosition(GameObject g)
    {
        return ObjectToGrid[g];
    }

    public List<GameObject> GetObjectsAtSamePlace(GameObject g)
    {
        if (GridToObject.TryGetValue(ObjectToGrid[g], out var allObjects))
            return allObjects.Where(x => x != g).ToList();
        else
            return new();
    }

    public bool TryMove(Player player, Vector2Int direction)
    {
        var newPos = Wrap(GetPosition(player.gameObject) + direction);
        if (CanMove(player))
        {
            Remove(player.gameObject);
            Add(player.gameObject, newPos);
            return true;
        }
        return false;

        bool CanMove(Player player)
        {
            var collisions = GetObjects(newPos);
            if (collisions.Count is 0)
                return true;

            return collisions.All(x => !x.TryGetComponent<ExplodingBomb>(out _)) &&
                   collisions.All(x => !x.TryGetComponent<Block>(out var block) ||
                   block.state is Block.State.Walkable || (player.appearance is Player.Appearance.Ghost && block.state is Block.State.Solid));
        }
    }

    public Vector2Int Remove(GameObject g)
    {
        var pos = ObjectToGrid[g];
        if (GridToObject.TryGetValue(pos, out var list))
            list.Remove(g);

        ObjectToGrid.Remove(g);
        return pos;
    }

    public Vector3 ToWorld(Vector2Int v, bool raw = false)
    {
        if (!raw)
            v = Wrap(v);

        return Camera.main.ScreenToWorldPoint(ToScreen(v)) + centerOffset;
    }

    private Vector3 ToScreen(Vector2Int local)
    {
        local -= new Vector2Int(1, 1);
        return new Vector3(local.x * CellSize.x * scale, local.y * CellSize.y * scale, 10);
    }

    private Vector2Int Wrap(Vector2Int raw)
    {
        return new Vector2Int((((raw.x - 1) % Dimensions.x) + Dimensions.x) % Dimensions.x + 1,
                              (((raw.y - 1) % Dimensions.y) + Dimensions.y) % Dimensions.y + 1);
    }
}
