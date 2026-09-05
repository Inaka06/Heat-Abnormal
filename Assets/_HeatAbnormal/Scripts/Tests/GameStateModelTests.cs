using NUnit.Framework;

public class GameStateModelTests
{
    [Test]
    public void DefaultsMatchDesignDocument()
    {
        var state = new GameStateModel();

        Assert.AreEqual(350000, state.Dana);
        Assert.AreEqual(60, state.KepuasanRakyat);
        Assert.AreEqual(0, state.KekuatanPolitik);
        Assert.IsEmpty(state.SelectedPolitikus);
        Assert.IsFalse(state.IsGameOver);
        Assert.IsFalse(state.IsGameWon);
    }

    [Test]
    public void PoliticianSelectionRequiresExactlyThree()
    {
        var state = new GameStateModel();

        Assert.IsFalse(state.SetSelectedPolitikus(new object[] { new object(), new object() }));
        Assert.IsTrue(state.SetSelectedPolitikus(new object[] { new object(), new object(), new object() }));
        Assert.AreEqual(3, state.SelectedPolitikus.Count);
    }
}
