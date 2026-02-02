using System.Linq;
using UnityEngine;

public class Mine : ExplosiveBase
{
    public Sprite[] sprites;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        int index = (int)LevelManager.LightLevel;
        sr.sprite = sprites[index];
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out var player))
            Explode(player.Strength, false);
    }
}
