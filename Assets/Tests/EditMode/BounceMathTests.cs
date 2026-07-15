using NUnit.Framework;
using ReflecBeatSimulator.Core;

namespace ReflecBeatSimulator.Tests
{
    public class BounceMathTests
    {
        [Test]
        public void BounceX_ReturnsStartX_WhenDisabled()
        {
            Assert.That(BounceMath.BounceX(0.3f, 0.5f, 1f, 0), Is.EqualTo(0.3f).Within(1e-6f));
            Assert.That(BounceMath.BounceX(0.3f, 0.5f, 0f, 2), Is.EqualTo(0.3f).Within(1e-6f));
        }

        [Test]
        public void BounceX_StartsAtStartX()
        {
            Assert.That(BounceMath.BounceX(0.2f, 0f, 1f, 2), Is.EqualTo(0.2f).Within(1e-6f));
        }

        [Test]
        public void BounceX_EndsAtStartX_WhenWallHitsEven()
        {
            Assert.That(BounceMath.BounceX(0.2f, 1f, 1f, 2), Is.EqualTo(0.2f).Within(1e-6f));
            Assert.That(BounceMath.BounceX(0.7f, 1f, 1f, 4), Is.EqualTo(0.7f).Within(1e-6f));
        }

        [Test]
        public void BounceX_EndsAtOppositeWall_WhenWallHitsOdd()
        {
            Assert.That(BounceMath.BounceX(0.2f, 1f, 1f, 1), Is.EqualTo(0.8f).Within(1e-6f));
            Assert.That(BounceMath.BounceX(0.2f, 1f, 1f, 3), Is.EqualTo(0.8f).Within(1e-6f));
        }

        [Test]
        public void BounceX_HitsRightWallAtExpectedTime()
        {
            // startX=0.2, wallHits=2 -> travels two widths; first wall hit at right (1)
            // when position = startX + 2*t = 1, i.e. t = (1 - startX) / 2 = 0.4.
            Assert.That(BounceMath.BounceX(0.2f, 0.4f, 1f, 2), Is.EqualTo(1f).Within(1e-6f));

            // startX=0.9, wallHits=2 -> first wall hit at right when t = (1 - 0.9) / 2 = 0.05.
            Assert.That(BounceMath.BounceX(0.9f, 0.05f, 1f, 2), Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void BounceX_HitsLeftWallAtExpectedTime()
        {
            // startX=0.2, wallHits=2 -> left wall (0) is hit at position = 2,
            // i.e. t = (2 - startX) / 2 = 0.9.
            Assert.That(BounceMath.BounceX(0.2f, 0.9f, 1f, 2), Is.EqualTo(0f).Within(1e-6f));

            // startX=0.9, wallHits=2 -> left wall is hit at position = 2,
            // i.e. t = (2 - 0.9) / 2 = 0.55.
            Assert.That(BounceMath.BounceX(0.9f, 0.55f, 1f, 2), Is.EqualTo(0f).Within(1e-6f));
        }

        [Test]
        public void BounceX_StaysWithinBounds()
        {
            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                float x = BounceMath.BounceX(0.15f, t, 1f, 4);
                Assert.That(x, Is.GreaterThanOrEqualTo(-1e-6f));
                Assert.That(x, Is.LessThanOrEqualTo(1f + 1e-6f));
            }
        }

        [Test]
        public void BounceX_MovesLeftFirst_WhenDirectionNegative()
        {
            // startX=0.8, wallHits=2, direction=-1 -> first wall hit at left (0)
            // when position = 0.8 - 2*t = 0, i.e. t = 0.4.
            Assert.That(BounceMath.BounceX(0.8f, 0.4f, 1f, 2, -1f), Is.EqualTo(0f).Within(1e-6f));

            // At t=1 it should end back at startX because wallHits is even.
            Assert.That(BounceMath.BounceX(0.8f, 1f, 1f, 2, -1f), Is.EqualTo(0.8f).Within(1e-6f));
        }

        [Test]
        public void PingPong_BouncesBetweenZeroAndLength()
        {
            Assert.That(BounceMath.PingPong(0.25f, 1f), Is.EqualTo(0.25f).Within(1e-6f));
            Assert.That(BounceMath.PingPong(1.25f, 1f), Is.EqualTo(0.75f).Within(1e-6f));
            Assert.That(BounceMath.PingPong(2.25f, 1f), Is.EqualTo(0.25f).Within(1e-6f));
            Assert.That(BounceMath.PingPong(-0.25f, 1f), Is.EqualTo(0.25f).Within(1e-6f));
        }
    }
}
