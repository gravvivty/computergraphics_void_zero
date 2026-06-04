using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidZero.Game.Entities;

namespace VoidZero.UI
{
    public class StageStats
    {
        public float ElapsedTime { get; private set; }
        public float CompletionTime { get; private set; }
        public bool Completed { get; private set; }
        public int HitsTaken { get; private set; }
        public int EnemiesKilled { get; private set; }
        public int Score { get; private set; }
        public int FinalScore { get; private set; }

        public float KillMultiplier { get; private set; }
        public float HitMultiplier { get; private set; }
        public float TimeMultiplier { get; private set; }

        public void Update(float dt)
        {
            if (!Completed)
            {
                ElapsedTime += dt;
            }
        }

        public void RegisterHit()
        {
            HitsTaken++;
        }

        public void RegisterKill(Entity entity)
        {
            EnemiesKilled++;
            Score += entity.Score;
        }

        public void Complete()
        {
            Completed = true;
            CompletionTime = ElapsedTime;
            CalculateFinalScore();
        }

        private void CalculateFinalScore()
        {
            KillMultiplier = EnemiesKilled switch
            {
                < 10 => 1.02f,
                < 20 => 1.04f,
                < 30 => 1.06f,
                < 40 => 1.08f,
                < 50 => 1.10f,
                < 60 => 1.12f,
                < 70 => 1.14f,
                < 80 => 1.16f,
                < 90 => 1.18f,
                < 100 => 1.20f,
                _ => 1.30f,
            };

            HitMultiplier = HitsTaken switch
            {
                0 => 2.0f,
                <= 3 => 1.4f,
                <= 6 => 1.3f,
                <= 10 => 1.2f,
                <= 15 => 1.1f,
                _ => 1.0f,
            };

            TimeMultiplier = CompletionTime switch
            {
                < 60 => 2.0f,
                < 120 => 1.7f,
                < 150 => 1.5f,
                < 180 => 1.3f,
                < 210 => 1.2f,
                < 240 => 1.1f,
                _ => 1.0f,
            };

            FinalScore = (int)(Score * KillMultiplier * HitMultiplier * TimeMultiplier);
        }
    }
}
