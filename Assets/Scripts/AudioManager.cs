using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip alarmSound;
    public static AudioManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public float vol = 1f;
        public AudioClip musicClip;
        public bool isRhythmTrack = false;
        public float startBeat = 0f;
        public float bpm = 120f;
        public float startDelay = 1f;
    }

    [Header("Scene Music Mapping")]
    [SerializeField] private List<SceneMusic> sceneMusicList;
    [SerializeField] private AudioClip defaultMusicClip;
    [SerializeField] private float defaultVol = 1f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private SceneMusic currentSceneMusic;
    private double dspSongStartTime;
    private bool rhythmTrackStarted;

    public float SongLength => musicSource.clip != null ? musicSource.clip.length : 0f;
    public float BPM => currentSceneMusic != null ? currentSceneMusic.bpm : 120f;
    public float SecondsPerBeat => 60f / BPM;

    public float CurrentBeat
    {
        get
        {
            if (!rhythmTrackStarted || currentSceneMusic == null)
                return 0f;
            return currentSceneMusic.startBeat +
                   (float)(AudioSettings.dspTime - dspSongStartTime) / SecondsPerBeat;
        }
    }

    public float BeatToSeconds(float beat) => beat * SecondsPerBeat;

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
        SceneMusic music = sceneMusicList.Find(x => scene.name.StartsWith(x.sceneName));
        if (music == null)
        {
            PlayMusic(defaultMusicClip, defaultVol);
            return;
        }

        if (music.isRhythmTrack)
            PlayRhythmTrack(music);
        else
            PlayMusic(music.musicClip, music.vol);
    }

    private void PlayRhythmTrack(SceneMusic music)
    {
        StopAllCoroutines();
        rhythmTrackStarted = false;
        currentSceneMusic = music;

        musicSource.Stop();
        musicSource.loop = false;
        musicSource.clip = music.musicClip;
        musicSource.volume = music.vol;

        float startTime = music.startBeat * SecondsPerBeat;
        dspSongStartTime = AudioSettings.dspTime + music.startDelay;

        musicSource.time = Mathf.Min(startTime, music.musicClip.length);
        musicSource.PlayScheduled(dspSongStartTime);

        rhythmTrackStarted = true;
    }

    public void PlayMusic(AudioClip clip, float vol)
    {
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StopAllCoroutines();
        rhythmTrackStarted = false;
        StartCoroutine(FadeAndSwitch(clip, vol));
    }

    private IEnumerator FadeAndSwitch(AudioClip newClip, float targetVol = 1f)
    {
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

        currentSceneMusic = null;
        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        float elapsed2 = 0f;
        while (elapsed2 < fadeDuration)
        {
            elapsed2 += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVol, elapsed2 / fadeDuration);
            yield return null;
        }
        musicSource.volume = targetVol;
    }

    public void PlayAlarm()
    {
        PlayAudio(alarmSound);
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
