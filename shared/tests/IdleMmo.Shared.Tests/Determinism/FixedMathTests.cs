using IdleMmo.Shared.Determinism;

namespace IdleMmo.Shared.Tests.Determinism;

[TestFixture]
public sealed class FixedMathTests
{
    [TestCase(100, 1000, 100)]
    [TestCase(100, 1100, 110)]
    [TestCase(100, 1075, 107)]   // truncated, 107.5 → 107
    [TestCase(1000, 1500, 1500)]
    [TestCase(0, 1234, 0)]
    [TestCase(-100, 1100, -110)]
    public void MulPermille_TruncatesTowardZero(int value, int permille, int expected)
    {
        Assert.That(FixedMath.MulPermille(value, permille), Is.EqualTo(expected));
    }

    [TestCase(100, 1075, 108)]  // 107.5 rounds up
    [TestCase(100, 1074, 107)]  // 107.4 rounds down
    [TestCase(100, 1100, 110)]
    [TestCase(0, 1234, 0)]
    public void MulPermilleRounded_RoundsHalfUp(int value, int permille, int expected)
    {
        Assert.That(FixedMath.MulPermilleRounded(value, permille), Is.EqualTo(expected));
    }

    [Test]
    public void Clamp_BasicCases()
    {
        Assert.That(FixedMath.Clamp(5, 0, 10), Is.EqualTo(5));
        Assert.That(FixedMath.Clamp(-5, 0, 10), Is.EqualTo(0));
        Assert.That(FixedMath.Clamp(20, 0, 10), Is.EqualTo(10));
        Assert.That(FixedMath.Clamp(0, 0, 0), Is.EqualTo(0));
    }

    [Test]
    public void Clamp_Throws_WhenMaxLessThanMin()
    {
        Assert.Throws<ArgumentException>(() => FixedMath.Clamp(5, 10, 0));
    }

    [TestCase(1100, 1200, 1320)]
    [TestCase(1000, 1500, 1500)]
    [TestCase(1000, 1000, 1000)]
    [TestCase(2000, 2000, 4000)]
    public void CombinePermille_Multiplies(int a, int b, int expected)
    {
        Assert.That(FixedMath.CombinePermille(a, b), Is.EqualTo(expected));
    }
}
