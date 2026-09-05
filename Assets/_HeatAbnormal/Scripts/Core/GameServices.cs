public static class GameServices
{
    private static IRandomService random = new SeededRandomService(0);

    public static IRandomService Random => random;

    public static void SetRandomSeed(int seed)
    {
        random = new SeededRandomService(seed);
    }
}
