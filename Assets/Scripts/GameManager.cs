using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float currentTime = 3f;
    [SerializeField] private float endTime = 7f;

    [Header("Stage")]
    [SerializeField] private int stageIndex = 0;
    [SerializeField] private string nextSceneName = "";

    private float realSecondsToFinish = 180f;
    private bool gameEnded = false;

    [SerializeField] private SpriteRenderer[] clockDigits = new SpriteRenderer[4];

    [Tooltip("Index 0 = sprite for 0, index 1 = sprite for 1, etc.")]
    [SerializeField] private Sprite[] digitSprites = new Sprite[10];
    [SerializeField] private Animator skyController;
    [Header("Lighting")]
    [SerializeField] private Light2D roomLight;
    [SerializeField] private float startIntensity = 0f;
    [SerializeField] private float endIntensity = 0.5f;

    private int lastDisplayedMinutes = -1;

    private void Start()
    {
        realSecondsToFinish = AudioManager.Instance.SongLength;
        UpdateClock();
    }

    private void Update()
    {
        if (gameEnded)
            return;

        if (currentTime >= endTime)
        {
            StartCoroutine(Victory());
            return;
        }

        float hoursPerSecond = (endTime - 3f) / realSecondsToFinish;

        currentTime += hoursPerSecond * Time.deltaTime;
        currentTime = Mathf.Min(currentTime, endTime);

        UpdateClock();
        float t = Mathf.InverseLerp(3f, endTime, currentTime);

        roomLight.intensity = Mathf.Lerp(startIntensity, endIntensity, t);
    }

    public IEnumerator Victory()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            AudioManager.Instance.PlayAlarm();
            yield return new WaitForSeconds(3f);
            AudioManager.Instance.StopAudio();
            // save so player doesnt have to start again if died
            PlayerPrefs.SetInt("StageUnlocked_" + (stageIndex + 1), 1);
            PlayerPrefs.Save();

            Debug.Log($"Stage {stageIndex + 1} unlocked");

            if (!string.IsNullOrEmpty(nextSceneName))
                SceneTransitioner.Instance.LoadSceneWithTransition(nextSceneName);
        }
    }

    public void GameOver()
    {
        if (gameEnded) return;
        gameEnded = true;

        SceneTransitioner.Instance.LoadSceneWithTransition("GameOver");
    }

    private void UpdateClock()
    {
        int totalMinutes = Mathf.FloorToInt(currentTime * 60f);

        if (totalMinutes == lastDisplayedMinutes)
            return;
        skyController.SetFloat("Time", currentTime);
        lastDisplayedMinutes = totalMinutes;

        int hour = totalMinutes / 60;
        int minute = totalMinutes % 60;

        clockDigits[0].sprite = digitSprites[hour / 10];
        clockDigits[1].sprite = digitSprites[hour % 10];
        clockDigits[2].sprite = digitSprites[minute / 10];
        clockDigits[3].sprite = digitSprites[minute % 10];
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
}