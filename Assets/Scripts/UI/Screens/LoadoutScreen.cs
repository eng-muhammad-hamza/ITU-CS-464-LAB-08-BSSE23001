using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutScreen : UIScreen
{
    public TMP_Text levelTitle;
    public TMP_Text objectiveText;
    public RectTransform topBar;
    public RectTransform previewHolder;
    public Image previewBody;
    public TMP_Text orbName;
    public TMP_Text orbPerk;
    public OrbTile[] tiles;
    public RectTransform tileRow;
    public RectTransform boostRow;
    public TMP_Text[] boostStateText;
    public TMP_Text[] boostCountText;
    public Image[] boostBackground;
    public UiButton[] boostButtons;
    public RectTransform playWrapper;

    protected override void OnShowStart()
    {
        LevelDef level = Catalog.Levels[GameData.SelectedLevel];
        if (levelTitle != null) levelTitle.text = "LEVEL " + (GameData.SelectedLevel + 1) + "  " + level.name.ToUpper();
        if (objectiveText != null) objectiveText.text = "Goal: " + level.shardTarget + " shards";
        RefreshAll(false);
    }

    protected override void OnShown() => Stagger(0f, topBar, previewHolder, tileRow, boostRow, playWrapper);

    void RefreshAll(bool animateOrb)
    {
        int selected = GameData.SelectedOrb;
        for (int i = 0; i < tiles.Length; i++) tiles[i].Refresh(i, i == selected);

        OrbDef orb = Catalog.Orbs[selected];
        if (orbName != null) orbName.text = orb.name.ToUpper();
        if (orbPerk != null) orbPerk.text = orb.perk;
        if (previewBody != null) previewBody.color = orb.color;

        for (int i = 0; i < boostButtons.Length; i++)
        {
            int count = GameData.BoostCount(i);
            bool on = GameData.BoostEnabled(i);
            boostCountText[i].text = "x" + count;
            boostStateText[i].text = count == 0 ? "GET" : (on ? "ON" : "OFF");
            Color c = count == 0 ? UiTheme.Surface : (on ? UiTheme.Success : UiTheme.SurfaceHi);
            boostBackground[i].color = c;
            boostButtons[i].SetRestColor(c);
            boostStateText[i].color = on ? UiTheme.TextOnBright : UiTheme.Text;
        }
    }

    public void PickOrb(int index)
    {
        if (!GameData.OwnsOrb(index))
        {
            OrbUiChimes.Play(UiChime.Error);
            UIManager.Instance.Toast(Catalog.Orbs[index].name + " is locked - unlock it in the Shop");
            return;
        }
        GameData.SelectedOrb = index;
        PlayerPrefs.Save();
        RefreshAll(true);
    }

    public void ToggleBoost(int index)
    {
        if (GameData.BoostCount(index) == 0)
        {
            UIManager.Instance.Toast("None left - opening the Shop");
            UIManager.Instance.GetScreen<ShopScreen>().RequestTab(1);
            UIManager.Instance.Go(ScreenId.Shop);
            return;
        }
        GameData.SetBoostEnabled(index, !GameData.BoostEnabled(index));
        PlayerPrefs.Save();
        RefreshAll(false);
    }
}
