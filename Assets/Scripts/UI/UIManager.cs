using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum ButtonAction
{
    None, GoTo, Back, Home, Levels, SelectLevel, Play, Pause, Resume, Restart, Next, Dash,
    ShopTab, BuyOrb, PickLoadoutOrb, BuyBoost, ToggleBoost, ResetProgress, CompleteLevel, FailLevel
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public UIScreen[] screens;
    public ScreenId startScreen = ScreenId.Splash;
    public CanvasGroup fadeOverlay;
    public CanvasGroup inputBlocker;
    public CanvasGroup toastGroup;
    public RectTransform toastRect;
    public TMP_Text toastText;

    readonly Dictionary<ScreenId, UIScreen> map = new Dictionary<ScreenId, UIScreen>();
    readonly Stack<UIScreen> history = new Stack<UIScreen>();
    UIScreen current;
    bool busy;
    Coroutine toastRoutine;
    Vector2 toastRest;

    public ScreenId CurrentId => current != null ? current.id : startScreen;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        foreach (UIScreen s in screens)
        {
            s.Init();
            map[s.id] = s;
        }
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
        inputBlocker.blocksRaycasts = false;
        toastGroup.alpha = 0f;
        toastRest = toastRect.anchoredPosition;
    }

    void Start()
    {
        StartCoroutine(Transition(map[startScreen], true));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) HandleBackKey();
    }

    public T GetScreen<T>() where T : UIScreen
    {
        foreach (UIScreen s in screens)
        {
            T match = s as T;
            if (match != null) return match;
        }
        return null;
    }

    public void Go(ScreenId target)
    {
        if (busy || !map.TryGetValue(target, out UIScreen next) || next == current) return;
        if (current != null && current.keepInHistory) history.Push(current);
        StartCoroutine(Transition(next, true));
    }

    public void Back()
    {
        if (busy || history.Count == 0) return;
        UIScreen previous = history.Pop();
        StartCoroutine(Transition(previous, false));
    }

    void GoClean(ScreenId target, bool forward, params ScreenId[] baseStack)
    {
        if (busy) return;
        history.Clear();
        foreach (ScreenId id in baseStack) history.Push(map[id]);
        StartCoroutine(Transition(map[target], forward));
    }

    IEnumerator Transition(UIScreen target, bool forward)
    {
        busy = true;
        inputBlocker.blocksRaycasts = true;
        if (forward && IsPanel(target.id)) OrbUiChimes.Play(UiChime.PanelOpen);
        else if (!forward && current != null && IsPanel(current.id)) OrbUiChimes.Play(UiChime.PanelClose);

        UIScreen underlay = null;
        if (target.overlay && history.Count > 0)
        {
            underlay = history.Peek();
            if (!underlay.IsVisible) underlay.ShowInstant();
        }

        List<Coroutine> running = new List<Coroutine>();
        foreach (UIScreen s in screens)
        {
            if (s == target || s == underlay || !s.IsVisible) continue;
            running.Add(StartCoroutine(s.Hide(forward)));
        }
        if (!target.IsVisible) running.Add(StartCoroutine(target.Show(forward)));
        foreach (Coroutine c in running) yield return c;

        current = target;
        inputBlocker.blocksRaycasts = false;
        busy = false;
    }

    static bool IsPanel(ScreenId screenId) => screenId == ScreenId.Settings || screenId == ScreenId.Credits || screenId == ScreenId.Shop;

    void HandleBackKey()
    {
        switch (CurrentId)
        {
            case ScreenId.Hud: Perform(ButtonAction.Pause, ScreenId.Pause, 0); break;
            case ScreenId.Pause: Perform(ButtonAction.Resume, ScreenId.Hud, 0); break;
            case ScreenId.Results: Perform(ButtonAction.Levels, ScreenId.LevelSelect, 0); break;
            case ScreenId.MainMenu:
            case ScreenId.Splash: break;
            default: Back(); break;
        }
    }

    public void Perform(ButtonAction action, ScreenId target, int arg)
    {
        GameSession session = GameSession.Instance;
        switch (action)
        {
            case ButtonAction.GoTo: Go(target); break;
            case ButtonAction.Back: Back(); break;
            case ButtonAction.Home:
                if (busy) return;
                if (session != null) session.Cleanup();
                CloseArena();
                GoClean(ScreenId.MainMenu, false);
                break;
            case ButtonAction.Levels:
                if (busy) return;
                if (session != null) session.Cleanup();
                CloseArena();
                GoClean(ScreenId.LevelSelect, false, ScreenId.MainMenu);
                break;
            case ButtonAction.SelectLevel: GetScreen<LevelSelectScreen>().SelectLevel(arg); break;
            case ButtonAction.Play:
            case ButtonAction.Restart:
                StartCoroutine(StartLevelRoutine(GameData.SelectedLevel));
                break;
            case ButtonAction.Next:
                if (GameData.SelectedLevel + 1 < Catalog.Levels.Length) StartCoroutine(StartLevelRoutine(GameData.SelectedLevel + 1));
                else Perform(ButtonAction.Levels, ScreenId.LevelSelect, 0);
                break;
            case ButtonAction.Pause:
                if (busy || session == null || !session.Running) return;
                session.SetPaused(true);
                OrbUiChimes.Play(UiChime.Pause);
                Go(ScreenId.Pause);
                break;
            case ButtonAction.Resume:
                if (busy) return;
                if (session != null) session.SetPaused(false);
                OrbUiChimes.Play(UiChime.Resume);
                Back();
                break;
            case ButtonAction.Dash:
                if (PlayerOrb.Instance != null) PlayerOrb.Instance.TryDash();
                break;
            case ButtonAction.CompleteLevel:
                if (session != null) session.EndLevel(true);
                break;
            case ButtonAction.FailLevel:
                if (session != null) session.EndLevel(false);
                break;
            case ButtonAction.ShopTab: GetScreen<ShopScreen>().SetTab(arg, true); break;
            case ButtonAction.BuyOrb: GetScreen<ShopScreen>().BuyOrb(arg); break;
            case ButtonAction.BuyBoost: GetScreen<ShopScreen>().BuyBoost(arg); break;
            case ButtonAction.PickLoadoutOrb: GetScreen<LoadoutScreen>().PickOrb(arg); break;
            case ButtonAction.ToggleBoost: GetScreen<LoadoutScreen>().ToggleBoost(arg); break;
            case ButtonAction.ResetProgress: GetScreen<SettingsScreen>().ResetProgress(); break;
        }
    }

    void CloseArena()
    {
        if (OrbArenaLoader.Instance != null) OrbArenaLoader.Instance.CloseArena();
    }

    IEnumerator StartLevelRoutine(int level)
    {
        if (busy) yield break;
        busy = true;
        inputBlocker.blocksRaycasts = true;
        fadeOverlay.blocksRaycasts = true;
        GameData.SelectedLevel = level;

        yield return UiAnim.Fade(fadeOverlay, 0f, 1f, 0.2f, EaseType.OutQuad);

        foreach (UIScreen s in screens) if (s.IsVisible) s.HideInstant();
        history.Clear();
        history.Push(map[ScreenId.MainMenu]);
        history.Push(map[ScreenId.LevelSelect]);

        OrbArenaLoader loader = OrbArenaLoader.Instance;
        if (loader != null && !loader.IsArenaLoaded)
        {
            UIScreen loadingScreen = map[ScreenId.Loading];
            loadingScreen.ShowInstant();
            current = loadingScreen;
            yield return UiAnim.Fade(fadeOverlay, 1f, 0f, 0.2f, EaseType.OutQuad);
            yield return loader.LoadArena();
            yield return UiAnim.Fade(fadeOverlay, 0f, 1f, 0.2f, EaseType.OutQuad);
            loadingScreen.HideInstant();
        }

        if (GameSession.Instance == null)
        {
            OrbUiChimes.Play(UiChime.Error);
            map[ScreenId.LevelSelect].ShowInstant();
            current = map[ScreenId.LevelSelect];
            yield return UiAnim.Fade(fadeOverlay, 1f, 0f, 0.2f, EaseType.OutQuad);
            fadeOverlay.blocksRaycasts = false;
            inputBlocker.blocksRaycasts = false;
            busy = false;
            Toast("Arena scene is missing from Build Settings");
            yield break;
        }

        GameSession.Instance.Cleanup();
        GameSession.Instance.BuildLevel(level);
        map[ScreenId.Hud].ShowInstant();
        current = map[ScreenId.Hud];

        yield return UiAnim.Fade(fadeOverlay, 1f, 0f, 0.2f, EaseType.OutQuad);
        fadeOverlay.blocksRaycasts = false;
        inputBlocker.blocksRaycasts = false;
        busy = false;
        GameSession.Instance.StartCountdown();
    }

    public void ShowResults()
    {
        if (current != null && current.id == ScreenId.Results) return;
        Go(ScreenId.Results);
    }

    public void Toast(string message)
    {
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine(message));
    }

    IEnumerator ToastRoutine(string message)
    {
        toastText.text = message;
        Vector2 rest = toastRest;
        toastRect.anchoredPosition = rest + Vector2.down * 40f;
        yield return UiAnim.Parallel(this,
            UiAnim.Move(toastRect, rest + Vector2.down * 40f, rest, 0.2f, EaseType.OutQuad),
            UiAnim.Fade(toastGroup, 0f, 1f, 0.15f, EaseType.OutQuad));
        yield return new WaitForSecondsRealtime(1.2f);
        yield return UiAnim.Fade(toastGroup, 1f, 0f, 0.2f, EaseType.InQuad);
        toastRoutine = null;
    }
}
