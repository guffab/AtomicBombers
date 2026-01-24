using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    static int currentIndex = 0;
    static List<LevelData> Levels;

    static LevelManager Instance;

    public static LevelData Level => Levels[currentIndex];
    List<Player> players = new();

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

    public static void Load()
    {
        Levels ??= LoadLevels();
    }

    void Awake()
    {
        Instance = this;
        var grid = new Grid(new Vector2Int(28, 28));
        Levels ??= LoadLevels();
        currentIndex = (StartGame.GameRound + Setup.LevelOffset) % Levels.Count;

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
        if (players.Count <= 1)
        {
            StartCoroutine(ExecuteAfterWait(5f));

        }
    }

    void OnDestroy()
    {
        currentIndex = (StartGame.GameRound + Setup.LevelOffset + 1) % Levels.Count;
    }

    public static void Register(Player player)
    {
        Instance.players.Add(player);
    }

    public static void Unregister(Player player)
    {
        Instance.UnregisterInstance(player);
    }

    private void UnregisterInstance(Player player)
    {
        players.Remove(player);
        Highscore.AddPlayerDetails(new Highscore.PlayerDetails(player.playerNumber, player.Strength, player.Bombs, 0, false), false);

        if (players.Count <= 1)
        {
            StartCoroutine(ExecuteAfterWait(5f));
        }
    }

    public static void PlaceNewElement(Vector2Int gridPos)
    {
        var objects = Grid.Current.GetObjects(gridPos);
        if (objects.Have<Player>() || objects.Have<DeadPlayer>())
            return;

        var gridElement = Level.GetNewItemAt(gridPos);
        Instance.PlaceElement(gridElement, gridPos);
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
            LevelData.GridElement.Player4 => Player4Prefab,
            LevelData.GridElement.Player3 => Player3Prefab,
            LevelData.GridElement.Player2 => Player2Prefab,
            LevelData.GridElement.Player1 => Player1Prefab,
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
            catch (System.Exception)
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
}
