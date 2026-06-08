using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 80f;
    public float edgeScrollSize = 20f;

    void Update() {
        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) move.z += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) move.z -= 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move.x += 1;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (mousePos.x < edgeScrollSize) move.x -= 1;
        if (mousePos.x > Screen.width - edgeScrollSize) move.x += 1;
        if (mousePos.y < edgeScrollSize) move.z -= 1;
        if (mousePos.y > Screen.height - edgeScrollSize) move.z += 1;

        Vector3 forward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0, transform.right.z).normalized;
        transform.position += (forward * move.z + right * move.x) * moveSpeed * Time.deltaTime;

        float scroll = Mouse.current.scroll.ReadValue().y;
        Camera.main.fieldOfView -= scroll * zoomSpeed;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minZoom, maxZoom);

        if (Mouse.current.rightButton.isPressed) {
            float mouseX = Mouse.current.delta.ReadValue().x;
            float mouseY = Mouse.current.delta.ReadValue().y;
            transform.eulerAngles += new Vector3(-mouseY, mouseX, 0);
        }
    }
}