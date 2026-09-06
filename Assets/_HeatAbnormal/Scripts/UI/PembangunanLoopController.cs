using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using System;

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
    private Text politicsStatusText;
    private Text fundsStatusText;
    private Text buildStatusText;
    private Text politicalActionStatusText;
    private Text politicalRepresentativesText;
    private Text peopleEventTimerText;
    private Text connectionRequestStatusText;
    private HeatAbnormalAudioFeedback audioFeedback;
    private GameObject naturalEventPopup;
    private Text naturalEventPopupText;
    private GameObject peopleEventModal;
    private int targetPeriods;
    private int elapsedPeriods;
    private bool gameOverTriggered;
    private bool connectionRequestPending;
    private bool lastConnectionRequestSucceeded;
    private float peopleEventChoiceSeconds = 10f;
    private float peopleEventChoiceDeadline;
    private int lastBudgetRequestPeriod = -1;
    private const float SmallRequestPercent = 0.10f;
    private const float MediumRequestPercent = 0.20f;
    private const float LargeRequestPercent = 0.35f;
    private EventRakyatData pendingPeopleEvent;
    private bool blockingNaturalPopup;
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
        if (peopleEventModal != null && peopleEventModal.activeSelf)
        {
            peopleEventChoiceSeconds = Mathf.Max(0f, peopleEventChoiceDeadline - Time.realtimeSinceStartup);
            if (peopleEventTimerText != null) peopleEventTimerText.text = "Pilih respon: " + Mathf.CeilToInt(peopleEventChoiceSeconds) + " dtk";
            if (peopleEventChoiceSeconds <= 0f) AutoChooseWorstPeopleOption();
        }
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
        elapsedPeriods = Mathf.Min(elapsedPeriods + 1, targetPeriods);
        GameSession.State.PeriodeTerpakai = elapsedPeriods;
        GameSession.State.ProgressPembangunanPeriode = (float)elapsedPeriods / targetPeriods;
        GameStateEvents.RaiseProgressChanged(GameSession.State.ProgressPembangunanPeriode);
        progressBar.value = GameSession.State.ProgressPembangunanPeriode;
        progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
        periodText.text = "Periode pembangunan: " + elapsedPeriods + " / " + targetPeriods;
        if (politicalActionStatusText != null) politicalActionStatusText.text = "Aksi politik: request tersedia";
        if (progressBar.value >= 1f)
        {
            connectionButton.interactable = true;
            connectionButton.GetComponentInChildren<Text>().text = "Request Penyambungan";
            connectionStatusText.text = "Status: Selesai — Menu Penyambungan terbuka";
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

        var contractor = GameSession.State.SelectedKontraktor;
        if (contractor == null)
        {
            Debug.LogError("Natural event requires a selected KontraktorData to calculate damage.");
            return;
        }

        var baseDamage = GameServices.Random.NextFloat(eventData.baseDamageMin, eventData.baseDamageMax);
        var resistanceFactor = SelectedStatsSystem.NaturalResistance(contractor);
        var damage = Mathf.RoundToInt(GameFormulas.DampakEventAlam(baseDamage, resistanceFactor));
        elapsedPeriods = Mathf.Max(0, elapsedPeriods - damage);
        GameSession.State.PeriodeTerpakai = elapsedPeriods;
        GameSession.State.ProgressPembangunanPeriode = (float)elapsedPeriods / targetPeriods;
        GameStateEvents.RaiseProgressChanged(GameSession.State.ProgressPembangunanPeriode);
        progressBar.value = GameSession.State.ProgressPembangunanPeriode;
        progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
        periodText.text = "Periode pembangunan: " + elapsedPeriods + " / " + targetPeriods;
        audioFeedback.PlayWarning();
        ShowNaturalEventPopup(eventData.namaEvent, eventData.narasi + "\nKemunduran: " + damage + " periode");
    }

    private void ShowNaturalEventPopup(string eventName, string narrative, bool blocking = true)
    {
        if (naturalEventPopup == null || naturalEventPopupText == null) return;
        blockingNaturalPopup = blocking;
        naturalEventPopupText.text = eventName + "\n\n" + narrative;
        naturalEventPopup.SetActive(true);
        CancelInvoke(nameof(HideNaturalEventPopup));
        Invoke(nameof(HideNaturalEventPopup), 4f);
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
        if (gameClock != null) gameClock.StopCurrentAdvance();
        gameClock.Pause();
        pendingPeopleEvent = eventData;
        peopleEventChoiceSeconds = 10f;
        peopleEventChoiceDeadline = Time.realtimeSinceStartup + peopleEventChoiceSeconds;
        var message = peopleEventModal.transform.Find("Message").GetComponent<Text>();
        message.text = eventData.id + "\n\n" + eventData.narasiPembuka;
        var options = peopleEventModal.transform.Find("Options");
        foreach (Transform child in options) Destroy(child.gameObject);
        if (eventData.ragamList.Count == 0)
        {
            CreateText(options, "MissingOptions", "TODO: Opsi event belum diisi dari dokumen sumber.", 20, Vector2.zero, Vector2.one);
        }
        else
        {
            for (var i = 0; i < eventData.ragamList.Count; i++)
            {
                var optionIndex = i;
                CreateButton(options, "Option_" + i, eventData.ragamList[i].dialogText, new Vector2(.05f, .8f - (i * .18f)), new Vector2(.95f, .95f - (i * .18f)), () => ApplyPeopleOption(eventData, optionIndex));
            }
        }
        peopleEventModal.SetActive(true);
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
        var options = peopleEventModal.transform.Find("Options");
        foreach (Transform child in options) Destroy(child.gameObject);
        if (option.isInstantGameOver)
        {
            gameOverTriggered = true;
            var reason = option.gameOverReason == GameOverReason.None ? GameOverReason.Other : option.gameOverReason;
            GameSession.State.MarkGameOver(reason);
            GameStateEvents.RaiseGameOver(reason);
            gameClock.Pause();
            CreateText(options, "GameOverText", "GAME OVER", 28, new Vector2(.05f, .45f), new Vector2(.95f, .7f));
            CreateButton(options, "Dismiss", "Tutup", new Vector2(.25f, .05f), new Vector2(.75f, .35f), () => peopleEventModal.SetActive(false));
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
        elapsedPeriods = Mathf.Clamp(elapsedPeriods + timeDelta, 0, targetPeriods);
        state.PeriodeTerpakai = elapsedPeriods;
        state.ProgressPembangunanPeriode = (float)elapsedPeriods / targetPeriods;
        GameStateEvents.RaiseKepuasanChanged(state.KepuasanRakyat);
        GameStateEvents.RaiseKekuatanPolitikChanged(state.KekuatanPolitik);
        GameStateEvents.RaiseProgressChanged(state.ProgressPembangunanPeriode);
        progressBar.value = state.ProgressPembangunanPeriode;
        progressText.text = Mathf.RoundToInt(progressBar.value * 100f) + "%";
        periodText.text = "Periode pembangunan: " + elapsedPeriods + " / " + targetPeriods;
        CreateButton(options, "Continue", "Lanjut", new Vector2(.25f, .05f), new Vector2(.75f, .35f), DismissPeopleEvent);
    }

    private void DismissPeopleEvent()
    {
        CancelInvoke(nameof(AutoChooseWorstPeopleOption));
        peopleEventModal.SetActive(false);
        if (!gameOverTriggered)
        {
            var callback = OnPeopleEventDismissed;
            if (callback != null) callback.Invoke();
        }
    }

    private void AutoChooseWorstPeopleOption()
    {
        if (peopleEventModal == null || !peopleEventModal.activeSelf || pendingPeopleEvent == null) return;
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
    }

    private void HandleDanaChanged(int value) { if (fundsStatusText != null) fundsStatusText.text = "Dana: " + value.ToString("N0") + " Gj"; }
    private void HandleCitizenChanged(int value) { if (citizenStatusText != null) citizenStatusText.text = "Kepuasan rakyat: " + CitizenBucket(value); }
    private void HandlePoliticsChanged(int value) { if (politicsStatusText != null) politicsStatusText.text = "Kekuatan politik: " + PoliticsBucket(value); }
    private void HandleProgressChanged(float value) { if (buildStatusText != null) buildStatusText.text = "Status pembangunan: " + Mathf.RoundToInt(value * 100f) + "%"; }

    private static string CitizenBucket(int value)
    {
        if (value >= 80) return "😀";
        if (value >= 60) return "🙂";
        if (value >= 40) return "😐";
        if (value >= 20) return "☹️";
        return "😡";
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
        CreateText(canvas.transform, "Title", "FASE PEMBANGUNAN", 48, new Vector2(.1f, .82f), new Vector2(.9f, .94f));
        var dashboard = new GameObject("Dashboard", typeof(RectTransform), typeof(Image)); dashboard.transform.SetParent(canvas.transform, false); var dashboardRect = dashboard.GetComponent<RectTransform>(); dashboardRect.anchorMin = new Vector2(.05f, .60f); dashboardRect.anchorMax = new Vector2(.28f, .78f); dashboardRect.offsetMin = dashboardRect.offsetMax = Vector2.zero; dashboard.GetComponent<Image>().color = new Color(.08f, .3f, .16f);
        citizenStatusText = CreateText(dashboard.transform, "CitizenStatus", "", 20, new Vector2(.05f, .7f), new Vector2(.95f, .95f));
        politicsStatusText = CreateText(dashboard.transform, "PoliticsStatus", "", 20, new Vector2(.05f, .48f), new Vector2(.95f, .7f));
        fundsStatusText = CreateText(dashboard.transform, "FundsStatus", "", 20, new Vector2(.05f, .26f), new Vector2(.95f, .48f));
        buildStatusText = CreateText(dashboard.transform, "BuildStatus", "", 20, new Vector2(.05f, .04f), new Vector2(.95f, .26f));
        var currentState = GameSession.State;
        HandleCitizenChanged(currentState.KepuasanRakyat); HandlePoliticsChanged(currentState.KekuatanPolitik); HandleDanaChanged(currentState.Dana); HandleProgressChanged(currentState.ProgressPembangunanPeriode);
        CreateText(canvas.transform, "PlantName", plant == null ? "Pembangkit belum dipilih" : "Proyek: " + plant.displayName, 28, new Vector2(.1f, .73f), new Vector2(.9f, .81f));
        progressBar = CreateProgressBar(canvas.transform);
        progressText = CreateText(canvas.transform, "ProgressText", "0%", 32, new Vector2(.1f, .46f), new Vector2(.9f, .56f));
        periodText = CreateText(canvas.transform, "PeriodText", "Periode pembangunan: 0 / " + targetPeriods, 24, new Vector2(.1f, .37f), new Vector2(.9f, .45f));
        timerText = CreateText(canvas.transform, "PeriodTimer", "Periode berikutnya: 30 dtk", 24, new Vector2(.28f, .30f), new Vector2(.72f, .36f));
        CreateButton(canvas.transform, "Speed1x", "1x", new Vector2(.70f, .28f), new Vector2(.78f, .35f), () => SetSpeed(1f));
        CreateButton(canvas.transform, "Speed3x", "3x", new Vector2(.79f, .28f), new Vector2(.87f, .35f), () => SetSpeed(3f));
        CreateButton(canvas.transform, "Speed9x", "9x", new Vector2(.88f, .28f), new Vector2(.96f, .35f), () => SetSpeed(9f));
        connectionButton = CreateButton(canvas.transform, "ConnectionButton", "Menu Penyambungan (terkunci)", new Vector2(.3f, .12f), new Vector2(.7f, .24f), RequestConnection);
        connectionButton.interactable = false;
        connectionStatusText = CreateText(canvas.transform, "ConnectionStatus", "Status: Pembangunan berlangsung", 20, new Vector2(.1f, .27f), new Vector2(.9f, .35f));
        connectionRequestStatusText = CreateText(canvas.transform, "ConnectionRequestStatus", "", 18, new Vector2(.1f, .04f), new Vector2(.9f, .11f));
        var politicalPanel = new GameObject("PoliticalActions", typeof(RectTransform), typeof(Image)); politicalPanel.transform.SetParent(canvas.transform, false);
        var politicalRect = politicalPanel.GetComponent<RectTransform>(); politicalRect.anchorMin = new Vector2(.72f, .58f); politicalRect.anchorMax = new Vector2(.96f, .88f); politicalRect.offsetMin = politicalRect.offsetMax = Vector2.zero; politicalPanel.GetComponent<Image>().color = new Color(.08f, .12f, .3f);
        CreateText(politicalPanel.transform, "Header", "AKSI POLITIK", 22, new Vector2(.05f, .82f), new Vector2(.95f, .98f));
        politicalRepresentativesText = CreateText(politicalPanel.transform, "Representatives", "Politikus aktif: -", 14, new Vector2(.05f, .72f), new Vector2(.95f, .82f));
        CreateText(politicalPanel.transform, "LobbyRule", "Peluang lobby memakai total lobby politikus", 11, new Vector2(.05f, .67f), new Vector2(.95f, .73f));
        var plantCost = plant == null ? 0 : plant.biayaDipilih;
        CreateButton(politicalPanel.transform, "RequestSmall", "Sedikit (+" + Mathf.RoundToInt(plantCost * SmallRequestPercent).ToString("N0") + ")", new Vector2(.08f, .50f), new Vector2(.92f, .66f), () => RequestBudget(0));
        CreateButton(politicalPanel.transform, "RequestMedium", "Sedang (+" + Mathf.RoundToInt(plantCost * MediumRequestPercent).ToString("N0") + ")", new Vector2(.08f, .30f), new Vector2(.92f, .46f), () => RequestBudget(1));
        CreateButton(politicalPanel.transform, "RequestLarge", "Banyak (+" + Mathf.RoundToInt(plantCost * LargeRequestPercent).ToString("N0") + ")", new Vector2(.08f, .10f), new Vector2(.92f, .26f), () => RequestBudget(2));
        politicalActionStatusText = CreateText(politicalPanel.transform, "Status", "Aksi: tersedia", 14, new Vector2(.05f, .01f), new Vector2(.95f, .08f));
        var selectedNames = new System.Collections.Generic.List<string>();
        foreach (var selectedPolitician in GameSession.State.SelectedPolitikus)
        {
            var politician = selectedPolitician;
            if (politician != null) selectedNames.Add(politician.displayName);
        }
        politicalRepresentativesText.text = "Politikus aktif:\n" + (selectedNames.Count == 0 ? "-" : string.Join("\n", selectedNames));
        naturalEventPopup = new GameObject("NaturalEventPopup", typeof(RectTransform), typeof(Image)); naturalEventPopup.transform.SetParent(canvas.transform, false);
        var popupRect = naturalEventPopup.GetComponent<RectTransform>(); popupRect.anchorMin = new Vector2(.28f, .32f); popupRect.anchorMax = new Vector2(.72f, .68f); popupRect.offsetMin = popupRect.offsetMax = Vector2.zero; naturalEventPopup.GetComponent<Image>().color = new Color(.35f, .12f, .08f, .98f);
        naturalEventPopupText = CreateText(naturalEventPopup.transform, "Message", "", 24, new Vector2(.06f, .08f), new Vector2(.94f, .92f)); naturalEventPopup.SetActive(false);
        peopleEventModal = new GameObject("PeopleEventModal", typeof(RectTransform), typeof(Image)); peopleEventModal.transform.SetParent(canvas.transform, false);
        var peopleRect = peopleEventModal.GetComponent<RectTransform>(); peopleRect.anchorMin = new Vector2(.2f, .18f); peopleRect.anchorMax = new Vector2(.8f, .82f); peopleRect.offsetMin = peopleRect.offsetMax = Vector2.zero; peopleEventModal.GetComponent<Image>().color = new Color(.08f, .16f, .32f, .99f);
        CreateText(peopleEventModal.transform, "Message", "", 22, new Vector2(.06f, .55f), new Vector2(.94f, .95f));
        peopleEventTimerText = CreateText(peopleEventModal.transform, "ChoiceTimer", "Pilih respon: 10 dtk", 18, new Vector2(.06f, .50f), new Vector2(.94f, .57f));
        var options = new GameObject("Options", typeof(RectTransform)); options.transform.SetParent(peopleEventModal.transform, false); var optionsRect = options.GetComponent<RectTransform>(); optionsRect.anchorMin = new Vector2(.05f, .05f); optionsRect.anchorMax = new Vector2(.95f, .52f); optionsRect.offsetMin = optionsRect.offsetMax = Vector2.zero;
        peopleEventModal.SetActive(false);
        if (peopleEventSystem == null) peopleEventSystem = GetComponentInChildren<PeopleEventSystem>();
        if (budgetOverrunSystem == null) budgetOverrunSystem = GetComponentInChildren<BudgetOverrunSystem>();
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
        if (politicalActionStatusText != null) politicalActionStatusText.text = "Speed periode: " + speed.ToString("0") + "x";
    }

    private void RequestConnection()
    {
        if (connectionRequestPending || gameOverTriggered || GameSession.State.ProgressPembangunanPeriode < 1f)
        {
            return;
        }
        var politicians = GameSession.State.SelectedPolitikus;
        if (politicians.Count != 3)
        {
            connectionRequestStatusText.text = "Data politikus belum lengkap.";
            return;
        }
        connectionRequestPending = true;
        connectionButton.interactable = false;
        connectionButton.GetComponentInChildren<Text>().text = "PENDING...";
        var chance = SelectedStatsSystem.ConnectionChance(GameSession.State);
        var success = GameServices.Random.Roll(chance);
        lastConnectionRequestSucceeded = success;
        connectionRequestStatusText.text = "Request PENDING — peluang hasil: " + Mathf.RoundToInt(chance) + "%";
        Invoke(nameof(FinishConnectionRequest), 1.0f);
    }

    private void FinishConnectionRequest()
    {
        // Result is determined at request time; this method only restores the request UI for P11-T2.
        connectionRequestPending = false;
        connectionButton.interactable = true;
        connectionButton.GetComponentInChildren<Text>().text = lastConnectionRequestSucceeded ? "ACCEPTED" : "REJECTED";
        connectionRequestStatusText.text = lastConnectionRequestSucceeded ? "Request diterima." : "Request ditolak.";
        if (lastConnectionRequestSucceeded) audioFeedback.PlaySuccess();
        else audioFeedback.PlayFailure();
        if (lastConnectionRequestSucceeded)
        {
            GameSession.State.MarkGameWon();
            GameStateEvents.RaiseGameWon();
            connectionButton.interactable = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Ending_Win");
        }
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
        var percentage = tier == 0 ? SmallRequestPercent : tier == 1 ? MediumRequestPercent : LargeRequestPercent;
        var amount = Mathf.RoundToInt(plant.biayaDipilih * percentage);
        if (GameServices.Random.Roll(chance))
        {
            audioFeedback.PlaySuccess();
            GameSession.State.Dana += amount;
            GameStateEvents.RaiseDanaChanged(GameSession.State.Dana);
            politicalActionStatusText.text = "Aksi berhasil — request " + (tier == 0 ? "Sedikit" : tier == 1 ? "Sedang" : "Banyak") + ": +" + amount.ToString("N0") + " Gajayan (" + Mathf.RoundToInt(chance) + "%).";
        }
        else
        {
            audioFeedback.PlayFailure();
            politicalActionStatusText.text = "Aksi gagal — request ditolak (peluang " + Mathf.RoundToInt(chance) + "%).";
        }
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
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false); var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; var text = obj.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action = null)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = new Color(.15f, .45f, .7f);
        if (action != null) obj.GetComponent<Button>().onClick.AddListener(action);
        CreateText(obj.transform, "Label", label, 22, Vector2.zero, Vector2.one);
        return obj.GetComponent<Button>();
    }
}
