using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : UIScreen
{
    public Image barFill;
    public TMP_Text percentText;
    public TMP_Text tipText;
    public TMP_Text levelText;
    public RectTransform spinner;

    static readonly string[] arenaTips =
    {
        "Dash through hazards - you are untouchable for a moment.",
        "Hunter drones flash before they lunge. Step aside.",
        "Pink hearts heal. Ice shields block one hit.",
        "Collect every shard to clear the arena.",
        "Fewer hits means more stars."
    };

    void OnEnable() => OrbArenaLoader.ProgressChanged += onProgressChanged;

    void OnDisable() => OrbArenaLoader.ProgressChanged -= onProgressChanged;

    protected override void OnShowStart()
    {
        Level();
        barFill.fillAmount = 0f;
        percentText.text = "0%";
        tipText.text = arenaTips[Random.Range(0, arenaTips.Length)];
    }

    void Level()
    {
        LevelDef def = Catalog.Levels[GameData.SelectedLevel];
        levelText.text = "LEVEL " + (GameData.SelectedLevel + 1) + "  " + def.name.ToUpper();
    }

    void onProgressChanged(float progress)
    {
        barFill.fillAmount = progress;
        percentText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }

    void Update()
    {
        if (spinner != null) spinner.Rotate(0f, 0f, -220f * Time.unscaledDeltaTime);
    }
}
