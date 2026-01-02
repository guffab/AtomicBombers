using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem
{
    private Dictionary<Vector2Int, List<GameObject>> GridToObject = new();
    private Dictionary<GameObject, Vector2Int> ObjectToGrid = new();

    [HideInInspector] public static GridSystem Current;
    [HideInInspector] public Vector2Int GridDimensions { get; }

    public GridSystem(Vector2Int gridDimensions)
    {
        Current = this;
        GridDimensions = gridDimensions;
    }

    public void Add(GameObject g, Vector2Int v)
    {
        ObjectToGrid[g] = v;
        if (GridToObject.TryGetValue(v, out var list))
            list.Add(g);
        else
            GridToObject[v] = new List<GameObject> { g };
    }

    public bool IsOccupied(Vector2Int v, out List<GameObject> objects)
    {
        return GridToObject.TryGetValue(v, out objects);
    }

    public Vector2Int GetPosition(GameObject g)
    {
        return ObjectToGrid[g];
    }

    public bool TryMove(Player player, Vector2Int direction)
    {
        var newPos = GetPosition(player.gameObject) + direction;
        if (CanMove(player))
        {
            Remove(player.gameObject);
            Add(player.gameObject, newPos);
            return true;
        }
        return false;

        bool CanMove(Player player)
        {
            if (!IsOccupied(newPos, out var collisions))
                return true;

            return collisions.All(x => !x.TryGetComponent<ExplodingBomb>(out _)) &&
                   collisions.All(x => !x.TryGetComponent<Block>(out var block) || block.CurrentState is Block.State.Walkable);
        }
    }

    public void Remove(GameObject g)
    {
        var v = ObjectToGrid[g];
        if (GridToObject.TryGetValue(v, out var list) && list.Count > 1)
            list.Remove(g);
        else
            GridToObject.Remove(v);

        ObjectToGrid.Remove(g);
    }

    public Vector3 ToWorld(Vector2Int v)
    {
        var screenPos = new Vector3((float)(v.x * 28), (float)(v.y * 28), 10);
        var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.y -= .12f; //not sure why all coordinates are wrong by this factor
        return worldPos;
    }
}
