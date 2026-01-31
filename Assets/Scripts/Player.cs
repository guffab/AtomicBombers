using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    static readonly Appearance[] appearances = new Appearance[] { Appearance.Pacman, Appearance.Immortal, Appearance.Sick, Appearance.Ghost };

    const int framesPerAnimation = 3;
    float timer;
    Vector2Int moveInput = Vector2Int.zero;
    bool plantBomb = false;

    const float fastSpeed = 3.5f;
    const float slowSpeed = 1f;
    const float fastBomb = .8f;
    const float slowBomb = 5.5f;
    const float bufferTime = 0.01f;
    const float sicknessTime = 15f;

    bool isMoving = false;
    Vector2Int bufferedDirection = Vector2Int.zero;
    float bufferTimer = 0f;
    float sicknessTimer;
    bool blink;
    float currentSpeed;
    float currentBombDelay;

    Vector2Int lastGridPos;
    Vector3 targetWorldPos;
    Direction direction;
    InputSystem_Actions actions;

    internal int availableBombs;

    public int Strength { get; private set; } = 2;
    public int Bombs { get; private set; } = 1;
    public int AtomicBombs { get; private set; } = 0;
    public bool KeepForce { get; private set; } = false;

    SpriteRenderer sr;
    InputAction moveAction;
    InputAction plantBombAction;

    public int playerNumber;
    public float speed = 2f;
    public float bombDelay = 1.5f;
    public Sprite[] sprites;
    public Appearance appearance;
    public float animationFrameTime = 0.15f;

    public GameObject DeadPlayerPrefab;
    public GameObject ExplodingBombPrefab;
    public GameObject ExplodingAtomicBombPrefab;
    private bool invertedMovement;

    private Grid Grid => Grid.Current;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        actions = new InputSystem_Actions();
        moveAction = actions.FindAction($"Player{playerNumber}/Move", throwIfNotFound: true);
        plantBombAction = actions.FindAction($"Player{playerNumber}/PlantBomb", throwIfNotFound: true);
        LevelManager.Register(this);
    }

    void Start()
    {
        (Strength, Bombs, AtomicBombs) = Highscore.GetPlayerConfig(playerNumber);

        lastGridPos = Grid.GetPosition(gameObject);
        targetWorldPos = Grid.ToWorld(lastGridPos);
        availableBombs = Bombs;
        currentSpeed = speed;
        currentBombDelay = bombDelay;

        StartCoroutine(BlinkSetter());
    }

    private IEnumerator BlinkSetter()
    {
        while (true)
        {
            if (sicknessTimer is <= 0 or >= 4)
                yield return new WaitForSeconds(0.01f);

            yield return new WaitForSeconds(0.1f);

            if (sicknessTimer is >= 0 and <= 4)
                blink = true;

            yield return new WaitForSeconds(0.1f);
            blink = false;
        }
    }

    // called every frame
    void Update()
    {
        if (sicknessTimer > 0)
            sicknessTimer -= Time.deltaTime;
        else
        {
            if (appearance is not Appearance.Pacman)
            {
                if (Grid.GetObjectsAtSamePlace(gameObject).Any(x => x.TryGetComponent<Block>(out var block) && block.state is Block.State.Solid))
                    Kill();

                appearance = Appearance.Normal;
                ResetSickness();
            }
        }

        if (appearance is Appearance.Atomic && AtomicBombs < 1)
            appearance = Appearance.Normal;

        else if (appearance is Appearance.Normal && AtomicBombs > 0)
            appearance = Appearance.Atomic;

        if (plantBomb)
        {
            var objects = Grid.GetObjects(lastGridPos);
            if (availableBombs > 0 && objects.All(x => !x.TryGetComponent<ExplodingBomb>(out _) && !x.TryGetComponent<Block>(out _)))
            {
                var bombPrefab = AtomicBombs > 0 ? ExplodingAtomicBombPrefab : ExplodingBombPrefab;
                var bombObject = Instantiate(bombPrefab, Grid.ToWorld(lastGridPos), Quaternion.identity);
                Grid.Add(bombObject, lastGridPos);

                availableBombs--;
                if (AtomicBombs > 0)
                    AtomicBombs--;


                var bomb = bombObject.GetComponent<ExplodingBomb>();
                bomb.player = this;
                bomb.strength = Strength;
                bomb.delay = currentBombDelay;

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
        }

        if (!isMoving && bufferedDirection != Vector2Int.zero)
        {
            var grid = Grid.Current;

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

        SetCurrentSprite();
    }

    void LateUpdate()
    {
        if (!isMoving)
            return;

        if ((transform.position - targetWorldPos).magnitude <= 1f)
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPos,
                currentSpeed * Time.deltaTime
            );
        else
        {
            var nextGridpos = Grid.GetPosition(gameObject);
            var direction = (lastGridPos - nextGridpos).Normalize();

            //position + half of neighboring position
            var leftFakePos = Grid.ToWorld(lastGridPos) + ((Grid.ToWorld(lastGridPos + direction, true) - Grid.ToWorld(lastGridPos)) / 2);
            var rightFakePos = Grid.ToWorld(nextGridpos) + ((Grid.ToWorld(nextGridpos - direction, true) - Grid.ToWorld(nextGridpos)) / 2);

            //walk half off one side, then walk half in from other side
            if ((transform.position - leftFakePos).magnitude < (transform.position - rightFakePos).magnitude)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    leftFakePos,
                    currentSpeed * Time.deltaTime
                );

                //jump to other side
                if (transform.position == leftFakePos)
                    transform.position = rightFakePos;
            }
        }

        if (transform.position == targetWorldPos)
        {
            isMoving = false;
            lastGridPos = Grid.GetPosition(gameObject);
        }
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

            if (invertedMovement)
                moveInput = -moveInput;
        };
        moveAction.canceled += ctx => moveInput = Vector2Int.zero;

        plantBombAction.Enable();
        plantBombAction.performed += ctx => plantBomb = true;
        plantBombAction.canceled += ctx => plantBomb = false;
    }

    void OnDisable()
    {
        moveAction.Disable();
        plantBombAction.Disable();
        var playerStruct = actions.GetType().GetProperty("Player" + playerNumber).GetValue(actions);
        playerStruct.GetType().GetMethod("Disable").Invoke(playerStruct, new object[0]);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CollisionCheck(collision);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionCheck(collision);
    }

    private void CollisionCheck(Collision2D collision)
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
            other.ResetSickness();

            if (appearance is Appearance.Sick)
            {
                other.currentSpeed = currentSpeed;
                other.currentBombDelay = currentBombDelay;
                other.invertedMovement = invertedMovement;
            }
        }
    }

    void SetCurrentSprite()
    {
        int frame = Mathf.FloorToInt(timer / animationFrameTime) % framesPerAnimation;

        int appearanceOffset = GetAppearanceOffset(appearance, sicknessTimer, blink, AtomicBombs > 0);

        //player + style offset
        int offset = (playerNumber - 1) * 12 + 48 * appearanceOffset;
        int index = (int)direction * framesPerAnimation + frame + offset;

        sr.sprite = sprites[index];

        static int GetAppearanceOffset(Appearance appearance, float sicknessTimer, bool blink, bool hasAtomics)
        {
            if (appearance is Appearance.Normal or Appearance.Atomic or Appearance.Pacman)
                return (int)appearance;

            if (sicknessTimer <= 0 || sicknessTimer >= 4)
                return (int)appearance;

            if (!blink)
                return (int)appearance;

            return hasAtomics ? (int)Appearance.Atomic : (int)Appearance.Normal;
        }
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

        LevelManager.Unregister(this);
        Destroy(gameObject);
    }

    internal void Eat(DeadPlayer deadPlayer)
    {
        Strength = Math.Max(Strength, deadPlayer.Strength);
        AtomicBombs = Math.Max(AtomicBombs, deadPlayer.AtomicBombs);
        KeepForce = KeepForce || deadPlayer.KeepForce;

        //increase available bombs as well
        var tmpBombs = Math.Max(Bombs, deadPlayer.Bombs);
        availableBombs += tmpBombs - Bombs;
        Bombs = tmpBombs;
    }

    internal void Consume(Consumable.Kind type)
    {
        if (type is Consumable.Kind.KeepForce)
            KeepForce = true;

        else if (type is Consumable.Kind.Powder)
            Strength = Math.Min(Strength + 1, 20);

        else if (type is Consumable.Kind.Bomb)
        {
            Bombs = Math.Min(Bombs + 1, 15);
            availableBombs = Math.Min(availableBombs + 1, 30);
        }

        else if (type is Consumable.Kind.Atomicbomb)
        {
            AtomicBombs = Math.Min(AtomicBombs + 1, 999_999);
            if (appearance is Appearance.Normal)
                appearance = Appearance.Atomic;
        }

        else if (type is Consumable.Kind.Megabomb)
        {
            Strength = 20;
            AtomicBombs = 999_999;
        }

        else if (type is Consumable.Kind.Pacman)
        {
            appearance = SelectWorst(Appearance.Pacman);
            ResetSickness();
        }

        else if (type is Consumable.Kind.Immortal)
        {
            appearance = SelectWorst(Appearance.Immortal);
            ResetSickness();
        }

        else if (type is Consumable.Kind.Ghost)
        {
            appearance = SelectWorst(Appearance.Ghost);
            ResetSickness();
        }

        else if (type is Consumable.Kind.Surprise)
        {
            var newAppearance = appearances[SharedRandom.Next(appearances.Length)];
            appearance = SelectWorst(newAppearance);
            ResetSickness();

            if (newAppearance is Appearance.Sick)
            {
                var sicknessType = SharedRandom.Next(5);

                if (sicknessType is 0) currentSpeed = slowSpeed;
                else if (sicknessType is 1) currentSpeed = fastSpeed;
                else if (sicknessType is 2) currentBombDelay = slowBomb;
                else if (sicknessType is 3) currentBombDelay = fastBomb;
                else if (sicknessType is 4) invertedMovement = true;
            }
        }

        Appearance SelectWorst(Appearance newAppearance)
        {
            if (newAppearance is Appearance.Pacman)
                return newAppearance;

            if (appearance is Appearance.Pacman or Appearance.Sick)
                return appearance;

            if (appearance is Appearance.Immortal)
                return newAppearance is Appearance.Sick ? newAppearance : appearance;

            return newAppearance;
        }
    }

    private void ResetSickness()
    {
        currentSpeed = speed;
        currentBombDelay = bombDelay;
        invertedMovement = false;

        SetCurrentSprite();
        if (appearance is not (Appearance.Normal or Appearance.Atomic or Appearance.Pacman))
            sicknessTimer = sicknessTime;
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
