using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResizableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public RectTransform window;

    public bool allowHorizontal = true;
    public bool allowVertical = true;

    public Vector2 minSize = new Vector2(220f, 160f);
    public Vector2 maxSize = new Vector2(1200f, 800f);

    Canvas _canvas;

    void Awake()
    {
        if (window == null)
            window = transform.parent as RectTransform;

        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (window != null)
            window.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (window == null)
            return;

        float scale = _canvas != null ? _canvas.scaleFactor : 1f;
        if (scale <= 0f) scale = 1f;

        Vector2 size = window.sizeDelta;
        if (allowHorizontal)
            size.x += eventData.delta.x / scale;
        if (allowVertical)
            size.y -= eventData.delta.y / scale;

        size.x = Mathf.Clamp(size.x, minSize.x, maxSize.x);
        size.y = Mathf.Clamp(size.y, minSize.y, maxSize.y);

        window.sizeDelta = size;

        ScrollRect sr = window.GetComponentInChildren<ScrollRect>();
        if (sr != null)
            UiScrollHelper.Refresh(sr);
    }
}
