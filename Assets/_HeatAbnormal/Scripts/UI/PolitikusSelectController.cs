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
    private Button confirmButton;

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
        CreateText(canvas.transform, "Title", "PILIH 3 POLITIKUS", 46, new Vector2(.1f, .84f), new Vector2(.9f, .95f));
        selectionStatus = CreateText(canvas.transform, "SelectionStatus", "Terpilih: 0 / 3", 26, new Vector2(.1f, .75f), new Vector2(.9f, .82f));

        var container = new GameObject("PoliticianCards", typeof(RectTransform)); container.transform.SetParent(canvas.transform, false);
        var rect = container.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(.06f, .28f); rect.anchorMax = new Vector2(.94f, .72f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        var grid = container.AddComponent<GridLayoutGroup>(); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3; grid.spacing = new Vector2(18f, 18f); grid.cellSize = new Vector2(270f, 175f);
        foreach (var politician in registry.politikusList) if (politician != null) CreateCard(container.transform, politician);
        confirmButton = CreateButton(canvas.transform, "ConfirmButton", "Konfirmasi (pilih 3)", new Vector2(.32f, .1f), new Vector2(.68f, .22f), ConfirmSelection);
        confirmButton.interactable = false;
    }

    private void CreateCard(Transform parent, PolitikusData politician)
    {
        var card = new GameObject(politician.displayName + "Card", typeof(RectTransform), typeof(Image), typeof(Button)); card.transform.SetParent(parent, false);
        var image = card.GetComponent<Image>(); image.color = new Color(.12f, .2f, .28f);
        var button = card.GetComponent<Button>(); button.onClick.AddListener(() => ToggleSelection(politician, button)); cardButtons.Add(button);
        CreateText(card.transform, "Name", politician.displayName, 21, new Vector2(.04f, .62f), new Vector2(.96f, .94f));
        CreateText(card.transform, "Description", politician.deskripsi, 15, new Vector2(.06f, .08f), new Vector2(.94f, .58f));
    }

    private void ToggleSelection(PolitikusData politician, Button button)
    {
        if (selected.Contains(politician)) { selected.Remove(politician); button.GetComponent<Image>().color = new Color(.12f, .2f, .28f); }
        else if (selected.Count < 3) { selected.Add(politician); button.GetComponent<Image>().color = new Color(.18f, .5f, .3f); }
        UpdateSelectionState();
    }

    private void UpdateSelectionState()
    {
        selectionStatus.text = "Terpilih: " + selected.Count + " / 3";
        confirmButton.interactable = selected.Count == 3;
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
        var text = obj.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
        obj.GetComponent<Image>().color = new Color(.15f, .45f, .7f); var button = obj.GetComponent<Button>(); button.onClick.AddListener(action); CreateText(obj.transform, "Label", label, 24, Vector2.zero, Vector2.one); return button;
    }
}
