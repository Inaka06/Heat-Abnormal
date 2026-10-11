using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using System;
using System.Collections;

public class PembangunanLoopController : MonoBehaviour
{
    [SerializeField] private GameClock gameClock;
    [SerializeField] private NaturalEventSystem naturalEventSystem;
    [SerializeField] private PeopleEventSystem peopleEventSystem;
    [SerializeField] private BudgetOverrunSystem budgetOverrunSystem;
    private Slider progressBar;
    private Text progressText;
    private Text periodText;
    private Text timerText;
    private Text connectionStatusText;
    private Button connectionButton;
    private Text citizenStatusText;
    private Image citizenSatisfactionIcon;
    private Text politicsStatusText;
    private Text fundsStatusText;
    private Text buildStatusText;
    private Text politicalActionStatusText;
    private Text speedStatusText;
    private Button speed1xButton;
    private Button speed3xButton;
    private Button speed9xButton;
    private Text politicalRepresentativesText;
    private GameObject politicalModal;
    private Transform politicalInfoContainer;
    private GameObject contractorModal;
    private Text contractorInfoText;
    private Image contractorLogo;
    private Transform naturalEventHistoryContainer;
    private RectTransform bottomBarRect;
    private Vector2 bottomBarShownPosition;
    private Coroutine bottomBarAnimation;
    private GameObject dashboardModal;
    private Text dashboardModalText;
    private Slider dashboardModalProgress;
    private Text dashboardModalProgressText;
    private Text dashboardModalTimerText;
    private Text bottomPeriodText;
    private Text bottomTimerText;
    private Text peopleEventTimerText;
    private Text connectionRequestStatusText;
    private Text setupStatusText;
    private Button skipPeriodButton;
    private Button peopleEventsButton;
    private Button politicsButton;
    private Button contractorButton;
    private Text peopleEventsButtonLabel;
    private Text peopleSatisfactionPopupText;
    private Image peopleSatisfactionIcon;
    private Transform peopleEventHistoryContainer;
    private HeatAbnormalAudioFeedback audioFeedback;
    private GameObject naturalEventPopup;
    private Text naturalEventPopupText;
    private Transform naturalEventPopupOptions;
    private EventAlamData pendingNaturalEvent;
    private GameObject peopleEventModal;
    private GameObject peopleEventNotification;
    private Text peopleEventNotificationText;
    private int targetPeriods;
    private int elapsedPeriods;
    private bool gameOverTriggered;
    private bool connectionRequestPending;
    private bool lastConnectionRequestSucceeded;
    private float connectionRequestDeadline;
    private int lastConnectionAttemptPeriod = -100;
    private float peopleEventChoiceSeconds = 30f;
    private float peopleEventChoiceDeadline;
    private int lastBudgetRequestPeriod = -1;
    private const float SmallRequestMinPercent = 0.06f;
    private const float SmallRequestMaxPercent = 0.12f;
    private const float MediumRequestMinPercent = 0.16f;
    private const float MediumRequestMaxPercent = 0.30f;
    private const float LargeRequestMinPercent = 0.35f;
    private const float LargeRequestMaxPercent = 0.65f;
    private EventRakyatData pendingPeopleEvent;
    private bool blockingNaturalPopup;
    private bool peopleEventCallbackSent;
    public event Action OnBlockingNaturalEventDismissed;
    public event Action OnPeopleEventDismissed;

    private void Awake()
    {
        if (gameClock == null) gameClock = GetComponentInChildren<GameClock>();
        var plant = GameSession.State.SelectedPembangkit;
        if (plant == null)
        {
            Debug.LogError("PembangunanLoop requires a selected PembangkitData.");
            targetPeriods = 1;
        }
        else targetPeriods = Mathf.Max(1, plant.baseLamaPeriode);
        if (gameClock != null)
        {
            gameClock.PeriodDurationSeconds = 300f;
            if (GameSession.State.SelectedKontraktor == null || GameSession.State.SelectedPolitikus.Count != 3)
            {
                gameClock.Pause();
            }
        }
        audioFeedback = gameObject.AddComponent<HeatAbnormalAudioFeedback>();
        BuildInterface(plant);
    }

    private void OnEnable()
    {
        GameStateEvents.OnDanaChanged += HandleDanaChanged;
        GameStateEvents.OnKepuasanChanged += HandleCitizenChanged;
        GameStateEvents.OnKekuatanPolitikChanged += HandlePoliticsChanged;
        GameStateEvents.OnProgressChanged += HandleProgressChanged;
        GameStateEvents.OnGameOver += HandleGameOver;
    }

    private void Update()
    {
        if (timerText != null && gameClock != null)
            timerText.text = gameClock.IsPaused ? "Timer dijeda" : "Periode berikutnya: " + Mathf.CeilToInt(gameClock.SecondsUntilNextPeriod) + " dtk";
        if (pendingPeopleEvent != null)
        {
            peopleEventChoiceSeconds = Mathf.Max(0f, peopleEventChoiceDeadline - Time.realtimeSinceStartup);
            if (peopleEventTimerText != null) peopleEventTimerText.text = "Pilih respon: " + Mathf.CeilToInt(peopleEventChoiceSeconds) + " dtk";
            if (peopleEventChoiceSeconds <= 0f) AutoChooseWorstPeopleOption();
        }
        if (connectionRequestPending && naturalEventPopup != null && naturalEventPopup.activeSelf && naturalEventPopupText != null)
        {
            var secondsLeft = Mathf.Max(0, Mathf.CeilToInt(connectionRequestDeadline - Time.realtimeSinceStartup));
            naturalEventPopupText.text = "PENYAMBUNGAN\n\nProses penyambungan sedang berlangsung...\nMohon tunggu " + secondsLeft + " detik.";
        }
        if (peopleEventsButton != null && pendingPeopleEvent != null)
        {
            var pulse = (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * .5f;
            peopleEventsButton.GetComponent<Image>().color = Color.Lerp(new Color(.15f, .45f, .7f), new Color(.95f, .65f, .1f), pulse);
        }
        if (peopleEventsButton != null) peopleEventsButton.interactable = true;
        var selectionPulse = (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * .5f;
        if (politicsButton != null)
            politicsButton.GetComponent<Image>().color = GameSession.State.SelectedPolitikus.Count == 3 ? Color.white : Color.Lerp(Color.white, new Color(1f, .75f, .2f), selectionPulse);
        if (contractorButton != null)
            contractorButton.GetComponent<Image>().color = GameSession.State.SelectedKontraktor != null ? Color.white : Color.Lerp(Color.white, new Color(1f, .75f, .2f), selectionPulse);
        if (connectionButton != null && !connectionRequestPending && !gameOverTriggered)
            connectionButton.interactable = GameSession.State.ProgressPembangunanPeriode >= 1f;
        if (dashboardModal != null && dashboardModal.activeSelf) RefreshDashboardModal();
        if (bottomPeriodText != null) bottomPeriodText.text = "Periode: " + elapsedPeriods + " / " + targetPeriods;
        if (bottomTimerText != null) bottomTimerText.text = gameClock == null ? "" : (gameClock.IsPaused ? "Timer dijeda" : "Sisa waktu: " + Mathf.CeilToInt(gameClock.SecondsUntilNextPeriod) + " dtk");
    }

    private void OnDisable()
    {
        GameStateEvents.OnDanaChanged -= HandleDanaChanged;
        GameStateEvents.OnKepuasanChanged -= HandleCitizenChanged;
        GameStateEvents.OnKekuatanPolitikChanged -= HandlePoliticsChanged;
        GameStateEvents.OnProgressChanged -= HandleProgressChanged;
        GameStateEvents.OnGameOver -= HandleGameOver;
    }

    public void AdvanceProgressOnly()
    {
        if (gameOverTriggered) return;
        elapsedPeriods += 1;
        GameSession.State.PeriodeTerpakai = elapsedPeriods;
        GameSession.State.ProgressPembangunanPeriode = Mathf.Clamp01((float)elapsedPeriods / targetPeriods);
        GameStateEvents.RaiseProgressChanged(GameSession.State.ProgressPembangunanPeriode);
        progressBar.value = GameSession.State.ProgressPembangunanPeriode;
        progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
        periodText.text = "Periode pembangunan: " + elapsedPeriods + " / " + targetPeriods;
        if (gameClock != null) gameClock.PeriodDurationSeconds = 60f;
        if (skipPeriodButton != null) skipPeriodButton.interactable = !gameOverTriggered;
        if (politicalActionStatusText != null) politicalActionStatusText.text = "Aksi politik: request tersedia";
        if (progressBar.value >= 1f)
        {
            if (connectionButton != null) connectionButton.interactable = true;
            if (connectionStatusText != null) connectionStatusText.text = "Status: Selesai — Menu Penyambungan terbuka";
        }

        var state = GameSession.State;
        if (state.Dana <= 0)
        {
            state.PeriodeDanaKosongBerturut++;
        }
        else
        {
            state.PeriodeDanaKosongBerturut = 0;
        }

        if (state.PeriodeDanaKosongBerturut >= 2)
        {
            gameOverTriggered = true;
            state.MarkGameOver(GameOverReason.DanaHabis);
            GameStateEvents.RaiseGameOver(GameOverReason.DanaHabis);
            gameClock.Pause();
            connectionStatusText.text = "Game Over: Dana Habis";
        }
    }

    public void ReceiveNaturalEvent(EventAlamData eventData)
    {
        if (eventData == null || gameOverTriggered) return;
        if (gameClock != null) gameClock.StopCurrentAdvance();
        if (eventData.isInstantGameOver)
        {
            gameOverTriggered = true;
            GameSession.State.MarkGameOver(GameOverReason.Kiamat);
            GameStateEvents.RaiseGameOver(GameOverReason.Kiamat);
            gameClock.Pause();
            ShowNaturalEventPopup(eventData.namaEvent, eventData.narasi + "\nGame Over: Kiamat");
            return;
        }

        pendingNaturalEvent = eventData;
        ShowNaturalEventChoice(eventData);
    }

    private void ShowNaturalEventChoice(EventAlamData eventData)
    {
        var plant = GameSession.State.SelectedPembangkit;
        var repairRate = eventData.additionalPeriodsMax >= 3 ? 0.20f : 0.10f;
        var repairCost = plant == null ? 0 : Mathf.RoundToInt(plant.biayaDipilih * repairRate);
        naturalEventPopupText.text = eventData.namaEvent + "\n\n" + eventData.narasi + "\n\nPilih tindakan:";
        foreach (Transform child in naturalEventPopupOptions) Destroy(child.gameObject);
        var repair = CreateButton(naturalEventPopupOptions, "Repair", "Perbaiki (" + repairCost.ToString("N0") + " Gj)", new Vector2(.05f, .52f), new Vector2(.95f, .78f), () => ResolveNaturalEvent(true));
        repair.interactable = plant != null && GameSession.State.Dana >= repairCost;
        if (!repair.interactable) repair.GetComponentInChildren<Text>().text = "Perbaiki (dana tidak cukup)";
        CreateButton(naturalEventPopupOptions, "Ignore", "Biarkan dan terima efek", new Vector2(.05f, .16f), new Vector2(.95f, .42f), () => ResolveNaturalEvent(false));
        blockingNaturalPopup = true;
        naturalEventPopup.SetActive(true);
    }

    private void ResolveNaturalEvent(bool repair)
    {
        var eventData = pendingNaturalEvent;
        var plant = GameSession.State.SelectedPembangkit;
        var contractor = GameSession.State.SelectedKontraktor;
        if (eventData == null || plant == null || contractor == null) return;
        var repairRate = eventData.additionalPeriodsMax >= 3 ? 0.20f : 0.10f;
        var repairCost = Mathf.RoundToInt(plant.biayaDipilih * repairRate);
        if (repair && GameSession.State.Dana >= repairCost)
        {
            GameSession.State.Dana -= repairCost;
            GameStateEvents.RaiseDanaChanged(GameSession.State.Dana);
            audioFeedback.PlaySuccess();
            naturalEventPopupText.text = eventData.namaEvent + "\n\nPerbaikan berhasil. Dampak bencana dinegasikan.\nBiaya: " + repairCost.ToString("N0") + " Gajayan.";
            GameSession.State.NaturalEventHistory.Add(new NaturalEventRecord { EventName = eventData.namaEvent, EventDescription = eventData.narasi, Action = "Diperbaiki", Effects = "Biaya: " + repairCost.ToString("N0") + " Gj. Dampak dinegasikan." });
        }
        else
        {
            var baseDamage = GameServices.Random.NextFloat(eventData.progressDamageMin, eventData.progressDamageMax);
            var damagePercent = GameFormulas.DampakRelatifEventAlam(baseDamage, SelectedStatsSystem.NaturalResistance(contractor));
            var newProgress = GameSession.State.ProgressPembangunanPeriode * (1f - damagePercent / 100f);
            var addedPeriods = GameServices.Random.NextInt(eventData.additionalPeriodsMin, eventData.additionalPeriodsMax + 1);
            targetPeriods += Mathf.Max(0, addedPeriods);
            elapsedPeriods = Mathf.Max(0, Mathf.RoundToInt(newProgress * targetPeriods));
            GameSession.State.PeriodeTerpakai = elapsedPeriods;
            GameSession.State.ProgressPembangunanPeriode = Mathf.Clamp01(newProgress);
            GameStateEvents.RaiseProgressChanged(GameSession.State.ProgressPembangunanPeriode);
            progressBar.value = GameSession.State.ProgressPembangunanPeriode;
            progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
            periodText.text = "Periode pembangunan: " + elapsedPeriods + " / " + targetPeriods;
            audioFeedback.PlayWarning();
            naturalEventPopupText.text = eventData.namaEvent + "\n\nEfek diterima.\nProgress rusak: " + Mathf.RoundToInt(damagePercent) + "%\nTambahan durasi: +" + addedPeriods + " periode.";
            GameSession.State.NaturalEventHistory.Add(new NaturalEventRecord { EventName = eventData.namaEvent, EventDescription = eventData.narasi, Action = "Dibiarkan", Effects = "Progress rusak: " + Mathf.RoundToInt(damagePercent) + "%. Tambahan durasi: +" + addedPeriods + " periode." });
        }
        pendingNaturalEvent = null;
        foreach (Transform child in naturalEventPopupOptions) Destroy(child.gameObject);
        CancelInvoke(nameof(HideNaturalEventPopup));
        Invoke(nameof(HideNaturalEventPopup), 2.5f);
    }

    private void ShowNaturalEventPopup(string eventName, string narrative, bool blocking = true, float duration = 4f)
    {
        if (naturalEventPopup == null || naturalEventPopupText == null) return;
        blockingNaturalPopup = blocking;
        naturalEventPopupText.text = eventName + "\n\n" + narrative;
        naturalEventPopup.SetActive(true);
        naturalEventPopup.transform.SetAsLastSibling();
        CancelInvoke(nameof(HideNaturalEventPopup));
        Invoke(nameof(HideNaturalEventPopup), duration);
    }

    private void HideNaturalEventPopup()
    {
        if (naturalEventPopup != null) naturalEventPopup.SetActive(false);
        if (blockingNaturalPopup)
        {
            blockingNaturalPopup = false;
            var callback = OnBlockingNaturalEventDismissed;
            if (callback != null) callback.Invoke();
        }
    }

    public void ReceivePeopleEvent(EventRakyatData eventData)
    {
        if (eventData == null || gameOverTriggered || peopleEventModal == null) return;
        peopleEventCallbackSent = false;
        if (peopleEventNotification != null)
        {
            peopleEventNotificationText.text = "EVENT RAKYAT BARU!\nKlik tombol Rakyat untuk melihat";
            peopleEventNotification.SetActive(true);
            peopleEventNotification.transform.SetAsLastSibling();
        }
        pendingPeopleEvent = eventData;
        if (peopleEventHistoryContainer != null) peopleEventHistoryContainer.gameObject.SetActive(false);
        peopleEventChoiceSeconds = 30f;
        peopleEventChoiceDeadline = Time.realtimeSinceStartup + peopleEventChoiceSeconds;
        var message = peopleEventModal.transform.Find("Message").GetComponent<Text>();
        message.text = eventData.id + "\n\n" + eventData.narasiPembuka;
        var options = peopleEventModal.transform.Find("Options/Content");
        foreach (Transform child in options) Destroy(child.gameObject);
        if (eventData.ragamList.Count == 0)
        {
            CreateText(options, "MissingOptions", "TODO: Opsi event belum diisi dari dokumen sumber.", 20, Vector2.zero, Vector2.one);
        }
        else
        {
            var contentRect = options.GetComponent<RectTransform>();
            var contentWidth = 900f;
            var contentY = 18f;
            var optionOrder = new System.Collections.Generic.List<int>();
            for (var i = 0; i < eventData.ragamList.Count; i++) optionOrder.Add(i);
            for (var i = optionOrder.Count - 1; i > 0; i--)
            {
                var swapIndex = GameServices.Random.NextInt(0, i + 1);
                var swap = optionOrder[i];
                optionOrder[i] = optionOrder[swapIndex];
                optionOrder[swapIndex] = swap;
            }

            for (var displayIndex = 0; displayIndex < optionOrder.Count; displayIndex++)
            {
                var optionIndex = optionOrder[displayIndex];
                var option = eventData.ragamList[optionIndex];
                var label = option.dialogText;
                var optionButton = CreateButton(options, "Option_" + displayIndex, label, Vector2.zero, Vector2.one, () => ApplyPeopleOption(eventData, optionIndex));
                var optionRect = optionButton.GetComponent<RectTransform>();
                var optionLabel = optionButton.GetComponentInChildren<Text>();
                var estimatedLines = Mathf.Max(1, Mathf.CeilToInt(label.Length / 55f));
                var optionHeight = Mathf.Clamp(estimatedLines * 30f + 32f, 92f, 300f);
                optionRect.anchorMin = new Vector2(.03f, 1f); optionRect.anchorMax = new Vector2(.97f, 1f); optionRect.pivot = new Vector2(.5f, 1f);
                optionRect.sizeDelta = new Vector2(0f, optionHeight); optionRect.anchoredPosition = new Vector2(0f, -contentY);
                contentY += optionHeight + 16f;
            }
            contentRect.sizeDelta = new Vector2(0f, Mathf.Max(900f, contentY));
        }
        if (peopleEventsButtonLabel != null) peopleEventsButtonLabel.text = "Rakyat (event baru)";
        RefreshPeopleEventHistory();
    }

    public void ReceiveBudgetOverrun(float amount)
    {
        audioFeedback.PlayWarning();
        ShowNaturalEventPopup("Pembengkakan Dana", "Terjadi pembengkakan dana sebesar " + amount.ToString("N0") + " Gajayan.", false);
    }

    private void HandleGameOver(GameOverReason reason)
    {
        if (gameOverTriggered) gameOverTriggered = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Ending_GameOver");
    }

    private void ApplyPeopleOption(EventRakyatData eventData, int optionIndex)
    {
        if (eventData == null || optionIndex < 0 || optionIndex >= eventData.ragamList.Count) return;
        CancelInvoke(nameof(AutoChooseWorstPeopleOption));
        var option = eventData.ragamList[optionIndex];
        var message = peopleEventModal.transform.Find("Message").GetComponent<Text>();
        message.text = eventData.id + "\n\n" + option.aftermathText;
        var options = peopleEventModal.transform.Find("Options/Content");
        foreach (Transform child in options) Destroy(child.gameObject);
        if (option.isInstantGameOver)
        {
            gameOverTriggered = true;
            var reason = option.gameOverReason == GameOverReason.None ? GameOverReason.Other : option.gameOverReason;
            GameSession.State.MarkGameOver(reason);
            GameStateEvents.RaiseGameOver(reason);
            gameClock.Pause();
            CreateText(options, "GameOverText", "GAME OVER", 28, new Vector2(.05f, .45f), new Vector2(.95f, .7f));
            CreateButton(options, "Dismiss", "Tutup", new Vector2(.25f, .05f), new Vector2(.75f, .35f), DismissPeopleEvent);
            return;
        }

        var satisfaction = GameServices.Random.NextInt(option.satisfactoryMin, option.satisfactoryMax + 1);
        var politics = GameServices.Random.NextInt(option.kekuatanPolitikMin, option.kekuatanPolitikMax + 1);
        var timeDelta = GameServices.Random.NextInt(option.waktuPembangunanMin, option.waktuPembangunanMax + 1);
        var state = GameSession.State;
        var avgTrust = 0f;
        var politicians = state.SelectedPolitikus;
        if (politicians.Count == 3)
        {
            avgTrust = SelectedStatsSystem.AveragePublicTrust(GameSession.State);
        }
        var mr = GameFormulas.PengaruhPolitikusTerhadapEvent(avgTrust);
        var effectiveSatisfaction = GameFormulas.DampakEventTerhadapKepuasan(satisfaction, mr);
        state.KepuasanRakyat = Mathf.RoundToInt(GameFormulas.KepuasanPascaEvent(state.KepuasanRakyat, effectiveSatisfaction));
        state.KekuatanPolitik = Mathf.Clamp(state.KekuatanPolitik + politics, 0, 300);
        elapsedPeriods += timeDelta;
        state.PeriodeTerpakai = elapsedPeriods;
        state.ProgressPembangunanPeriode = Mathf.Clamp01((float)elapsedPeriods / targetPeriods);
        GameStateEvents.RaiseKepuasanChanged(state.KepuasanRakyat);
        GameStateEvents.RaiseKekuatanPolitikChanged(state.KekuatanPolitik);
        GameStateEvents.RaiseProgressChanged(state.ProgressPembangunanPeriode);
        progressBar.value = state.ProgressPembangunanPeriode;
        progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
        periodText.text = "Periode pembangunan: " + elapsedPeriods + " / " + targetPeriods;
        state.PeopleEventHistory.Add(new PeopleEventRecord
        {
            EventName = eventData.id,
            EventDescription = eventData.narasiPembuka,
            Choice = option.dialogText,
            Effects = "Kepuasan " + FormatSignedValue(Mathf.RoundToInt(effectiveSatisfaction)) + ", Politik " + FormatSignedValue(politics) + ", Waktu " + FormatSignedValue(timeDelta) + " periode"
        });
        pendingPeopleEvent = null;
        if (gameClock != null && !gameOverTriggered) gameClock.Resume();
        if (!peopleEventCallbackSent)
        {
            peopleEventCallbackSent = true;
            var callback = OnPeopleEventDismissed;
            if (callback != null) callback.Invoke();
        }
        if (peopleEventsButtonLabel != null) peopleEventsButtonLabel.text = "Rakyat";
        message.text = eventData.id + "\n\n" + option.aftermathText;
        CreateButton(options, "Continue", "Lanjut", new Vector2(.25f, .05f), new Vector2(.75f, .35f), DismissPeopleEvent);
        RefreshPeopleEventHistory();
    }

    private void DismissPeopleEvent()
    {
        CancelInvoke(nameof(AutoChooseWorstPeopleOption));
        peopleEventModal.SetActive(false);
        if (peopleEventHistoryContainer != null) peopleEventHistoryContainer.gameObject.SetActive(true);
        if (peopleEventNotification != null) peopleEventNotification.SetActive(false);
        ShowBottomBar();
        if (peopleEventsButtonLabel != null) peopleEventsButtonLabel.text = "Rakyat";
        if (!gameOverTriggered && !peopleEventCallbackSent)
        {
            peopleEventCallbackSent = true;
            var callback = OnPeopleEventDismissed;
            if (callback != null) callback.Invoke();
        }
    }

    private void OpenPeopleEventsPanel()
    {
        if (peopleEventModal == null) return;
        HideBottomBar();
        peopleEventModal.SetActive(true);
        if (peopleEventHistoryContainer != null) peopleEventHistoryContainer.gameObject.SetActive(pendingPeopleEvent == null);
        peopleSatisfactionPopupText.text = "Kepuasan rakyat";
        if (pendingPeopleEvent != null)
        {
            var message = peopleEventModal.transform.Find("Message").GetComponent<Text>();
            message.text = "EVENT RAKYAT BERLANGSUNG\n\n" + pendingPeopleEvent.id + "\n" + pendingPeopleEvent.narasiPembuka;
        }
        RefreshPeopleEventHistory();
    }

    private void ShowPeopleEventOverview()
    {
        var detailBack = peopleEventModal.transform.Find("BackToOverview");
        if (detailBack != null) Destroy(detailBack.gameObject);
        var message = peopleEventModal.transform.Find("Message").GetComponent<Text>();
        message.text = pendingPeopleEvent == null ? "EVENT RAKYAT\n\nPilih event dari histori di sebelah kanan." : "EVENT RAKYAT BERLANGSUNG\n\n" + pendingPeopleEvent.id + "\n" + pendingPeopleEvent.narasiPembuka;
        var content = peopleEventModal.transform.Find("Options/Content");
        var viewport = peopleEventModal.transform.Find("Options");
        if (viewport != null) viewport.gameObject.SetActive(true);
        if (content != null)
        {
            foreach (Transform child in content) Destroy(child.gameObject);
            if (pendingPeopleEvent != null) CreateText(content, "Hint", "Event yang sedang berlangsung ada di panel utama.", 18, new Vector2(.05f, .35f), new Vector2(.95f, .65f));
        }
        RefreshPeopleEventHistory();
    }

    private void ClosePeopleEventsPanel()
    {
        if (pendingPeopleEvent != null) return;
        if (peopleEventModal != null) peopleEventModal.SetActive(false);
        ShowBottomBar();
        if (pendingPeopleEvent == null && peopleEventNotification != null) peopleEventNotification.SetActive(false);
        if (pendingPeopleEvent == null && gameClock != null && !gameOverTriggered) gameClock.Resume();
    }

    private void RefreshPeopleEventHistory()
    {
        if (peopleEventHistoryContainer == null) return;
        foreach (Transform child in peopleEventHistoryContainer) Destroy(child.gameObject);
        CreateText(peopleEventHistoryContainer, "Header", "EVENT SEBELUMNYA", 18, new Vector2(0f, .88f), new Vector2(1f, 1f));
        var history = GameSession.State.PeopleEventHistory;
        for (var i = 0; i < history.Count; i++)
        {
            var record = history[i];
            var index = i;
            CreateButton(peopleEventHistoryContainer, "History_" + i, record.EventName, new Vector2(.04f, .72f - i * .16f), new Vector2(.96f, .84f - i * .16f), () => ShowPeopleEventDetail(index));
        }
    }

    private void ShowPeopleEventDetail(int index)
    {
        var history = GameSession.State.PeopleEventHistory;
        if (index < 0 || index >= history.Count) return;
        var record = history[index];
        var message = peopleEventModal.transform.Find("Message").GetComponent<Text>();
        message.text = record.EventName + "\n\nApa yang terjadi:\n" + record.EventDescription + "\n\nPilihan kita:\n" + record.Choice + "\n\nEfek:\n" + SentimentEffect(record.Effects);
        var content = peopleEventModal.transform.Find("Options/Content");
        foreach (Transform child in content) Destroy(child.gameObject);
        var viewport = peopleEventModal.transform.Find("Options");
        if (viewport != null) viewport.gameObject.SetActive(false);
        var existingBack = peopleEventModal.transform.Find("BackToOverview");
        if (existingBack != null) Destroy(existingBack.gameObject);
        CreateButton(peopleEventModal.transform, "BackToOverview", "Kembali ke event rakyat", new Vector2(.33f, .14f), new Vector2(.77f, .25f), ShowPeopleEventOverview);
    }

    private void AutoChooseWorstPeopleOption()
    {
        if (pendingPeopleEvent == null) return;
        var worstIndex = 0;
        for (var i = 1; i < pendingPeopleEvent.ragamList.Count; i++)
        {
            var current = pendingPeopleEvent.ragamList[i];
            var worst = pendingPeopleEvent.ragamList[worstIndex];
            if (current.satisfactoryMin < worst.satisfactoryMin || (current.satisfactoryMin == worst.satisfactoryMin && current.waktuPembangunanMax > worst.waktuPembangunanMax)) worstIndex = i;
        }
        ApplyPeopleOption(pendingPeopleEvent, worstIndex);
        // Timeout is the player's response: apply the bad outcome and close
        // the blocking modal so the period resolver can continue.
        if (peopleEventModal != null && peopleEventModal.activeSelf) DismissPeopleEvent();
        else
        {
            pendingPeopleEvent = null;
            if (peopleEventsButtonLabel != null) peopleEventsButtonLabel.text = "Rakyat";
            var callback = OnPeopleEventDismissed;
            if (callback != null) callback.Invoke();
        }
    }

    private void HandleDanaChanged(int value) { if (fundsStatusText != null) fundsStatusText.text = "Dana: " + value.ToString("N0") + " Gj"; }
    private void HandleCitizenChanged(int value)
    {
        if (citizenStatusText != null) citizenStatusText.text = "Kepuasan rakyat";
        if (peopleSatisfactionPopupText != null) peopleSatisfactionPopupText.text = "Kepuasan rakyat";
        var sprite = SatisfactionSprite(value);
        if (citizenSatisfactionIcon != null) citizenSatisfactionIcon.sprite = sprite;
        if (peopleSatisfactionIcon != null) peopleSatisfactionIcon.sprite = sprite;
    }
    private void HandlePoliticsChanged(int value) { if (politicsStatusText != null) politicsStatusText.text = "Kekuatan politik: " + value + " (" + PoliticsBucket(value) + ")"; }
    private void HandleProgressChanged(float value) { if (buildStatusText != null) buildStatusText.text = "Status pembangunan: " + Mathf.RoundToInt(value * 100f) + "%"; }

    private static string FormatSignedRange(int min, int max)
    {
        if (min == max) return (min > 0 ? "+" : "") + min;
        return (min > 0 ? "+" : "") + min + " sampai " + (max > 0 ? "+" : "") + max;
    }

    private static string FormatSignedValue(int value)
    {
        return (value > 0 ? "+" : "") + value;
    }

    private static string SentimentEffect(string effects)
    {
        if (string.IsNullOrEmpty(effects)) return "Rakyat tidak senang.";
        var marker = effects.IndexOf("Kepuasan ", StringComparison.Ordinal);
        var start = marker < 0 ? -1 : marker + "Kepuasan ".Length;
        var end = start < 0 ? -1 : effects.IndexOf(',', start);
        var value = start < 0 ? "" : (end < 0 ? effects.Substring(start) : effects.Substring(start, end - start)).Trim();
        return value.StartsWith("+", StringComparison.Ordinal) ? "Rakyat senang." : "Rakyat tidak senang.";
    }

    private static string CitizenBucket(int value)
    {
        if (value >= 80) return "😀";
        if (value >= 60) return "🙂";
        if (value >= 40) return "😐";
        if (value >= 20) return "☹️";
        return "😡";
    }

    private static Sprite SatisfactionSprite(int value)
    {
        if (value >= 81) return Resources.Load<Sprite>("HeatAbnormal/Satisfaction/VeryHappy");
        if (value >= 61) return Resources.Load<Sprite>("HeatAbnormal/Satisfaction/Happy");
        if (value >= 41) return Resources.Load<Sprite>("HeatAbnormal/Satisfaction/Neutral");
        if (value >= 21) return Resources.Load<Sprite>("HeatAbnormal/Satisfaction/Unhappy");
        return Resources.Load<Sprite>("HeatAbnormal/Satisfaction/Angry");
    }

    private static string PoliticsBucket(int value)
    {
        if (value >= 200) return "Kuat";
        if (value >= 100) return "Sedang";
        if (value >= 10) return "Lemah";
        return "Tidak Ada";
    }

    private void BuildInterface(PembangkitData plant)
    {
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920f, 1080f);
        var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        var mapBackground = CreateImage(canvas.transform, "KotaBackground", Resources.Load<Sprite>("HeatAbnormal/Kota"), Vector2.zero, Vector2.one, false, true);
        mapBackground.GetComponent<Image>().raycastTarget = true;
        mapBackground.AddComponent<MapPanZoomController>();
        var mainTitle = CreateText(canvas.transform, "Title", "FASE PEMBANGUNAN", 42, new Vector2(.1f, .91f), new Vector2(.9f, .98f));
        var dashboard = new GameObject("Dashboard", typeof(RectTransform), typeof(Image)); dashboard.transform.SetParent(canvas.transform, false); var dashboardRect = dashboard.GetComponent<RectTransform>(); dashboardRect.anchorMin = new Vector2(.03f, .66f); dashboardRect.anchorMax = new Vector2(.25f, .84f); dashboardRect.offsetMin = dashboardRect.offsetMax = Vector2.zero; dashboard.GetComponent<Image>().color = new Color(.08f, .3f, .16f);
        citizenStatusText = CreateText(dashboard.transform, "CitizenStatus", "", 20, new Vector2(.05f, .7f), new Vector2(.95f, .95f));
        var citizenIconObject = new GameObject("CitizenSatisfactionIcon", typeof(RectTransform), typeof(Image)); citizenIconObject.transform.SetParent(dashboard.transform, false); var citizenIconRect = citizenIconObject.GetComponent<RectTransform>(); citizenIconRect.anchorMin = new Vector2(.40f, .10f); citizenIconRect.anchorMax = new Vector2(.60f, .62f); citizenIconRect.offsetMin = citizenIconRect.offsetMax = Vector2.zero; citizenSatisfactionIcon = citizenIconObject.GetComponent<Image>(); citizenSatisfactionIcon.preserveAspect = true; citizenSatisfactionIcon.raycastTarget = false;
        citizenStatusText.gameObject.SetActive(false); citizenSatisfactionIcon.gameObject.SetActive(false);
        politicsStatusText = CreateText(dashboard.transform, "PoliticsStatus", "", 20, new Vector2(.05f, .48f), new Vector2(.95f, .7f));
        fundsStatusText = CreateText(dashboard.transform, "FundsStatus", "", 20, new Vector2(.05f, .26f), new Vector2(.95f, .48f));
        buildStatusText = CreateText(dashboard.transform, "BuildStatus", "", 20, new Vector2(.05f, .04f), new Vector2(.95f, .26f));
        var currentState = GameSession.State;
        HandleCitizenChanged(currentState.KepuasanRakyat); HandlePoliticsChanged(currentState.KekuatanPolitik); HandleDanaChanged(currentState.Dana); HandleProgressChanged(currentState.ProgressPembangunanPeriode);
        var plantNameText = CreateText(canvas.transform, "PlantName", plant == null ? "Pembangkit belum dipilih" : "Proyek: " + plant.displayName, 28, new Vector2(.04f, .86f), new Vector2(.38f, .93f));
        plantNameText.alignment = TextAnchor.UpperLeft;
        progressBar = CreateProgressBar(canvas.transform);
        progressText = CreateText(canvas.transform, "ProgressText", "0%", 30, new Vector2(.28f, .48f), new Vector2(.70f, .55f));
        periodText = CreateText(canvas.transform, "PeriodText", "Periode pembangunan: 0 / " + targetPeriods, 22, new Vector2(.28f, .42f), new Vector2(.70f, .47f));
        timerText = CreateText(canvas.transform, "PeriodTimer", "Periode berikutnya: 300 dtk", 22, new Vector2(.28f, .36f), new Vector2(.70f, .41f));
        setupStatusText = CreateText(canvas.transform, "SetupStatus", "", 20, new Vector2(.28f, .30f), new Vector2(.70f, .35f));
        UpdateSetupStatus();
        speedStatusText = CreateText(canvas.transform, "SpeedStatus", "Speed: 1x", 15, new Vector2(.74f, .30f), new Vector2(.96f, .33f));
        speed1xButton = CreateButton(canvas.transform, "Speed1x", "1x", new Vector2(.74f, .24f), new Vector2(.81f, .29f), () => SetSpeed(1f));
        speed3xButton = CreateButton(canvas.transform, "Speed3x", "3x", new Vector2(.82f, .24f), new Vector2(.89f, .29f), () => SetSpeed(3f));
        speed9xButton = CreateButton(canvas.transform, "Speed9x", "9x", new Vector2(.90f, .24f), new Vector2(.97f, .29f), () => SetSpeed(9f));
        UpdateSpeedButtonColors(1f);
        skipPeriodButton = CreateButton(canvas.transform, "SkipPeriod", "Skip Periode", new Vector2(.74f, .17f), new Vector2(.97f, .22f), SkipPeriod);
        skipPeriodButton.interactable = GameSession.State.SelectedKontraktor != null && GameSession.State.SelectedPolitikus.Count == 3;
        connectionStatusText = CreateText(canvas.transform, "ConnectionStatus", "Status: Pembangunan berlangsung", 18, new Vector2(.28f, .21f), new Vector2(.70f, .26f));
        connectionRequestStatusText = CreateText(canvas.transform, "ConnectionRequestStatus", "", 16, new Vector2(.28f, .09f), new Vector2(.70f, .12f));
        mainTitle.gameObject.SetActive(false); dashboard.SetActive(false); progressBar.gameObject.SetActive(false); progressText.gameObject.SetActive(false); periodText.gameObject.SetActive(false); timerText.gameObject.SetActive(false); setupStatusText.gameObject.SetActive(false); speedStatusText.gameObject.SetActive(false); skipPeriodButton.gameObject.SetActive(false); connectionStatusText.gameObject.SetActive(false); connectionRequestStatusText.gameObject.SetActive(false); canvas.transform.Find("Speed1x").gameObject.SetActive(false); canvas.transform.Find("Speed3x").gameObject.SetActive(false); canvas.transform.Find("Speed9x").gameObject.SetActive(false);
        canvas.transform.Find("Speed1x").gameObject.SetActive(true); canvas.transform.Find("Speed3x").gameObject.SetActive(true); canvas.transform.Find("Speed9x").gameObject.SetActive(true);
        bottomPeriodText = CreateText(canvas.transform, "BottomPeriod", "Periode: 0 / " + targetPeriods, 26, new Vector2(.34f, .25f), new Vector2(.66f, .30f));
        bottomPeriodText.fontStyle = FontStyle.Bold;
        bottomTimerText = CreateText(canvas.transform, "BottomTimer", "", 22, new Vector2(.30f, .20f), new Vector2(.70f, .25f));
        bottomTimerText.fontStyle = FontStyle.Bold;
        politicalModal = new GameObject("PoliticalModal", typeof(RectTransform), typeof(Image)); politicalModal.transform.SetParent(canvas.transform, false);
        var politicalModalRect = politicalModal.GetComponent<RectTransform>(); politicalModalRect.anchorMin = new Vector2(.08f, .10f); politicalModalRect.anchorMax = new Vector2(.92f, .90f); politicalModalRect.offsetMin = politicalModalRect.offsetMax = Vector2.zero; ApplyBrowserPopupFrame(politicalModal);
        CreateText(politicalModal.transform, "Header", "POLITIKUS AKTIF", 32, new Vector2(.04f, .90f), new Vector2(.58f, .98f));
        CreateButton(politicalModal.transform, "Close", "Tutup", new Vector2(.84f, .91f), new Vector2(.97f, .98f), ClosePoliticalModal);
        var infoPanel = CreateContainer(politicalModal.transform, "PoliticianInfo", new Vector2(.04f, .10f), new Vector2(.58f, .86f), new Color(.10f, .20f, .34f, .98f));
        politicalInfoContainer = infoPanel.transform;
        politicalRepresentativesText = CreateText(politicalInfoContainer, "Representatives", "Politikus aktif: -", 24, new Vector2(.04f, .02f), new Vector2(.96f, .98f));
        var politicalActions = CreateContainer(politicalModal.transform, "PoliticalActions", new Vector2(.62f, .10f), new Vector2(.96f, .86f), new Color(.08f, .12f, .30f, .98f));
        CreateText(politicalActions.transform, "Header", "AKSI POLITIK", 26, new Vector2(.05f, .82f), new Vector2(.95f, .98f));
        CreateText(politicalActions.transform, "LobbyRule", "Peluang lobby memakai total lobby politikus", 14, new Vector2(.05f, .67f), new Vector2(.95f, .73f));
        CreateButton(politicalActions.transform, "RequestSmall", "Sedikit — chance aman", new Vector2(.08f, .50f), new Vector2(.92f, .66f), () => RequestBudget(0));
        CreateButton(politicalActions.transform, "RequestMedium", "Sedang — chance menengah", new Vector2(.08f, .30f), new Vector2(.92f, .46f), () => RequestBudget(1));
        CreateButton(politicalActions.transform, "RequestLarge", "Banyak — chance ekstrem", new Vector2(.08f, .10f), new Vector2(.92f, .26f), () => RequestBudget(2));
        politicalActionStatusText = CreateText(politicalActions.transform, "Status", "Aksi: tersedia", 18, new Vector2(.05f, .01f), new Vector2(.95f, .08f));
        var selectedNames = new System.Collections.Generic.List<string>();
        foreach (var selectedPolitician in GameSession.State.SelectedPolitikus)
        {
            var politician = selectedPolitician;
            if (politician != null) selectedNames.Add(politician.displayName);
        }
        politicalRepresentativesText.text = "Politikus aktif:\n" + (selectedNames.Count == 0 ? "-" : string.Join("\n", selectedNames));
        politicalModal.SetActive(false);
        contractorModal = new GameObject("ContractorModal", typeof(RectTransform), typeof(Image)); contractorModal.transform.SetParent(canvas.transform, false);
        var contractorModalRect = contractorModal.GetComponent<RectTransform>(); contractorModalRect.anchorMin = new Vector2(.08f, .10f); contractorModalRect.anchorMax = new Vector2(.92f, .90f); contractorModalRect.offsetMin = contractorModalRect.offsetMax = Vector2.zero; ApplyBrowserPopupFrame(contractorModal);
        CreateText(contractorModal.transform, "Header", "KONTRAKTOR AKTIF", 32, new Vector2(.04f, .90f), new Vector2(.96f, .98f));
        CreateButton(contractorModal.transform, "Close", "Tutup", new Vector2(.84f, .91f), new Vector2(.97f, .98f), CloseContractorModal);
        var contractorInfoPanel = CreateContainer(contractorModal.transform, "ContractorInfo", new Vector2(.04f, .52f), new Vector2(.96f, .86f), new Color(.10f, .20f, .34f, .98f));
        var contractorLogoObject = new GameObject("Logo", typeof(RectTransform), typeof(Image)); contractorLogoObject.transform.SetParent(contractorInfoPanel.transform, false); var contractorLogoRect = contractorLogoObject.GetComponent<RectTransform>(); contractorLogoRect.anchorMin = new Vector2(.03f, .18f); contractorLogoRect.anchorMax = new Vector2(.25f, .82f); contractorLogoRect.offsetMin = contractorLogoRect.offsetMax = Vector2.zero; contractorLogo = contractorLogoObject.GetComponent<Image>(); contractorLogo.preserveAspect = true; contractorLogo.raycastTarget = false;
        contractorInfoText = CreateText(contractorInfoPanel.transform, "Info", "Belum memilih kontraktor.", 24, new Vector2(.28f, .05f), new Vector2(.97f, .95f));
        CreateContainer(contractorModal.transform, "NaturalHistoryContainer", new Vector2(.04f, .10f), new Vector2(.96f, .47f), new Color(.07f, .12f, .22f, .98f));
        naturalEventHistoryContainer = new GameObject("NaturalHistory", typeof(RectTransform)).transform; naturalEventHistoryContainer.SetParent(contractorModal.transform, false); var naturalHistoryRect = naturalEventHistoryContainer.GetComponent<RectTransform>(); naturalHistoryRect.anchorMin = new Vector2(.06f, .12f); naturalHistoryRect.anchorMax = new Vector2(.94f, .45f); naturalHistoryRect.offsetMin = naturalHistoryRect.offsetMax = Vector2.zero;
        contractorModal.SetActive(false);
        dashboardModal = new GameObject("DashboardModal", typeof(RectTransform), typeof(Image)); dashboardModal.transform.SetParent(canvas.transform, false);
        var dashboardModalRect = dashboardModal.GetComponent<RectTransform>(); dashboardModalRect.anchorMin = new Vector2(.18f, .16f); dashboardModalRect.anchorMax = new Vector2(.82f, .84f); dashboardModalRect.offsetMin = dashboardModalRect.offsetMax = Vector2.zero; ApplyBrowserPopupFrame(dashboardModal);
        CreateText(dashboardModal.transform, "Header", "DASHBOARD PEMBANGUNAN", 30, new Vector2(.05f, .88f), new Vector2(.95f, .98f));
        CreateButton(dashboardModal.transform, "Close", "Tutup", new Vector2(.82f, .90f), new Vector2(.97f, .98f), CloseDashboardModal);
        dashboardModalText = CreateText(dashboardModal.transform, "Details", "", 22, new Vector2(.08f, .57f), new Vector2(.92f, .86f));
        dashboardModalProgress = CreateProgressBar(dashboardModal.transform);
        dashboardModalProgressText = CreateText(dashboardModal.transform, "Progress", "0%", 24, new Vector2(.30f, .42f), new Vector2(.70f, .49f));
        dashboardModalTimerText = CreateText(dashboardModal.transform, "Timer", "", 20, new Vector2(.20f, .34f), new Vector2(.80f, .40f));
        CreateButton(dashboardModal.transform, "SkipPeriod", "Skip Periode", new Vector2(.30f, .17f), new Vector2(.70f, .27f), SkipPeriod);
        dashboardModal.SetActive(false);
        naturalEventPopup = new GameObject("NaturalEventPopup", typeof(RectTransform), typeof(Image)); naturalEventPopup.transform.SetParent(canvas.transform, false);
        var popupRect = naturalEventPopup.GetComponent<RectTransform>(); popupRect.anchorMin = new Vector2(.28f, .25f); popupRect.anchorMax = new Vector2(.72f, .75f); popupRect.offsetMin = popupRect.offsetMax = Vector2.zero; ApplyBrowserPopupFrame(naturalEventPopup); naturalEventPopup.transform.Find("BrowserFrame").GetComponent<Image>().color = new Color(.58f, .58f, .58f, 1f);
        naturalEventPopupText = CreateText(naturalEventPopup.transform, "Message", "", 28, new Vector2(.06f, .42f), new Vector2(.94f, .84f)); naturalEventPopupText.fontStyle = FontStyle.Bold;
        var naturalOptions = new GameObject("Options", typeof(RectTransform)); naturalOptions.transform.SetParent(naturalEventPopup.transform, false); var naturalOptionsRect = naturalOptions.GetComponent<RectTransform>(); naturalOptionsRect.anchorMin = new Vector2(.05f, .06f); naturalOptionsRect.anchorMax = new Vector2(.95f, .52f); naturalOptionsRect.offsetMin = naturalOptionsRect.offsetMax = Vector2.zero; naturalEventPopupOptions = naturalOptions.transform;
        naturalEventPopup.SetActive(false);
        peopleEventModal = new GameObject("PeopleEventModal", typeof(RectTransform), typeof(Image)); peopleEventModal.transform.SetParent(canvas.transform, false);
        var peopleRect = peopleEventModal.GetComponent<RectTransform>(); peopleRect.anchorMin = new Vector2(.10f, .12f); peopleRect.anchorMax = new Vector2(.90f, .88f); peopleRect.offsetMin = peopleRect.offsetMax = Vector2.zero; ApplyBrowserPopupFrame(peopleEventModal); peopleEventModal.transform.Find("BrowserFrame").GetComponent<Image>().color = new Color(.58f, .58f, .58f, 1f);
        CreateButton(peopleEventModal.transform, "Close", "Tutup", new Vector2(.82f, .91f), new Vector2(.97f, .98f), ClosePeopleEventsPanel);
        CreateContainer(peopleEventModal.transform, "SatisfactionContainer", new Vector2(.03f, .12f), new Vector2(.27f, .88f), Color.clear);
        CreateContainer(peopleEventModal.transform, "OngoingEventContainer", new Vector2(.30f, .42f), new Vector2(.77f, .88f), Color.clear);
        CreateContainer(peopleEventModal.transform, "HistoryContainer", new Vector2(.77f, .12f), new Vector2(.94f, .84f), Color.clear);
        peopleSatisfactionPopupText = CreateText(peopleEventModal.transform, "Satisfaction", "Kepuasan rakyat", 28, new Vector2(.05f, .55f), new Vector2(.25f, .78f));
        var peopleIconObject = new GameObject("SatisfactionIcon", typeof(RectTransform), typeof(Image)); peopleIconObject.transform.SetParent(peopleEventModal.transform, false); var peopleIconRect = peopleIconObject.GetComponent<RectTransform>(); peopleIconRect.anchorMin = new Vector2(.08f, .25f); peopleIconRect.anchorMax = new Vector2(.22f, .52f); peopleIconRect.offsetMin = peopleIconRect.offsetMax = Vector2.zero; peopleSatisfactionIcon = peopleIconObject.GetComponent<Image>(); peopleSatisfactionIcon.preserveAspect = true; peopleSatisfactionIcon.raycastTarget = false;
        peopleSatisfactionIcon.sprite = SatisfactionSprite(GameSession.State.KepuasanRakyat);
        CreateText(peopleEventModal.transform, "Message", "", 20, new Vector2(.33f, .58f), new Vector2(.77f, .82f));
        peopleEventTimerText = CreateText(peopleEventModal.transform, "ChoiceTimer", "Pilih respon: 30 dtk", 18, new Vector2(.33f, .48f), new Vector2(.77f, .55f));
        var options = new GameObject("Options", typeof(RectTransform), typeof(RectMask2D), typeof(ScrollRect)); options.transform.SetParent(peopleEventModal.transform, false); var optionsRect = options.GetComponent<RectTransform>(); optionsRect.anchorMin = new Vector2(.33f, .12f); optionsRect.anchorMax = new Vector2(.77f, .48f); optionsRect.offsetMin = optionsRect.offsetMax = Vector2.zero;
        var optionsContent = new GameObject("Content", typeof(RectTransform)); optionsContent.transform.SetParent(options.transform, false); var contentRect = optionsContent.GetComponent<RectTransform>(); contentRect.anchorMin = new Vector2(0f, 1f); contentRect.anchorMax = new Vector2(1f, 1f); contentRect.pivot = new Vector2(.5f, 1f); contentRect.anchoredPosition = Vector2.zero; contentRect.sizeDelta = new Vector2(0f, 900f);
        var optionsScroll = options.GetComponent<ScrollRect>(); optionsScroll.viewport = optionsRect; optionsScroll.content = contentRect; optionsScroll.horizontal = false; optionsScroll.vertical = true; optionsScroll.movementType = ScrollRect.MovementType.Clamped;
        peopleEventHistoryContainer = new GameObject("History", typeof(RectTransform)).transform; peopleEventHistoryContainer.SetParent(peopleEventModal.transform, false); var historyRect = peopleEventHistoryContainer.GetComponent<RectTransform>(); historyRect.anchorMin = new Vector2(.72f, .16f); historyRect.anchorMax = new Vector2(.89f, .84f); historyRect.offsetMin = historyRect.offsetMax = Vector2.zero;
        peopleEventModal.SetActive(false);
        peopleEventNotification = new GameObject("PeopleEventNotification", typeof(RectTransform), typeof(Image)); peopleEventNotification.transform.SetParent(canvas.transform, false);
        var notificationRect = peopleEventNotification.GetComponent<RectTransform>(); notificationRect.anchorMin = new Vector2(.03f, .78f); notificationRect.anchorMax = new Vector2(.33f, .95f); notificationRect.offsetMin = notificationRect.offsetMax = Vector2.zero;
        var notificationImage = peopleEventNotification.GetComponent<Image>(); notificationImage.sprite = Resources.Load<Sprite>("HeatAbnormal/PixelFrame16x9RoundedNoTitle"); notificationImage.preserveAspect = false; notificationImage.color = new Color(.68f, .68f, .68f, .98f); notificationImage.raycastTarget = false;
        peopleEventNotificationText = CreateText(peopleEventNotification.transform, "Message", "EVENT RAKYAT BARU!", 24, new Vector2(.08f, .12f), new Vector2(.92f, .88f)); peopleEventNotificationText.fontStyle = FontStyle.Bold;
        peopleEventNotification.SetActive(false);
        if (peopleEventSystem == null) peopleEventSystem = GetComponentInChildren<PeopleEventSystem>();
        if (budgetOverrunSystem == null) budgetOverrunSystem = GetComponentInChildren<BudgetOverrunSystem>();
        BuildBottomNavigation(canvas.transform, plant);
    }

    private void BuildBottomNavigation(Transform canvas, PembangkitData plant)
    {
        var bar = new GameObject("BottomButtonBar", typeof(RectTransform), typeof(Image));
        bar.transform.SetParent(canvas, false);
        var barRect = bar.GetComponent<RectTransform>(); barRect.anchorMin = new Vector2(0f, .01f); barRect.anchorMax = new Vector2(1f, .20f); barRect.offsetMin = barRect.offsetMax = Vector2.zero; bottomBarRect = barRect; bottomBarShownPosition = Vector2.zero;
        bar.GetComponent<Image>().sprite = Resources.Load<Sprite>("HeatAbnormal/BottomUI/ButtonBar"); bar.GetComponent<Image>().preserveAspect = false; bar.GetComponent<Image>().raycastTarget = false;
        politicsButton = CreateBottomButton(bar.transform, "PoliticsBottom", "HeatAbnormal/BottomUI/Politik", new Vector2(.02f, .10f), new Vector2(.19f, .88f), OpenPoliticianSelection);
        peopleEventsButton = CreateBottomButton(bar.transform, "PeopleBottom", "HeatAbnormal/BottomUI/Rakyat", new Vector2(.21f, .10f), new Vector2(.38f, .88f), OpenPeopleEventsPanel);
        peopleEventsButtonLabel = peopleEventsButton.GetComponentInChildren<Text>();
        contractorButton = CreateBottomButton(bar.transform, "ContractorBottom", "HeatAbnormal/BottomUI/Kontraktor", new Vector2(.62f, .10f), new Vector2(.79f, .88f), OpenContractorSelection);
        connectionButton = CreateBottomButton(bar.transform, "ConnectionBottom", "HeatAbnormal/BottomUI/Penyambungan", new Vector2(.81f, .10f), new Vector2(.98f, .88f), RequestConnection);
        connectionButton.interactable = GameSession.State.ProgressPembangunanPeriode >= 1f && !gameOverTriggered;
        var plantButton = CreateBottomButton(bar.transform, "PlantBottom", "HeatAbnormal/BottomUI/PlantButton", new Vector2(.40f, .02f), new Vector2(.60f, 1.02f), OpenDashboardModal);
        var plantIcon = new GameObject("SelectedPlantIcon", typeof(RectTransform), typeof(Image)); plantIcon.transform.SetParent(plantButton.transform, false);
        var iconRect = plantIcon.GetComponent<RectTransform>(); iconRect.anchorMin = new Vector2(.25f, .25f); iconRect.anchorMax = new Vector2(.75f, .75f); iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        plantIcon.GetComponent<Image>().sprite = ResolvePlantIcon(plant); plantIcon.GetComponent<Image>().preserveAspect = true; plantIcon.GetComponent<Image>().raycastTarget = false;
    }

    private void HideBottomBar()
    {
        if (bottomBarRect == null) return;
        if (bottomBarAnimation != null) StopCoroutine(bottomBarAnimation);
        bottomBarAnimation = StartCoroutine(AnimateBottomBar(new Vector2(0f, -bottomBarRect.rect.height - 40f)));
    }

    private void ShowBottomBar()
    {
        if (bottomBarRect == null) return;
        if (bottomBarAnimation != null) StopCoroutine(bottomBarAnimation);
        bottomBarAnimation = StartCoroutine(AnimateBottomBar(bottomBarShownPosition));
    }

    private IEnumerator AnimateBottomBar(Vector2 target)
    {
        var start = bottomBarRect.anchoredPosition;
        var elapsed = 0f;
        const float duration = .22f;
        while (elapsed < duration && bottomBarRect != null)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            bottomBarRect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }
        if (bottomBarRect != null) bottomBarRect.anchoredPosition = target;
        bottomBarAnimation = null;
    }

    private Button CreateBottomButton(Transform parent, string name, string spritePath, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = obj.GetComponent<Image>(); image.sprite = Resources.Load<Sprite>(spritePath); image.preserveAspect = true; image.raycastTarget = true;
        var button = obj.GetComponent<Button>(); if (action != null) button.onClick.AddListener(action); return button;
    }

    private void TogglePoliticalPanel()
    {
        var panel = GameObject.Find("PoliticalActions"); if (panel != null) panel.SetActive(!panel.activeSelf);
    }

    private void ShowContractorStatus()
    {
        var contractor = GameSession.State.SelectedKontraktor;
        ShowNaturalEventPopup("Kontraktor", contractor == null ? "Belum memilih kontraktor." : contractor.displayName + "\n\n" + contractor.deskripsi, false);
    }

    private void OpenDashboardModal()
    {
        if (dashboardModal == null) return;
        HideBottomBar();
        RefreshDashboardModal();
        dashboardModal.SetActive(true);
    }

    private void CloseDashboardModal()
    {
        if (dashboardModal != null) dashboardModal.SetActive(false);
        ShowBottomBar();
    }

    private void RefreshDashboardModal()
    {
        if (dashboardModalText == null) return;
        var state = GameSession.State;
        var plant = state.SelectedPembangkit;
        dashboardModalText.text = "Proyek: " + (plant == null ? "-" : plant.displayName)
            + "\nDana: " + state.Dana.ToString("N0") + " Gj"
            + "\nKekuatan politik: " + state.KekuatanPolitik
            + "\nStatus: " + (gameOverTriggered ? "Game Over" : (state.ProgressPembangunanPeriode >= 1f ? "Pembangunan selesai" : "Pembangunan berlangsung"));
        var progress = state.ProgressPembangunanPeriode;
        if (dashboardModalProgress != null) dashboardModalProgress.value = progress;
        if (dashboardModalProgressText != null) dashboardModalProgressText.text = Mathf.RoundToInt(progress * 100f) + "%  —  Periode " + elapsedPeriods + " / " + targetPeriods;
        if (dashboardModalTimerText != null) dashboardModalTimerText.text = gameClock == null ? "Timer tidak tersedia" : (gameClock.IsPaused ? "Timer dijeda" : "Periode berikutnya: " + Mathf.CeilToInt(gameClock.SecondsUntilNextPeriod) + " dtk");
    }

    private static Sprite ResolvePlantIcon(PembangkitData plant)
    {
        if (plant == null) return null;
        if (plant.displayName == "Nuklir") return Resources.Load<Sprite>("HeatAbnormal/Logo_Nuklir");
        if (plant.displayName == "Surya") return Resources.Load<Sprite>("HeatAbnormal/Logo_Surya");
        if (plant.displayName == "Air") return Resources.Load<Sprite>("HeatAbnormal/Logo_Air");
        if (plant.displayName == "Angin") return Resources.Load<Sprite>("HeatAbnormal/Logo_Angin");
        return plant.icon;
    }

    private static Sprite ContractorLogo(KontraktorData contractor)
    {
        if (contractor == null) return null;
        if (contractor.displayName == "Pokhrov Uma Pyoi Construction") return Resources.Load<Sprite>("HeatAbnormal/Contractors/Pokhrov");
        if (contractor.displayName == "Gunung Salju Pembangunan") return Resources.Load<Sprite>("HeatAbnormal/Contractors/GunungSalju");
        if (contractor.displayName == "Neo Vanta IX") return Resources.Load<Sprite>("HeatAbnormal/Contractors/NeoVanta");
        if (contractor.displayName == "Dashi Group") return Resources.Load<Sprite>("HeatAbnormal/Contractors/Dashi");
        return null;
    }

    private void FastForwardForTesting(int multiplier)
    {
        if (gameClock == null || gameOverTriggered || gameClock.IsPaused)
        {
            if (politicalActionStatusText != null) politicalActionStatusText.text = "Fast forward tidak tersedia saat dijeda.";
            return;
        }
        gameClock.AdvancePeriodsNow(multiplier);
        if (politicalActionStatusText != null) politicalActionStatusText.text = "Fast forward x" + multiplier + " aktif.";
    }

    private void SetSpeed(float speed)
    {
        if (gameClock == null) return;
        gameClock.SpeedMultiplier = speed;
        UpdateSpeedButtonColors(speed);
        if (speedStatusText != null) speedStatusText.text = "Speed: " + gameClock.SpeedMultiplier.ToString("0") + "x";
        if (politicalActionStatusText != null) politicalActionStatusText.text = "Speed periode: " + speed.ToString("0") + "x";
    }

    private void UpdateSpeedButtonColors(float selectedSpeed)
    {
        var selected = new Color(.75f, .9f, .18f, 1f);
        var normal = new Color(.15f, .45f, .7f, 1f);
        if (speed1xButton != null) speed1xButton.GetComponent<Image>().color = Mathf.Approximately(selectedSpeed, 1f) ? selected : normal;
        if (speed3xButton != null) speed3xButton.GetComponent<Image>().color = Mathf.Approximately(selectedSpeed, 3f) ? selected : normal;
        if (speed9xButton != null) speed9xButton.GetComponent<Image>().color = Mathf.Approximately(selectedSpeed, 9f) ? selected : normal;
    }

    private void UpdateSetupStatus()
    {
        if (setupStatusText == null) return;
        if (GameSession.State.PeriodeTerpakai > 0)
        {
            setupStatusText.text = "Pilihan kontraktor dan politikus sudah dikunci.";
        }
        else if (GameSession.State.SelectedKontraktor == null || GameSession.State.SelectedPolitikus.Count != 3)
        {
            setupStatusText.text = "Lengkapi pilihan kontraktor dan 3 politikus untuk memulai periode pertama (5 menit).";
        }
        else
        {
            setupStatusText.text = "Periode pertama aktif — durasi 5 menit.";
        }
    }

    private void OpenContractorSelection()
    {
        if (GameSession.State.SelectedKontraktor != null)
        {
            if (gameClock != null) gameClock.Pause();
            HideBottomBar();
            var contractor = GameSession.State.SelectedKontraktor;
            contractorInfoText.text = contractor.displayName + "\n\n" + contractor.deskripsi;
            contractorLogo.sprite = ContractorLogo(contractor);
            RefreshNaturalEventHistory();
            contractorModal.SetActive(true);
            return;
        }
        if (GameSession.State.PeriodeTerpakai > 0) return;
        if (gameClock != null) gameClock.Pause();
        UnityEngine.SceneManagement.SceneManager.LoadScene("KontraktorSelect");
    }

    private void CloseContractorModal()
    {
        if (contractorModal != null) contractorModal.SetActive(false);
        ShowBottomBar();
        if (gameClock != null && !gameOverTriggered) gameClock.Resume();
    }

    private void RefreshNaturalEventHistory()
    {
        if (naturalEventHistoryContainer == null) return;
        foreach (Transform child in naturalEventHistoryContainer) Destroy(child.gameObject);
        CreateText(naturalEventHistoryContainer, "Header", "HISTORI BENCANA", 22, new Vector2(.02f, .82f), new Vector2(.98f, 1f));
        var history = GameSession.State.NaturalEventHistory;
        if (history.Count == 0)
        {
            CreateText(naturalEventHistoryContainer, "Empty", "Belum ada bencana.", 20, new Vector2(.02f, .20f), new Vector2(.98f, .75f));
            return;
        }
        var text = "";
        for (var i = 0; i < history.Count; i++)
        {
            var record = history[i];
            if (i > 0) text += "\n\n";
            text += record.EventName + " — " + record.Action + "\n" + record.Effects;
        }
        CreateText(naturalEventHistoryContainer, "Records", text, 18, new Vector2(.02f, .02f), new Vector2(.98f, .80f));
    }

    private void OpenPoliticianSelection()
    {
        if (GameSession.State.SelectedPolitikus.Count == 3)
        {
            if (gameClock != null) gameClock.Pause();
            HideBottomBar();
            var details = "";
            foreach (var politician in GameSession.State.SelectedPolitikus)
            {
                if (politician == null) continue;
                if (details.Length > 0) details += "\n\n";
                details += politician.displayName + "\n" + politician.deskripsi;
            }
            politicalRepresentativesText.text = details.Length == 0 ? "Belum ada politikus yang dipilih." : details;
            politicalModal.SetActive(true);
            return;
        }
        if (GameSession.State.PeriodeTerpakai > 0) return;
        if (gameClock != null) gameClock.Pause();
        UnityEngine.SceneManagement.SceneManager.LoadScene("PolitikusSelect");
    }

    private void ClosePoliticalModal()
    {
        if (politicalModal != null) politicalModal.SetActive(false);
        ShowBottomBar();
        if (gameClock != null && !gameOverTriggered) gameClock.Resume();
    }

    private void SkipPeriod()
    {
        if (gameClock == null || gameOverTriggered) return;
        if (gameClock.IsPaused) return;
        gameClock.AdvancePeriodNow();
    }

    public void ResetSpeedToNormal()
    {
        SetSpeed(1f);
    }

    private void RequestConnection()
    {
        if (connectionRequestPending || gameOverTriggered)
        {
            if (connectionRequestPending) ShowNaturalEventPopup("PENYAMBUNGAN", "Request masih diproses...", false, 2.5f);
            return;
        }
        if (GameSession.State.ProgressPembangunanPeriode < 1f)
        {
            connectionRequestStatusText.text = "Pembangunan belum selesai.";
            ShowNaturalEventPopup("PENYAMBUNGAN", "Pembangunan belum selesai. Progress harus mencapai 100%.", false, 3f);
            return;
        }
        var currentPeriod = gameClock == null ? elapsedPeriods : gameClock.CurrentPeriodCount;
        if (currentPeriod - lastConnectionAttemptPeriod < 3)
        {
            var remaining = 3 - (currentPeriod - lastConnectionAttemptPeriod);
            connectionRequestStatusText.text = "Cooldown penyambungan: " + remaining + " periode lagi.";
            ShowNaturalEventPopup("PENYAMBUNGAN", "Belum bisa menyambung lagi.\nCooldown: " + remaining + " periode.", false, 3f);
            return;
        }
        if (GameSession.State.SelectedPolitikus == null || GameSession.State.SelectedPolitikus.Count < 3)
        {
            connectionRequestStatusText.text = "Data politikus belum lengkap.";
            ShowNaturalEventPopup("PENYAMBUNGAN", "Data politikus belum lengkap. Pilih 3 politikus terlebih dahulu.", false, 3f);
            return;
        }
        connectionRequestPending = true;
        lastConnectionAttemptPeriod = currentPeriod;
        connectionButton.interactable = false;
        var chance = SelectedStatsSystem.ConnectionChance(GameSession.State);
        lastConnectionRequestSucceeded = GameServices.Random.Roll(chance);
        connectionRequestDeadline = Time.realtimeSinceStartup + 5.0f;
        connectionRequestStatusText.text = "Request PENDING — peluang hasil: " + Mathf.RoundToInt(chance) + "%";
        ShowNaturalEventPopup("PENYAMBUNGAN", "Proses penyambungan sedang berlangsung...\nMohon tunggu 5 detik.", false, 5.2f);
        Invoke(nameof(FinishConnectionRequest), 5.0f);
    }

    private void FinishConnectionRequest()
    {
        // Result is determined at request time; this method only restores the request UI for P11-T2.
        connectionRequestPending = false;
        connectionButton.interactable = true;
        connectionRequestStatusText.text = lastConnectionRequestSucceeded ? "Request diterima." : "Request ditolak.";
        var tips = lastConnectionRequestSucceeded ? "" : "\n\nTips: naikkan kepuasan rakyat dan pilih politikus dengan lobby serta stat penyambungan yang lebih tinggi.";
        ShowNaturalEventPopup("PENYAMBUNGAN", lastConnectionRequestSucceeded ? "Penyambungan berhasil!" : "Penyambungan gagal." + tips, false, 4f);
        if (lastConnectionRequestSucceeded) audioFeedback.PlaySuccess();
        else audioFeedback.PlayFailure();
        if (lastConnectionRequestSucceeded)
        {
            GameSession.State.MarkGameWon();
            GameStateEvents.RaiseGameWon();
            connectionButton.interactable = false;
            Invoke(nameof(LoadWinningScene), 4f);
        }
    }

    private void LoadWinningScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Ending_Win");
    }

    private void RequestBudget(int tier)
    {
        var period = gameClock == null ? elapsedPeriods : gameClock.CurrentPeriodCount;
        if (period == lastBudgetRequestPeriod)
        {
            politicalActionStatusText.text = "Request hanya 1x per periode.";
            return;
        }
        var plant = GameSession.State.SelectedPembangkit;
        if (plant == null || GameSession.State.SelectedPolitikus.Count != 3)
        {
            politicalActionStatusText.text = "Data pilihan belum lengkap.";
            return;
        }
        lastBudgetRequestPeriod = period;
        var chance = SelectedStatsSystem.LobbyChance(GameSession.State, tier);
        if (GameServices.Random.Roll(chance))
        {
            var minPercent = tier == 0 ? SmallRequestMinPercent : tier == 1 ? MediumRequestMinPercent : LargeRequestMinPercent;
            var maxPercent = tier == 0 ? SmallRequestMaxPercent : tier == 1 ? MediumRequestMaxPercent : LargeRequestMaxPercent;
            var amount = Mathf.RoundToInt(plant.biayaDipilih * GameServices.Random.NextFloat(minPercent, maxPercent));
            audioFeedback.PlaySuccess();
            GameSession.State.Dana += amount;
            GameStateEvents.RaiseDanaChanged(GameSession.State.Dana);
            politicalActionStatusText.text = "Aksi berhasil — request " + (tier == 0 ? "Sedikit" : tier == 1 ? "Sedang" : "Banyak") + ": +" + amount.ToString("N0") + " Gajayan.";
        }
        else
        {
            audioFeedback.PlayFailure();
            politicalActionStatusText.text = "Aksi gagal — request ditolak (peluang " + Mathf.RoundToInt(chance) + "%).";
        }
    }

    private static void ApplyBrowserPopupFrame(GameObject popup)
    {
        if (popup == null) return;
        var baseImage = popup.GetComponent<Image>();
        if (baseImage != null) { baseImage.color = new Color(1f, 1f, 1f, 0f); baseImage.raycastTarget = false; }
        var frame = new GameObject("BrowserFrame", typeof(RectTransform), typeof(Image));
        frame.transform.SetParent(popup.transform, false);
        var rect = frame.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = frame.GetComponent<Image>(); image.sprite = Resources.Load<Sprite>("HeatAbnormal/PixelBrowserFrame16x9_Solid"); image.preserveAspect = false; image.raycastTarget = false;
        frame.transform.SetAsFirstSibling();
    }

    private static GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, bool preserve, bool behind)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = obj.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = preserve; image.raycastTarget = false;
        if (behind) obj.transform.SetAsFirstSibling();
        return obj;
    }

    private static Slider CreateProgressBar(Transform parent)
    {
        var obj = new GameObject("ProgressBar", typeof(RectTransform), typeof(Slider)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.30f, .55f); rect.anchorMax = new Vector2(.70f, .63f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        var background = new GameObject("Background", typeof(RectTransform), typeof(Image)); background.transform.SetParent(obj.transform, false); background.GetComponent<RectTransform>().anchorMin = Vector2.zero; background.GetComponent<RectTransform>().anchorMax = Vector2.one; background.GetComponent<RectTransform>().offsetMin = background.GetComponent<RectTransform>().offsetMax = Vector2.zero; background.GetComponent<Image>().color = new Color(.12f, .2f, .28f);
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(obj.transform, false); fill.GetComponent<RectTransform>().anchorMin = Vector2.zero; fill.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, 1f); fill.GetComponent<RectTransform>().offsetMin = fill.GetComponent<RectTransform>().offsetMax = Vector2.zero; fill.GetComponent<Image>().color = new Color(.18f, .65f, .3f);
        var slider = obj.GetComponent<Slider>(); slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0f; slider.interactable = false; slider.fillRect = fill.GetComponent<RectTransform>(); return slider;
    }

    private static Text CreateText(Transform parent, string name, string value, int size, Vector2 min, Vector2 max)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false); var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; var text = obj.GetComponent<Text>(); text.text = value; text.supportRichText = true; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow; text.color = Color.white; text.raycastTarget = false; return text;
    }

    private static GameObject CreateContainer(Transform parent, string name, Vector2 min, Vector2 max, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = color;
        return obj;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action = null)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var buttonImage = obj.GetComponent<Image>(); buttonImage.color = new Color(.15f, .45f, .7f);
        if (name == "Close")
        {
            buttonImage.sprite = Resources.Load<Sprite>("HeatAbnormal/UI/OKEButton");
            buttonImage.color = Color.white;
            buttonImage.preserveAspect = false;
        }
        if (action != null) obj.GetComponent<Button>().onClick.AddListener(action);
        if (name != "Close")
        {
            var labelText = CreateText(obj.transform, "Label", label, 22, Vector2.zero, Vector2.one); labelText.fontStyle = FontStyle.Bold;
        }
        return obj.GetComponent<Button>();
    }
}
