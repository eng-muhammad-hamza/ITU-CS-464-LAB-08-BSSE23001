using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudScreen : UIScreen
{
    public Image[] hearts;
    public TMP_Text shardText;
    public Image shardFill;
    public TMP_Text timerText;
    public TMP_Text levelText;
    public RectTransform shieldChip;
    public RectTransform dashButton;
    public Image dashFill;
    public Image dashGlow;
    public TMP_Text banner;

    bool lastDashReady = true;
    float smoothFill;

    void OnEnable()
    {
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.Healed += onOrbHealed;
        OrbVitals.ShieldRaised += onShieldRaised;
        OrbVitals.ShieldBlocked += onShieldBlocked;
        GameSession.ShardCollected += onShardCollected;
        GameSession.CountdownChanged += onCountdownChanged;
    }

    void OnDisable()
    {
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.Healed -= onOrbHealed;
        OrbVitals.ShieldRaised -= onShieldRaised;
        OrbVitals.ShieldBlocked -= onShieldBlocked;
        GameSession.ShardCollected -= onShardCollected;
        GameSession.CountdownChanged -= onCountdownChanged;
    }

    protected override void OnShowStart()
    {
        GameSession session = GameSession.Instance;
        PlayerOrb orb = PlayerOrb.Instance;
        levelText.text = "LEVEL " + (GameData.SelectedLevel + 1);
        smoothFill = 0f;
        shardFill.fillAmount = 0f;
        banner.text = "";
        timerText.text = "0:00";
        if (session != null && session.HasWorld) shardText.text = "0 / " + session.Target;
        if (orb != null)
        {
            DrawHearts(orb.Vitals.Hearts, orb.Vitals.MaxHearts);
            shieldChip.gameObject.SetActive(orb.Vitals.HasShield);
            shieldChip.localScale = Vector3.one;
        }
    }

    void Update()
    {
        GameSession session = GameSession.Instance;
        PlayerOrb orb = PlayerOrb.Instance;
        if (!IsVisible || session == null || !session.HasWorld) return;

        int seconds = Mathf.FloorToInt(session.Elapsed);
        timerText.text = (seconds / 60) + ":" + (seconds % 60).ToString("00");

        float goal = session.Target > 0 ? (float)session.Shards / session.Target : 0f;
        smoothFill = Mathf.Lerp(smoothFill, goal, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
        shardFill.fillAmount = smoothFill;

        if (orb != null) UpdateDash(orb);
    }

    void DrawHearts(int current, int maximum)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            bool exists = i < maximum;
            hearts[i].gameObject.SetActive(exists);
            if (!exists) continue;
            hearts[i].color = i < current ? OrbPalette.HeartPink : UiTheme.WithAlpha(UiTheme.Outline, 0.9f);
        }
    }

    void onOrbDamaged(OrbHitReport report)
    {
        DrawHearts(report.heartsLeft, report.maxHearts);
        int lost = Mathf.Clamp(report.heartsLeft, 0, hearts.Length - 1);
        StartCoroutine(UiAnim.Punch(hearts[lost].transform, 0.7f, 0.5f));
    }

    void onOrbHealed(int heartsNow, Vector3 at)
    {
        PlayerOrb orb = PlayerOrb.Instance;
        int maximum = orb != null ? orb.Vitals.MaxHearts : hearts.Length;
        DrawHearts(heartsNow, maximum);
        StartCoroutine(UiAnim.Punch(hearts[Mathf.Clamp(heartsNow - 1, 0, hearts.Length - 1)].transform, 0.5f, 0.5f));
    }

    void onShieldRaised(Vector3 at)
    {
        shieldChip.gameObject.SetActive(true);
        StartCoroutine(UiAnim.Scale(shieldChip, Vector3.zero, Vector3.one, 0.35f, EaseType.OutBack));
    }

    void onShieldBlocked(Vector3 at) => StartCoroutine(HideShieldChip());

    IEnumerator HideShieldChip()
    {
        yield return UiAnim.Scale(shieldChip, Vector3.one, Vector3.zero, 0.2f, EaseType.InBack);
        shieldChip.gameObject.SetActive(false);
    }

    void onShardCollected(int collected, int target, Vector3 at)
    {
        shardText.text = collected + " / " + target;
        StartCoroutine(UiAnim.Punch(shardText.transform, 0.3f, 0.4f));
    }

    void onCountdownChanged(string text)
    {
        banner.text = text;
        if (!string.IsNullOrEmpty(text))
            StartCoroutine(UiAnim.Scale(banner.transform, Vector3.one * 1.7f, Vector3.one, 0.45f, EaseType.OutBack));
    }

    void UpdateDash(PlayerOrb orb)
    {
        float ready = orb.DashReady01;
        dashFill.fillAmount = ready;
        bool isReady = ready >= 1f;
        if (isReady && !lastDashReady) StartCoroutine(UiAnim.Punch(dashButton, 0.18f, 0.4f));
        lastDashReady = isReady;
        Color glow = dashGlow.color;
        glow.a = isReady ? 0.5f : 0f;
        dashGlow.color = glow;
    }
}
