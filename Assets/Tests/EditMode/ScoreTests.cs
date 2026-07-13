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
    }
}
