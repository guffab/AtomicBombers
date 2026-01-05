using System.Linq;
using UnityEngine;

public class Mine : ExplosiveBase
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out var player))
            Explode(player.Strength);
    }
}
