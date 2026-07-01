using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Master volume")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    [Header("Opcionalno: AudioMixer")]
    public AudioMixer mixer;
    public string sfxVolumeParam = "SFXVolume";
    public AudioMixerGroup sfxGroup;

    public bool HasMixer => mixer != null && sfxGroup != null;

    const string MusicKey = "opt_music_vol";
    const string SfxKey = "opt_sfx_vol";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        musicVolume = PlayerPrefs.GetFloat(MusicKey, musicVolume);
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, sfxVolume));
        ApplySfxToMixer();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicKey, musicVolume);

        if (MusicManager.Instance != null)
            MusicManager.Instance.RefreshVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxKey, sfxVolume);
        ApplySfxToMixer();
    }

    void ApplySfxToMixer()
    {
        if (!HasMixer)
            return;

        float dB = sfxVolume <= 0.0001f ? -80f : Mathf.Log10(Mathf.Max(sfxVolume, 0.0001f)) * 20f;
        mixer.SetFloat(sfxVolumeParam, dB);
    }
}
