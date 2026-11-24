using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler, IPointerEnterHandler
{
    public ButtonAction action;
    public ScreenId target;
    public int argument;
    public bool interactable = true;
    public bool fireOnDown;

    public Graphic tint;
    public TMP_Text label;

    public bool needsConfirm;
    public string confirmLabel = "TAP AGAIN TO CONFIRM";

    Vector3 restScale = Vector3.one;
    Color restColor;
    string restLabel;
    Coroutine scaleRoutine;
    bool pressed;
    bool confirming;
    float confirmUntil;

    void Awake()
    {
        restScale = transform.localScale;
        if (tint != null) restColor = tint.color;
        if (label != null) restLabel = label.text;
    }

    public void SetRestColor(Color color)
    {
        restColor = color;
        if (tint != null) tint.color = color;
    }

    public void SetLabel(string text)
    {
        restLabel = text;
        if (label != null) label.text = text;
    }

    void Update()
    {
        if (confirming && Time.unscaledTime > confirmUntil) CancelConfirm();
    }

    void OnDisable()
    {
        pressed = false;
        transform.localScale = restScale;
        if (confirming) CancelConfirm();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable || eventData.pointerId >= 0) return;
        OrbUiChimes.Play(UiChime.Hover);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable) return;
        pressed = true;
        AnimateScale(restScale * 0.97f, 0.05f, EaseType.InQuad);
        if (tint != null) tint.color = Color.Lerp(restColor, Color.white, 0.12f);
        if (fireOnDown) Fire();
    }

    public void OnPointerUp(PointerEventData eventData) => Release();
    public void OnPointerExit(PointerEventData eventData) { if (pressed) Release(); }

    void Release()
    {
        if (!pressed) return;
        pressed = false;
        AnimateScale(restScale, 0.1f, EaseType.OutQuad);
        if (tint != null && !confirming) tint.color = restColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!fireOnDown) Fire();
    }

    void Fire()
    {
        if (!interactable) return;

        if (needsConfirm)
        {
            if (!confirming)
            {
                confirming = true;
                confirmUntil = Time.unscaledTime + 2.5f;
                if (label != null) label.text = confirmLabel;
                if (tint != null) tint.color = UiTheme.Danger;
                OrbUiChimes.Play(UiChime.Press, 0.8f);
                return;
            }
            confirming = false;
            if (label != null) label.text = restLabel;
            if (tint != null) tint.color = restColor;
        }

        if (action != ButtonAction.Pause && action != ButtonAction.Resume && action != ButtonAction.Dash) OrbUiChimes.Play(ChimeFor(action));
        UIManager.Instance.Perform(action, target, argument);
    }

    static UiChime ChimeFor(ButtonAction buttonAction)
    {
        switch (buttonAction)
        {
            case ButtonAction.Back:
            case ButtonAction.Home:
            case ButtonAction.Levels: return UiChime.Back;
            case ButtonAction.Play:
            case ButtonAction.Restart:
            case ButtonAction.Next:
            case ButtonAction.SelectLevel: return UiChime.Select;
            default: return UiChime.Press;
        }
    }

    void CancelConfirm()
    {
        confirming = false;
        if (label != null) label.text = restLabel;
        if (tint != null) tint.color = restColor;
    }

    void AnimateScale(Vector3 targetScale, float duration, EaseType ease)
    {
        if (!gameObject.activeInHierarchy)
        {
            transform.localScale = targetScale;
            return;
        }
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(UiAnim.Scale(transform, transform.localScale, targetScale, duration, ease));
    }
}
