using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplodingBomb : ExplosiveBase
{
    public int strength;
    public float delay;
    public Kind kind;
    internal Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ExecuteAfterWait(delay));
    }

    IEnumerator ExecuteAfterWait(float duration)
    {
        yield return new WaitForSeconds(duration);
        Explode(strength, kind is Kind.Atomic);
    }

    public enum Kind
    {
        Normal,
        Atomic,
    }

    public override void Explode(int strength, bool unstoppable)
    {
        if (player != null) player.availableBombs++;
        base.Explode(strength, kind is Kind.Atomic);
    }
}

public abstract class ExplosiveBase : MonoBehaviour
{
    public GameObject ExplosionPrefab;

    public virtual void Explode(int strength, bool unstoppable)
    {
        var grid = Grid.Current;
        var currentPos = grid.Remove(gameObject);

        //spawn explosions unless already present
        TryInstantiateExplosion(grid, currentPos, strength, unstoppable);

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.up * i), strength, unstoppable))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.down * i), strength, unstoppable))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.left * i), strength, unstoppable))
                break;

        for (int i = 1; i <= strength; i++)
            if (!TryInstantiateExplosion(grid, currentPos + (Vector2Int.right * i), strength, unstoppable))
                break;

        Destroy(gameObject);
    }

    private bool TryInstantiateExplosion(Grid grid, Vector2Int position, int strength, bool unstoppable)
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
        }

        if (objects.Have<Block>(out var block))
            return unstoppable && block.state is Block.State.Solid or Block.State.Walkable;// or Block.State.HeavyDamaged;

        if (unstoppable)
            return true;
        
        return !objects.Have<Player>() && !objects.Have<Consumable>() && !objects.Have<DeadPlayer>();
    }
}