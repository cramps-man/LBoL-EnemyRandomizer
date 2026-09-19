using HarmonyLib;

namespace EnemyRandomizer
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "cramps-enemyrandomizer";
        public const string Name = "EnemyRandomizer";
        public const string version = "1.4.0";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
