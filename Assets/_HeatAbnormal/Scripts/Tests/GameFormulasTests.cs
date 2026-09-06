using NUnit.Framework;

public class GameFormulasTests
{
    [Test] public void AvgKepercayaanPublik_Normal() => Assert.AreEqual(60f, GameFormulas.AvgKepercayaanPublik(50f, 60f, 70f));
    [Test] public void AvgKepercayaanPublik_Boundary() => Assert.AreEqual(0f, GameFormulas.AvgKepercayaanPublik(0f, 0f, 0f));

    [Test] public void PengaruhPolitikusTerhadapEvent_Normal() => Assert.AreEqual(0.2f, GameFormulas.PengaruhPolitikusTerhadapEvent(70f));
    [Test] public void PengaruhPolitikusTerhadapEvent_Boundary() => Assert.AreEqual(-0.5f, GameFormulas.PengaruhPolitikusTerhadapEvent(0f));

    [Test] public void DampakEventTerhadapKepuasan_Normal() => Assert.AreEqual(12f, GameFormulas.DampakEventTerhadapKepuasan(10f, 0.2f));
    [Test] public void DampakEventTerhadapKepuasan_Boundary() => Assert.AreEqual(0f, GameFormulas.DampakEventTerhadapKepuasan(0f, 1f));

    [Test] public void KepuasanPascaEvent_Normal() => Assert.AreEqual(55f, GameFormulas.KepuasanPascaEvent(60f, -5f));
    [Test] public void KepuasanPascaEvent_ClampsToHundred() => Assert.AreEqual(100f, GameFormulas.KepuasanPascaEvent(95f, 20f));

    [Test] public void AvgPembengkakanDanaPolitikus_Normal() => Assert.AreEqual(50f, GameFormulas.AvgPembengkakanDanaPolitikus(40f, 50f, 60f));
    [Test] public void AvgPembengkakanDanaPolitikus_Boundary() => Assert.AreEqual(100f, GameFormulas.AvgPembengkakanDanaPolitikus(100f, 100f, 100f));

    [Test] public void RisikoPembengkakanDanaTotal_Normal() => Assert.AreEqual(50f, GameFormulas.RisikoPembengkakanDanaTotal(40f, 60f));
    [Test] public void RisikoPembengkakanDanaTotal_Boundary() => Assert.AreEqual(0f, GameFormulas.RisikoPembengkakanDanaTotal(0f, 0f));

    [Test] public void PeluangPembengkakanDana_Normal() => Assert.AreEqual(20f, GameFormulas.PeluangPembengkakanDana(50f));
    [Test] public void PeluangPembengkakanDana_Boundary() => Assert.AreEqual(5f, GameFormulas.PeluangPembengkakanDana(0f));

    [Test] public void BesarPembengkakanDana_Normal() => Assert.AreEqual(5000f, GameFormulas.BesarPembengkakanDana(100000f, 0.05f));
    [Test] public void BesarPembengkakanDana_Boundary() => Assert.AreEqual(0f, GameFormulas.BesarPembengkakanDana(100000f, 0f));

    [Test] public void AvgLobbyPolitikus_Normal() => Assert.AreEqual(60f, GameFormulas.AvgLobbyPolitikus(50f, 60f, 70f));
    [Test] public void AvgLobbyPolitikus_Boundary() => Assert.AreEqual(100f, GameFormulas.AvgLobbyPolitikus(100f, 100f, 100f));

    [Test] public void PeluangLobbyBerhasil_Normal() => Assert.AreEqual(40f, GameFormulas.PeluangLobbyBerhasil(50f, 10f));
    [Test] public void PeluangLobbyBerhasil_ClampsToFive() => Assert.AreEqual(5f, GameFormulas.PeluangLobbyBerhasil(10f, 20f));

    [Test] public void FaktorKetahananEventAlam_Normal() => Assert.AreEqual(1f, GameFormulas.FaktorKetahananEventAlam(50f));
    [Test] public void FaktorKetahananEventAlam_Boundaries() { Assert.AreEqual(1.4f, GameFormulas.FaktorKetahananEventAlam(0f), 0.0001f); Assert.AreEqual(0.6f, GameFormulas.FaktorKetahananEventAlam(100f), 0.0001f); }

    [Test] public void DampakEventAlam_Normal() => Assert.AreEqual(4f, GameFormulas.DampakEventAlam(3f, 1.4f));
    [Test] public void DampakEventAlam_RoundsResult() => Assert.AreEqual(4f, GameFormulas.DampakEventAlam(3f, 1.2f));

    [Test] public void TingkatEventRakyat_Normal() => Assert.AreEqual(20f, GameFormulas.TingkatEventRakyat(100f, 50f));
    [Test] public void TingkatEventRakyat_ClampsToForty() => Assert.AreEqual(40f, GameFormulas.TingkatEventRakyat(0f, 0f));

    [Test] public void AvgPenyambunganPolitikus_Normal() => Assert.AreEqual(60f, GameFormulas.AvgPenyambunganPolitikus(50f, 60f, 70f));
    [Test] public void AvgPenyambunganPolitikus_Boundary() => Assert.AreEqual(100f, GameFormulas.AvgPenyambunganPolitikus(100f, 100f, 100f));

    [Test] public void PeluangPenyambunganBerhasil_Normal() => Assert.AreEqual(60f, GameFormulas.PeluangPenyambunganBerhasil(60f, 180f, 60f));
    [Test] public void PeluangPenyambunganBerhasil_ClampsToHundred() => Assert.AreEqual(100f, GameFormulas.PeluangPenyambunganBerhasil(100f, 300f, 100f));
}
