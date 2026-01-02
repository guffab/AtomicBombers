using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    static int currentIndex;
    static List<LevelData> Levels;

    [HideInInspector] public LevelData Current => Levels[currentIndex];

    //prefabs
    public GameObject BlockPrefab;
    public GameObject MinePrefab;
    public GameObject Player4Prefab;
    public GameObject Player3Prefab;
    public GameObject Player2Prefab;
    public GameObject Player1Prefab;
    public GameObject LightPrefab;
    public GameObject KeepForcePrefab;
    public GameObject PowderPrefab;
    public GameObject BombPrefab;
    public GameObject AtomicBombPrefab;
    public GameObject MegabombPrefab;
    public GameObject PacmanPrefab;
    public GameObject BlackPrefab;
    public GameObject WhitePrefab;
    public GameObject SurprisePrefab;

    [Tooltip("Grid extends from (1,1) to (x, y) entered in this property. Positive by convention.")]
    public Vector2Int GridDimensions;

    void Awake()
    {
        var grid = new GridSystem(GridDimensions);
        Levels ??= LoadLevels();
        currentIndex = (currentIndex + 1) % Levels.Count;

        for (int x = 1; x <= GridDimensions.x; x++)
        {
            for (int y = 1; y <= GridDimensions.y; y++)
            {
                var gridPos = new Vector2Int(x, y);
                var gridData = Current.GetItemAt(x, y);
                GameObject itemToInstantiate = Current.GetItemAt(x, y) switch
                {
                    LevelData.GridElement.PersistentBlock => BlockPrefab,
                    LevelData.GridElement.DurableBlock => BlockPrefab,
                    LevelData.GridElement.SolidBlock => BlockPrefab,
                    LevelData.GridElement.WalkableBlock => BlockPrefab,
                    LevelData.GridElement.Mine => MinePrefab,
                    LevelData.GridElement.Player4 => Player4Prefab,
                    LevelData.GridElement.Player3 => Player3Prefab,
                    LevelData.GridElement.Player2 => Player2Prefab,
                    LevelData.GridElement.Player1 => Player1Prefab,
                    LevelData.GridElement.Light => LightPrefab,
                    LevelData.GridElement.KeepForce => KeepForcePrefab,
                    LevelData.GridElement.Powder => PowderPrefab,
                    LevelData.GridElement.Bomb => BombPrefab,
                    LevelData.GridElement.AtomicBomb => AtomicBombPrefab,
                    LevelData.GridElement.Megabomb => MegabombPrefab,
                    LevelData.GridElement.Pacman => PacmanPrefab,
                    LevelData.GridElement.Immortal => BlackPrefab,
                    LevelData.GridElement.Ghost => WhitePrefab,
                    LevelData.GridElement.Surprise => SurprisePrefab,
                    _ => null,
                };

                if (itemToInstantiate != null)
                {
                    var position = grid.ToWorld(gridPos);
                    var newObject = Instantiate(itemToInstantiate, position, Quaternion.identity);
                    grid.Add(newObject, gridPos);
                    
                    if (itemToInstantiate == BlockPrefab)
                    {
                        newObject.GetComponent<Block>().CurrentState = gridData switch
                        {
                            LevelData.GridElement.PersistentBlock => Block.State.Persistent,
                            LevelData.GridElement.DurableBlock => Block.State.DurableFull,
                            LevelData.GridElement.SolidBlock => Block.State.Solid,
                            _ => Block.State.Walkable,
                        };
                    }
                }
            }
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
