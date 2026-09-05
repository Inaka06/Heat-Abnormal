public interface IRandomService
{
    float NextFloat(float min, float max);
    int NextInt(int min, int max);
    bool Roll(float chancePercent);
}
