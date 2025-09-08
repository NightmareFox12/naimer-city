using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    void Awake()
    {
        playButton.onClick.AddListener(() =>
        {
            SceneManager.LoadSceneAsync("VirtualCity");
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
