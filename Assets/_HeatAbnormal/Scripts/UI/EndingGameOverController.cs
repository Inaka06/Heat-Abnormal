using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class EndingGameOverController : MonoBehaviour
{
    private void Awake()
    {
        var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(InputSystemUIInputModule)); es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); var canvas=canvasObject.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; var scaler=canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1920,1080); CreateBackground(canvas.transform); CreateResultFrame(canvas.transform);
        var textObject=new GameObject("GameOverMessage",typeof(RectTransform),typeof(Text));textObject.transform.SetParent(canvas.transform,false);var rect=textObject.GetComponent<RectTransform>();rect.anchorMin=new Vector2(.1f,.35f);rect.anchorMax=new Vector2(.9f,.68f);rect.offsetMin=rect.offsetMax=Vector2.zero;var text=textObject.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=42;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.text=ReasonText(GameSession.State.GameOverReason);
        Button(canvas.transform,"PlayAgain","Main Lagi",new Vector2(.25f,.16f),new Vector2(.48f,.26f),()=>{GameSession.Reset();SceneManager.LoadScene("MainMenu");}); Button(canvas.transform,"Quit","Keluar",new Vector2(.52f,.16f),new Vector2(.75f,.26f),QuitGame);
    }
    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit();
#endif
    }
    private static void CreateBackground(Transform parent){var o=new GameObject("KotaBackground",typeof(RectTransform),typeof(Image));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var i=o.GetComponent<Image>();i.sprite=Resources.Load<Sprite>("HeatAbnormal/Kota");i.preserveAspect=false;i.raycastTarget=false;o.transform.SetAsFirstSibling();}
    private static void CreateResultFrame(Transform parent){var o=new GameObject("ResultFrame",typeof(RectTransform),typeof(Image));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.anchorMin=new Vector2(.08f,.24f);r.anchorMax=new Vector2(.92f,.80f);r.offsetMin=r.offsetMax=Vector2.zero;var i=o.GetComponent<Image>();i.sprite=Resources.Load<Sprite>("HeatAbnormal/PixelFrame16x9RoundedNoTitle");i.preserveAspect=false;i.color=new Color(.42f,.42f,.42f,.96f);i.raycastTarget=false;o.transform.SetSiblingIndex(1);}
    private static string ReasonText(GameOverReason r){switch(r){case GameOverReason.DanaHabis:return "GAME OVER\n\nDana habis selama dua periode berturut-turut.";case GameOverReason.Kiamat:return "GAME OVER\n\nKiamat menghancurkan proyek pembangunan.";case GameOverReason.KembaliKeBatuBara:return "GAME OVER\n\nNagajaya kembali bergantung pada batu bara.";case GameOverReason.BeralihKeGasAlam:return "GAME OVER\n\nProyek beralih sepenuhnya ke gas alam.";case GameOverReason.Other:return "GAME OVER\n\nProyek gagal karena alasan lain.";default:return "GAME OVER\n\nPenyebab belum ditentukan.";}}
    private static void Button(Transform p,string n,string l,Vector2 a,Vector2 b,UnityEngine.Events.UnityAction x){var o=new GameObject(n,typeof(RectTransform),typeof(Image),typeof(Button));o.transform.SetParent(p,false);var r=o.GetComponent<RectTransform>();r.anchorMin=a;r.anchorMax=b;r.offsetMin=r.offsetMax=Vector2.zero;o.GetComponent<Image>().color=new Color(.15f,.45f,.7f);o.GetComponent<Button>().onClick.AddListener(x);var t=new GameObject("Label",typeof(RectTransform),typeof(Text));t.transform.SetParent(o.transform,false);var tr=t.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;var tx=t.GetComponent<Text>();tx.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tx.fontSize=24;tx.alignment=TextAnchor.MiddleCenter;tx.color=Color.white;tx.text=l;}
}
