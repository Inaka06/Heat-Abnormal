using System;

public sealed class SeededRandomService : IRandomService
{
    private readonly Random random;

    public int Seed { get; }

    public SeededRandomService(int seed)
    {
        Seed = seed;
        random = new Random(seed);
    }

    public float NextFloat(float min, float max)
    {
        if (min > max)
        {
            throw new ArgumentException("min must be less than or equal to max.");
        }

        return min + ((float)random.NextDouble() * (max - min));
    }

    public int NextInt(int min, int max)
    {
        return random.Next(min, max);
    }

    public bool Roll(float chancePercent)
    {
        if (chancePercent <= 0f)
        {
            return false;
        }

        if (chancePercent >= 100f)
        {
            return true;
        }

        return random.NextDouble() < chancePercent / 100f;
    }
}
