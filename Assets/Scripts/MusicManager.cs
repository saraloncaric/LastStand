using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Glazba")]
    public AudioClip prepareMusic;
    public AudioClip waveMusic;

    [Header("Postavke")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private bool fading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = volume;
    }

    void Start()
    {
        GameManager.OnWaveChanged += OnWaveChanged;
        GameManager.OnPreparePhase += OnPreparePhase;

        PlayMusic(prepareMusic);
    }

    void OnDestroy()
    {
        GameManager.OnWaveChanged -= OnWaveChanged;
        GameManager.OnPreparePhase -= OnPreparePhase;
    }

    void OnWaveChanged(int wave)
    {
        PlayMusic(waveMusic);
    }

    void OnPreparePhase()
    {
        PlayMusic(prepareMusic);
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null || audioSource.clip == clip) return;

        if (fading)
            StopAllCoroutines();

        StartCoroutine(FadeToClip(clip));
    }

    System.Collections.IEnumerator FadeToClip(AudioClip newClip)
    {
        fading = true;

        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
        fading = false;
    }
}
