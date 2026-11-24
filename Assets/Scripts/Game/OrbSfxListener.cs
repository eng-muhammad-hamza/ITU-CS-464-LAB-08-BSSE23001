using UnityEngine;

public class OrbSfxListener : MonoBehaviour
{
    void OnEnable()
    {
        OrbPickup.Collected += onPickupCollected;
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.Healed += onOrbHealed;
        OrbVitals.ShieldBlocked += onShieldBlocked;
        OrbVitals.ShieldRaised += onShieldRaised;
        OrbVitals.Died += onOrbDied;
        PlayerOrb.Dashed += onOrbDashed;
        GameSession.CountdownChanged += onCountdownChanged;
        GameSession.RunWon += onRunWon;
        GameSession.RunLost += onRunLost;
    }

    void OnDisable()
    {
        OrbPickup.Collected -= onPickupCollected;
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.Healed -= onOrbHealed;
        OrbVitals.ShieldBlocked -= onShieldBlocked;
        OrbVitals.ShieldRaised -= onShieldRaised;
        OrbVitals.Died -= onOrbDied;
        PlayerOrb.Dashed -= onOrbDashed;
        GameSession.CountdownChanged -= onCountdownChanged;
        GameSession.RunWon -= onRunWon;
        GameSession.RunLost -= onRunLost;
    }

    void onPickupCollected(PickupKind kind, Vector3 at)
    {
        switch (kind)
        {
            case PickupKind.Shard: OrbSfxPlayer.Play(ArenaCue.ShardPickup, 1f, 0.9f, 0.1f); break;
            case PickupKind.Boost: OrbSfxPlayer.Play(ArenaCue.BoostPickup); break;
        }
    }

    void onOrbDamaged(OrbHitReport report) => OrbSfxPlayer.Play(ArenaCue.OrbDamaged, 1f, 1f, 0.08f);

    void onOrbHealed(int hearts, Vector3 at) => OrbSfxPlayer.Play(ArenaCue.OrbHealed);

    void onShieldBlocked(Vector3 at) => OrbSfxPlayer.Play(ArenaCue.ShieldBlock);

    void onShieldRaised(Vector3 at) => OrbSfxPlayer.Play(ArenaCue.ShieldRaise);

    void onOrbDied(Vector3 at) => OrbSfxPlayer.Play(ArenaCue.OrbDestroyed, 1f, 1f, 0.02f);

    void onOrbDashed(Vector3 at, Color color) => OrbSfxPlayer.Play(ArenaCue.OrbDash, 1f, 0.8f, 0.1f);

    void onCountdownChanged(string text)
    {
        if (text == "GO!") OrbSfxPlayer.Play(ArenaCue.CountdownGo, 1f, 1f, 0.02f);
        else if (text.Length > 0) OrbSfxPlayer.Play(ArenaCue.CountdownTick, 1f, 0.9f, 0.02f);
    }

    void onRunWon(Vector3 at) => OrbSfxPlayer.Play(ArenaCue.RunWon, 1f, 1f, 0f);

    void onRunLost(Vector3 at) => OrbSfxPlayer.Play(ArenaCue.RunLost, 1f, 1f, 0f);
}
