using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPreview : UIScene
{
    public Sprite[] platformSprites;
    public Sprite trophySprite;

    void Awake()
    {
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

        for (int i = 2; i <= 20; i++)
            PlaceUI(i, 7, block);

        //platform
        PlaceUI(5, 2, platformSprites[27]);
        PlaceUI(6, 2, platformSprites[28]);
        PlaceUI(7, 2, platformSprites[29]);
        PlaceUI(8, 2, platformSprites[30]);
        PlaceUI(9, 2, platformSprites[31]);
        PlaceUI(10, 2, platformSprites[32]);
        PlaceUI(11, 2, platformSprites[33]);
        PlaceUI(12, 2, platformSprites[34]);
        PlaceUI(13, 2, platformSprites[35]);
        PlaceUI(14, 2, platformSprites[36]);
        PlaceUI(15, 2, platformSprites[37]);
        PlaceUI(16, 2, platformSprites[38]);

        PlaceUI(5, 3, platformSprites[15]);
        PlaceUI(6, 3, platformSprites[16]);
        PlaceUI(7, 3, platformSprites[17]);
        PlaceUI(8, 3, platformSprites[18]);
        PlaceUI(9, 3, platformSprites[19]);
        PlaceUI(10, 3, platformSprites[20]);
        PlaceUI(11, 3, platformSprites[21]);
        PlaceUI(12, 3, platformSprites[22]);
        PlaceUI(13, 3, platformSprites[23]);
        PlaceUI(14, 3, platformSprites[24]);
        PlaceUI(15, 3, platformSprites[25]);
        PlaceUI(16, 3, platformSprites[26]);

        PlaceUI(5, 4, platformSprites[6]);
        PlaceUI(6, 4, platformSprites[7]);
        PlaceUI(7, 4, platformSprites[8]);
        PlaceUI(8, 4, platformSprites[9]);
        PlaceUI(9, 4, platformSprites[10]);
        PlaceUI(10, 4, platformSprites[11]);
        PlaceUI(11, 4, platformSprites[12]);
        PlaceUI(12, 4, platformSprites[13]);
        PlaceUI(13, 4, platformSprites[14]);

        PlaceUI(8, 5, platformSprites[0]);
        PlaceUI(9, 5, platformSprites[1]);
        PlaceUI(10, 5, platformSprites[2]);
        PlaceUI(11, 5, platformSprites[3]);
        PlaceUI(12, 5, platformSprites[4]);
        PlaceUI(13, 5, platformSprites[5]);

        //players
        int place = 0;

        foreach (var players in Highscore.SortPlayersByWins())
        {
            place++;

            if (place is 1)
            {
                PlacePlayer(11, 5, players[0], trophySprite);

                if (players.Count > 1)
                    PlacePlayer(10, 5, players[1], trophySprite);

                if (players.Count > 2)
                    PlacePlayer(9, 5, players[2], trophySprite);

                if (players.Count > 3)
                    PlacePlayer(12, 5, players[3], trophySprite);
            }

            if (place is 2)
            {
                PlacePlayer(7, 4, players[0]);

                if (players.Count > 1)
                    PlacePlayer(6, 4, players[1]);

                if (players.Count > 2)
                    PlacePlayer(5, 4, players[2]);
            }

            if (place is 3)
            {
                PlacePlayer(15, 3, players[0]);

                if (players.Count > 1)
                    PlacePlayer(14, 3, players[1]);
            }
        }

        //text (large)
        grid = new Grid(new Vector2Int(28, 1));
        PlaceLargeText(4, 25 * 16 + 4, LevelManager.Level.WorldName);
        PlaceLargeText(4, 21 * 16, LevelManager.Level.Name);

        //text (small)
        grid = new Grid(new Vector2Int(16, 16));
        PlaceText(6, 26, $"WORLD {(LevelManager.GameRound + Setup.LevelOffset) / 10 + 1}");
        PlaceText(6, 22, $"LEVEL {(LevelManager.GameRound + Setup.LevelOffset) % 10 + 1}");

        PlaceText(6, 17, $"DURING {LevelManager.Level.TimeOfDay.ToString().ToUpper()}");
        PlaceText(6, 16, "OPT.  : *****");
        PlaceText(6, 15, "ILL.  : **");

        PlaceText(28, 10, $"ROUND {LevelManager.GameRound + 1}".PadLeft(10));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("World");

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Setup");
    }

    private void PlacePlayer(int x, int y, int playerNumber, Sprite extraSprite = null)
    {
        PlaceUI(x, y, playerSprites[Setup.AvatarOffsets[playerNumber] - 1], -1);
        PlaceUI(x, y, extraSprite, 1);
    }
}
