using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    const int framesPerAnimation = 3;
    float timer;
    Vector2 moveInput = new Vector2(0, 0);
    Direction direction;
    InputSystem_Actions actions;

    public int Strength { get; private set; }
    public int Bombs { get; private set; }
    public int AtomicBombs { get; private set; }
    public bool KeepForce { get; private set; }

    Rigidbody2D rb;
    SpriteRenderer sr;
    InputAction moveAction;

    public int playerNumber;
    public float speed = 1f;
    public Sprite[] sprites;
    public Appearance appearance;
    public float animationFrameTime = 0.15f;

    public GameObject DeadPlayerPrefab;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        actions = new InputSystem_Actions();
        moveAction = actions.FindAction($"Player{playerNumber}/Move", throwIfNotFound: true);
    }

    // called every frame
    void Update()
    {
        if (moveInput.magnitude > 0)
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                direction = moveInput.x > 0 ? Direction.Right : Direction.Left;
            else
                direction = moveInput.y > 0 ? Direction.Up : Direction.Down;

            timer += Time.deltaTime;
            SetCurrentSprite();
        }
    }

    // called at fixed intervals (for physics)
    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed;
    }

    void OnEnable()
    {
        moveAction.Enable();
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnDisable()
    {
        moveAction.Disable();
        var playerStruct = actions.GetType().GetProperty("Player" + playerNumber).GetValue(actions);
        playerStruct.GetType().GetMethod("Disable").Invoke(playerStruct, new object[0]);
    }

    void SetCurrentSprite()
    {
        int frame = Mathf.FloorToInt(timer / animationFrameTime) % framesPerAnimation;

        //player + style offset
        int offset = (playerNumber - 1) * 12 + 48 * (int)appearance;
        int index = (int)direction * framesPerAnimation + frame + offset;

        sr.sprite = sprites[index];
    }

    public void Kill()
    {
        var grid = GridSystem.Current;
        var position = grid.GetPosition(gameObject);
        var deadPlayer = Instantiate(DeadPlayerPrefab, grid.ToWorld(position), Quaternion.identity);
        grid.Add(deadPlayer, position);

        Destroy(gameObject);
    }

    internal void Eat(DeadPlayer deadPlayer)
    {
        Strength = Math.Max(Strength, deadPlayer.Strength);
        Bombs = Math.Max(Bombs, deadPlayer.Bombs);
        AtomicBombs = Math.Max(AtomicBombs, deadPlayer.AtomicBombs);
        KeepForce = KeepForce || deadPlayer.KeepForce;
        Destroy(deadPlayer.gameObject);
    }

    public enum Direction
    {
        Right,
        Left,
        Down,
        Up,
    }

    public enum Appearance
    {
        Normal,
        Pacman,
        Immortal,
        Sick,
        Atomic,
        Ghost,
    }
}
