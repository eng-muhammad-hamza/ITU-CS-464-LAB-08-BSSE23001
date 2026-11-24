using System.Collections;
using TMPro;
using UnityEngine;

public class ShopScreen : UIScreen
{
    public RectTransform topBar;
    public GemChip gemChip;
    public RectTransform tabIndicator;
    public RectTransform[] tabButtons;
    public RectTransform orbsPanel;
    public RectTransform boostsPanel;
    public ShopRow[] orbRows;
    public ShopRow[] boostRows;

    int tab;
    int requestedTab = -1;
    Coroutine tabRoutine;

    public void RequestTab(int index) => requestedTab = index;

    protected override void OnShowStart()
    {
        int want = requestedTab >= 0 ? requestedTab : 0;
        requestedTab = -1;
        tab = want;
        PlaceTab(want);
        Refresh();
    }

    protected override void OnShown()
    {
        RectTransform[] rows = new RectTransform[orbRows.Length];
        for (int i = 0; i < rows.Length; i++) rows[i] = (RectTransform)orbRows[i].transform;
        if (tab == 0) Stagger(0f, rows);
    }

    float PanelWidth => ((RectTransform)orbsPanel.parent).rect.width;

    void PlaceTab(int index)
    {
        orbsPanel.anchoredPosition = new Vector2(index == 0 ? 0f : -PanelWidth, orbsPanel.anchoredPosition.y);
        boostsPanel.anchoredPosition = new Vector2(index == 0 ? PanelWidth : 0f, boostsPanel.anchoredPosition.y);
        orbsPanel.gameObject.SetActive(index == 0);
        boostsPanel.gameObject.SetActive(index == 1);
        tabIndicator.anchoredPosition = new Vector2(IndicatorX(index), tabIndicator.anchoredPosition.y);
        PaintTabLabels(index);
    }

    float IndicatorX(int index) => tabButtons[index].anchoredPosition.x - tabButtons[0].anchoredPosition.x;

    void PaintTabLabels(int index)
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            TMP_Text label = tabButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.color = i == index ? UiTheme.TextOnBright : UiTheme.Text;
        }
    }

    public void SetTab(int index, bool animate)
    {
        if (index == tab) return;
        tab = index;
        if (tabRoutine != null) StopCoroutine(tabRoutine);
        tabRoutine = StartCoroutine(SlideTab(index));
    }

    IEnumerator SlideTab(int index)
    {
        float w = PanelWidth;
        orbsPanel.gameObject.SetActive(true);
        boostsPanel.gameObject.SetActive(true);
        PaintTabLabels(index);
        float y1 = orbsPanel.anchoredPosition.y, y2 = boostsPanel.anchoredPosition.y;

        yield return UiAnim.Parallel(this,
            UiAnim.Move(orbsPanel, orbsPanel.anchoredPosition, new Vector2(index == 0 ? 0f : -w, y1), 0.35f, EaseType.InOutCubic),
            UiAnim.Move(boostsPanel, boostsPanel.anchoredPosition, new Vector2(index == 0 ? w : 0f, y2), 0.35f, EaseType.InOutCubic),
            UiAnim.Move(tabIndicator, tabIndicator.anchoredPosition, new Vector2(IndicatorX(index), tabIndicator.anchoredPosition.y), 0.35f, EaseType.InOutCubic));
        orbsPanel.gameObject.SetActive(index == 0);
        boostsPanel.gameObject.SetActive(index == 1);
    }

    void Refresh()
    {
        for (int i = 0; i < orbRows.Length; i++)
        {
            OrbDef orb = Catalog.Orbs[i];
            ShopRow row = orbRows[i];
            row.icon.color = orb.color;
            row.title.text = orb.name.ToUpper();
            row.detail.text = orb.perk;
            if (!GameData.OwnsOrb(i)) row.SetState("BUY  " + orb.price, UiTheme.Reward, UiTheme.TextOnBright, true);
            else if (GameData.SelectedOrb == i) row.SetState("EQUIPPED", UiTheme.Success, UiTheme.TextOnBright, false);
            else row.SetState("EQUIP", UiTheme.Action, UiTheme.TextOnBright, true);
        }
        for (int i = 0; i < boostRows.Length; i++)
        {
            BoostDef boost = Catalog.Boosts[i];
            ShopRow row = boostRows[i];
            row.title.text = boost.name.ToUpper();
            row.detail.text = boost.description;
            row.extra.text = "Owned: " + GameData.BoostCount(i);
            row.SetState("BUY  " + boost.price, UiTheme.Reward, UiTheme.TextOnBright, true);
        }
    }

    public void BuyOrb(int index)
    {
        OrbDef orb = Catalog.Orbs[index];
        ShopRow row = orbRows[index];
        if (GameData.OwnsOrb(index))
        {
            GameData.SelectedOrb = index;
            PlayerPrefs.Save();
            OrbUiChimes.Play(UiChime.Select);
            UIManager.Instance.Toast(orb.name + " equipped");
        }
        else if (GameData.TrySpend(orb.price))
        {
            GameData.GiveOrb(index);
            GameData.SelectedOrb = index;
            PlayerPrefs.Save();
            OrbUiChimes.Play(UiChime.Success);
            GameData.Vibrate();
            StartCoroutine(UiAnim.Punch(row.transform, 0.06f, 0.5f));
            UIManager.Instance.Toast(orb.name + " unlocked!");
        }
        else Denied(row);
        Refresh();
    }

    public void BuyBoost(int index)
    {
        BoostDef boost = Catalog.Boosts[index];
        ShopRow row = boostRows[index];
        if (GameData.TrySpend(boost.price))
        {
            GameData.AddBoost(index, 1);
            OrbUiChimes.Play(UiChime.Success);
            StartCoroutine(UiAnim.Punch(row.extra.transform, 0.5f, 0.5f));
            UIManager.Instance.Toast(boost.name + " added");
        }
        else Denied(row);
        Refresh();
    }

    void Denied(ShopRow row)
    {
        OrbUiChimes.Play(UiChime.Error);
        StartCoroutine(UiAnim.Shake((RectTransform)row.transform, 16f, 0.35f));
        gemChip.FlashDenied();
        UIManager.Instance.Toast("Not enough gems - win levels to earn more");
    }
}
