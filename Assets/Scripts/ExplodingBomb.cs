using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplodingBomb : ExplosiveBase
{
    public int strength;
    public float delay;
    public int byPlayer;

    public Kind kind;
    public Sprite[] sprites;
    public GameObject BombRestoreProxy;

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
        Explode(strength, kind is Kind.Atomic, byPlayer);
    }

    public enum Kind
    {
        Normal,
        Atomic,
    }

    public override void Explode(int strength, bool unstoppable, int byPlayer)
    {
        if (player != null)
        {
            var proxy = Instantiate(BombRestoreProxy);
            proxy.GetComponent<BombRestoreProxy>().player = player;
        }

        base.Explode(strength, kind is Kind.Atomic, byPlayer);
    }
}
