using System;

public static class GameStateEvents
{
    public static event Action<int> OnDanaChanged;
    public static event Action<int> OnKepuasanChanged;
    public static event Action<int> OnKekuatanPolitikChanged;
    public static event Action<float> OnProgressChanged;
    public static event Action<GameOverReason> OnGameOver;
    public static event Action OnGameWon;

    public static void RaiseDanaChanged(int dana) => OnDanaChanged?.Invoke(dana);
    public static void RaiseKepuasanChanged(int value) => OnKepuasanChanged?.Invoke(value);
    public static void RaiseKekuatanPolitikChanged(int value) => OnKekuatanPolitikChanged?.Invoke(value);
    public static void RaiseProgressChanged(float progress) => OnProgressChanged?.Invoke(progress);
    public static void RaiseGameOver(GameOverReason reason) => OnGameOver?.Invoke(reason);
    public static void RaiseGameWon() => OnGameWon?.Invoke();
}
