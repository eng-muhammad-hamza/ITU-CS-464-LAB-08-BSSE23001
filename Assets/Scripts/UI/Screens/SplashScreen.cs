using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : UIScreen
{
    public RectTransform logo;
    public Image loadingFill;
    public TMP_Text loadingText;
    public TMP_Text tipText;
    public float loadSeconds = 1.0f;

    static readonly string[] Tips =
    {
        "Tip: Complete levels to earn stars and gems.",
        "Tip: Check the Shop to unlock new orbs.",
        "Tip: Configure audio and options in Settings.",
    };

    protected override void OnShowStart()
    {
        if (logo != null) logo.localScale = Vector3.zero;
        if (loadingFill != null) loadingFill.fillAmount = 0f;
        if (loadingText != null) loadingText.text = "0%";
        if (tipText != null) tipText.text = Tips[Random.Range(0, Tips.Length)];
    }

    protected override void OnShown() => StartCoroutine(Run());

    IEnumerator Run()
    {
        if (logo != null)
            yield return UiAnim.Scale(logo, Vector3.zero, Vector3.one, 0.35f, EaseType.OutQuad);
        OrbUiChimes.Play(UiChime.Select);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / loadSeconds;
            float eased = Ease.Evaluate(EaseType.OutQuad, t);
            if (loadingFill != null) loadingFill.fillAmount = eased;
            if (loadingText != null) loadingText.text = Mathf.RoundToInt(eased * 100f) + "%";
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.15f);
        UIManager.Instance.Go(ScreenId.MainMenu);
    }
}
