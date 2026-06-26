using UnityEngine;

[System.Serializable]
public class SpatialSound
{
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 1f;

    [Tooltip("0 = 2D (same volume everywhere), 1 = 3D (quieter when far from camera)")]
    [Range(0f, 1f)] public float spatialBlend = 1f;

    [Tooltip("Full volume inside this distance")]
    public float minDistance = 2f;

    [Tooltip("Silent beyond this distance")]
    public float maxDistance = 30f;

    public bool loop;

    public bool IsValid => clip != null;
}

public static class GameAudio
{
    public static float GetEffectiveSfxVolume(float localVolume)
    {
        if (AudioSettingsManager.Instance != null)
            return localVolume * AudioSettingsManager.Instance.sfxVolume;

        return localVolume;
    }

    public static float GetEffectiveMusicVolume(float localVolume)
    {
        if (AudioSettingsManager.Instance != null)
            return localVolume * AudioSettingsManager.Instance.musicVolume;

        return localVolume;
    }

    public static void ApplySettings(AudioSource source, SpatialSound sound)
    {
        if (source == null || sound == null || !sound.IsValid)
            return;

        source.clip = sound.clip;
        source.volume = GetEffectiveSfxVolume(sound.volume);
        source.spatialBlend = sound.spatialBlend;
        source.minDistance = sound.minDistance;
        source.maxDistance = sound.maxDistance;
        source.loop = sound.loop;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.dopplerLevel = 0f;
        source.playOnAwake = false;
    }

    public static void PlayLoop(AudioSource source, SpatialSound sound)
    {
        if (!sound.IsValid || source == null)
            return;

        ApplySettings(source, sound);

        if (!source.isPlaying)
            source.Play();
    }

    public static void StopLoop(AudioSource source)
    {
        if (source != null && source.isPlaying)
            source.Stop();
    }

    public static void PlayOneShot(AudioSource source, SpatialSound sound)
    {
        if (!sound.IsValid || source == null)
            return;

        ApplySettings(source, sound);
        source.PlayOneShot(sound.clip, GetEffectiveSfxVolume(sound.volume));
    }

    public static void PlayAtPoint(SpatialSound sound, Vector3 position)
    {
        if (!sound.IsValid)
            return;

        var temp = new GameObject("Sfx_" + sound.clip.name);
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        ApplySettings(source, sound);
        source.loop = false;
        source.Play();

        Object.Destroy(temp, sound.clip.length + 0.1f);
    }
}
