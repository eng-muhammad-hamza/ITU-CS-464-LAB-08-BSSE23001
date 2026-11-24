using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiToggle : MonoBehaviour, IPointerClickHandler
{
    public enum Setting { Haptics, ScreenShake, SoundOn }

    public Setting setting;
    public RectTransform knob;
    public Image track;
    public TMP_Text stateLabel;
    public float offX = -42f;
    public float onX = 42f;

    bool value;

    public void Refresh()
    {
        value = ReadValue();
        Apply(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        value = !value;
        WriteValue();
        PlayerPrefs.Save();
        OrbUiChimes.Play(UiChime.Toggle, value ? 1.15f : 0.85f);
        if (value && setting != Setting.SoundOn) GameData.Vibrate();
        Apply(true);
    }

    bool ReadValue()
    {
        switch (setting)
        {
            case Setting.Haptics: return GameData.Haptics;
            case Setting.ScreenShake: return GameData.ScreenShake;
            default: return !OrbMixerSettings.IsMuted;
        }
    }

    void WriteValue()
    {
        switch (setting)
        {
            case Setting.Haptics: GameData.Haptics = value; break;
            case Setting.ScreenShake: GameData.ScreenShake = value; break;
            default: OrbMixerSettings.SetMuted(!value); break;
        }
    }

    void Apply(bool animate)
    {
        Vector2 to = new Vector2(value ? onX : offX, knob.anchoredPosition.y);
        Color trackColor = value ? UiTheme.Action : UiTheme.Locked;
        if (stateLabel != null) stateLabel.text = value ? "ON" : "OFF";

        if (!animate || !gameObject.activeInHierarchy)
        {
            knob.anchoredPosition = to;
            track.color = trackColor;
            return;
        }
        StopAllCoroutines();
        StartCoroutine(UiAnim.Move(knob, knob.anchoredPosition, to, 0.22f, EaseType.InOutCubic));
        StartCoroutine(UiAnim.ColorTo(track, trackColor, 0.22f, EaseType.InOutSine));
        StartCoroutine(UiAnim.Punch(knob, 0.18f, 0.4f));
    }
}
