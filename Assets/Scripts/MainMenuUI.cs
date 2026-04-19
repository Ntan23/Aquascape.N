using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        playButton.onClick.AddListener(OnClickPlay);
        quitButton.onClick.AddListener(OnClickQuit);
    }

    private void OnClickPlay()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OnClickQuit()
    {
        Application.Quit();
    }
}
