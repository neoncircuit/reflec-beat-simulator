using System;

namespace ReflecBeatSimulator.Core
{
    /// <summary>Outcome of a single-player duel chart.</summary>
    public enum DuelOutcome
    {
        Win,
        Lose,
        Draw
    }

    /// <summary>Pure C# duel state: HP bars, Just Reflect gauge, and finish outcome.</summary>
    public sealed class DuelState
    {
        public int MaxPlayerHp { get; }
        public int MaxRivalHp { get; }

        public int PlayerHp { get; private set; }
        public int RivalHp { get; private set; }
        public int Gauge { get; private set; }

        public bool IsFinished { get; private set; }
        public DuelOutcome Outcome { get; private set; }

        public DuelState(RivalConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            MaxPlayerHp = config.PlayerHp;
            MaxRivalHp = config.RivalHp;
            PlayerHp = MaxPlayerHp;
            RivalHp = MaxRivalHp;
        }

        /// <summary>Applies a player judgment, adding gauge and handling misses.</summary>
        public void OnPlayerJudgment(Note note, Judgment grade)
        {
            if (IsFinished)
                return;

            if (grade == Judgment.Miss)
            {
                DamagePlayer(Constants.PlayerMissDamage);
                return;
            }

            int gain = GradeToGauge(grade);
            if (note != null && note.IsGold)
                gain += Constants.GoldGaugeBonus;

            Gauge = Math.Min(Constants.MaxGauge, Gauge + gain);
        }

        /// <summary>Applies a CPU judgment on a rival note, damaging the player.</summary>
        public void OnCpuJudgment(Note note, Judgment grade)
        {
            if (IsFinished)
                return;

            int damage = Constants.CpuHitDamage;
            if (note != null && note.IsGold)
                damage += Constants.CpuGoldDamageBonus;

            DamagePlayer(damage);
        }

        /// <summary>Attempts to activate Just Reflect. Returns true if the attack was used.</summary>
        public bool ActivateJustReflect()
        {
            if (IsFinished || Gauge < Constants.MaxGauge)
                return false;

            Gauge = 0;
            DamageRival(Constants.ReflectDamage);
            return true;
        }

        /// <summary>Force-finish the duel using remaining HP. Call after the chart ends if not already finished.</summary>
        public DuelOutcome ResolveOutcome()
        {
            if (IsFinished)
                return Outcome;

            IsFinished = true;
            Outcome = DetermineOutcome();
            return Outcome;
        }

        private void DamagePlayer(int damage)
        {
            PlayerHp = Math.Max(0, PlayerHp - damage);
            CheckFinish();
        }

        private void DamageRival(int damage)
        {
            RivalHp = Math.Max(0, RivalHp - damage);
            CheckFinish();
        }

        private void CheckFinish()
        {
            if (IsFinished)
                return;

            if (PlayerHp <= 0 || RivalHp <= 0)
            {
                IsFinished = true;
                Outcome = DetermineOutcome();
            }
        }

        private DuelOutcome DetermineOutcome()
        {
            if (RivalHp <= 0 && PlayerHp <= 0)
                return DuelOutcome.Draw;
            if (RivalHp <= 0)
                return DuelOutcome.Win;
            if (PlayerHp <= 0)
                return DuelOutcome.Lose;

            if (PlayerHp > RivalHp)
                return DuelOutcome.Win;
            if (PlayerHp < RivalHp)
                return DuelOutcome.Lose;
            return DuelOutcome.Draw;
        }

        private static int GradeToGauge(Judgment grade)
        {
            return grade switch
            {
                Judgment.Just => Constants.JustGaugeGain,
                Judgment.Great => Constants.GreatGaugeGain,
                Judgment.Good => Constants.GoodGaugeGain,
                _ => 0
            };
        }
    }
}
