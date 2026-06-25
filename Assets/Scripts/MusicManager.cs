using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private float startBeat = 0f;
    [SerializeField] public AudioClip song;
    [SerializeField] private float bpm = 120f;
    [SerializeField] private float startDelay = 1f;

    private AudioSource audioSource;
    private double dspSongStartTime;
    private bool hasStarted;

    public float BPM => bpm;
    public float SecondsPerBeat => 60f / bpm;

    public float CurrentBeat
    {
        get
        {
            if (!hasStarted)
                return startBeat;

            return startBeat +
                   (float)(AudioSettings.dspTime - dspSongStartTime) / SecondsPerBeat;
        }
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = song;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        PlaySong();
    }

    public void PlaySong()
    {
        float startTime = BeatToSeconds(startBeat);

        dspSongStartTime = AudioSettings.dspTime + startDelay;

        audioSource.time = Mathf.Min(startTime, song.length);
        audioSource.PlayScheduled(dspSongStartTime);

        hasStarted = true;
    }

    public float BeatToSeconds(float beat)
    {
        return beat * SecondsPerBeat;
    }
}