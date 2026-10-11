using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private void Awake()
    {
        var startButton = GameObject.Find("StartButton");
        var quitButton = GameObject.Find("QuitButton");
        if (startButton == null || quitButton == null)
        {
            Debug.LogError("MainMenu requires StartButton and QuitButton.");
            return;
        }

        startButton.GetComponent<Button>().onClick.AddListener(StartGame);
        quitButton.GetComponent<Button>().onClick.AddListener(QuitGame);
        StyleMenuButton(startButton);
        StyleMenuButton(quitButton);
    }

    private static void StyleMenuButton(GameObject buttonObject)
    {
        var image = buttonObject.GetComponent<Image>();
        if (image != null) image.color = new Color(1f, 1f, 1f, 0f);
        var label = buttonObject.GetComponentInChildren<Text>();
        if (label != null)
        {
            var plainText = label.text.Replace("<u>", string.Empty).Replace("</u>", string.Empty);
            label.supportRichText = false;
            label.text = plainText;
            label.fontStyle = FontStyle.Bold;
            var underline = new GameObject("Underline", typeof(RectTransform), typeof(Image)); underline.transform.SetParent(buttonObject.transform, false); var underlineRect = underline.GetComponent<RectTransform>(); underlineRect.anchorMin = new Vector2(.22f, .12f); underlineRect.anchorMax = new Vector2(.78f, .16f); underlineRect.offsetMin = underlineRect.offsetMax = Vector2.zero; underline.GetComponent<Image>().color = Color.white; underline.GetComponent<Image>().raycastTarget = false;
        }
    }

    public void StartGame()
    {
        // Always begin a fresh run with no stale contractor or politician selections.
        GameSession.Reset();
        SceneManager.LoadScene("OpeningNarration");
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
