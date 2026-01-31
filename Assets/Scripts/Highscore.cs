using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Highscore : UIScene
{
    private static Dictionary<int, Stats> PlayerStats = new();

    public Sprite[] otherSprites;

    void Awake()
    {
        var grid = new Grid(new Vector2Int(28, 28));

        int y = 19;
        for (int i = 0; i < 4; i++)
        {
            if (!PlayerStats.TryGetValue(i + 1, out var stats))
                continue;

            y -= 4;

            PlaceUI(1, y, playerSprites[i]);

            PlaceUI(3, y, otherSprites[0]);
            PlaceLargeText(4, y, Math.Min(stats.Wins, 999).ToString().PadLeft(3, '0'));

            PlaceUI(8, y, otherSprites[1]);
            PlaceLargeText(9, y, Math.Min(stats.Wins, 999).ToString().PadLeft(3, '0'));

            if (stats.Override.KeepForce)
                PlaceUI(13, y, otherSprites[otherSprites.Length - 1]);
            else
            {
                PlaceUI(13, y, otherSprites[2]);
                PlaceLargeText(14, y, Math.Min(stats.Bombs, 99).ToString().PadLeft(2, '0'));

                PlaceUI(17, y, otherSprites[3]);
                PlaceLargeText(18, y, Math.Min(stats.Strength, 99).ToString().PadLeft(2, '0'));
            }

            if (stats.WonLastRound) //store if this happened this round
            {
                if (stats.Wins % 10 is 0)
                {
                    PlaceText(10, y - 1, "+");
                    PlaceUI(11, y - 1, otherSprites[3]);
                }
                else if (stats.Wins % 5 is 0)
                {
                    PlaceText(10, y - 1, "+");
                    PlaceUI(11, y - 1, otherSprites[2]);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("LevelPreview");

        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Setup");
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
        stats ??= new Stats(0, false, 2, 1, null);

        if (stats.Override?.KeepForce is true)
            return (Math.Max(stats.Strength, stats.Override.Strength), Math.Max(stats.Bombs, stats.Override.Bombs), Math.Max(0, stats.Override.AtomicBombs));

        return (stats.Strength, stats.Bombs, 0);
    }

    public static List<List<int>> SortPlayersByWins()
    {
        var result = new Dictionary<int, List<int>>();

        foreach (var (number, stat) in PlayerStats)
        {
            if (result.TryGetValue(stat.Wins, out var players))
                players.Add(number);
            else
                result[stat.Wins] = new List<int>() { number };
        }

        if (result.Count is 0)
            return new List<List<int>> { Enumerable.Range(1, Setup.Players).ToList() };

        return result.OrderByDescending(x => x.Key)
                     .Select(x => x.Value)
                     .ToList();
    }

    public record PlayerDetails(int Number, int Strength, int Bombs, int AtomicBombs, bool KeepForce);
    private record Stats(int Wins, bool WonLastRound, int Strength, int Bombs, PlayerDetails Override)
    {
        public Stats Merge(PlayerDetails newDetails, bool winner)
        {
            var wins = winner ? Wins + 1 : Wins;
            var strength = 2 + wins / 10; //start with 2, plus one every 10 wins
            var bombs = 1 + wins / 10 + (wins % 10 >= 5 ? 1 : 0); //start with 1, plus one every 5 wins (but not 10th)

            var kf = newDetails.KeepForce;
            return new Stats(wins, winner, strength, bombs, new PlayerDetails(newDetails.Number, kf ? newDetails.Strength : strength, kf ? newDetails.Bombs : bombs, kf ? newDetails.AtomicBombs : 0, kf));
        }
    }
}
