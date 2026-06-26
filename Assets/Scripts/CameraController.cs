using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Kretanje")]
    public float moveSpeed = 80f;
    public float edgeScrollSize = 12f;
    public bool edgeScrollEnabled = true;

    [Header("Zoom (visina kamere)")]
    public float zoomSpeed = 600f;
    public float minHeight = 40f;
    public float maxHeight = 350f;

    [Header("Rotacija")]
    public float rotateSpeed = 120f;
    public float minPitch = 25f;
    public float maxPitch = 80f;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        HandleMove();
        HandleZoom();
        HandleRotate();
    }

    void HandleMove()
    {
        Vector3 move = Vector3.zero;
        var kb = Keyboard.current;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.z += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.z -= 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;

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

        transform.position += dir * moveSpeed * Time.unscaledDeltaTime;
    }

    void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Approximately(scroll, 0f))
            return;

        Vector3 pos = transform.position;
        pos += transform.forward * Mathf.Sign(scroll) * zoomSpeed * Time.unscaledDeltaTime;
        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        transform.position = pos;
    }

    void HandleRotate()
    {
        float rotateInput = 0f;

        if (Keyboard.current.qKey.isPressed) rotateInput -= 1f;
        if (Keyboard.current.eKey.isPressed) rotateInput += 1f;

        Vector3 euler = transform.eulerAngles;
        float yaw = euler.y;
        float pitch = euler.x;

        if (rotateInput != 0f)
            yaw += rotateInput * rotateSpeed * Time.unscaledDeltaTime;

        if (Mouse.current != null && Mouse.current.middleButton.isPressed)
        {
            float dx = Mouse.current.delta.ReadValue().x;
            float dy = Mouse.current.delta.ReadValue().y;
            yaw += dx * rotateSpeed * Time.unscaledDeltaTime;
            pitch -= dy * rotateSpeed * Time.unscaledDeltaTime;
        }

        if (pitch > 180f)
            pitch -= 360f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
