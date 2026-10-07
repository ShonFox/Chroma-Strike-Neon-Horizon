using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Имена сцен
    [SerializeField] private string _mainMenuSceneName = "01_MainMenu";
    [SerializeField] private string _gameLevelSceneName = "02_GameLevel";
    [SerializeField] private string _fatalSceneName = "03_FatalScene";

    public void LoadMainMenu()
    {
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
        }

        SceneManager.LoadScene(_mainMenuSceneName);
    }


    public void LoadGameLevel()
    {
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResetTimer();
            GameTimer.Instance.StartTimer();
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        SceneManager.LoadScene(_gameLevelSceneName);
    }


    public void LoadFatalScene()
    {
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.StopTimer();
        }
        SceneManager.LoadScene(_fatalSceneName);
    }

}
