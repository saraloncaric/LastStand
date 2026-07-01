using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public static float NormalizedHeight
    {
        get
        {
            if (Instance == null)
                return 0.5f;
            return Instance.GetNormalizedHeight();
        }
    }

    [Header("Kretanje")]
    public float moveSpeed = 28f;
    public float edgeScrollSize = 12f;
    public bool edgeScrollEnabled = true;

    [Header("Gas (Ctrl + scroll)")]
    public float throttleStep = 0.05f;

    [Header("Visina kamere")]
    public float minHeight = 22f;
    public float maxHeight = 350f;

    [Header("Rotacija")]
    public float rotateSpeed = 120f;
    public float minPitch = 25f;
    public float maxPitch = 80f;

    float _throttle = 1f;

    public float Throttle => _throttle;
    public int ThrottlePercent => Mathf.RoundToInt(_throttle * 100f);

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    float GetNormalizedHeight()
    {
        float range = maxHeight - minHeight;
        if (range <= 0.001f)
            return 0f;
        return Mathf.Clamp01((transform.position.y - minHeight) / range);
    }

    float MouseSensitivity => CameraSettingsManager.Instance != null
        ? CameraSettingsManager.Instance.MouseSensitivity
        : 0.2f;

    float MoveSpeedMultiplier => CameraSettingsManager.Instance != null
        ? CameraSettingsManager.Instance.MoveSpeedMultiplier
        : 1f;

    void Update()
    {
        if (PauseMenu.IsPaused)
            return;

        HandleThrottle();

        if (Keyboard.current == null && Mouse.current == null)
            return;

        HandleMove();
        HandleRotate();
    }

    void HandleThrottle()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        bool ctrl = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        if (!ctrl)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Approximately(scroll, 0f))
            return;

        _throttle += Mathf.Sign(scroll) * throttleStep;
        _throttle = Mathf.Clamp(_throttle, 0.01f, 1f);
    }

    void HandleMove()
    {
        Vector3 move = Vector3.zero;
        var ctrl = ControlSettingsManager.Instance;
        var kb = Keyboard.current;

        if (ctrl != null && kb != null)
        {
            if (ctrl.IsDown(ctrl.moveUp)) move.z += 1f;
            if (ctrl.IsDown(ctrl.moveDown)) move.z -= 1f;
            if (ctrl.IsDown(ctrl.moveLeft)) move.x -= 1f;
            if (ctrl.IsDown(ctrl.moveRight)) move.x += 1f;
        }
        else if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.z += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.z -= 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
        }

        if (edgeScrollEnabled && Mouse.current != null)
        {
            Vector2 mp = Mouse.current.position.ReadValue();
            if (mp.x >= 0f && mp.x <= Screen.width && mp.y >= 0f && mp.y <= Screen.height)
            {
                if (mp.x < edgeScrollSize) move.x -= 1f;
                if (mp.x > Screen.width - edgeScrollSize) move.x += 1f;
                if (mp.y < edgeScrollSize) move.z -= 1f;
                if (mp.y > Screen.height - edgeScrollSize) move.z += 1f;
            }
        }

        if (move == Vector3.zero)
            return;

        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 dir = (forward * move.z + right * move.x).normalized;

        float speed = moveSpeed * MoveSpeedMultiplier * _throttle;
        transform.position += dir * speed * Time.deltaTime;
    }

    void HandleRotate()
    {
        float rotateInput = 0f;
        var ctrl = ControlSettingsManager.Instance;

        if (ctrl != null && Keyboard.current != null)
        {
            if (ctrl.IsDown(ctrl.rotateLeft)) rotateInput -= 1f;
            if (ctrl.IsDown(ctrl.rotateRight)) rotateInput += 1f;
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.qKey.isPressed) rotateInput -= 1f;
            if (Keyboard.current.eKey.isPressed) rotateInput += 1f;
        }

        Vector3 euler = transform.eulerAngles;
        float yaw = euler.y;
        float pitch = euler.x;

        if (rotateInput != 0f)
            yaw += rotateInput * rotateSpeed * Time.deltaTime;

        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            float sens = MouseSensitivity;
            float dx = Mouse.current.delta.ReadValue().x;
            float dy = Mouse.current.delta.ReadValue().y;
            yaw += dx * rotateSpeed * sens * Time.deltaTime;
            pitch -= dy * rotateSpeed * sens * Time.deltaTime;
        }

        if (pitch > 180f)
            pitch -= 360f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
