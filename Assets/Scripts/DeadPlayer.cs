using System;
using UnityEngine;

public class DeadPlayer : MonoBehaviour
{
    public int Strength;
    public int Bombs;
    public int AtomicBombs;
    public bool KeepForce;
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
        if (other.gameObject.TryGetComponent<Player>(out var player))
        {
            player.Eat(this);
            Remove();
        }
    }
    
    public void Remove()
    {
        Grid.Current.Remove(gameObject);
        Destroy(gameObject);
    }

}
