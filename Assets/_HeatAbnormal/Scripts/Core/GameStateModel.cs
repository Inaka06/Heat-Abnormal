using System;
using System.Collections.Generic;

public enum GameOverReason
{
    None,
    DanaHabis,
    Kiamat,
    KembaliKeBatuBara,
    BeralihKeGasAlam,
    Other
}

public sealed class GameStateModel
{
    public const int InitialDana = 350000;
    public const int InitialKepuasanRakyat = 60;
    public const int InitialKekuatanPolitik = 0;
    public const int MaxSelectedPolitikus = 3;

    public int Dana { get; set; } = InitialDana;
    public int KepuasanRakyat { get; set; } = InitialKepuasanRakyat;
    public int KekuatanPolitik { get; set; } = InitialKekuatanPolitik;
    public float ProgressPembangunanPeriode { get; set; }
    public int PeriodeTerpakai { get; set; }
    public PembangkitData SelectedPembangkit { get; set; }
    public KontraktorData SelectedKontraktor { get; set; }
    public List<PolitikusData> SelectedPolitikus { get; } = new List<PolitikusData>(MaxSelectedPolitikus);
    public int PeriodeDanaKosongBerturut { get; set; }
    public bool IsGameOver { get; private set; }
    public GameOverReason GameOverReason { get; private set; } = GameOverReason.None;
    public bool IsGameWon { get; private set; }

    public bool SetSelectedPolitikus(IEnumerable<PolitikusData> politikus)
    {
        if (politikus == null)
        {
            return false;
        }

        var selected = new List<PolitikusData>(politikus);
        if (selected.Count != MaxSelectedPolitikus)
        {
            return false;
        }

        SelectedPolitikus.Clear();
        SelectedPolitikus.AddRange(selected);
        return true;
    }

    public void MarkGameOver(GameOverReason reason)
    {
        IsGameOver = true;
        GameOverReason = reason;
        IsGameWon = false;
    }

    public void MarkGameWon()
    {
        IsGameWon = true;
        IsGameOver = false;
        GameOverReason = GameOverReason.None;
    }
}
