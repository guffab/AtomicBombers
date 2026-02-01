using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Setup : UIScene
{
    public static int Players { get; private set; } = 2;
    public static bool MusicOn { get; private set; } = true;
    public static int LevelOffset { get; private set; } = 0;
    public static Dictionary<int, int> AvatarOffsets = new()
    {
        {1, 1},
        {2, 2},
        {3, 3},
        {4, 4},
    };

    InputSystem_Actions actions;
    bool controlSetupMode = false;
    int playerNumber = 1;

    void Awake()
    {
        actions = new InputSystem_Actions();
        RedrawUI();

        var rebinds = PlayerPrefs.GetString("Rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
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

        if (controlSetupMode)
        {
            for (int i = 2; i <= 20; i++)
                PlaceUI(i, 7, block);

            //text (large)
            PlaceLargeText(5, 15, "CONTROL SETUP");

            //text (small)
            grid = new Grid(new Vector2Int(16, 16));

            for (int i = 0; i < Players; i++)
            {
                PlaceText(6, 21 - i * 2, $"F{i + 1} - PLAYER {i + 1}:");
                PlaceUI(6 + 15, 21 - i * 2, playerSprites[AvatarOffsets[i + 1] - 1]);
            }

            var moveComposite = actions.FindAction($"Player{playerNumber}/Move", throwIfNotFound: true);
            var keys = moveComposite.bindings.Select((binding, index) => (binding, index)).Where(x => x.binding.isPartOfComposite).Select(x => x.index).ToArray();

            var fireAction = actions.FindAction($"Player{playerNumber}/PlantBomb", throwIfNotFound: true);

            PlaceText(6, 10, $"F5 - KEYBOARD CONFIG: PLAYER {playerNumber}");
            PlaceText(7, 8, $"1 - UP   : {moveComposite.GetBindingDisplayString(keys[0]).ToUpper()}");
            PlaceText(7, 7, $"2 - DOWN : {moveComposite.GetBindingDisplayString(keys[1]).ToUpper()}");
            PlaceText(7, 6, $"3 - LEFT : {moveComposite.GetBindingDisplayString(keys[2]).ToUpper()}");
            PlaceText(7, 5, $"4 - RIGHT: {moveComposite.GetBindingDisplayString(keys[3]).ToUpper()}");
            PlaceText(7, 4, $"5 - FIRE : {fireAction.GetBindingDisplayString(0).ToUpper()}");
        }
        else
        {
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
    }

    void Update()
    {
        RedrawUI();

        if (controlSetupMode)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                controlSetupMode = false;
                PlayerPrefs.SetString("Rebinds", actions.SaveBindingOverridesAsJson());
            }

            if (Input.GetKeyDown(KeyCode.F1))
                AvatarOffsets[1] = (AvatarOffsets[1] % 4) + 1;

            if (Input.GetKeyDown(KeyCode.F2))
                AvatarOffsets[2] = (AvatarOffsets[2] % 4) + 1;

            if (Input.GetKeyDown(KeyCode.F3))
                AvatarOffsets[3] = (AvatarOffsets[3] % 4) + 1;

            if (Input.GetKeyDown(KeyCode.F4))
                AvatarOffsets[4] = (AvatarOffsets[4] % 4) + 1;

            if (Input.GetKeyDown(KeyCode.F5))
                playerNumber = (playerNumber % Players) + 1;

            if (Input.GetKeyDown(KeyCode.Alpha1))
                RebindCompositeKey("Move", 0);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                RebindCompositeKey("Move", 1);

            if (Input.GetKeyDown(KeyCode.Alpha3))
                RebindCompositeKey("Move", 2);

            if (Input.GetKeyDown(KeyCode.Alpha4))
                RebindCompositeKey("Move", 3);

            if (Input.GetKeyDown(KeyCode.Alpha5))
                RebindKey("PlantBomb");
        }
        else
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
                Players = Math.Max(2, (Players + 1) % 5);

            if (Input.GetKeyDown(KeyCode.F2))
            {
                MusicOn = !MusicOn;

                if (GameMusicPlayer.Instance != null)
                    GameMusicPlayer.Instance.GetComponent<AudioSource>().mute = !MusicOn;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (LevelOffset is not 0)
                    LevelOffset = 0;
                else
                    LevelOffset = SharedRandom.Next(LevelManager.LevelCount - 1);
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                if (!controlSetupMode)
                    controlSetupMode = true;
            }
        }
    }

    private void RebindKey(string name)
    {
        var action = actions.FindAction($"Player{playerNumber}/{name}", throwIfNotFound: true);

        action.PerformInteractiveRebinding(0)
              .WithControlsExcluding("Mouse")
              .WithControlsExcluding("<Keyboard>/escape")
              .OnMatchWaitForAnother(0.01f)
              .Start();
    }

    private void RebindCompositeKey(string name, int index)
    {
        var composite = actions.FindAction($"Player{playerNumber}/{name}", throwIfNotFound: true);
        var keys = composite.bindings.Select((binding, index) => (binding, index))
                                     .Where(x => x.binding.isPartOfComposite)
                                     .Select(x => x.index)
                                     .ToArray();

        composite.PerformInteractiveRebinding(keys[index])
                 .WithControlsExcluding("Mouse")
                 .WithControlsExcluding("<Keyboard>/escape")
                 .OnMatchWaitForAnother(0.01f)
                 .Start();
    }

    private void PlacePlayers(int x, int y)
    {
        for (int i = 0; i < Players; i++)
            PlaceUI(x + 2 * i, y, playerSprites[AvatarOffsets[i + 1] - 1]);
    }
}
