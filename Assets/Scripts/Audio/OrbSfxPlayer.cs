using UnityEngine;
using UnityEngine.Audio;

public class OrbSfxPlayer : MonoBehaviour
{
    public static OrbSfxPlayer Instance { get; private set; }

    const int VoiceCount = 8;

    OrbSoundBank bank;
    AudioSource[] voices;
    int nextVoice;

    public void Configure(OrbSoundBank soundBank, AudioMixerGroup sfxGroup)
    {
        bank = soundBank;
        voices = new AudioSource[VoiceCount];
        for (int i = 0; i < VoiceCount; i++)
        {
            AudioSource voice = gameObject.AddComponent<AudioSource>();
            voice.outputAudioMixerGroup = sfxGroup;
            voice.playOnAwake = false;
            voice.loop = false;
            voice.spatialBlend = 0f;
            voice.ignoreListenerPause = false;
            voices[i] = voice;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Play(ArenaCue cue, float pitch = 1f, float volume = 1f, float variation = 0.06f)
    {
        if (Instance != null) Instance.PlayCue(cue, pitch, volume, variation);
    }

    void PlayCue(ArenaCue cue, float pitch, float volume, float variation)
    {
        if (bank == null || voices == null) return;
        AudioClip clip = bank.GetArenaClip(cue);
        if (clip == null) return;
        AudioSource voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        voice.pitch = pitch * Random.Range(1f - variation, 1f + variation);
        voice.PlayOneShot(clip, volume);
    }
}
