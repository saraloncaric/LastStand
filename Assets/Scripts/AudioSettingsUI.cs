using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Glazba (0 - 100%)")]
    public Slider musicSlider;
    public TMP_Text musicLabel;

    [Header("Ostali zvukovi (0 - 100%)")]
    public Slider sfxSlider;
    public TMP_Text sfxLabel;

    [Header("Opcionalno: mute gumb za glazbu")]
    public Button muteMusicButton;

    float _lastMusicBeforeMute = 0.5f;

    void Start()
    {
        var s = AudioSettingsManager.Instance;
        if (s == null)
        {
            Debug.LogWarning("AudioSettingsUI: nema AudioSettingsManager u sceni.");
            return;
        }

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(s.musicVolume);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            UpdateMusicLabel(s.musicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(s.sfxVolume);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            UpdateSfxLabel(s.sfxVolume);
        }

        if (muteMusicButton != null)
            muteMusicButton.onClick.AddListener(ToggleMuteMusic);
    }

    void OnMusicChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetMusicVolume(value);
        UpdateMusicLabel(value);
    }

    void OnSfxChanged(float value)
    {
        if (AudioSettingsManager.Instance != null)
            AudioSettingsManager.Instance.SetSfxVolume(value);
        UpdateSfxLabel(value);
    }

    void ToggleMuteMusic()
    {
        if (musicSlider == null)
            return;

        if (musicSlider.value > 0.001f)
        {
            _lastMusicBeforeMute = musicSlider.value;
            musicSlider.value = 0f;
        }
        else
        {
            musicSlider.value = _lastMusicBeforeMute > 0.001f ? _lastMusicBeforeMute : 0.5f;
        }
    }

    void UpdateMusicLabel(float value)
    {
        if (musicLabel != null)
            musicLabel.text = "Glazba: " + Mathf.RoundToInt(value * 100f) + "%";
    }

    void UpdateSfxLabel(float value)
    {
        if (sfxLabel != null)
            sfxLabel.text = "Zvukovi: " + Mathf.RoundToInt(value * 100f) + "%";
    }
}
