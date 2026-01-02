using System.Linq;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public GameObject ExplosionPrefab;

    void OnTriggerEnter2D(Collider2D other)
    {
        Explode(1);
    }

    public void Explode(int strength)
    {
        var grid = GridSystem.Current;
        var currentPos = grid.GetPosition(gameObject);
        grid.Remove(gameObject);

        //spawn explosions unless already present
        InstantiateExplosion(grid, currentPos, strength);
        for (int i = 1; i <= strength; i++)
        {
            InstantiateExplosion(grid, currentPos + (Vector2Int.up * i), strength);
            InstantiateExplosion(grid, currentPos + (Vector2Int.down * i), strength);
            InstantiateExplosion(grid, currentPos + (Vector2Int.left * i), strength);
            InstantiateExplosion(grid, currentPos + (Vector2Int.right * i), strength);
        }
        Destroy(gameObject);

        void InstantiateExplosion(GridSystem grid, Vector2Int position, int strength)
        {
            if ((uint)position.x > grid.GridDimensions.x || (uint)position.y > grid.GridDimensions.y)
                return;

            if (grid.IsOccupied(position, out var objects) && objects.Any(x => x.TryGetComponent<Explosion>(out _)))
                return;
            
            var explosion = Instantiate(ExplosionPrefab, grid.ToWorld(position), Quaternion.identity);
            explosion.GetComponent<Explosion>().Strength = strength;

            grid.Add(explosion, position);
        }
    }
}
