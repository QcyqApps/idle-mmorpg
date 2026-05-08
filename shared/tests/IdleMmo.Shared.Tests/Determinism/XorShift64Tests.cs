using IdleMmo.Shared.Determinism;

namespace IdleMmo.Shared.Tests.Determinism;

[TestFixture]
public sealed class XorShift64Tests
{
    [Test]
    public void SameSeed_ProducesSameSequence()
    {
        var a = new XorShift64(42UL);
        var b = new XorShift64(42UL);

        for (int i = 0; i < 1000; i++)
        {
            Assert.That(a.NextUInt64(), Is.EqualTo(b.NextUInt64()),
                $"Sequence diverged at index {i}");
        }
    }

    [Test]
    public void DifferentSeeds_DivergeWithinFirstThousand()
    {
        var a = new XorShift64(1UL);
        var b = new XorShift64(2UL);

        bool diverged = false;
        for (int i = 0; i < 1000 && !diverged; i++)
        {
            if (a.NextUInt64() != b.NextUInt64())
            {
                diverged = true;
            }
        }
        Assert.That(diverged, Is.True);
    }

    [Test]
    public void ZeroSeed_DoesNotProduceAllZeros()
    {
        var rng = new XorShift64(0UL);
        ulong v1 = rng.NextUInt64();
        ulong v2 = rng.NextUInt64();
        Assert.That(v1, Is.Not.EqualTo(0UL));
        Assert.That(v2, Is.Not.EqualTo(v1));
    }

    [Test]
    public void NextInt_StaysWithinRange()
    {
        var rng = new XorShift64(0xCAFEBABEUL);
        for (int i = 0; i < 10_000; i++)
        {
            int v = rng.NextInt(-50, 50);
            Assert.That(v, Is.GreaterThanOrEqualTo(-50));
            Assert.That(v, Is.LessThan(50));
        }
    }

    [Test]
    public void NextInt_EmptyRange_Throws()
    {
        var rng = new XorShift64(1UL);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(5, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.NextInt(10, 5));
    }

    [Test]
    public void RollPermille_ZeroAndThousand_AreDeterministic()
    {
        var rng = new XorShift64(123UL);
        Assert.That(rng.RollPermille(0), Is.False);
        Assert.That(rng.RollPermille(1000), Is.True);
        Assert.That(rng.RollPermille(-50), Is.False);
        Assert.That(rng.RollPermille(2000), Is.True);
    }

    [Test]
    public void RollPermille_500_IsApproximatelyHalf()
    {
        var rng = new XorShift64(7UL);
        int hits = 0;
        const int iterations = 100_000;
        for (int i = 0; i < iterations; i++)
        {
            if (rng.RollPermille(500)) hits++;
        }
        // Tolerate +/- 2 percentage points; XorShift mod-bias is small at this range.
        double ratio = (double)hits / iterations;
        Assert.That(ratio, Is.InRange(0.48, 0.52));
    }

    [Test]
    public void State_IsObservable_ForReplay()
    {
        var rng = new XorShift64(999UL);
        ulong before = rng.State;
        rng.NextUInt64();
        ulong after = rng.State;
        Assert.That(before, Is.Not.EqualTo(after));
    }
}
