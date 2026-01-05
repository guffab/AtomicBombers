using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    static readonly System.Random random = new();
    static readonly Appearance[] appearances = new Appearance[] { Appearance.Pacman, Appearance.Immortal, Appearance.Sick, Appearance.Ghost };

    const int framesPerAnimation = 3;
    float timer;
    Vector2Int moveInput = Vector2Int.zero;
    bool plantBomb = false;

    const float moveFast = 3.5f;
    const float moveSlow = 1f;

    bool isMoving = false;
    Vector2Int bufferedDirection = Vector2Int.zero;
    float bufferTime = 0.01f;
    float bufferTimer = 0f;

    Vector3 targetWorldPos;
    Direction direction;
    InputSystem_Actions actions;

    public int Strength { get; private set; } = 1;
    public int Bombs { get; private set; } = 1;
    public int AtomicBombs { get; private set; }
    public bool KeepForce { get; private set; }

    Rigidbody2D rb;
    SpriteRenderer sr;
    InputAction moveAction;
    InputAction plantBombAction;

    public int playerNumber;
    public float speed = 2f;
    public Sprite[] sprites;
    public Appearance appearance;
    public float animationFrameTime = 0.15f;

    public GameObject DeadPlayerPrefab;
    public GameObject ExplodingBombPrefab;
    public GameObject ExplodingAtomicBombPrefab;

    private GridSystem Grid => GridSystem.Current;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        actions = new InputSystem_Actions();
        moveAction = actions.FindAction($"Player{playerNumber}/Move", throwIfNotFound: true);
        plantBombAction = actions.FindAction($"Player{playerNumber}/PlantBomb", throwIfNotFound: true);
    }

    void Start()
    {
        targetWorldPos = GridSystem.Current.ToWorld(GridSystem.Current.GetPosition(gameObject));
    }

    // called every frame
    void Update()
    {
        if (plantBomb)
        {
            if (Grid.GetObjectsAtSamePlace(gameObject).All(x => !x.TryGetComponent<ExplodingBomb>(out _) && !x.TryGetComponent<Block>(out _)))
            {
                var bombPrefab = AtomicBombs > 0 ? ExplodingAtomicBombPrefab : ExplodingBombPrefab;
                var bombObject = Instantiate(bombPrefab, Grid.ToWorld(Grid.GetPosition(gameObject)), Quaternion.identity);
                Grid.Add(bombObject, Grid.GetPosition(gameObject));
                
                var bomb = bombObject.GetComponent<ExplodingBomb>();
                bomb.strength = Strength;
                bomb.delay = 1f;

                if (appearance is Appearance.Pacman)
                    appearance = AtomicBombs > 0 ? Appearance.Atomic : Appearance.Normal;
            }
        }

        if (moveInput != Vector2Int.zero)
        {
            bufferedDirection = moveInput;
            bufferTimer = bufferTime;

            if (Math.Abs(moveInput.x) > Math.Abs(moveInput.y))
                direction = moveInput.x > 0 ? Direction.Right : Direction.Left;
            else
                direction = moveInput.y > 0 ? Direction.Up : Direction.Down;

            timer += Time.deltaTime;
            SetCurrentSprite();
        }

        if (!isMoving && bufferedDirection != Vector2Int.zero)
        {
            var grid = GridSystem.Current;

            if (grid.TryMove(this, bufferedDirection))
            {
                isMoving = true;
                targetWorldPos = grid.ToWorld(grid.GetPosition(gameObject));
                bufferedDirection = Vector2Int.zero;
            }
        }

        if (bufferTimer > 0f)
            bufferTimer -= Time.deltaTime;
        else
            bufferedDirection = Vector2Int.zero;
    }

    void LateUpdate()
    {
        if (!isMoving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPos,
            speed * Time.deltaTime
        );

        if (transform.position == targetWorldPos)
            isMoving = false;
    }

    void OnEnable()
    {
        moveAction.Enable();
        moveAction.performed += ctx =>
        {
            var rawInput = ctx.ReadValue<Vector2>();

            if (Mathf.Abs(rawInput.x) > Mathf.Abs(rawInput.y))
                moveInput = new Vector2Int(Math.Sign(rawInput.x), 0);
            else
                moveInput = new Vector2Int(0, Math.Sign(rawInput.y));
        };
        moveAction.canceled += ctx => moveInput = Vector2Int.zero;

        plantBombAction.Enable();
        plantBombAction.performed += ctx => plantBomb = true;
        plantBombAction.canceled += ctx => plantBomb = false;
    }

    void OnDisable()
    {
        moveAction.Disable();
        var playerStruct = actions.GetType().GetProperty("Player" + playerNumber).GetValue(actions);
        playerStruct.GetType().GetMethod("Disable").Invoke(playerStruct, new object[0]);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent<Player>(out var other))
            return;
        
        if (appearance is Appearance.Pacman && other.appearance is not Appearance.Pacman or Appearance.Immortal)
        {
            if (other.playerNumber == this.playerNumber) //for when level spawns same player multiple times
                return;

            //bug/feature of original game
            var block = Grid.GetObjectsAtSamePlace(other.gameObject).FirstOrDefault(x => x.TryGetComponent<Block>(out var block));
            if (block != null)
            {
                Grid.Remove(block);
                Destroy(block);
            }

            other.Kill();
        }

        if (appearance is Appearance.Ghost or Appearance.Immortal or Appearance.Sick && other.appearance is Appearance.Atomic or Appearance.Normal)
        {
            other.appearance = appearance;
            other.SetCurrentSprite();
#warning for sickness the stats also need to be set
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
        if (appearance is Appearance.Immortal)
            return;

        var position = Grid.Remove(gameObject);
        var deadPlayerObject = Instantiate(DeadPlayerPrefab, Grid.ToWorld(position), Quaternion.identity);
        Grid.Add(deadPlayerObject, position);

        var deadPlayer = deadPlayerObject.GetComponent<DeadPlayer>();
        deadPlayer.Strength = Strength;
        deadPlayer.Bombs = Bombs;
        deadPlayer.AtomicBombs = AtomicBombs;
        deadPlayer.KeepForce = KeepForce;

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
            if (appearance is Appearance.Pacman or Appearance.Sick)
                return appearance;

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
