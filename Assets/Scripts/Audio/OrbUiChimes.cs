using UnityEngine;
using UnityEngine.Audio;

public class OrbUiChimes : MonoBehaviour
{
    public static OrbUiChimes Instance { get; private set; }

    const float HoverGap = 0.07f;

    OrbSoundBank bank;
    AudioSource chimeSource;
    float lastHoverTime = -10f;

    public void Configure(OrbSoundBank soundBank, AudioMixerGroup uiGroup)
    {
        bank = soundBank;
        chimeSource = gameObject.AddComponent<AudioSource>();
        chimeSource.outputAudioMixerGroup = uiGroup;
        chimeSource.playOnAwake = false;
        chimeSource.loop = false;
        chimeSource.spatialBlend = 0f;
        chimeSource.ignoreListenerPause = true;
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Play(UiChime chime, float pitch = 1f)
    {
        if (Instance != null) Instance.PlayChime(chime, pitch);
    }

    void PlayChime(UiChime chime, float pitch)
    {
        if (bank == null || chimeSource == null) return;
        AudioClip clip = bank.GetUiClip(chime);
        if (clip == null) return;
        if (chime == UiChime.Hover)
        {
            if (Time.unscaledTime - lastHoverTime < HoverGap) return;
            lastHoverTime = Time.unscaledTime;
        }
        chimeSource.pitch = pitch * Random.Range(0.97f, 1.03f);
        chimeSource.PlayOneShot(clip);
    }
}
