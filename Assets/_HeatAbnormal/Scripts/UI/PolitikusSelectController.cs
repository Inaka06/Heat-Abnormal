using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public class PolitikusSelectController : MonoBehaviour
{
    [SerializeField] private GameDataRegistry registry;
    private readonly List<PolitikusData> selected = new List<PolitikusData>();
    private readonly List<Button> cardButtons = new List<Button>();
    private Text selectionStatus;
    private Text detailName;
    private Text detailDescription;
    private Text detailSelectionStatus;
    private Text selectedSummary;
    private Button confirmButton;
    private PolitikusData focusedPolitician;

    private void Awake()
    {
        if (registry == null)
        {
            Debug.LogError("PolitikusSelectController requires a GameDataRegistry reference.");
            return;
        }

        BuildInterface();
    }

    private void BuildInterface()
    {
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920f, 1080f);
        var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        CreateImage(canvas.transform, "KotaBackground", Resources.Load<Sprite>("HeatAbnormal/Kota"), Vector2.zero, Vector2.one, false, true);
        CreateImage(canvas.transform, "MainSelectionFrame", Resources.Load<Sprite>("HeatAbnormal/PixelFrame16x9RoundedNoTitle"), new Vector2(.06f, .06f), new Vector2(.94f, .94f), true, false);
        CreateText(canvas.transform, "Title", "PILIH 3 POLITIKUS", 34, new Vector2(.22f, .84f), new Vector2(.78f, .92f));
        selectionStatus = CreateText(canvas.transform, "SelectionStatus", "Terpilih: 0 / 3", 21, new Vector2(.22f, .78f), new Vector2(.78f, .83f));

        var listPanel = new GameObject("PoliticianList", typeof(RectTransform), typeof(Image)); listPanel.transform.SetParent(canvas.transform, false);
        var listRect = listPanel.GetComponent<RectTransform>(); listRect.anchorMin = new Vector2(.09f, .15f); listRect.anchorMax = new Vector2(.54f, .81f); listRect.offsetMin = listRect.offsetMax = Vector2.zero; listPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        CreateText(listPanel.transform, "Header", "DAFTAR POLITIKUS", 22, new Vector2(.08f, .88f), new Vector2(.64f, .98f));
        var list = new GameObject("Names", typeof(RectTransform)); list.transform.SetParent(listPanel.transform, false); var listRectInner = list.GetComponent<RectTransform>(); listRectInner.anchorMin = new Vector2(.03f, .02f); listRectInner.anchorMax = new Vector2(.97f, .86f); listRectInner.offsetMin = listRectInner.offsetMax = Vector2.zero;
        var layout = list.AddComponent<GridLayoutGroup>(); layout.spacing = new Vector2(16f, 16f); layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount; layout.constraintCount = 2; layout.cellSize = new Vector2(280f, 150f);
        foreach (var politician in registry.politikusList) if (politician != null) CreateCard(list.transform, politician);

        var detailPanel = new GameObject("PoliticianDetail", typeof(RectTransform), typeof(Image)); detailPanel.transform.SetParent(canvas.transform, false);
        var detailRect = detailPanel.GetComponent<RectTransform>(); detailRect.anchorMin = new Vector2(.40f, .28f); detailRect.anchorMax = new Vector2(.95f, .76f); detailRect.offsetMin = detailRect.offsetMax = Vector2.zero; var detailImage = detailPanel.GetComponent<Image>(); detailImage.sprite = null; detailImage.color = new Color(0f, 0f, 0f, 0f);
        detailName = CreateText(detailPanel.transform, "Name", "Pilih politikus", 36, new Vector2(.01f, .72f), new Vector2(.99f, .90f));
        detailDescription = CreateText(detailPanel.transform, "Description", "Klik nama di sebelah kiri untuk melihat deskripsi.", 28, new Vector2(.06f, .26f), new Vector2(.94f, .71f));
        detailSelectionStatus = CreateText(detailPanel.transform, "SelectionStatus", "Belum dipilih", 21, new Vector2(.03f, .19f), new Vector2(.97f, .27f));
        CreateButton(detailPanel.transform, "ToggleSelection", "Pilih / Batalkan", new Vector2(.22f, .08f), new Vector2(.78f, .19f), ToggleFocusedSelection);

        selectedSummary = CreateText(canvas.transform, "SelectedSummary", "Pilihan: -", 20, new Vector2(.08f, .20f), new Vector2(.92f, .25f));
        confirmButton = CreateButton(canvas.transform, "ConfirmButton", "Konfirmasi (pilih 3)", new Vector2(.34f, .12f), new Vector2(.66f, .19f), ConfirmSelection);
        confirmButton.interactable = false;
    }

    private void CreateCard(Transform parent, PolitikusData politician)
    {
        var card = new GameObject(politician.displayName + "Name", typeof(RectTransform), typeof(Image), typeof(Button)); card.transform.SetParent(parent, false);
        var rect = card.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(0f, 150f);
        var layoutElement = card.AddComponent<LayoutElement>(); layoutElement.minHeight = 150f; layoutElement.preferredHeight = 150f;
        var cardImage = card.GetComponent<Image>(); cardImage.sprite = Resources.Load<Sprite>("HeatAbnormal/PixelBrowserFrame16x9_Solid"); cardImage.color = Color.white; cardImage.preserveAspect = false;
        var button = card.GetComponent<Button>(); button.onClick.AddListener(() => FocusPolitician(politician)); cardButtons.Add(button);
        CreateText(card.transform, "Name", politician.displayName, 26, new Vector2(.06f, .16f), new Vector2(.94f, .82f));
    }

    private void FocusPolitician(PolitikusData politician)
    {
        focusedPolitician = politician;
        detailName.text = politician.displayName;
        detailDescription.text = politician.deskripsi;
        detailSelectionStatus.text = selected.Contains(politician) ? "Sudah dipilih" : "Belum dipilih";
    }

    private void ToggleFocusedSelection()
    {
        if (focusedPolitician == null) return;
        if (selected.Contains(focusedPolitician)) selected.Remove(focusedPolitician);
        else if (selected.Count < 3) selected.Add(focusedPolitician);
        detailSelectionStatus.text = selected.Contains(focusedPolitician) ? "Sudah dipilih" : "Belum dipilih";
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        selectionStatus.text = "Terpilih: " + selected.Count + " / 3";
        confirmButton.interactable = selected.Count == 3;
        var names = new List<string>();
        foreach (var politician in selected) names.Add(politician.displayName);
        selectedSummary.text = "Pilihan: " + (names.Count == 0 ? "-" : string.Join("  |  ", names));
        foreach (var button in cardButtons)
        {
            var image = button.GetComponent<Image>();
            image.color = Color.white;
            var politicianName = button.gameObject.name.Replace("Name", "");
            foreach (var politician in selected) if (politician.displayName == politicianName) image.color = new Color(.30f, .68f, .38f, 1f);
        }
    }

    private void ConfirmSelection()
    {
        if (selected.Count != 3 || !GameSession.State.SetSelectedPolitikus(new List<PolitikusData> { selected[0], selected[1], selected[2] })) return;
        GameSession.State.KekuatanPolitik = 0;
        SceneManager.LoadScene("PembangunanLoop");
    }

    private static Text CreateText(Transform parent, string name, string value, int size, Vector2 min, Vector2 max)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var text = obj.GetComponent<Text>(); text.text = value; text.supportRichText = true; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = new Color(.15f, .45f, .7f); var button = obj.GetComponent<Button>(); button.onClick.AddListener(action); var labelText = CreateText(obj.transform, "Label", label, 24, Vector2.zero, Vector2.one); labelText.fontStyle = FontStyle.Bold; return button;
    }

    private static GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max, bool preserve, bool behind)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = obj.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = preserve; image.raycastTarget = false;
        if (behind) obj.transform.SetAsFirstSibling(); else obj.transform.SetAsLastSibling();
        return obj;
    }
}
