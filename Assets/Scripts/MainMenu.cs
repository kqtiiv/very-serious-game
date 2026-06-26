using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Sound Effects")]
    public AudioClip buttonClick;

    [Header("Stages (in order)")]
    [SerializeField] private string[] stageScenes;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void OnStart()
    {
        Debug.Log("Starting...");
        AudioManager.Instance.PlaySFX(buttonClick);
        SceneTransitioner.Instance.LoadSceneWithTransition(GetFurthestScene());
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("Returnng to main menu");
        SceneTransitioner.Instance.LoadSceneWithTransition(mainMenuSceneName);
    }

    public void OnExit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
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
