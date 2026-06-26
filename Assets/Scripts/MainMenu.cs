using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Sound Effects")]
    public AudioClip buttonClick;

    [Header("Stages (in order)")]
    [SerializeField] private string[] stageScenes;

    public void OnStart()
    {
        AudioManager.Instance.PlaySFX(buttonClick);
        TransitionManager.Instance.LoadSceneWithTransition(GetFurthestScene());
    }

    public void OnExit()
    {
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
