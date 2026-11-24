using UnityEngine;

public class OrbMusicCueListener : MonoBehaviour
{
    void OnEnable()
    {
        GameSession.LevelPrepared += onLevelPrepared;
        GameSession.RunWon += onRunWon;
        GameSession.RunLost += onRunLost;
        GameSession.PauseChanged += onPauseChanged;
        OrbArenaLoader.ArenaClosed += onArenaClosed;
    }

    void OnDisable()
    {
        GameSession.LevelPrepared -= onLevelPrepared;
        GameSession.RunWon -= onRunWon;
        GameSession.RunLost -= onRunLost;
        GameSession.PauseChanged -= onPauseChanged;
        OrbArenaLoader.ArenaClosed -= onArenaClosed;
    }

    void onLevelPrepared(int levelIndex)
    {
        OrbMusicDirector.SetPaused(false);
        OrbMusicDirector.PlayArena();
    }

    void onRunWon(Vector3 at) => OrbMusicDirector.PlayResults();

    void onRunLost(Vector3 at) => OrbMusicDirector.PlayResults();

    void onPauseChanged(bool paused) => OrbMusicDirector.SetPaused(paused);

    void onArenaClosed()
    {
        OrbMusicDirector.SetPaused(false);
        OrbMusicDirector.PlayMenu();
    }
}
