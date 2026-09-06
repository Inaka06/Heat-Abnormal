using NUnit.Framework;
using UnityEngine;

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
        var first = ScriptableObject.CreateInstance<PolitikusData>();
        var second = ScriptableObject.CreateInstance<PolitikusData>();
        var third = ScriptableObject.CreateInstance<PolitikusData>();

        try
        {
            Assert.IsFalse(state.SetSelectedPolitikus(new PolitikusData[] { first, second }));
            Assert.IsTrue(state.SetSelectedPolitikus(new PolitikusData[] { first, second, third }));
            Assert.AreEqual(3, state.SelectedPolitikus.Count);
        }
        finally
        {
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(third);
        }
    }
}
