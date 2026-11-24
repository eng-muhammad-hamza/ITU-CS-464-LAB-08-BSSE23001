using UnityEngine;

public class SettingsScreen : UIScreen
{
    public RectTransform topBar;
    public UiSlider[] sliders;
    public UiToggle[] toggles;
    public RectTransform[] items;

    protected override void OnShowStart()
    {
        foreach (UiSlider s in sliders) s.Refresh();
        foreach (UiToggle t in toggles) t.Refresh();
    }

    protected override void OnShown() => Stagger(0f, items);

    public void ResetProgress()
    {
        GameData.ResetProgress();
        OnShowStart();
        OrbUiChimes.Play(UiChime.Error);
        UIManager.Instance.Toast("Progress reset");
    }
}
