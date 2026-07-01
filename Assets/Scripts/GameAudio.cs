using UnityEngine;

public enum SfxCategory
{
    Default,
    SoldierAttack,
    EnemyAttack,
    Death,
    Footstep
}

[System.Serializable]
public class SpatialSound
{
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 1f;

    [Range(0f, 1f)] public float spatialBlend = 1f;

    public float minDistance = 2f;
    public float maxDistance = 30f;
    public bool loop;

    public bool IsValid => clip != null;
}

public static class GameAudio
{
    const float SoldierAttackMult = 0.14f;
    const float EnemyAttackMult = 0.39f;
    const float DeathMult = 0.70f;
    const float FootstepMult = 0.3f;
    const float EnemyHearRangeMult = 0.5f;

    public static float GetEffectiveSfxVolume(float localVolume, SfxCategory category = SfxCategory.Default)
    {
        float cat = GetCategoryMultiplier(category);
        var s = AudioSettingsManager.Instance;
        float master = s != null ? s.sfxVolume : 1f;

        if (s != null && s.HasMixer)
            return Mathf.Clamp01(localVolume * cat * master);

        return Mathf.Clamp01(localVolume * cat * master);
    }

    static float GetCategoryMultiplier(SfxCategory category)
    {
        switch (category)
        {
            case SfxCategory.SoldierAttack: return SoldierAttackMult;
            case SfxCategory.EnemyAttack: return EnemyAttackMult;
            case SfxCategory.Death: return DeathMult;
            case SfxCategory.Footstep: return GetFootstepMultiplier();
            default: return 1f;
        }
    }

    static float GetFootstepMultiplier()
    {
        float h = CameraController.NormalizedHeight;
        return FootstepMult * Mathf.Lerp(1f, 0.08f, h);
    }

    static float GetAdjustedMaxDistance(SfxCategory category, float baseMax)
    {
        float h = CameraController.NormalizedHeight;

        switch (category)
        {
            case SfxCategory.Death:
                return Mathf.Lerp(baseMax, baseMax * 2.8f, h);
            case SfxCategory.EnemyAttack:
                return Mathf.Lerp(baseMax, baseMax * 2.2f, h) * EnemyHearRangeMult;
            case SfxCategory.SoldierAttack:
                return Mathf.Lerp(baseMax, baseMax * 3f, h);
            case SfxCategory.Footstep:
                return Mathf.Lerp(baseMax, baseMax * 0.45f, h) * EnemyHearRangeMult;
            default:
                return baseMax;
        }
    }

    static float GetSpatialBlend(SpatialSound sound, SfxCategory category)
    {
        float h = CameraController.NormalizedHeight;
        float blend = sound.spatialBlend;

        switch (category)
        {
            case SfxCategory.Death:
                return Mathf.Lerp(blend, 0.35f, h * 0.6f);
            case SfxCategory.EnemyAttack:
                return Mathf.Lerp(0.35f, blend, 1f - h * 0.5f);
            case SfxCategory.SoldierAttack:
                return Mathf.Lerp(0.2f, blend, 1f - h * 0.65f);
            case SfxCategory.Footstep:
                return blend;
            default:
                return blend;
        }
    }

    public static float GetEffectiveMusicVolume(float localVolume)
    {
        if (AudioSettingsManager.Instance != null)
            return localVolume * AudioSettingsManager.Instance.musicVolume;

        return localVolume;
    }

    static void ApplyCommon(AudioSource source, SpatialSound sound, SfxCategory category)
    {
        source.clip = sound.clip;
        source.spatialBlend = GetSpatialBlend(sound, category);
        source.minDistance = sound.minDistance;
        source.maxDistance = GetAdjustedMaxDistance(category, sound.maxDistance);
        source.loop = sound.loop;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.dopplerLevel = 0f;
        source.playOnAwake = false;

        if (AudioSettingsManager.Instance != null)
            source.outputAudioMixerGroup = AudioSettingsManager.Instance.sfxGroup;
    }

    public static void ApplySettings(AudioSource source, SpatialSound sound, SfxCategory category = SfxCategory.Default)
    {
        if (source == null || sound == null || !sound.IsValid)
            return;

        ApplyCommon(source, sound, category);
        source.volume = GetEffectiveSfxVolume(sound.volume, category);
    }

    public static void PlayLoop(AudioSource source, SpatialSound sound, SfxCategory category = SfxCategory.Default)
    {
        if (!sound.IsValid || source == null)
            return;

        ApplySettings(source, sound, category);

        if (!source.isPlaying)
            source.Play();
    }

    public static void StopLoop(AudioSource source)
    {
        if (source != null && source.isPlaying)
            source.Stop();
    }

    public static void PlayOneShot(AudioSource source, SpatialSound sound, SfxCategory category = SfxCategory.Default)
    {
        if (!sound.IsValid || source == null)
            return;

        ApplyCommon(source, sound, category);
        source.volume = 1f;
        source.PlayOneShot(sound.clip, GetEffectiveSfxVolume(sound.volume, category));
    }

    public static void PlayAtPoint(SpatialSound sound, Vector3 position, SfxCategory category = SfxCategory.Default)
    {
        if (!sound.IsValid)
            return;

        var temp = new GameObject("Sfx_" + sound.clip.name);
        temp.transform.position = position;

        AudioSource source = temp.AddComponent<AudioSource>();
        ApplyCommon(source, sound, category);
        source.loop = false;
        source.volume = 1f;
        source.PlayOneShot(sound.clip, GetEffectiveSfxVolume(sound.volume, category));

        Object.Destroy(temp, sound.clip.length + 0.25f);
    }
}
