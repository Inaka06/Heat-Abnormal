using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningNarrationController : MonoBehaviour
{
    private Text narrationText;
    private Text continueLabel;
    private int pageIndex;
    private readonly string[] pages =
    {
        "Nagajaya merupakan sebuah metropolitan dengan banyak penduduk.\n\nSayangnya, banyak dari penduduknya mengidap berbagai penyakit yang disebabkan oleh adanya pembangkit listrik bertenaga kotor.",
        "Kamu, sebagai Komisi Perusahaan Listrik Cabang Nagajaya ditugaskan untuk menyelesaikan masalah tersebut.\n\nSetelah disepakati parlemen, kamu diberikan dana awal sebanyak 350.000 Gajayan untuk menjalankan proyek ini."
    };

    private void Awake()
    {
        var continueButton = GameObject.Find("ContinueButton");
        if (continueButton == null)
        {
            Debug.LogError("OpeningNarration requires ContinueButton.");
            return;
        }

        narrationText = GameObject.Find("NarrationText")?.GetComponent<Text>();
        continueLabel = continueButton.transform.Find("Label")?.GetComponent<Text>();
        pageIndex = 0;
        if (narrationText != null) narrationText.text = pages[pageIndex];
        continueButton.GetComponent<Button>().onClick.AddListener(Continue);
    }

    public void Continue()
    {
        if (pageIndex < pages.Length - 1)
        {
            pageIndex++;
            if (narrationText != null) narrationText.text = pages[pageIndex];
            if (continueLabel != null) continueLabel.text = "Mulai";
            return;
        }
        SceneManager.LoadScene("PembangkitSelect");
    }
}
