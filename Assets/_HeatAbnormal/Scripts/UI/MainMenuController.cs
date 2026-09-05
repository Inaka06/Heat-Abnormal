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
    }

    public void StartGame()
    {
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
