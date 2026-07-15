using System.Collections.Generic;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class DuelStateTests
    {
        private static RivalConfig DefaultConfig => RivalConfig.Default;

        private static Note PlayerNote(bool gold = false) =>
            new Note { Type = NoteType.Tap, X = 0.5f, IsRival = false, IsGold = gold };

        private static Note RivalNote(bool gold = false) =>
            new Note { Type = NoteType.Tap, X = 0.5f, IsRival = true, IsGold = gold };

        [Test]
        public void Construct_UsesConfigHp()
        {
            var config = new RivalConfig { PlayerHp = 80, RivalHp = 120 };
            var duel = new DuelState(config);

            Assert.That(duel.MaxPlayerHp, Is.EqualTo(80));
            Assert.That(duel.PlayerHp, Is.EqualTo(80));
            Assert.That(duel.MaxRivalHp, Is.EqualTo(120));
            Assert.That(duel.RivalHp, Is.EqualTo(120));
        }

        [Test]
        public void Construct_NullConfigThrows()
        {
            Assert.That(() => new DuelState(null), Throws.ArgumentNullException);
        }

        [Test]
        public void OnPlayerJudgment_AddsGaugeByGrade()
        {
            var duel = new DuelState(DefaultConfig);

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Just);
            Assert.That(duel.Gauge, Is.EqualTo(Constants.JustGaugeGain));

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Great);
            Assert.That(duel.Gauge, Is.EqualTo(Constants.JustGaugeGain + Constants.GreatGaugeGain));

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Good);
            Assert.That(duel.Gauge, Is.EqualTo(Constants.JustGaugeGain + Constants.GreatGaugeGain + Constants.GoodGaugeGain));
        }

        [Test]
        public void OnPlayerJudgment_GoldAddsBonusGauge()
        {
            var duel = new DuelState(DefaultConfig);

            duel.OnPlayerJudgment(PlayerNote(gold: true), Judgment.Just);
            Assert.That(duel.Gauge, Is.EqualTo(Constants.JustGaugeGain + Constants.GoldGaugeBonus));
        }

        [Test]
        public void OnPlayerJudgment_GaugeCapsAtMax()
        {
            var duel = new DuelState(DefaultConfig);

            for (int i = 0; i < 20; i++)
                duel.OnPlayerJudgment(PlayerNote(), Judgment.Just);

            Assert.That(duel.Gauge, Is.EqualTo(Constants.MaxGauge));
        }

        [Test]
        public void OnPlayerJudgment_MissDamagesPlayer()
        {
            var duel = new DuelState(DefaultConfig);

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss);

            Assert.That(duel.PlayerHp, Is.EqualTo(DefaultConfig.PlayerHp - Constants.PlayerMissDamage));
            Assert.That(duel.Gauge, Is.EqualTo(0));
        }

        [Test]
        public void OnCpuJudgment_DamagesPlayer()
        {
            var duel = new DuelState(DefaultConfig);

            duel.OnCpuJudgment(RivalNote(), Judgment.Just);

            Assert.That(duel.PlayerHp, Is.EqualTo(DefaultConfig.PlayerHp - Constants.CpuHitDamage));
        }

        [Test]
        public void OnCpuJudgment_GoldDealsExtraDamage()
        {
            var duel = new DuelState(DefaultConfig);

            duel.OnCpuJudgment(RivalNote(gold: true), Judgment.Just);

            Assert.That(duel.PlayerHp, Is.EqualTo(DefaultConfig.PlayerHp - Constants.CpuHitDamage - Constants.CpuGoldDamageBonus));
        }

        [Test]
        public void ActivateJustReflect_RequiresFullGauge()
        {
            var duel = new DuelState(DefaultConfig);

            Assert.That(duel.ActivateJustReflect(), Is.False);
            Assert.That(duel.RivalHp, Is.EqualTo(DefaultConfig.RivalHp));

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Just);
            Assert.That(duel.ActivateJustReflect(), Is.False);
        }

        [Test]
        public void ActivateJustReflect_DamagesRivalAndResetsGauge()
        {
            var duel = new DuelState(DefaultConfig);

            // Fill the gauge with JUST judgments.
            int hits = (int)System.Math.Ceiling((double)Constants.MaxGauge / Constants.JustGaugeGain);
            for (int i = 0; i < hits; i++)
                duel.OnPlayerJudgment(PlayerNote(), Judgment.Just);

            Assert.That(duel.Gauge, Is.EqualTo(Constants.MaxGauge));
            Assert.That(duel.ActivateJustReflect(), Is.True);
            Assert.That(duel.Gauge, Is.EqualTo(0));
            Assert.That(duel.RivalHp, Is.EqualTo(DefaultConfig.RivalHp - Constants.ReflectDamage));
        }

        [Test]
        public void ActivateJustReflect_KillsRivalAndFinishes()
        {
            var duel = new DuelState(DefaultConfig);

            // Deal enough reflect damage to drop rival HP to zero.
            while (duel.RivalHp > 0)
            {
                int hits = (int)System.Math.Ceiling((double)Constants.MaxGauge / Constants.JustGaugeGain);
                for (int i = 0; i < hits; i++)
                    duel.OnPlayerJudgment(PlayerNote(), Judgment.Just);
                duel.ActivateJustReflect();
            }

            Assert.That(duel.RivalHp, Is.EqualTo(0));
            Assert.That(duel.IsFinished, Is.True);
            Assert.That(duel.Outcome, Is.EqualTo(DuelOutcome.Win));
        }

        [Test]
        public void MissReducesPlayerHpAndCanFinish()
        {
            var config = new RivalConfig { PlayerHp = 10, RivalHp = 100 };
            var duel = new DuelState(config);

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss);
            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss);

            Assert.That(duel.PlayerHp, Is.EqualTo(0));
            Assert.That(duel.IsFinished, Is.True);
            Assert.That(duel.Outcome, Is.EqualTo(DuelOutcome.Lose));
        }

        [Test]
        public void ResolveOutcome_ComparesRemainingHp()
        {
            var duel = new DuelState(DefaultConfig);

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss); // player loses 5 HP
            duel.OnCpuJudgment(RivalNote(), Judgment.Just);     // player loses 8 HP
            duel.ResolveOutcome();

            Assert.That(duel.IsFinished, Is.True);
            Assert.That(duel.Outcome, Is.EqualTo(DuelOutcome.Lose)); // rival HP still 100, player 87
        }

        [Test]
        public void ResolveOutcome_DrawWhenRemainingHpEqual()
        {
            var config = new RivalConfig { PlayerHp = 23, RivalHp = 10 };
            var duel = new DuelState(config);

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss); // player -5
            duel.OnCpuJudgment(RivalNote(), Judgment.Great);    // player -8
            duel.ResolveOutcome();

            Assert.That(duel.Outcome, Is.EqualTo(DuelOutcome.Draw));
        }

        [Test]
        public void NoOpAfterFinished()
        {
            var config = new RivalConfig { PlayerHp = 10, RivalHp = 100 };
            var duel = new DuelState(config);

            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss);
            duel.OnPlayerJudgment(PlayerNote(), Judgment.Miss);
            Assert.That(duel.IsFinished, Is.True);

            int playerHp = duel.PlayerHp;
            duel.OnCpuJudgment(RivalNote(), Judgment.Just);
            Assert.That(duel.PlayerHp, Is.EqualTo(playerHp));
        }
    }
}
