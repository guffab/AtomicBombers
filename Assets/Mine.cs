using System.Linq;
using UnityEngine;

public class Mine : ExplosiveBase
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Explode(1);
    }
}
