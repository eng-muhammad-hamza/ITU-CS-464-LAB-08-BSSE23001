using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class OrbSoundStage : MonoBehaviour
{
    public AudioMixer orbMixer;
    public OrbSoundBank soundBank;

    void Awake()
    {
        OrbMixerSettings.Attach(orbMixer);

        gameObject.AddComponent<OrbUiChimes>().Configure(soundBank, FindGroup("Master/UI"));
        gameObject.AddComponent<OrbSfxPlayer>().Configure(soundBank, FindGroup("Master/SFX"));
        gameObject.AddComponent<OrbMusicDirector>().Configure(soundBank, FindGroup("Master/Music"));
        gameObject.AddComponent<OrbMusicCueListener>();
    }

    void Start()
    {
        StartCoroutine(ApplyVolumesAfterFirstFrame());
        OrbMusicDirector.PlayMenu();
    }

    IEnumerator ApplyVolumesAfterFirstFrame()
    {
        OrbMixerSettings.ApplyAll();
        yield return null;
        OrbMixerSettings.ApplyAll();
    }

    AudioMixerGroup FindGroup(string path)
    {
        if (orbMixer == null) return null;
        AudioMixerGroup[] found = orbMixer.FindMatchingGroups(path);
        return found.Length > 0 ? found[0] : null;
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) OrbMixerSettings.Save();
    }

    void OnApplicationQuit() => OrbMixerSettings.Save();
}
