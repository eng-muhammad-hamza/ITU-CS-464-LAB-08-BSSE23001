using TMPro;
using UnityEngine;

public class LevelSelectScreen : UIScreen
{
    public LevelCard[] cards;
    public RectTransform topBar;
    public RectTransform footerHint;
    public TMP_Text starsText;

    protected override void OnShowStart()
    {
        for (int i = 0; i < cards.Length; i++) cards[i].Refresh(i);
        starsText.text = GameData.TotalStars() + " / " + Catalog.Levels.Length * 3;
    }

    protected override void OnShown()
    {
        RectTransform[] items = new RectTransform[cards.Length + 2];
        items[0] = topBar;
        for (int i = 0; i < cards.Length; i++) items[i + 1] = (RectTransform)cards[i].transform;
        items[items.Length - 1] = footerHint;
        Stagger(0f, items);
    }

    public void SelectLevel(int index)
    {
        if (!GameData.IsLevelUnlocked(index))
        {
            OrbUiChimes.Play(UiChime.Error);
            StartCoroutine(UiAnim.Shake((RectTransform)cards[index].transform, 16f, 0.35f));
            UIManager.Instance.Toast("Earn a star on level " + index + " to unlock");
            return;
        }
        GameData.SelectedLevel = index;
        UIManager.Instance.Go(ScreenId.Loadout);
    }
}
