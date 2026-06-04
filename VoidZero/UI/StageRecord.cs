using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.UI
{
    public class StageRecord
    {
        public int FinalScore { get; set; }
        public float CompletionTime { get; set; }
        public int EnemiesKilled { get; set; }
        public int HitsTaken { get; set; }
    }

    public class StageHighScores
    {
        private static StageHighScores _instance;
        public static StageHighScores Instance => _instance ??= new StageHighScores();

        private readonly Dictionary<int, StageRecord> _records = new();

        public void Submit(int stageIndex, StageStats stats)
        {
            if (!_records.TryGetValue(stageIndex, out var existing) ||
                stats.FinalScore > existing.FinalScore)
            {
                _records[stageIndex] = new StageRecord
                {
                    FinalScore = stats.FinalScore,
                    CompletionTime = stats.CompletionTime,
                    EnemiesKilled = stats.EnemiesKilled,
                    HitsTaken = stats.HitsTaken,
                };
            }
        }

        public StageRecord GetRecord(int stageIndex) =>
            _records.TryGetValue(stageIndex, out var r) ? r : null;
    }
}
