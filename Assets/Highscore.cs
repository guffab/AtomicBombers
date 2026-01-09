using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Highscore : MonoBehaviour
{
    private static Dictionary<int, Stats> PlayerStats = new();

    public Sprite[] fontSprites;
    public Sprite[] playerSprites;
    public Sprite[] otherSprites;
    public GameObject fontPrefab;

    void Awake()
    {
        var grid = new Grid(new Vector2Int(28, 28));

        int y = 19;
        for (int i = 0; i < 4; i++)
        {
            if (!PlayerStats.TryGetValue(i + 1, out var stats))
                continue;

            y -= 4;

            //numbers as string to index into
            var winstr = Math.Min(stats.Wins, 999).ToString().PadLeft(3, '0');
            var bombstr = Math.Min(stats.Bombs, 99).ToString().PadLeft(2, '0');
            var strenstr = Math.Min(stats.Strength, 99).ToString().PadLeft(2, '0');

            PlaceUI(1, y, playerSprites[i]);

            PlaceUI(3, y, otherSprites[0]);
            PlaceUI(4, y, fontSprites[winstr[0]]);
            PlaceUI(5, y, fontSprites[winstr[1]]);
            PlaceUI(6, y, fontSprites[winstr[2]]);

            PlaceUI(8, y, otherSprites[1]);
            PlaceUI(9, y, fontSprites[winstr[0]]);
            PlaceUI(10, y, fontSprites[winstr[1]]);
            PlaceUI(11, y, fontSprites[winstr[2]]);

            if (stats.Override.KeepForce)
                PlaceUI(13, y, otherSprites[otherSprites.Length - 1]);
            else
            {
                PlaceUI(13, y, otherSprites[2]);
                PlaceUI(14, y, fontSprites[bombstr[0]]);
                PlaceUI(15, y, fontSprites[bombstr[1]]);

                PlaceUI(17, y, otherSprites[3]);
                PlaceUI(18, y, fontSprites[strenstr[0]]);
                PlaceUI(19, y, fontSprites[strenstr[1]]);
            }

            if (stats.WonLastRound) //store if this happened this round#
            {
                if (stats.Wins % 10 is 0)
                {
                    PlaceUI(10, y - 1, fontSprites['+']);
                    PlaceUI(11, y - 1, otherSprites[3]);
                }
                else if (stats.Wins % 5 is 0)
                {
                    PlaceUI(10, y - 1, fontSprites['+']);
                    PlaceUI(11, y - 1, otherSprites[2]);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("World");
    }

    private void PlaceUI(int x, int y, Sprite sprite)
    {
        var worldPos = Grid.Current.ToWorld(new Vector2Int(x, y));
        var dummy = Instantiate(fontPrefab, worldPos, Quaternion.identity);
        dummy.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public static void AddPlayerDetails(PlayerDetails details, bool winner)
    {
        PlayerStats.TryGetValue(details.Number, out var stats);
        stats ??= new Stats(0, false, 1, 1, null);
        PlayerStats[details.Number] = stats.Merge(details, winner);
    }

    public static (int Strength, int Bombs, int AtomicBombs) GetPlayerConfig(int number)
    {
        PlayerStats.TryGetValue(number, out var stats);
        stats ??= new Stats(0, false, 1, 1, null);

        if (stats.Override?.KeepForce is true)
            return (Math.Max(stats.Strength, stats.Override.Strength), Math.Max(stats.Bombs, stats.Override.Bombs), Math.Max(0, stats.Override.AtomicBombs));
        
        return (stats.Strength, stats.Bombs, 0);
    }

    public record PlayerDetails(int Number, int Strength, int Bombs, int AtomicBombs, bool KeepForce);
    private record Stats(int Wins, bool WonLastRound, int Strength, int Bombs, PlayerDetails Override)
    {
        public Stats Merge(PlayerDetails newDetails, bool winner)
        {
            var wins = winner ? Wins + 1 : Wins;
            var strength = 1 + wins / 10; //start with 1, plus one every 10 wins
            var bombs = 1 + wins / 10 + (wins % 10 >= 5 ? 1 : 0); //start with 1, plus one every 5 wins

            var kf = newDetails.KeepForce;
            return new Stats(wins, winner, strength, bombs, new PlayerDetails(newDetails.Number, kf ? newDetails.Strength : strength, kf ? newDetails.Bombs : bombs, kf ? newDetails.AtomicBombs : 0, kf));
        }
    }
}
