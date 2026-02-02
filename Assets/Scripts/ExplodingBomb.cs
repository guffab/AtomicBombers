using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplodingBomb : ExplosiveBase
{
    public int strength;
    public float delay;
    public Kind kind;
    public Sprite[] sprites;

    internal Player player;

    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(ExecuteAfterWait(delay));
    }

    void Update()
    {
        int lightOffset = (int)LevelManager.LightLevel * (sprites.Length / 3);
        int index = (int)kind + lightOffset;
        
        sr.sprite = sprites[index];
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
