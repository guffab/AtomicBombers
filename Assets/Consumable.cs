using UnityEngine;

public class Consumable : MonoBehaviour
{
    public Sprite LightSprite;
    public Sprite KeepForceSprite;
    public Sprite PowderSprite;
    public Sprite BombSprite;
    public Sprite AtomicbombSprite;
    public Sprite MegabombSprite;
    public Sprite PacmanSprite;
    public Sprite GhostSprite;
    public Sprite ImmortalSprite;
    public Sprite SurpriseSprite;
    public Kind Type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum Kind
    {
        Light,
        KeepForce,
        Powder,
        Bomb,
        Atomicbomb,
        Megabomb,
        Pacman,
        Immortal,
        Ghost,
        Surprise,
    }
}
