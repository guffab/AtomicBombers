using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Setup : MonoBehaviour
{
    private List<GameObject> instantiatedPlayerSprites = new();

    public Sprite[] largeFontSprites;
    public Sprite[] smallFontSprites;
    public Sprite[] playerSprites;
    public Sprite[] blockSprites;
    public GameObject fontPrefab;

    public static int Players { get; private set; } = 2;
    public static bool Music { get; private set; }
    public static int LevelOffset { get; private set; }

    void Awake()
    {
        var grid = new Grid(new Vector2Int(28, 28));

        var block = blockSprites[0];

        for (int i = 1; i <= 21; i++)
            PlaceUI(i, 1, block);

        for (int i = 2; i <= 16; i++)
            PlaceUI(1, i, block);

        for (int i = 2; i <= 16; i++)
            PlaceUI(21, i, block);

        for (int i = 1; i <= 21; i++)
            PlaceUI(i, 17, block);

        grid = new Grid(new Vector2Int(16, 16));
        PlaceText(6, 25, "F1  - PLAYERS:  ");
        PlaceText(6, 23, "F2  - MUSIC ON");
        PlaceText(6, 21, "F3  - NORMAL LEVEL START");
        PlaceText(6, 19, "F4  - KEYBOARD CONFIG");

        PlacePlayers(6 + 16, 25);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("World");

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Players = Math.Max(2, (Players + 1) % 5);
            PlacePlayers(6 + 16, 25);
        }
    }

    private GameObject PlaceUI(int x, int y, Sprite sprite)
    {
        var worldPos = Grid.Current.ToWorld(new Vector2Int(x, y));
        var dummy = Instantiate(fontPrefab, worldPos, Quaternion.identity);
        dummy.GetComponent<SpriteRenderer>().sprite = sprite;
        return dummy;
    }

    private void PlaceText(int x, int y, string text)
    {
        for (int i = 0; i < text.Length; i++)
            PlaceUI(x + i, y, smallFontSprites[text[i] % smallFontSprites.Length]);
    }

    private void PlacePlayers(int x, int y)
    {
        foreach (var s in instantiatedPlayerSprites)
            Destroy(s);

        instantiatedPlayerSprites.Clear();
        for (int i = 0; i < Players; i++)
        {
            var go = PlaceUI(x + 2 * i, y, playerSprites[i]);
            instantiatedPlayerSprites.Add(go);
        }
    }
}
