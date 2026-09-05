public static class SelectedStatsSystem
{
    public static float NaturalResistance(KontraktorData contractor)
    {
        return contractor == null ? 1.4f : GameFormulas.FaktorKetahananEventAlam(contractor.kekokohan);
    }
    public static float AveragePublicTrust(GameStateModel state)
    {
        var p1 = state.SelectedPolitikus[0] as PolitikusData; var p2 = state.SelectedPolitikus[1] as PolitikusData; var p3 = state.SelectedPolitikus[2] as PolitikusData;
        return p1 == null || p2 == null || p3 == null ? 0f : GameFormulas.AvgKepercayaanPublik(p1.kepercayaanPublik, p2.kepercayaanPublik, p3.kepercayaanPublik);
    }
    public static float ConnectionChance(GameStateModel state)
    {
        var p1 = state.SelectedPolitikus[0] as PolitikusData; var p2 = state.SelectedPolitikus[1] as PolitikusData; var p3 = state.SelectedPolitikus[2] as PolitikusData;
        return p1 == null || p2 == null || p3 == null ? 0f : GameFormulas.PeluangPenyambunganBerhasil(state.KepuasanRakyat, p1.lobby + p2.lobby + p3.lobby, GameFormulas.AvgPenyambunganPolitikus(p1.penyambungan, p2.penyambungan, p3.penyambungan));
    }
    public static float LobbyChance(GameStateModel state, int tier)
    {
        var p1 = state.SelectedPolitikus[0] as PolitikusData; var p2 = state.SelectedPolitikus[1] as PolitikusData; var p3 = state.SelectedPolitikus[2] as PolitikusData;
        if (p1 == null || p2 == null || p3 == null) return 0f;
        return GameFormulas.PeluangLobbyBerhasil(GameFormulas.AvgLobbyPolitikus(p1.lobby, p2.lobby, p3.lobby), tier == 0 ? 0f : tier == 1 ? 10f : 20f);
    }
}
