using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Sound Effects")]
    public AudioClip buttonClick;

    [Header("Stages (in order)")]
    [SerializeField] private string[] stageScenes;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private Text playButtonText;
    [SerializeField] private GameObject restartButton;

    private void Start()
    {
        bool hasProgress = HasSavedProgress();
        if (playButtonText != null)
            playButtonText.text = hasProgress ? "Continue" : "Play";
        if (restartButton != null)
            restartButton.SetActive(hasProgress);
    }

    public void OnStart()
    {
        AudioManager.Instance.PlaySFX(buttonClick);
        SceneTransitioner.Instance.LoadSceneWithTransition(GetFurthestScene());
    }

    public void OnRestart()
    {
        AudioManager.Instance.PlaySFX(buttonClick);
        for (int i = 1; i < stageScenes.Length; i++)
            PlayerPrefs.DeleteKey("StageUnlocked_" + i);
        PlayerPrefs.Save();

        if (playButtonText != null)
            playButtonText.text = "Play";
        if (restartButton != null)
            restartButton.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        SceneTransitioner.Instance.LoadSceneWithTransition(mainMenuSceneName);
    }

    public void OnExit()
    {
        Application.Quit();
    }

    private bool HasSavedProgress()
    {
        return PlayerPrefs.GetInt("StageUnlocked_1", 0) == 1;
    }

    private string GetFurthestScene()
    {
        int targetStage = 0;
        for (int i = 1; i < stageScenes.Length; i++)
        {
            if (PlayerPrefs.GetInt("StageUnlocked_" + i, 0) == 1)
                targetStage = i;
            else
                break;
        }
        return stageScenes[targetStage];
    }
}
