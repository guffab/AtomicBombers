using System.Linq;
using UnityEngine;

public abstract class ExplosiveBase : MonoBehaviour
{
    public GameObject ExplosionPrefab;

    public virtual void Explode(int strength, bool unstoppable, int byPlayer)
    {
        var grid = Grid.Current;
        var currentPos = grid.Remove(gameObject);

        //spawn explosions unless already present
        TryInstantiateExplosion(grid, currentPos, strength, unstoppable, byPlayer);

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.up * i), strength, unstoppable, byPlayer))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.down * i), strength, unstoppable, byPlayer))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.left * i), strength, unstoppable, byPlayer))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.right * i), strength, unstoppable, byPlayer))
                break;

        Destroy(gameObject);
    }

    private bool TryInstantiateExplosion(Grid grid, Vector2Int position, int strength, bool unstoppable, int byPlayer)
    {
        var objects = grid.GetObjects(position);
        bool isExploding = objects.Any(x => x.TryGetComponent<Explosion>(out _));

        if (!objects.Have<Explosion>()) //prevent doing more damage
        {
            var explosionObject = Instantiate(ExplosionPrefab, grid.ToWorld(position), Quaternion.identity);
            grid.Add(explosionObject, position);

            var explosion = explosionObject.GetComponent<Explosion>();
            explosion.Strength = strength;
            explosion.Unstoppable = unstoppable;
            explosion.ByPlayer = byPlayer;
        }

        if (objects.Have<Block>(out var block))
            return unstoppable && block.state is Block.State.Solid or Block.State.Walkable;// or Block.State.HeavyDamaged;

        if (unstoppable)
            return true;

        return (!objects.Have<Player>(out var player) || player.appearance is Player.Appearance.Immortal) && !objects.Have<Consumable>() && !objects.Have<DeadPlayer>();
    }
}
