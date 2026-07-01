using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectRefresher : MonoBehaviour
{
    ScrollRect _scroll;

    void Awake()
    {
        _scroll = GetComponent<ScrollRect>();
    }

    void OnEnable()
    {
        RefreshNow();
        StartCoroutine(RefreshEndOfFrame());
    }

    public void RefreshNow()
    {
        UiScrollHelper.Refresh(_scroll);
    }

    IEnumerator RefreshEndOfFrame()
    {
        yield return null;
        UiScrollHelper.Refresh(_scroll);
    }
}
