using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    static List<LevelData> Levels;

    static LevelManager Instance;

    public static LevelData Level => Levels[(GameRound + Setup.LevelOffset) % Levels.Count];
    public static int LevelCount => Levels.Count;
    public static int GameRound { get; private set; } = 0;

    List<Player> players = new();
    List<Explosion> explosions = new();

    //prefabs
    public GameObject IndestructibleBlockPrefab;
    public GameObject SlightlyDamagedBlockPrefab;
    public GameObject MediumDamagedBlockPrefab;
    public GameObject HeavilyDamagedBlockPrefab;
    public GameObject SolidBlockPrefab;
    public GameObject WalkableBlockPrefab;
    public GameObject MinePrefab;
    public GameObject Player4Prefab;
    public GameObject Player3Prefab;
    public GameObject Player2Prefab;
    public GameObject Player1Prefab;
    public GameObject LightPrefab;
    public GameObject KeepForcePrefab;
    public GameObject PowderPrefab;
    public GameObject BombPrefab;
    public GameObject AtomicbombPrefab;
    public GameObject MegabombPrefab;
    public GameObject PacmanPrefab;
    public GameObject ImmortalPrefab;
    public GameObject GhostPrefab;
    public GameObject SurprisePrefab;
    public GameObject Foreground;
    public GameObject Background;
    private LevelData.During lightLevel;

    public static void Load()
    {
        Levels ??= LoadLevels();
    }

    void Awake()
    {
        Instance = this;
        var grid = new Grid(new Vector2Int(28, 28));
        Levels ??= LoadLevels();

        Debug.Log(Level.WorldName + Level.Name);

        for (int x = 1; x <= grid.Dimensions.x; x++)
        {
            for (int y = 1; y <= grid.Dimensions.y; y++)
            {
                var gridPos = new Vector2Int(x, y);
                var gridElement = Level.GetItemAt(gridPos);
                PlaceElement(gridElement, gridPos);
            }
        }


    }

    void Start()
    {
        lightLevel = Level.TimeOfDay;

        if (players.Count <= 1)
            StartCoroutine(ExecuteAfterWait(3f));

        StartCoroutine(HandleDangerLight());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameRound--; //because increased on destroy
            SceneManager.LoadScene("LevelPreview");
        }

        if (explosions.Any())
            TempChangeLight(LevelData.During.Day);
        else
            TempChangeLight(lightLevel);
    }

    void OnDestroy()
    {
        GameRound++;
    }

    public static void Register(Player player) => Instance.players.Add(player);
    public static void Unregister(Player player) => Instance.UnregisterInstance(player);
    public static void Register(Explosion explosion) => Instance.explosions.Add(explosion);
    public static void Unregister(Explosion explosion) => Instance.explosions.Remove(explosion);

    private void UnregisterInstance(Player player)
    {
        players.Remove(player);
        Highscore.AddPlayerDetails(new Highscore.PlayerDetails(player.playerNumber, player.Strength, player.Bombs, 0, false), false);

        if (players.Count <= 1)
            StartCoroutine(ExecuteAfterWait(7f));
    }

    public static void PlaceNewElement(Vector2Int gridPos)
    {
        var objects = Grid.Current.GetObjects(gridPos);
        if (objects.Have<Player>() || objects.Have<DeadPlayer>())
            return;

        var gridElement = Level.GetNewItemAt(gridPos);
        Instance.PlaceElement(gridElement, gridPos);
    }

    public static void ChangeLight(bool lightOn)
    {
        Instance.lightLevel = lightOn ? LevelData.During.Day : LevelData.During.Night;
    }

    private void TempChangeLight(LevelData.During during)
    {
        var darkness = during switch
        {
            LevelData.During.Day => 0,
            LevelData.During.Night => 253,
            LevelData.During.Dawn or (LevelData.During)3 => 185, //not sure what this value means
            _ => SharedRandom.Next(250)
        };

        var renderer = Instance.Foreground.GetComponent<SpriteRenderer>();
        renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, darkness / 255f);
    }

    private void PlaceElement(LevelData.GridElement gridElement, Vector2Int gridPos)
    {
        var itemToInstantiate = gridElement switch
        {
            LevelData.GridElement.IndestructibleBlock => IndestructibleBlockPrefab,
            LevelData.GridElement.SlightlyDamagedBlock => SlightlyDamagedBlockPrefab,
            LevelData.GridElement.MediumDamagedBlock => MediumDamagedBlockPrefab,
            LevelData.GridElement.HeavilyDamagedBlock => HeavilyDamagedBlockPrefab,
            LevelData.GridElement.SolidBlock => SolidBlockPrefab,
            LevelData.GridElement.WalkableBlock => WalkableBlockPrefab,
            LevelData.GridElement.Mine => MinePrefab,
            LevelData.GridElement.Player4 => GetPlayerPrefab(4),
            LevelData.GridElement.Player3 => GetPlayerPrefab(3),
            LevelData.GridElement.Player2 => GetPlayerPrefab(2),
            LevelData.GridElement.Player1 => GetPlayerPrefab(1),
            LevelData.GridElement.Light => LightPrefab,
            LevelData.GridElement.KeepForce => KeepForcePrefab,
            LevelData.GridElement.Powder => PowderPrefab,
            LevelData.GridElement.Bomb => BombPrefab,
            LevelData.GridElement.AtomicBomb => AtomicbombPrefab,
            LevelData.GridElement.Megabomb => MegabombPrefab,
            LevelData.GridElement.Pacman => PacmanPrefab,
            LevelData.GridElement.Immortal => ImmortalPrefab,
            LevelData.GridElement.Ghost => GhostPrefab,
            LevelData.GridElement.Surprise => SurprisePrefab,
            _ => null,
        };

        if (itemToInstantiate != null)
        {
            if (itemToInstantiate == Player4Prefab && Setup.Players < 4 || itemToInstantiate == Player3Prefab && Setup.Players < 3)
                return;

            var position = Grid.Current.ToWorld(gridPos);
            var newObject = Instantiate(itemToInstantiate, position, Quaternion.identity);
            Grid.Current.Add(newObject, gridPos);

            if (newObject.TryGetComponent<Block>(out var block))
                block.style = Level.BlockStyle;
        }

        //local helper
        GameObject GetPlayerPrefab(int i)
        {
            var playerRankings = Highscore.SortPlayersByWins().SelectMany(x => x);
            return playerRankings.ElementAtOrDefault(i - 1) switch
            {
                4 => Player4Prefab,
                3 => Player3Prefab,
                2 => Player2Prefab,
                1 => Player1Prefab,
                _ => null,
            };
        }
    }

    private static List<LevelData> LoadLevels()
    {
        var levelData = new List<LevelData>();
        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Worlds");

        foreach (string file in Directory.GetFiles(levelsPath, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                string json = File.ReadAllText(file);
                var data = JsonConvert.DeserializeObject<LevelData>(json);
                levelData.Add(data);
            }
            catch (Exception)
            {
            }
        }
        return levelData;
    }


    private IEnumerator ExecuteAfterWait(float duration)
    {
        yield return new WaitForSeconds(duration);

        foreach (var player in players)
            Highscore.AddPlayerDetails(new Highscore.PlayerDetails(player.playerNumber, player.Strength, player.Bombs, player.AtomicBombs, player.KeepForce), true);

        SceneManager.LoadScene("Highscore");
    }

    private IEnumerator HandleDangerLight()
    {
        while (true)
        {
            if (!players.Any(x => x.appearance is Player.Appearance.Pacman))
            {
                yield return new WaitForSeconds(0.01f);
                continue;
            }

            //red blinking effect
            var renderer = Instance.Background.GetComponent<SpriteRenderer>();
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1);

            yield return new WaitForSeconds(.4f);
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0);

            yield return new WaitForSeconds(.4f);
        }
    }
}
