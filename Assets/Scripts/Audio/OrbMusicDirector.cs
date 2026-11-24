using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class OrbMusicDirector : MonoBehaviour
{
    public static OrbMusicDirector Instance { get; private set; }

    const float PausedLevel = 0.35f;
    const float DuckSpeed = 4f;

    OrbSoundBank bank;
    AudioSource[] decks;
    float[] deckLevels;
    int liveDeck;
    float duck = 1f;
    float duckTarget = 1f;
    Coroutine crossfade;

    public void Configure(OrbSoundBank soundBank, AudioMixerGroup musicGroup)
    {
        bank = soundBank;
        decks = new AudioSource[2];
        deckLevels = new float[2];
        for (int i = 0; i < decks.Length; i++)
        {
            AudioSource deck = gameObject.AddComponent<AudioSource>();
            deck.outputAudioMixerGroup = musicGroup;
            deck.playOnAwake = false;
            deck.loop = true;
            deck.spatialBlend = 0f;
            deck.volume = 0f;
            deck.ignoreListenerPause = true;
            decks[i] = deck;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        duck = Mathf.MoveTowards(duck, duckTarget, DuckSpeed * Time.unscaledDeltaTime);
        for (int i = 0; i < decks.Length; i++) decks[i].volume = deckLevels[i] * duck;
    }

    public static void PlayMenu() { if (Instance != null && Instance.bank != null) Instance.CrossfadeTo(Instance.bank.menuMusic, 1.2f); }
    public static void PlayArena() { if (Instance != null && Instance.bank != null) Instance.CrossfadeTo(Instance.bank.arenaMusic, 1.0f); }
    public static void PlayResults() { if (Instance != null && Instance.bank != null) Instance.CrossfadeTo(Instance.bank.resultsMusic, 1.4f); }

    public static void SetPaused(bool paused)
    {
        if (Instance != null) Instance.duckTarget = paused ? PausedLevel : 1f;
    }

    void CrossfadeTo(AudioClip clip, float seconds)
    {
        if (clip == null) return;
        AudioSource live = decks[liveDeck];
        if (live.clip == clip && live.isPlaying) return;
        if (crossfade != null) StopCoroutine(crossfade);
        crossfade = StartCoroutine(CrossfadeRoutine(clip, seconds));
    }

    IEnumerator CrossfadeRoutine(AudioClip clip, float seconds)
    {
        int outgoing = liveDeck;
        int incoming = 1 - liveDeck;
        liveDeck = incoming;

        decks[incoming].clip = clip;
        deckLevels[incoming] = 0f;
        decks[incoming].Play();

        float outgoingStart = deckLevels[outgoing];
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.05f, seconds);
            float eased = Ease.Evaluate(EaseType.InOutSine, t);
            deckLevels[incoming] = eased;
            deckLevels[outgoing] = outgoingStart * (1f - eased);
            yield return null;
        }
        deckLevels[incoming] = 1f;
        deckLevels[outgoing] = 0f;
        decks[outgoing].Stop();
        crossfade = null;
    }
}
