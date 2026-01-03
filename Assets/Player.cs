using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    private static WaitForSeconds shortWait = new WaitForSeconds(.05f);//Unity suggests to cache this
    private static WaitForSeconds longWait = new WaitForSeconds(1.5f);//Unity suggests to cache this
    static readonly System.Random random = new();
    static readonly Appearance[] appearances = new Appearance[] { Appearance.Pacman, Appearance.Immortal, Appearance.Sick, Appearance.Ghost };

    const int framesPerAnimation = 3;
    float timer;
    Vector2Int moveInput = Vector2Int.zero;
    bool moveCanceled = false;
    Vector3 targetWorldPos;
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
    public GameObject ExplodingBombPrefab;
    public GameObject ExplodingAtomicBombPrefab;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        actions = new InputSystem_Actions();
        moveAction = actions.FindAction($"Player{playerNumber}/Move", throwIfNotFound: true);
    }

    void Start()
    {
        targetWorldPos = GridSystem.Current.ToWorld(GridSystem.Current.GetPosition(gameObject));
        StartCoroutine(MoveRoutine());
    }

    // called every frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPos,
            speed * Time.deltaTime
        );

        if (moveInput.magnitude > 0)
        {
            if (Math.Abs(moveInput.x) > Math.Abs(moveInput.y))
                direction = moveInput.x > 0 ? Direction.Right : Direction.Left;
            else
                direction = moveInput.y > 0 ? Direction.Up : Direction.Down;

            timer += Time.deltaTime;
            SetCurrentSprite();
        }
    }

    void OnEnable()
    {
        moveAction.Enable();
        moveAction.performed += ctx =>
        {
            moveCanceled = false;
            var rawInput = ctx.ReadValue<Vector2>();

            if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
                moveInput = new Vector2Int(Math.Sign(rawInput.x), 0);
            else
                moveInput = new Vector2Int(0, Math.Sign(rawInput.y));
        };
        moveAction.canceled += ctx => moveCanceled = true;
    }

    void OnDisable()
    {
        moveAction.Disable();
        var playerStruct = actions.GetType().GetProperty("Player" + playerNumber).GetValue(actions);
        playerStruct.GetType().GetMethod("Disable").Invoke(playerStruct, new object[0]);
    }

    IEnumerator MoveRoutine()
    {
        var grid = GridSystem.Current;

        while (true)
        {
            yield return shortWait;
            if (moveInput.magnitude > 0 && grid.TryMove(this, moveInput))
            {
                targetWorldPos = grid.ToWorld(grid.GetPosition(gameObject));

                if (moveCanceled)
                    moveInput = Vector2Int.zero;
                else
                    yield return longWait;
            }
        }
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
        var position = grid.Remove(gameObject);
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
    }

    internal void Consume(Consumable.Kind type)
    {
        if (type is Consumable.Kind.Light)
        {
            //turn on light
            Debug.Log("Light turned on");
        }

        else if (type is Consumable.Kind.KeepForce)
            KeepForce = true;

        else if (type is Consumable.Kind.Powder)
            Strength = Math.Min(Strength + 1, 10);

        else if (type is Consumable.Kind.Bomb)
            Bombs = Math.Min(Bombs + 1, 999_999);

        else if (type is Consumable.Kind.Atomicbomb)
            AtomicBombs = Math.Min(AtomicBombs + 1, 999_999);

        else if (type is Consumable.Kind.Megabomb)
        {
            Strength = 10;
            AtomicBombs = 999_999;
        }

        else if (type is Consumable.Kind.Pacman)
        {
            appearance = SelectWorst(Appearance.Pacman);
            SetCurrentSprite();
        }

        else if (type is Consumable.Kind.Immortal)
        {
            appearance = SelectWorst(Appearance.Immortal);
            SetCurrentSprite();
        }

        else if (type is Consumable.Kind.Ghost)
        {
            appearance = SelectWorst(Appearance.Ghost);
            SetCurrentSprite();
        }

        else if (type is Consumable.Kind.Surprise)
        {
            appearance = SelectWorst(appearances[random.Next(appearances.Length)]);
            SetCurrentSprite();
        }

        Appearance SelectWorst(Appearance newAppearance)
        {
            if (appearance is Appearance.Pacman)
                return Appearance.Pacman;

            if (appearance is Appearance.Immortal or Appearance.Ghost)
                return newAppearance is Appearance.Sick ? newAppearance : appearance;

            return newAppearance;
        }
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
