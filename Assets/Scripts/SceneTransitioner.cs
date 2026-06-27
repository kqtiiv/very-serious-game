using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitioner : MonoBehaviour
{
    public static SceneTransitioner Instance;

    [SerializeField] private RectTransform curtain;
    [SerializeField] private float slideSpeed = 1000f;
    [SerializeField] float waitBetweenScene = 1f;

    private float screenHeight;

    private void Awake()
    {
        //if (Instance != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        screenHeight = curtain.rect.height;
        curtain.anchoredPosition = new Vector2(0, screenHeight);
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        yield return StartCoroutine(SlideCurtain(0));
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(waitBetweenScene);
        yield return StartCoroutine(SlideCurtain(screenHeight));
    }

    private IEnumerator SlideCurtain(float targetY)
    {
        while (Mathf.Abs(curtain.anchoredPosition.y - targetY) > 50f)
        {
            float direction = Mathf.Sign(targetY - curtain.anchoredPosition.y);
            curtain.anchoredPosition += Vector2.up * direction * slideSpeed * Time.deltaTime;
            yield return null;
        }

        curtain.anchoredPosition = new Vector2(0, targetY);
    }
}