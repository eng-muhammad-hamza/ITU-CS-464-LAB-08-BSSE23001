using UnityEngine;
using UnityEngine.Audio;

public enum OrbBus { Master, Music, Sfx, Ui }

public static class OrbMixerSettings
{
    public const float SilentDecibels = -80f;
    const float FloorLinear = 0.0001f;
    const string MuteKey = "orbMuteAll";

    static readonly string[] exposedNames = { "masterVolume", "musicVolume", "sfxVolume", "uiVolume" };
    static readonly string[] prefKeys = { "orbMasterVolume", "orbMusicVolume", "orbSfxVolume", "orbUiVolume" };
    static readonly float[] defaultLevels = { 1f, 0.7f, 0.9f, 0.9f };

    static AudioMixer orbMixer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => orbMixer = null;

    public static bool IsMuted => PlayerPrefs.GetInt(MuteKey, 0) == 1;

    public static void Attach(AudioMixer mixer)
    {
        orbMixer = mixer;
        ApplyAll();
    }

    public static float GetLinear(OrbBus bus)
    {
        int index = (int)bus;
        return Mathf.Clamp01(PlayerPrefs.GetFloat(prefKeys[index], defaultLevels[index]));
    }

    public static void SetLinear(OrbBus bus, float linear)
    {
        PlayerPrefs.SetFloat(prefKeys[(int)bus], Mathf.Clamp01(linear));
        Apply(bus);
    }

    public static void SetMuted(bool muted)
    {
        PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
        Apply(OrbBus.Master);
    }

    public static void Save() => PlayerPrefs.Save();

    public static float LinearToDecibels(float linear)
    {
        float safe = Mathf.Max(linear, FloorLinear);
        return Mathf.Max(SilentDecibels, Mathf.Log10(safe) * 20f);
    }

    public static void Apply(OrbBus bus)
    {
        if (orbMixer == null) return;
        float decibels = LinearToDecibels(GetLinear(bus));
        if (bus == OrbBus.Master && IsMuted) decibels = SilentDecibels;
        orbMixer.SetFloat(exposedNames[(int)bus], decibels);
    }

    public static void ApplyAll()
    {
        for (int i = 0; i < exposedNames.Length; i++) Apply((OrbBus)i);
    }
}
