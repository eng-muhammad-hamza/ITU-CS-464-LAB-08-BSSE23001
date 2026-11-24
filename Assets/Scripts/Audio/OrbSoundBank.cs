using UnityEngine;

public enum UiChime { Hover, Press, Select, Back, PanelOpen, PanelClose, Pause, Resume, Success, Error, Toggle }

public enum ArenaCue
{
    ShardPickup, OrbDamaged, ShieldBlock, ShieldRaise, OrbHealed, OrbDestroyed, OrbDash,
    CannonFire, SpikeSurge, BoostPickup, CountdownTick, CountdownGo, RunWon, RunLost
}

[CreateAssetMenu(fileName = "OrbSoundBank", menuName = "Orb Rush/Sound Bank")]
public class OrbSoundBank : ScriptableObject
{
    public AudioClip[] uiChimeClips = new AudioClip[11];
    public AudioClip[] arenaCueClips = new AudioClip[14];
    public AudioClip menuMusic;
    public AudioClip arenaMusic;
    public AudioClip resultsMusic;

    public AudioClip GetUiClip(UiChime chime)
    {
        int index = (int)chime;
        return index < uiChimeClips.Length ? uiChimeClips[index] : null;
    }

    public AudioClip GetArenaClip(ArenaCue cue)
    {
        int index = (int)cue;
        return index < arenaCueClips.Length ? arenaCueClips[index] : null;
    }
}
