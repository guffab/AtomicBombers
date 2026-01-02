using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Explosion : MonoBehaviour
{
    public int Strength;

    void Start()
    {
        var pos = GridSystem.Current.GetPosition(gameObject);
        if (GridSystem.Current.IsOccupied(pos, out var elements))
        {
            foreach (var element in elements.ToList())
            {
                if (element.TryGetComponent<Explosion>(out _))
                    continue;
                
                if (element.TryGetComponent<Mine>(out var mine))
                    mine.Explode(Strength);
                
                else if (element.TryGetComponent<Player>(out var player))
                    player.Kill();

                else if (element.TryGetComponent<Block>(out var block))
                    block.Demolish();

                // else if (element.TryGetComponent<Consumable>(out var consumable))
                //     consumable.Remove();
                else
                    Debug.Log($"Unhandled explosion on {element.name}");
            }
        }
    }

    public void Hide()
    {
        GetComponent<Renderer>().enabled = false;
        GridSystem.Current.Remove(gameObject);
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }
}
