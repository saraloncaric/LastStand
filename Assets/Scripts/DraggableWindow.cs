using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class DraggableWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler
{
    public RectTransform window;

    public bool bringToFrontOnClick = true;

    public bool clampToScreen = true;

    Canvas _canvas;
    RectTransform _canvasRect;

    void Awake()
    {
        if (window == null)
            window = transform.parent as RectTransform;

        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null)
            _canvasRect = _canvas.transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (bringToFrontOnClick && window != null)
            window.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (window == null)
            return;

        float scale = _canvas != null ? _canvas.scaleFactor : 1f;
        if (scale <= 0f) scale = 1f;

        window.anchoredPosition += eventData.delta / scale;

        if (clampToScreen)
            ClampToCanvas();
    }

    void ClampToCanvas()
    {
        if (_canvasRect == null)
            return;

        Vector2 canvasSize = _canvasRect.rect.size;
        Vector2 winSize = window.rect.size;

        float halfCanvasX = canvasSize.x * 0.5f;
        float halfCanvasY = canvasSize.y * 0.5f;

        Vector2 pivotOffset = new Vector2(
            (0.5f - window.pivot.x) * winSize.x,
            (0.5f - window.pivot.y) * winSize.y);

        Vector2 pos = window.anchoredPosition;
        float maxX = halfCanvasX - winSize.x * 0.5f + pivotOffset.x;
        float maxY = halfCanvasY - winSize.y * 0.5f + pivotOffset.y;

        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
        window.anchoredPosition = pos;
    }
}
