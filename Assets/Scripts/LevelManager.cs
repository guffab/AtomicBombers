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
    public static LevelData.During LightLevel { get; private set; }

    List<Player> players = new();
    List<Explosion> explosions = new();
    LevelData.During lightLevel;

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
    public GameObject Background;

    public static void Load()
    {
        Levels ??= LoadLevels();
    }

    void Awake()
    {
        Instance = this;
        var grid = new Grid(new Vector2Int(28, 28));
        Levels ??= LoadLevels();

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
        //not sure, but most likely this was intended for random lighting
        lightLevel = Level.TimeOfDay is (LevelData.During)3 ? (LevelData.During)SharedRandom.Next(3) : Level.TimeOfDay;

        if (players.Count <= 1)
            StartCoroutine(CrownWinnersAndLeave(3f));

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
            LightLevel = LevelData.During.Day;
        else
            LightLevel = lightLevel;
    }

    void OnDestroy()
    {
        GameRound++;
    }

    public static void Register(Player player) => Instance.players.Add(player);
    public static void Unregister(Player player, int killedBy) => Instance.UnregisterInstance(player, killedBy);
    public static void Register(Explosion explosion) => Instance.explosions.Add(explosion);
    public static void Unregister(Explosion explosion) => Instance.explosions.Remove(explosion);

    private void UnregisterInstance(Player player, int killedBy)
    {
        players.Remove(player);
        Highscore.AddPlayerDetails(new Highscore.PlayerDetails(player.playerNumber, killedBy, player.Strength, player.Bombs, 0, false), false);

        if (players.Count <= 1)
            StartCoroutine(CrownWinnersAndLeave(7f));
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


    private IEnumerator CrownWinnersAndLeave(float duration)
    {
        yield return new WaitForSeconds(duration);

        foreach (var player in players)
            Highscore.AddPlayerDetails(new Highscore.PlayerDetails(player.playerNumber, -1, player.Strength, player.Bombs, player.AtomicBombs, player.KeepForce), true);

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
            renderer.color = new Color(148 / 255f, 16 / 255f, 0);

            yield return new WaitForSeconds(.4f);
            renderer.color = new Color(0,0,0);

            yield return new WaitForSeconds(.4f);
        }
    }
}
