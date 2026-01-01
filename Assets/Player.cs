using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    const int framesPerAnimation = 3;
    float timer;
    Vector2 moveInput = new Vector2(0, 0);
    Direction direction;
    InputSystem_Actions actions;

    Rigidbody2D rb;
    SpriteRenderer rendrer;
    InputAction moveAction;

    public int playerNumber;
    public float speed = 2f;
    public Sprite[] sprites;
    public Appearance appearance;
    public float animationFrameTime = 0.15f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rendrer = GetComponent<SpriteRenderer>();
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

        rendrer.sprite = sprites[index];
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
        Black,
        Sick,
        Atomic,
        White,
    }
}
