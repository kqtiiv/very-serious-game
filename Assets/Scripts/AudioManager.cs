using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public float vol = 1f;
        public AudioClip musicClip;
    }

    [Header("Scene Music Mapping")]
    [SerializeField] private List<SceneMusic> sceneMusicList;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneMusic music = sceneMusicList.Find(x => x.sceneName == scene.name.Substring(0, 3));
        if (music != null)
            PlayMusic(music.musicClip, music.vol);
    }

    public void PlayMusic(AudioClip clip, float vol)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(FadeAndSwitch(clip, vol));
    }

    private IEnumerator FadeAndSwitch(AudioClip newClip, float targetVol = 1f)
    {
        // Fade out
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
                yield return null;
            }
            musicSource.volume = 0f;
            musicSource.Stop();
        }

        if (newClip == null)
            yield break;

        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        // Fade in
        float elapsed2 = 0f;
        while (elapsed2 < fadeDuration)
        {
            elapsed2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVol, elapsed2 / fadeDuration);
            yield return null;
        }
        musicSource.volume = targetVol;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayAudio(AudioClip clip, float vol = 1f)
    {
        sfxSource.clip = clip;
        sfxSource.volume = vol;
        sfxSource.Play();
    }

    public void StopAudio()
    {
        sfxSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
