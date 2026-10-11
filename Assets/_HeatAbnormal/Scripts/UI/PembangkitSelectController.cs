using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class PembangkitSelectController : MonoBehaviour
{
    [SerializeField] private GameDataRegistry registry;
    private GameObject confirmationPanel;
    private PembangkitData pendingSelection;

    private void Awake()
    {
        if (registry == null)
        {
            Debug.LogError("PembangkitSelectController requires a GameDataRegistry reference.");
            return;
        }

        BuildInterface();
    }

    private void BuildInterface()
    {
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

        CreateImage(canvas.transform, "CitySkylineBackground", Resources.Load<Sprite>("HeatAbnormal/CitySkyline"), Vector2.zero, Vector2.one, false, true);
        CreateImage(canvas.transform, "MainSelectionFrame", Resources.Load<Sprite>("HeatAbnormal/PixelFrame16x9RoundedNoTitle"), new Vector2(.06f, .06f), new Vector2(.94f, .94f), true, false);

        CreateText(canvas.transform, "Title", "PILIH JENIS PEMBANGKIT", 34, new Vector2(.22f, .82f), new Vector2(.78f, .90f));
        CreateText(canvas.transform, "StartingFunds", "Dana awal: 350.000 Gj", 22, new Vector2(.22f, .75f), new Vector2(.78f, .81f));

        var container = new GameObject("PlantCards", typeof(RectTransform));
        container.transform.SetParent(canvas.transform, false);
        var containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(.13f, .25f); containerRect.anchorMax = new Vector2(.87f, .73f); containerRect.offsetMin = containerRect.offsetMax = Vector2.zero;
        var layout = container.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 18f; layout.childForceExpandWidth = true; layout.childForceExpandHeight = true;

        var displayOrder = new[] { "Nuklir", "Surya", "Air", "Angin" };
        foreach (var displayName in displayOrder)
        {
            foreach (var plant in registry.pembangkitList)
            {
                if (plant != null && plant.displayName == displayName) CreatePlantCard(container.transform, plant);
            }
        }

        confirmationPanel = CreateConfirmationPanel(canvas.transform);
        confirmationPanel.SetActive(false);
    }

    private void CreatePlantCard(Transform parent, PembangkitData plant)
    {
        var card = new GameObject(plant.displayName + "Card", typeof(RectTransform), typeof(Image), typeof(Button));
        card.transform.SetParent(parent, false);
        var cardImage = card.GetComponent<Image>();
        cardImage.sprite = Resources.Load<Sprite>("HeatAbnormal/PixelBrowserFramePortrait3x4");
        cardImage.color = new Color(.58f, .58f, .58f, 1f);
        cardImage.preserveAspect = false;
        card.GetComponent<Button>().onClick.AddListener(() => RequestSelection(plant));
        var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(card.transform, false);
        var iconRect = iconObject.GetComponent<RectTransform>(); iconRect.anchorMin = new Vector2(.25f, .52f); iconRect.anchorMax = new Vector2(.75f, .72f); iconRect.offsetMin = iconRect.offsetMax = Vector2.zero;
        iconObject.GetComponent<Image>().sprite = ResolvePlantIcon(plant);
        iconObject.GetComponent<Image>().preserveAspect = true;
        CreateText(card.transform, "Name", plant.displayName, 30, new Vector2(.05f, .78f), new Vector2(.95f, .95f));
        CreateText(card.transform, "Cost", "Biaya: " + plant.biayaDipilih.ToString("N0") + " Gj", 22, new Vector2(.05f, .38f), new Vector2(.95f, .50f));
        CreateText(card.transform, "Remaining", "Sisa proyek: " + plant.biayaSisa.ToString("N0") + " Gj", 20, new Vector2(.05f, .28f), new Vector2(.95f, .46f));
        CreateText(card.transform, "Duration", "Durasi: " + plant.baseLamaPeriode + " periode", 20, new Vector2(.05f, .08f), new Vector2(.95f, .26f));
    }

    private GameObject CreateConfirmationPanel(Transform parent)
    {
        var panel = new GameObject("ConfirmationPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.25f, .3f); rect.anchorMax = new Vector2(.75f, .7f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(.05f, .08f, .1f, .98f);
        CreateText(panel.transform, "Message", "", 30, new Vector2(.08f, .52f), new Vector2(.92f, .9f));
        CreateButton(panel.transform, "Confirm", "Ya, pilih", new Vector2(.1f, .12f), new Vector2(.42f, .35f), ConfirmSelection);
        CreateButton(panel.transform, "Cancel", "Batal", new Vector2(.58f, .12f), new Vector2(.9f, .35f), CancelSelection);
        return panel;
    }

    private void RequestSelection(PembangkitData plant)
    {
        pendingSelection = plant;
        confirmationPanel.SetActive(true);
        confirmationPanel.transform.Find("Message").GetComponent<Text>().text = "Yakin pilih " + plant.displayName + "? Ini tidak bisa diubah.";
    }

    private void ConfirmSelection()
    {
        if (pendingSelection == null) return;
        var state = GameSession.State;
        state.SelectedPembangkit = pendingSelection;
        state.Dana = GameStateModel.InitialDana - pendingSelection.biayaDipilih;
        state.ProgressPembangunanPeriode = 0f;
        GameStateEvents.RaiseDanaChanged(state.Dana);
        GameStateEvents.RaiseProgressChanged(state.ProgressPembangunanPeriode);
        SceneManager.LoadScene("PembangunanLoop");
    }

    private void CancelSelection() { pendingSelection = null; confirmationPanel.SetActive(false); }

    private static Text CreateText(Transform parent, string name, string value, int size, Vector2 min, Vector2 max)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var text = obj.GetComponent<Text>();
        text.text = value;
        text.supportRichText = true;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        return text;
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

    private static GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, bool preserve, bool behind)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = obj.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = preserve; image.raycastTarget = false;
        if (behind) obj.transform.SetAsFirstSibling();
        else obj.transform.SetAsLastSibling();
        return obj;
    }

    private static void CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = new Color(.15f, .45f, .7f); obj.GetComponent<Button>().onClick.AddListener(action);
        var labelText = CreateText(obj.transform, "Label", label, 24, Vector2.zero, Vector2.one); labelText.fontStyle = FontStyle.Bold;
    }
}
