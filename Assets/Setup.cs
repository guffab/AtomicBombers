using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Setup : UIScene
{
    public static int Players { get; private set; } = 2;
    public static bool MusicOn { get; private set; } = true;
    public static int LevelOffset { get; private set; } = 0;

    void Awake()
    {
        RedrawUI();
    }

    private void RedrawUI()
    {
        foreach (var a in GameObject.FindGameObjectsWithTag("UI"))
            Destroy(a);

        var grid = new Grid(new Vector2Int(28, 28));

        var block = blockSprites[LevelManager.Level.BlockStyle - 1];

        for (int i = 1; i <= 21; i++)
            PlaceUI(i, 1, block);

        for (int i = 2; i <= 16; i++)
            PlaceUI(1, i, block);

        for (int i = 2; i <= 16; i++)
            PlaceUI(21, i, block);

        for (int i = 1; i <= 21; i++)
            PlaceUI(i, 17, block);

        PlaceUI(6, 15, playerSprites[0]);
        PlaceUI(7, 15, playerSprites[1]);
        PlaceLargeText(9, 15, "SETUP");
        PlaceUI(15, 15, playerSprites[2]);
        PlaceUI(16, 15, playerSprites[3]);

        var onoff = MusicOn ? "ON" : "OFF";
        var normalrandom = LevelOffset is 0 ? "NORMAL" : "RANDOM";

        grid = new Grid(new Vector2Int(16, 1));
        PlaceText(5, 20 * 16, "F1 - PLAYERS:  ");
        PlaceText(5, 19 * 16 - 4, $"F2 - MUSIC {onoff}");
        PlaceText(5, 18 * 16 - 8, $"F3 - {normalrandom} LEVEL START");
        PlaceText(5, 17 * 16 - 12, $"F4 - KEYBOARD CONFIG");

        PlacePlayers(5 + 15, 20 * 16 + 7);

        PlaceText(5, 7 * 16, $"FOUND: {LevelManager.LevelCount} LEVELS");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("LevelPreview");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            Players = Math.Max(2, (Players + 1) % 5);
            RedrawUI();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            MusicOn = !MusicOn;

            if (GameMusicPlayer.Instance != null)
                GameMusicPlayer.Instance.GetComponent<AudioSource>().mute = !MusicOn;

            RedrawUI();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (LevelOffset is not 0)
                LevelOffset = 0;
            else
                LevelOffset = SharedRandom.Next(LevelManager.LevelCount - 1);

            RedrawUI();
        }
    }

    private void PlacePlayers(int x, int y)
    {
        for (int i = 0; i < Players; i++)
            PlaceUI(x + 2 * i, y, playerSprites[i]);
    }
}
