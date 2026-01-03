using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    static int currentIndex;
    static List<LevelData> Levels;

    public static LevelManager Instance;

    [HideInInspector] public LevelData Current => Levels[currentIndex];

    //prefabs
    public GameObject IndestructibleBlockPrefab;
    public GameObject DamagedBlockPrefab;
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

    [Tooltip("Grid extends from (1,1) to (x, y) entered in this property. Positive by convention.")]
    public Vector2Int GridDimensions;

    void Awake()
    {
        Instance = this;
        var grid = new GridSystem(GridDimensions);
        Levels ??= LoadLevels();
        currentIndex = (currentIndex + 1) % Levels.Count;

        for (int x = 1; x <= GridDimensions.x; x++)
        {
            for (int y = 1; y <= GridDimensions.y; y++)
            {
                var gridPos = new Vector2Int(x, y);
                var gridElement = Current.GetItemAt(gridPos);
                PlaceElement(gridElement, gridPos);
            }
        }
    }

    public void PlaceNewElement(Vector2Int gridPos)
    {
        var gridElement = Current.GetNewItemAt(gridPos);
        PlaceElement(gridElement, gridPos);
    }

    private void PlaceElement(LevelData.GridElement gridElement, Vector2Int gridPos)
    {
        var itemToInstantiate = gridElement switch
        {
            LevelData.GridElement.IndestructibleBlock => IndestructibleBlockPrefab,
            LevelData.GridElement.DamagedBlock => DamagedBlockPrefab,
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
            var position = GridSystem.Current.ToWorld(gridPos);
            var newObject = Instantiate(itemToInstantiate, position, Quaternion.identity);
            GridSystem.Current.Add(newObject, gridPos);

            if (newObject.TryGetComponent<Block>(out var block))
                block.style = Current.BlockStyle;
        }
    }

    private static List<LevelData> LoadLevels()
    {
        var levelData = new List<LevelData>();
        string levelsPath = Path.Combine(Application.streamingAssetsPath, "Levels");

        foreach (string file in Directory.GetFiles(levelsPath, "*.json"))
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
}
