using UnityEngine;
using UnityEngine.UI;

public static class UiScrollHelper
{
    public static void Refresh(ScrollRect scrollRect)
    {
        if (scrollRect == null || scrollRect.content == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        RectTransform viewport = scrollRect.viewport;
        float viewH = viewport != null ? viewport.rect.height : 0f;
        float contentH = scrollRect.content.rect.height;
        bool overflow = contentH > viewH + 1f;

        Scrollbar bar = scrollRect.verticalScrollbar;
        if (bar != null)
        {
            bar.gameObject.SetActive(overflow);
            if (overflow)
                bar.size = Mathf.Clamp01(viewH / Mathf.Max(contentH, 1f));
        }

        scrollRect.vertical = overflow;
    }
}
