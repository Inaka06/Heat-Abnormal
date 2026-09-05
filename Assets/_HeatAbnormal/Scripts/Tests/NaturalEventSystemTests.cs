using NUnit.Framework;

public class NaturalEventSystemTests
{
    [Test]
    public void CumulativeRangesSelectExpectedEventTypes()
    {
        Assert.AreEqual(NaturalEventType.Badai, NaturalEventSystem.DetermineEventType(0f));
        Assert.AreEqual(NaturalEventType.Badai, NaturalEventSystem.DetermineEventType(84.99f));
        Assert.AreEqual(NaturalEventType.GempaBumi, NaturalEventSystem.DetermineEventType(85f));
        Assert.AreEqual(NaturalEventType.GempaBumi, NaturalEventSystem.DetermineEventType(99.89f));
        Assert.AreEqual(NaturalEventType.Kiamat, NaturalEventSystem.DetermineEventType(99.9f));
    }

    [Test]
    public void SeededDistributionIsWithinTwoPercentOfTarget()
    {
        const int samples = 100000;
        var service = new SeededRandomService(20260905);
        var storm = 0; var earthquake = 0; var apocalypse = 0;
        for (var i = 0; i < samples; i++)
        {
            switch (NaturalEventSystem.DetermineEventType(service.NextFloat(0f, 100f)))
            {
                case NaturalEventType.Badai: storm++; break;
                case NaturalEventType.GempaBumi: earthquake++; break;
                case NaturalEventType.Kiamat: apocalypse++; break;
            }
        }
        Assert.That((float)storm / samples, Is.InRange(.83f, .87f));
        Assert.That((float)earthquake / samples, Is.InRange(.009f, .029f));
        Assert.That((float)apocalypse / samples, Is.InRange(0f, .02f));
    }
}
