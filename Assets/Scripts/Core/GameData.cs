using UnityEngine;

public static class GameData
{
    public const int StartingGems = 150;

    public static int SelectedLevel;

    public static int Gems
    {
        get => PlayerPrefs.GetInt("gems", StartingGems);
        private set => PlayerPrefs.SetInt("gems", Mathf.Max(0, value));
    }
    public static void AddGems(int amount) { Gems = Gems + amount; PlayerPrefs.Save(); }
    public static bool TrySpend(int amount)
    {
        if (Gems < amount) return false;
        Gems = Gems - amount;
        PlayerPrefs.Save();
        return true;
    }

    public static int GetStars(int level) => PlayerPrefs.GetInt("stars" + level, 0);
    public static void SetStarsIfBetter(int level, int stars)
    {
        if (stars > GetStars(level)) PlayerPrefs.SetInt("stars" + level, stars);
    }
    public static bool IsLevelUnlocked(int level) => level == 0 || GetStars(level - 1) > 0;
    public static int TotalStars()
    {
        int total = 0;
        for (int i = 0; i < Catalog.Levels.Length; i++) total += GetStars(i);
        return total;
    }
    public static int HighestUnlockedLevel()
    {
        int highest = 0;
        for (int i = 0; i < Catalog.Levels.Length; i++) if (IsLevelUnlocked(i)) highest = i;
        return highest;
    }

    public static bool OwnsOrb(int id) => id == 0 || PlayerPrefs.GetInt("orb" + id, 0) == 1;
    public static void GiveOrb(int id) { PlayerPrefs.SetInt("orb" + id, 1); PlayerPrefs.Save(); }
    public static int SelectedOrb
    {
        get => PlayerPrefs.GetInt("selectedOrb", 0);
        set => PlayerPrefs.SetInt("selectedOrb", value);
    }

    public static int BoostCount(int id) => PlayerPrefs.GetInt("boostCount" + id, 0);
    public static void AddBoost(int id, int amount)
    {
        PlayerPrefs.SetInt("boostCount" + id, Mathf.Max(0, BoostCount(id) + amount));
        if (BoostCount(id) == 0) PlayerPrefs.SetInt("boostOn" + id, 0);
        PlayerPrefs.Save();
    }
    public static bool BoostEnabled(int id) => PlayerPrefs.GetInt("boostOn" + id, 0) == 1 && BoostCount(id) > 0;
    public static void SetBoostEnabled(int id, bool on) => PlayerPrefs.SetInt("boostOn" + id, on ? 1 : 0);

    public static bool Haptics { get => PlayerPrefs.GetInt("haptics", 1) == 1; set => PlayerPrefs.SetInt("haptics", value ? 1 : 0); }
    public static bool ScreenShake { get => PlayerPrefs.GetInt("shake", 1) == 1; set => PlayerPrefs.SetInt("shake", value ? 1 : 0); }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey("gems");
        for (int i = 0; i < Catalog.Levels.Length; i++) PlayerPrefs.DeleteKey("stars" + i);
        for (int i = 0; i < Catalog.Orbs.Length; i++) PlayerPrefs.DeleteKey("orb" + i);
        for (int i = 0; i < Catalog.Boosts.Length; i++)
        {
            PlayerPrefs.DeleteKey("boostCount" + i);
            PlayerPrefs.DeleteKey("boostOn" + i);
        }
        PlayerPrefs.DeleteKey("selectedOrb");
        PlayerPrefs.Save();
        SelectedLevel = 0;
    }

    public static void Vibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Haptics) Handheld.Vibrate();
#endif
    }
}
