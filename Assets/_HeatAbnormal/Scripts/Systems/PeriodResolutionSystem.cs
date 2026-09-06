using UnityEngine;

public class PeriodResolutionSystem : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;
    [SerializeField] private NaturalEventSystem naturalEventSystem;
    [SerializeField] private PeopleEventSystem peopleEventSystem;
    [SerializeField] private BudgetOverrunSystem budgetOverrunSystem;
    [SerializeField] private PembangunanLoopController loopController;

    private void Awake()
    {
        if (gameClock == null) gameClock = GetComponentInChildren<GameClock>();
        if (naturalEventSystem == null) naturalEventSystem = GetComponentInChildren<NaturalEventSystem>();
        if (peopleEventSystem == null) peopleEventSystem = GetComponentInChildren<PeopleEventSystem>();
        if (budgetOverrunSystem == null) budgetOverrunSystem = GetComponentInChildren<BudgetOverrunSystem>();
        if (loopController == null) loopController = GetComponent<PembangunanLoopController>();
    }

    private void OnEnable() { if (gameClock != null) gameClock.OnPeriodElapsed += HandlePeriodElapsed; }
    private void OnDisable()
    {
        if (gameClock != null) gameClock.OnPeriodElapsed -= HandlePeriodElapsed;
        if (loopController != null)
        {
            loopController.OnBlockingNaturalEventDismissed -= ContinueAfterNatural;
            loopController.OnPeopleEventDismissed -= ContinueAfterPeople;
        }
    }

    private bool resolving;
    public bool IsResolvingBlockingEvent => resolving;

    private void HandlePeriodElapsed()
    {
        if (resolving || loopController == null || GameSession.State.IsGameOver) return;
        resolving = true;
        var natural = naturalEventSystem == null ? null : naturalEventSystem.TryTriggerEvent();
        if (natural != null)
        {
            if (natural.isInstantGameOver) { loopController.ReceiveNaturalEvent(natural); resolving = false; return; }
            if (gameClock != null) gameClock.Pause();
            loopController.OnBlockingNaturalEventDismissed += ContinueAfterNatural;
            loopController.ReceiveNaturalEvent(natural);
            return;
        }
        ResolvePeopleThenBudget();
    }

    private void ContinueAfterNatural()
    {
        loopController.OnBlockingNaturalEventDismissed -= ContinueAfterNatural;
        if (!GameSession.State.IsGameOver) ResolvePeopleThenBudget(); else resolving = false;
    }

    private void ResolvePeopleThenBudget()
    {
        if (GameSession.State.IsGameOver) { resolving = false; return; }
        var people = peopleEventSystem == null ? null : peopleEventSystem.TryTriggerEvent();
        if (people != null)
        {
            if (gameClock != null) gameClock.Pause();
            loopController.OnPeopleEventDismissed += ContinueAfterPeople;
            loopController.ReceivePeopleEvent(people);
            return;
        }
        FinishPeriod();
    }

    private void ContinueAfterPeople()
    {
        loopController.OnPeopleEventDismissed -= ContinueAfterPeople;
        if (!GameSession.State.IsGameOver) FinishPeriod(); else resolving = false;
    }

    private void FinishPeriod()
    {
        var overrun = budgetOverrunSystem == null ? (float?)null : budgetOverrunSystem.TryTriggerOverrun();
        if (overrun.HasValue) loopController.ReceiveBudgetOverrun(overrun.Value);
        loopController.AdvanceProgressOnly();
        resolving = false;
        if (!GameSession.State.IsGameOver && gameClock != null) gameClock.Resume();
    }
}
