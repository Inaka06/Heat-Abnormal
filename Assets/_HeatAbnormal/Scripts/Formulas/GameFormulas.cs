using System;

public static class GameFormulas
{
    /// <summary>Calculates T = (T1 + T2 + T3) / 3.</summary>
    public static float AvgKepercayaanPublik(float t1, float t2, float t3)
    {
        return (t1 + t2 + t3) / 3f;
    }

    /// <summary>Calculates Mr = (T - 50) / 100.</summary>
    public static float PengaruhPolitikusTerhadapEvent(float avgT)
    {
        return (avgT - 50f) / 100f;
    }

    /// <summary>Calculates Es = Bs * (1 + Mr).</summary>
    public static float DampakEventTerhadapKepuasan(float bs, float mr)
    {
        return bs * (1f + mr);
    }

    /// <summary>Calculates St+1 = Clamp(S0 + Es, 0, 100).</summary>
    public static float KepuasanPascaEvent(float s0, float es)
    {
        return Clamp(s0 + es, 0f, 100f);
    }

    /// <summary>Calculates Rp = (P1 + P2 + P3) / 3.</summary>
    public static float AvgPembengkakanDanaPolitikus(float p1, float p2, float p3)
    {
        return (p1 + p2 + p3) / 3f;
    }

    /// <summary>Calculates Rd = (Kd + Rp) / 2.</summary>
    public static float RisikoPembengkakanDanaTotal(float kd, float rp)
    {
        return (kd + rp) / 2f;
    }

    /// <summary>Calculates Cd = 5 + (Rd * 0.30), in percent.</summary>
    public static float PeluangPembengkakanDana(float rd)
    {
        return 5f + (rd * 0.30f);
    }

    /// <summary>Calculates O = G * r.</summary>
    public static float BesarPembengkakanDana(float g, float r)
    {
        return g * r;
    }

    /// <summary>Calculates La = (L1 + L2 + L3) / 3.</summary>
    public static float AvgLobbyPolitikus(float l1, float l2, float l3)
    {
        return (l1 + l2 + l3) / 3f;
    }

    /// <summary>Calculates Cl = Clamp(La - Df, 5, 95), in percent.</summary>
    public static float PeluangLobbyBerhasil(float la, float df)
    {
        return Clamp(la - df, 5f, 95f);
    }

    /// <summary>Calculates D = 1.4 - (0.8 * (K / 100)).</summary>
    public static float FaktorKetahananEventAlam(float k)
    {
        return 1.4f - (0.8f * (k / 100f));
    }

    /// <summary>Calculates Ea = round(Ba * D).</summary>
    public static float DampakEventAlam(float ba, float d)
    {
        return (float)Math.Round(ba * d, MidpointRounding.AwayFromZero);
    }

    /// <summary>Calculates eR = Clamp(20 + (100 - Ks) * 0.20 + (50 - T) * 0.10, 10, 40), in percent.</summary>
    public static float TingkatEventRakyat(float ks, float t)
    {
        return Clamp(20f + ((100f - ks) * 0.20f) + ((50f - t) * 0.10f), 10f, 40f);
    }

    /// <summary>Calculates Ca = (C1 + C2 + C3) / 3.</summary>
    public static float AvgPenyambunganPolitikus(float c1, float c2, float c3)
    {
        return (c1 + c2 + c3) / 3f;
    }

    /// <summary>Calculates Cc = 0.60S + 0.25((P / 300) * 100) + 0.15Ca, where P is totalLobby.</summary>
    public static float PeluangPenyambunganBerhasil(float s, float totalLobby, float ca)
    {
        return Clamp((0.60f * s) + (0.25f * ((totalLobby / 300f) * 100f)) + (0.15f * ca), 0f, 100f);
    }

    private static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
