using UnityEngine;

public class OrbBurstListener : MonoBehaviour
{
    void OnEnable()
    {
        OrbPickup.Collected += onPickupCollected;
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.Healed += onOrbHealed;
        OrbVitals.ShieldBlocked += onShieldBlocked;
        OrbVitals.Died += onOrbDied;
        PlayerOrb.Dashed += onOrbDashed;
        GameSession.RunWon += onRunWon;
    }

    void OnDisable()
    {
        OrbPickup.Collected -= onPickupCollected;
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.Healed -= onOrbHealed;
        OrbVitals.ShieldBlocked -= onShieldBlocked;
        OrbVitals.Died -= onOrbDied;
        PlayerOrb.Dashed -= onOrbDashed;
        GameSession.RunWon -= onRunWon;
    }

    void onPickupCollected(PickupKind kind, Vector3 at)
    {
        switch (kind)
        {
            case PickupKind.Shard:
                OrbBurstFx.Burst(at, OrbPalette.ShardCyan, 10, 4.5f, 0.2f, 0.45f);
                OrbBurstFx.FloatingText(at + Vector3.up * 0.5f, "+1", OrbPalette.ShardCyan);
                break;
            case PickupKind.Heart:
                OrbBurstFx.Burst(at, OrbPalette.HeartPink, 12, 4f, 0.24f, 0.6f);
                break;
            case PickupKind.Shield:
                OrbBurstFx.Ring(at, OrbPalette.ShieldIce, 2.2f, 0.5f);
                OrbBurstFx.FloatingText(at + Vector3.up * 0.5f, "SHIELD", OrbPalette.ShieldIce, 4.5f);
                break;
            case PickupKind.Boost:
                OrbBurstFx.Ring(at, OrbPalette.BoostAmber, 2.2f, 0.5f);
                OrbBurstFx.FloatingText(at + Vector3.up * 0.5f, "BOOST", OrbPalette.BoostAmber, 4.5f);
                break;
        }
    }

    void onOrbDamaged(OrbHitReport report)
    {
        OrbBurstFx.Burst(report.position, OrbPalette.DamageCoral, 16, 6f, 0.26f, 0.5f);
        OrbBurstFx.FloatingText(report.position + Vector3.up * 0.8f, "-1", OrbPalette.DamageCoral);
    }

    void onOrbHealed(int hearts, Vector3 at)
    {
        OrbBurstFx.Ring(at, OrbPalette.HeartPink, 2f, 0.55f);
        OrbBurstFx.FloatingText(at + Vector3.up * 0.8f, "+1", OrbPalette.HeartPink);
    }

    void onShieldBlocked(Vector3 at)
    {
        OrbBurstFx.Ring(at, OrbPalette.ShieldIce, 2.4f, 0.4f);
        OrbBurstFx.Burst(at, OrbPalette.ShieldIce, 14, 6f, 0.2f, 0.4f);
        OrbBurstFx.FloatingText(at + Vector3.up * 0.8f, "BLOCK", OrbPalette.ShieldIce, 4.5f);
    }

    void onOrbDied(Vector3 at) => OrbBurstFx.Burst(at, OrbPalette.DamageCoral, 36, 8f, 0.3f, 0.9f);

    void onOrbDashed(Vector3 at, Color color) => OrbBurstFx.Burst(at, color, 8, 3.5f, 0.2f, 0.3f);

    void onRunWon(Vector3 at)
    {
        OrbBurstFx.Ring(at, OrbPalette.VictoryGold, 4f, 0.8f);
        OrbBurstFx.Burst(at, OrbPalette.VictoryGold, 30, 7f, 0.3f, 0.8f);
    }
}
