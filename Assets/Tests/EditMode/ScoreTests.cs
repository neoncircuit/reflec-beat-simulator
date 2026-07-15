using System;
using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class ScoreTests
    {
        [Test]
        public void AllJust_IsOneHundredPercent()
        {
            var score = new Score(4);
            for (int i = 0; i < 4; i++)
                score.Apply(Judgment.Just);

            Assert.That(score.AchievementRate, Is.EqualTo(100.0).Within(1e-9));
            Assert.That(score.MaxCombo, Is.EqualTo(4));
        }

        [Test]
        public void MixedGrades_ComputesAchievementRate()
        {
            var score = new Score(5);
            score.Apply(Judgment.Just);
            score.Apply(Judgment.Great);
            score.Apply(Judgment.Good);
            score.Apply(Judgment.Miss);
            score.Apply(Judgment.Just);

            // earned = 100 + 70 + 40 + 0 + 100 = 310; max = 500
            Assert.That(score.AchievementRate, Is.EqualTo(62.0).Within(1e-9));
        }

        [Test]
        public void MissResetsCombo()
        {
            var score = new Score(3);
            score.Apply(Judgment.Just);
            score.Apply(Judgment.Just);
            score.Apply(Judgment.Miss);

            Assert.That(score.Combo, Is.EqualTo(0));
            Assert.That(score.MaxCombo, Is.EqualTo(2));
        }

        [Test]
        public void ZeroNotes_IsOneHundredPercent()
        {
            var score = new Score(0);
            Assert.That(score.AchievementRate, Is.EqualTo(100.0).Within(1e-9));
        }

        [TestCase(100.0, "SSS")]
        [TestCase(99.5, "SS")]
        [TestCase(98.5, "S")]
        [TestCase(96.0, "AAA")]
        [TestCase(92.0, "AA")]
        [TestCase(87.0, "A")]
        [TestCase(82.0, "B")]
        [TestCase(75.0, "C")]
        [TestCase(65.0, "D")]
        [TestCase(50.0, "E")]
        public void AchievementRate_MapsToGrade(double rate, string expectedGrade)
        {
            var score = new Score(100);
            int justs = (int)Math.Round(rate);
            for (int i = 0; i < justs; i++)
                score.Apply(Judgment.Just);
            for (int i = justs; i < 100; i++)
                score.Apply(Judgment.Miss);

            Assert.That(score.Grade, Is.EqualTo(expectedGrade));
        }
    }
}
