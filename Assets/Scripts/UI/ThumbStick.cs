using UnityEngine;

public class ThumbStick : MonoBehaviour
{
    public static Vector2 Value { get; private set; }

    public RectTransform layer;
    public RectTransform baseRing;
    public RectTransform knob;
    public CanvasGroup baseGroup;
    public float maxRadius = 130f;

    Vector2 idlePosition;
    Vector2 origin;
    bool tracking;
    int fingerId = -1;

    void Awake() => idlePosition = baseRing.anchoredPosition;

    void OnDisable() => Release(true);

    bool InRegion(Vector2 screenPos) => screenPos.x < Screen.width * 0.62f && screenPos.y < Screen.height * 0.78f;

    void Update()
    {
        GameSession session = GameSession.Instance;
        bool allowed = session != null && session.Running && !session.Paused;
        if (!allowed) { Release(false); return; }

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (!tracking && touch.phase == TouchPhase.Began && InRegion(touch.position)) Begin(touch.position, touch.fingerId);
                else if (tracking && touch.fingerId == fingerId)
                {
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) Release(false);
                    else Drag(touch.position);
                }
            }
        }
        else if (tracking && fingerId >= 0) Release(false);
        else
        {
            if (!tracking && Input.GetMouseButtonDown(0) && InRegion(Input.mousePosition)) Begin(Input.mousePosition, -1);
            else if (tracking && fingerId == -1)
            {
                if (Input.GetMouseButton(0)) Drag(Input.mousePosition); else Release(false);
            }
        }

        if (!tracking)
        {
            Vector2 keys = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Value = Vector2.ClampMagnitude(keys, 1f);
            knob.anchoredPosition = Vector2.Lerp(knob.anchoredPosition, keys * maxRadius * 0.6f, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
            baseRing.anchoredPosition = Vector2.Lerp(baseRing.anchoredPosition, idlePosition, 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime));
            baseGroup.alpha = Mathf.Lerp(baseGroup.alpha, 0.3f, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
        }
    }

    void Begin(Vector2 screenPos, int id)
    {
        tracking = true;
        fingerId = id;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPos, null, out origin);
        baseRing.anchoredPosition = origin;
        baseGroup.alpha = 0.85f;
        Drag(screenPos);
    }

    void Drag(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenPos, null, out Vector2 local);
        Vector2 delta = Vector2.ClampMagnitude(local - origin, maxRadius);
        knob.anchoredPosition = delta;
        Vector2 v = delta / maxRadius;
        Value = v.magnitude < 0.12f ? Vector2.zero : v;
    }

    void Release(bool instant)
    {
        if (tracking) { tracking = false; fingerId = -1; }
        Value = Vector2.zero;
        if (instant) { knob.anchoredPosition = Vector2.zero; baseRing.anchoredPosition = idlePosition; }
    }
}
