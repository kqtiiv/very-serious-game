using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform backdrop;
    [SerializeField] private float slideSpeed = 2000f;
    [SerializeField] private float fadeDuration = 0.2f;

    private bool isPaused = false;
    private float hiddenY;
    private const float visibleY = 0f;
    private Coroutine slideCoroutine;
    private Coroutine fadeCoroutine;
    private Image backdropImage;

    private void Start()
    {
        RectTransform canvasRect = panel.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        hiddenY = canvasRect.rect.height;
        panel.anchoredPosition = new Vector2(0f, hiddenY);

        if (backdrop != null)
        {
            backdropImage = backdrop.GetComponent<Image>();
            SetBackdropAlpha(0f);
        }
    }

    private void Update()
    {
        if (!SceneManager.GetActiveScene().name.StartsWith("Stage")) return;

        if (isPaused)
            Time.timeScale = 0f;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        AudioManager.Instance.PauseMusic();
        Slide(visibleY);
        Fade(0.5f, true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance.ResumeMusic();
        Slide(hiddenY);
        Fade(0f, false);
    }

    public void RestartStage()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneTransitioner.Instance.LoadSceneWithTransition(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SceneTransitioner.Instance.LoadSceneWithTransition("MainMenu");
    }

    private void Slide(float targetY)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideTo(targetY));
    }

    private IEnumerator SlideTo(float targetY)
    {
        while (Mathf.Abs(panel.anchoredPosition.y - targetY) > 1f)
        {
            float newY = Mathf.MoveTowards(panel.anchoredPosition.y, targetY, slideSpeed * Time.unscaledDeltaTime);
            panel.anchoredPosition = new Vector2(0f, newY);
            yield return null;
        }
        panel.anchoredPosition = new Vector2(0f, targetY);
    }

    private void Fade(float targetAlpha, bool blocksRaycasts)
    {
        if (backdropImage == null) return;
        backdropImage.raycastTarget = blocksRaycasts;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = backdropImage.color.a;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetBackdropAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration));
            yield return null;
        }
        SetBackdropAlpha(targetAlpha);
    }

    private void SetBackdropAlpha(float alpha)
    {
        Color c = backdropImage.color;
        c.a = alpha;
        backdropImage.color = c;
    }
}
