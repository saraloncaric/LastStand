using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Glazba")]
    public AudioClip prepareMusic;

    public AudioClip waveMusic;

    [Header("Zvuk pocetka vala")]
    public AudioClip waveStartSound;

    [Header("Postavke")]
    [Range(0f, 1f)] public float volume = 0.5f;
    public float fadeDuration = 1.5f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = GameAudio.GetEffectiveMusicVolume(volume);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
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
        if (waveStartSound != null)
            sfxSource.PlayOneShot(waveStartSound, GameAudio.GetEffectiveMusicVolume(1f));

        if (waveMusic != null)
            PlayMusic(waveMusic);
        else
            FadeMusicOut();
    }

    void OnPreparePhase()
    {
        PlayMusic(prepareMusic);
    }

    public void RefreshVolume()
    {
        if (musicSource != null && musicSource.isPlaying && fadeRoutine == null)
            musicSource.volume = GameAudio.GetEffectiveMusicVolume(volume);
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeToClip(clip));
    }

    void FadeMusicOut()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOut());
    }

    System.Collections.IEnumerator FadeOut()
    {
        float startVolume = musicSource.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        fadeRoutine = null;
    }

    System.Collections.IEnumerator FadeToClip(AudioClip newClip)
    {
        float targetVolume = GameAudio.GetEffectiveMusicVolume(volume);
        float startVolume = musicSource.volume;
        float t = 0f;

        if (musicSource.isPlaying)
        {
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.Play();

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = GameAudio.GetEffectiveMusicVolume(volume);
        fadeRoutine = null;
    }
}
