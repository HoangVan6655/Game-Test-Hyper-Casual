using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName = "Game"; 
    public Button playButton;

    void Start()
    {
        if (playButton) playButton.onClick.AddListener(OnPlayClick);
    }

    public void OnPlayClick()
    {
        PlayerPrefs.SetInt("SavedLevel", 0);
        SceneManager.LoadScene(gameSceneName);
    }
}