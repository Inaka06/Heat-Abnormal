using NUnit.Framework;

public class PeopleEventSystemTests
{
    [Test]
    public void EventRateUsesSelectedContractorAndThreePoliticians()
    {
        var expected = GameFormulas.TingkatEventRakyat(91f, GameFormulas.AvgKepercayaanPublik(91f, 76f, 36f));
        Assert.AreEqual(expected, PeopleEventSystem.CalculateEventRate(91f, 91f, 76f, 36f));
    }

    [Test]
    public void EventRateDelegatesBoundaryClampingToFormula()
    {
        Assert.AreEqual(10f, PeopleEventSystem.CalculateEventRate(100f, 100f, 100f, 100f));
    }
}
