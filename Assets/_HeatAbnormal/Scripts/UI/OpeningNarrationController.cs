using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningNarrationController : MonoBehaviour
{
    private void Awake()
    {
        var continueButton = GameObject.Find("ContinueButton");
        if (continueButton == null)
        {
            Debug.LogError("OpeningNarration requires ContinueButton.");
            return;
        }

        continueButton.GetComponent<Button>().onClick.AddListener(Continue);
    }

    public void Continue()
    {
        SceneManager.LoadScene("PembangkitSelect");
    }
}
