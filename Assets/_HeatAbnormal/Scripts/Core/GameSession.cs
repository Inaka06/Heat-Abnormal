public static class GameSession
{
    private static GameStateModel state;

    public static GameStateModel State
    {
        get
        {
            if (state == null)
            {
                state = new GameStateModel();
            }

            return state;
        }
    }

    public static void Reset()
    {
        state = new GameStateModel();
    }
}
