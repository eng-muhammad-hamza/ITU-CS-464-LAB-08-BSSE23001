using UnityEngine;

public struct LevelDef
{
    public string name;
    public int shardTarget;
    public int chasers;
    public int patrollers;
    public int spikes;
    public int turrets;
    public float chaserSpeed;
}

public struct OrbDef
{
    public string name;
    public Color color;
    public int price;
    public float speedMultiplier;
    public string perk;
}

public struct BoostDef
{
    public string name;
    public string description;
    public int price;
}

public static class Catalog
{
    public static readonly LevelDef[] Levels =
    {
        new LevelDef { name = "First Light",    shardTarget = 8,  chasers = 0, patrollers = 1, spikes = 0, turrets = 0, chaserSpeed = 2.6f },
        new LevelDef { name = "Drone Alley",    shardTarget = 10, chasers = 1, patrollers = 1, spikes = 0, turrets = 0, chaserSpeed = 2.8f },
        new LevelDef { name = "Spike Garden",   shardTarget = 12, chasers = 1, patrollers = 1, spikes = 3, turrets = 0, chaserSpeed = 3.0f },
        new LevelDef { name = "Crossfire",      shardTarget = 12, chasers = 1, patrollers = 1, spikes = 2, turrets = 2, chaserSpeed = 3.0f },
        new LevelDef { name = "Swarm",          shardTarget = 14, chasers = 3, patrollers = 1, spikes = 3, turrets = 1, chaserSpeed = 3.2f },
        new LevelDef { name = "Core Meltdown",  shardTarget = 16, chasers = 3, patrollers = 2, spikes = 4, turrets = 2, chaserSpeed = 3.4f },
    };

    public static readonly OrbDef[] Orbs =
    {
        new OrbDef { name = "Nova",   color = new Color(0.13f, 0.83f, 0.93f), price = 0,   speedMultiplier = 1.00f, perk = "Balanced. A reliable start." },
        new OrbDef { name = "Ember",  color = new Color(1f, 0.55f, 0.2f),     price = 120, speedMultiplier = 1.08f, perk = "+8% move speed" },
        new OrbDef { name = "Bloom",  color = new Color(1f, 0.31f, 0.64f),    price = 180, speedMultiplier = 1.12f, perk = "+12% move speed" },
        new OrbDef { name = "Mint",   color = new Color(0.24f, 0.86f, 0.52f), price = 240, speedMultiplier = 1.16f, perk = "+16% move speed" },
        new OrbDef { name = "Volt",   color = new Color(1f, 0.85f, 0.25f),    price = 320, speedMultiplier = 1.20f, perk = "+20% move speed" },
        new OrbDef { name = "Phantom",color = new Color(0.7f, 0.55f, 1f),     price = 450, speedMultiplier = 1.25f, perk = "+25% move speed" },
    };

    public static readonly BoostDef[] Boosts =
    {
        new BoostDef { name = "Start Shield", description = "Begin the run with a shield that blocks one hit.", price = 40 },
        new BoostDef { name = "Extra Heart",  description = "Begin the run with one extra heart.",              price = 60 },
    };
}
