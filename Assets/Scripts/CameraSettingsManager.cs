using UnityEngine;

public class CameraSettingsManager : MonoBehaviour
{
    public static CameraSettingsManager Instance { get; private set; }

    [Range(10f, 100f)] public float mouseSensitivityPercent = 50f;

    [Range(10f, 100f)] public float moveSpeedPercent = 100f;

    const string MouseKey = "opt_mouse_sens_pct";
    const string MoveKey = "opt_move_speed_pct";
    const string LegacyKey = "opt_cam_sensitivity";
    const string LegacyMoveKey = "opt_move_speed";

    public float MouseSensitivity => mouseSensitivityPercent * 0.004f;

    public float MoveSpeedMultiplier => moveSpeedPercent / 100f;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Load();
    }

    void Load()
    {
        if (PlayerPrefs.HasKey(MouseKey))
            mouseSensitivityPercent = PlayerPrefs.GetFloat(MouseKey, mouseSensitivityPercent);
        else if (PlayerPrefs.HasKey(LegacyKey))
            mouseSensitivityPercent = LegacyInternalToPercent(PlayerPrefs.GetFloat(LegacyKey, 0.2f));

        if (PlayerPrefs.HasKey(MoveKey))
            moveSpeedPercent = PlayerPrefs.GetFloat(MoveKey, moveSpeedPercent);
        else if (PlayerPrefs.HasKey(LegacyMoveKey))
            moveSpeedPercent = Mathf.Clamp(PlayerPrefs.GetFloat(LegacyMoveKey, 1f) * 100f, 10f, 100f);

        mouseSensitivityPercent = Mathf.Clamp(mouseSensitivityPercent, 10f, 100f);
        moveSpeedPercent = Mathf.Clamp(moveSpeedPercent, 10f, 100f);
    }

    static float LegacyInternalToPercent(float internalValue)
    {
        return Mathf.Clamp(internalValue / 0.004f, 10f, 100f);
    }

    public void SetMouseSensitivityPercent(float percent)
    {
        mouseSensitivityPercent = Mathf.Clamp(percent, 10f, 100f);
        PlayerPrefs.SetFloat(MouseKey, mouseSensitivityPercent);
    }

    public void SetMoveSpeedPercent(float percent)
    {
        moveSpeedPercent = Mathf.Clamp(percent, 10f, 100f);
        PlayerPrefs.SetFloat(MoveKey, moveSpeedPercent);
    }
}
