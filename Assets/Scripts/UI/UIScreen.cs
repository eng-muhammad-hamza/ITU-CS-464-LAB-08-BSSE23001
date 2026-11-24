using System.Collections;
using UnityEngine;

public enum ScreenId { Splash, MainMenu, LevelSelect, Loadout, Hud, Pause, Results, Shop, Settings, Credits, Loading }

public enum ScreenTransition { Fade, SlideHorizontal, SlideUp, Pop }

public abstract class UIScreen : MonoBehaviour
{
    public ScreenId id;
    public ScreenTransition transition = ScreenTransition.SlideHorizontal;
    public bool overlay;
    public bool keepInHistory = true;
    public RectTransform popTarget;

    protected CanvasGroup group;
    protected RectTransform rt;

    public bool IsVisible { get; private set; }

    public void Init()
    {
        rt = (RectTransform)transform;
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        HideInstant();
    }

    public void HideInstant()
    {
        IsVisible = false;
        gameObject.SetActive(false);
    }

    public void ShowInstant()
    {
        gameObject.SetActive(true);
        IsVisible = true;
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
        rt.anchoredPosition = Vector2.zero;
        if (popTarget != null) popTarget.localScale = Vector3.one;
        OnShowStart();
    }

    public IEnumerator Show(bool forward)
    {
        gameObject.SetActive(true);
        IsVisible = true;
        rt.anchoredPosition = Vector2.zero;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = true;
        OnShowStart();

        RectTransform parent = (RectTransform)rt.parent;
        float w = parent.rect.width, h = parent.rect.height;
        float sign = forward ? 1f : -1f;

        switch (transition)
        {
            case ScreenTransition.SlideHorizontal:
                yield return UiAnim.Parallel(this,
                    UiAnim.Move(rt, new Vector2(sign * w * 0.5f, 0f), Vector2.zero, 0.25f, EaseType.OutQuad),
                    UiAnim.Fade(group, 0f, 1f, 0.2f, EaseType.OutQuad));
                break;
            case ScreenTransition.SlideUp:
                yield return UiAnim.Parallel(this,
                    UiAnim.Move(rt, new Vector2(0f, -h * 0.08f), Vector2.zero, 0.25f, EaseType.OutQuad),
                    UiAnim.Fade(group, 0f, 1f, 0.2f, EaseType.OutQuad));
                break;
            case ScreenTransition.Pop:
                if (popTarget != null)
                    yield return UiAnim.Parallel(this,
                        UiAnim.Fade(group, 0f, 1f, 0.15f, EaseType.OutQuad),
                        UiAnim.Scale(popTarget, Vector3.one * 0.95f, Vector3.one, 0.2f, EaseType.OutQuad));
                else
                    yield return UiAnim.Fade(group, 0f, 1f, 0.15f, EaseType.OutQuad);
                break;
            default:
                yield return UiAnim.Fade(group, 0f, 1f, 0.2f, EaseType.OutQuad);
                break;
        }
        group.interactable = true;
        OnShown();
    }

    public IEnumerator Hide(bool forward)
    {
        group.interactable = false;
        RectTransform parent = (RectTransform)rt.parent;
        float w = parent.rect.width, h = parent.rect.height;
        float sign = forward ? -1f : 1f;

        switch (transition)
        {
            case ScreenTransition.SlideHorizontal:
                yield return UiAnim.Parallel(this,
                    UiAnim.Move(rt, Vector2.zero, new Vector2(sign * w * 0.5f, 0f), 0.2f, EaseType.InQuad),
                    UiAnim.Fade(group, 1f, 0f, 0.15f, EaseType.InQuad));
                break;
            case ScreenTransition.SlideUp:
                yield return UiAnim.Parallel(this,
                    UiAnim.Move(rt, Vector2.zero, new Vector2(0f, -h * 0.08f), 0.2f, EaseType.InQuad),
                    UiAnim.Fade(group, 1f, 0f, 0.15f, EaseType.InQuad));
                break;
            case ScreenTransition.Pop:
                yield return UiAnim.Fade(group, 1f, 0f, 0.15f, EaseType.InQuad);
                break;
            default:
                yield return UiAnim.Fade(group, 1f, 0f, 0.15f, EaseType.InQuad);
                break;
        }
        HideInstant();
    }

    protected virtual void OnShowStart() { }
    protected virtual void OnShown() { }

    protected void Stagger(float delayPerItem, params RectTransform[] items)
    {
        if (delayPerItem <= 0f) delayPerItem = 0.02f;
        StartCoroutine(StaggerRoutine(delayPerItem, items));
    }

    IEnumerator StaggerRoutine(float delay, RectTransform[] items)
    {
        foreach (RectTransform item in items)
        {
            if (item == null) continue;
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg == null) cg = item.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            item.localScale = Vector3.one * 0.96f;
        }

        foreach (RectTransform item in items)
        {
            if (item == null) continue;
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            StartCoroutine(UiAnim.Parallel(this,
                UiAnim.Fade(cg, 0f, 1f, 0.15f, EaseType.OutQuad),
                UiAnim.Scale(item, Vector3.one * 0.96f, Vector3.one, 0.18f, EaseType.OutQuad)));
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        }
    }
}
