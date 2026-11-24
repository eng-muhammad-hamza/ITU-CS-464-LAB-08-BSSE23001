using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbArenaLoader : MonoBehaviour
{
    public static OrbArenaLoader Instance { get; private set; }

    public static event Action<float> ProgressChanged;
    public static event Action ArenaReady;
    public static event Action ArenaClosed;

    public string arenaSceneName = "OrbRushArena";
    public float barSpeed = 1.4f;
    public float holdAtFull = 0.3f;

    Scene menuScene;
    bool closing;

    public bool IsArenaLoaded { get; private set; }
    public float Progress { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ProgressChanged = null;
        ArenaReady = null;
        ArenaClosed = null;
    }

    void Awake()
    {
        Instance = this;
        menuScene = gameObject.scene;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public IEnumerator LoadArena()
    {
        while (closing) yield return null;
        if (IsArenaLoaded) yield break;

        Progress = 0f;
        ProgressChanged?.Invoke(0f);

        AsyncOperation load = SceneManager.LoadSceneAsync(arenaSceneName, LoadSceneMode.Additive);
        load.allowSceneActivation = false;

        while (Progress < 1f)
        {
            float real = Mathf.Clamp01(load.progress / 0.9f);
            Progress = Mathf.MoveTowards(Progress, real, barSpeed * Time.unscaledDeltaTime);
            ProgressChanged?.Invoke(Progress);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(holdAtFull);

        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        Scene arena = SceneManager.GetSceneByName(arenaSceneName);
        if (arena.IsValid()) SceneManager.SetActiveScene(arena);
        yield return null;

        IsArenaLoaded = true;
        ArenaReady?.Invoke();
    }

    public void CloseArena()
    {
        if (!IsArenaLoaded || closing) return;
        StartCoroutine(CloseRoutine());
    }

    IEnumerator CloseRoutine()
    {
        closing = true;
        IsArenaLoaded = false;
        Scene arena = SceneManager.GetSceneByName(arenaSceneName);
        if (arena.isLoaded)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(arena);
            while (unload != null && !unload.isDone) yield return null;
        }
        if (menuScene.IsValid()) SceneManager.SetActiveScene(menuScene);
        closing = false;
        ArenaClosed?.Invoke();
    }
}
