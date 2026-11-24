using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultsScreen : UIScreen
{
    public TMP_Text title;
    public TMP_Text subtitle;
    public Image[] stars;
    public TMP_Text shardsText;
    public TMP_Text gemsText;
    public RectTransform newBest;
    public UiButton primaryButton;
    public CanvasGroup buttonsGroup;
    public RectTransform statsRow;

    protected override void OnShowStart()
    {
        GameSession s = GameSession.Instance;
        bool win = s != null && s.LastWin;
        title.text = win ? "LEVEL CLEAR" : "LEVEL FAILED";
        title.color = win ? UiTheme.Success : UiTheme.Danger;
        subtitle.text = "LEVEL " + (GameData.SelectedLevel + 1) + "  " + Catalog.Levels[GameData.SelectedLevel].name.ToUpper();
        shardsText.text = "0";
        gemsText.text = "+0";
        foreach (Image star in stars)
        {
            star.color = UiTheme.WithAlpha(UiTheme.Outline, 0.8f);
            star.transform.localScale = Vector3.one;
        }
        newBest.localScale = Vector3.zero;
        title.transform.localScale = Vector3.zero;
        buttonsGroup.alpha = 0f;
        buttonsGroup.interactable = false;

        bool hasNext = GameData.SelectedLevel + 1 < Catalog.Levels.Length;
        if (win)
        {
            primaryButton.action = hasNext ? ButtonAction.Next : ButtonAction.Levels;
            primaryButton.SetLabel(hasNext ? "NEXT LEVEL" : "ALL LEVELS");
        }
        else
        {
            primaryButton.action = ButtonAction.Restart;
            primaryButton.SetLabel("TRY AGAIN");
        }
    }

    protected override void OnShown() => StartCoroutine(Reveal());

    IEnumerator Reveal()
    {
        GameSession s = GameSession.Instance;

        yield return UiAnim.Scale(title.transform, Vector3.zero, Vector3.one, 0.25f, EaseType.OutQuad);

        int starCount = s != null ? s.LastStars : 0;
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < starCount)
            {
                stars[i].color = UiTheme.Reward;
                OrbUiChimes.Play(UiChime.Success, 1f + i * 0.2f);
                yield return UiAnim.Scale(stars[i].transform, Vector3.one * 0.4f, Vector3.one, 0.2f, EaseType.OutQuad);
            }
            yield return new WaitForSecondsRealtime(0.08f);
        }

        int shards = s != null ? s.Shards : 0;
        int gems = s != null ? s.LastGems : 0;
        yield return UiAnim.Parallel(this,
            UiAnim.Count(shardsText, 0, shards, 0.4f, EaseType.OutQuad),
            UiAnim.Count(gemsText, 0, gems, 0.5f, EaseType.OutQuad, "+"));
        if (gems > 0) OrbUiChimes.Play(UiChime.Success, 1.25f);

        if (s != null && s.LastNewBest)
        {
            OrbUiChimes.Play(UiChime.Select, 1.2f);
            yield return UiAnim.Scale(newBest, Vector3.zero, Vector3.one, 0.25f, EaseType.OutQuad);
        }

        yield return UiAnim.Fade(buttonsGroup, 0f, 1f, 0.2f, EaseType.OutQuad);
        buttonsGroup.interactable = true;
    }
}
