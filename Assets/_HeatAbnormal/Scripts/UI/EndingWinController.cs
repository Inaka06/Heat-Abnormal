using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class EndingWinController : MonoBehaviour
{
    private void Awake()
    {
        var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        var textObject = new GameObject("WinMessage", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(canvas.transform, false);
        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(.1f, .35f); rect.anchorMax = new Vector2(.9f, .65f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 32; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
        var state = GameSession.State;
        var plant = state.SelectedPembangkit;
        var contractor = state.SelectedKontraktor;
        var politicians = string.Join(", ", state.SelectedPolitikus.ConvertAll(p => p == null ? "Unknown" : p.displayName));
        text.text = "PENYAMBUNGAN BERHASIL\n\n" +
            "Pembangkit: " + (plant == null ? "-" : plant.displayName) + "\n" +
            "Kontraktor: " + (contractor == null ? "-" : contractor.displayName) + "\n" +
            "Politikus: " + politicians + "\n" +
            "Periode terpakai: " + state.PeriodeTerpakai + "\n" +
            "Dana akhir: " + state.Dana.ToString("N0") + " Gajayan";
        CreateButton(canvas.transform, "PlayAgain", "Main Lagi", new Vector2(.25f, .16f), new Vector2(.48f, .26f), () => { GameSession.Reset(); SceneManager.LoadScene("MainMenu"); });
        CreateButton(canvas.transform, "Quit", "Keluar", new Vector2(.52f, .16f), new Vector2(.75f, .26f), QuitGame);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
        obj.GetComponent<Image>().color = new Color(.15f,.45f,.7f); obj.GetComponent<Button>().onClick.AddListener(action);
        var t = new GameObject("Label", typeof(RectTransform), typeof(Text)); t.transform.SetParent(obj.transform,false); var tr=t.GetComponent<RectTransform>(); tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero; var tx=t.GetComponent<Text>();tx.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tx.fontSize=24;tx.alignment=TextAnchor.MiddleCenter;tx.color=Color.white;tx.text=label; return obj.GetComponent<Button>();
    }
}
