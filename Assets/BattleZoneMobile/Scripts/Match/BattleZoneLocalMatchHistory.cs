using System;
using UnityEngine;

namespace BattleZoneMobile
{
    public static class BattleZoneLocalMatchHistory
    {
        private const string MatchesKey = "BattleZone.Local.TotalMatches";
        private const string WinsKey = "BattleZone.Local.TotalWins";
        private const string KillsKey = "BattleZone.Local.TotalKills";
        private const string BestPlacementKey = "BattleZone.Local.BestPlacement";
        private const string HistoryKey = "BattleZone.Local.MatchHistory";
        private const int MaxHistoryEntries = 10;

        public static string SaveMatch(string result, int kills, int placement, int totalPlayers, float survivalSeconds, float damage, int shots, int hits)
        {
            int totalMatches = PlayerPrefs.GetInt(MatchesKey, 0) + 1;
            int totalWins = PlayerPrefs.GetInt(WinsKey, 0) + (placement == 1 ? 1 : 0);
            int totalKills = PlayerPrefs.GetInt(KillsKey, 0) + Mathf.Max(0, kills);
            int previousBest = PlayerPrefs.GetInt(BestPlacementKey, totalPlayers > 0 ? totalPlayers : 99);
            int bestPlacement = Mathf.Min(previousBest <= 0 ? placement : previousBest, Mathf.Max(1, placement));

            PlayerPrefs.SetInt(MatchesKey, totalMatches);
            PlayerPrefs.SetInt(WinsKey, totalWins);
            PlayerPrefs.SetInt(KillsKey, totalKills);
            PlayerPrefs.SetInt(BestPlacementKey, bestPlacement);

            string record = BuildRecord(result, kills, placement, totalPlayers, survivalSeconds, damage, shots, hits);
            string history = PlayerPrefs.GetString(HistoryKey, string.Empty);
            history = string.IsNullOrEmpty(history) ? record : $"{record}||{history}";
            string[] entries = history.Split(new[] { "||" }, StringSplitOptions.None);
            if (entries.Length > MaxHistoryEntries)
            {
                Array.Resize(ref entries, MaxHistoryEntries);
            }

            PlayerPrefs.SetString(HistoryKey, string.Join("||", entries));
            PlayerPrefs.Save();

            return $"Career: {totalWins}W / {totalMatches}M | Kills {totalKills} | Best #{bestPlacement}";
        }

        private static string BuildRecord(string result, int kills, int placement, int totalPlayers, float survivalSeconds, float damage, int shots, int hits)
        {
            int wholeSeconds = Mathf.FloorToInt(Mathf.Max(0f, survivalSeconds));
            int minutes = wholeSeconds / 60;
            int seconds = wholeSeconds % 60;
            float accuracy = shots <= 0 ? 0f : Mathf.Clamp01((float)hits / shots) * 100f;
            return $"{DateTime.Now:MM/dd HH:mm} {result} #{placement}/{Mathf.Max(1, totalPlayers)} K{kills} D{Mathf.RoundToInt(damage)} A{accuracy:0}% {minutes:00}:{seconds:00}";
        }
    }
}
