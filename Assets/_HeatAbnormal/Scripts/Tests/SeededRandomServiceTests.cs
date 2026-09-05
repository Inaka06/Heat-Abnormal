using NUnit.Framework;

public class SeededRandomServiceTests
{
    [Test]
    public void SameSeedProducesIdenticalFloatAndIntSequence()
    {
        var first = new SeededRandomService(12345);
        var second = new SeededRandomService(12345);

        for (var i = 0; i < 10; i++)
        {
            Assert.AreEqual(first.NextFloat(-10f, 25f), second.NextFloat(-10f, 25f));
            Assert.AreEqual(first.NextInt(0, 100), second.NextInt(0, 100));
        }
    }

    [Test]
    public void GameServicesCanResetRandomSequenceWithSameSeed()
    {
        GameServices.SetRandomSeed(77);
        var firstFloat = GameServices.Random.NextFloat(0f, 1f);
        var firstInt = GameServices.Random.NextInt(0, 1000);

        GameServices.SetRandomSeed(77);
        Assert.AreEqual(firstFloat, GameServices.Random.NextFloat(0f, 1f));
        Assert.AreEqual(firstInt, GameServices.Random.NextInt(0, 1000));
    }
}
