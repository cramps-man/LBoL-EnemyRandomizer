using HarmonyLib;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;

namespace EnemyRandomizer
{
    [HarmonyPatch]
    internal class EnemyPatches
    {
        [HarmonyPatch(typeof(Sunny), nameof(Sunny.OnEnterBattle))]
        private static bool Prefix(Sunny __instance)
        {
            if (!__instance.GameRun.HasJadeBox<EnemyRandomizerJadebox>())
                return true;
            __instance.Spell();
            return false;
        }
        [HarmonyPatch(typeof(Unit), "OnEnterBattle")]
        private static bool Prefix(Unit __instance)
        {
            if (!__instance.GameRun.HasJadeBox<EnemyRandomizerJadebox>())
                return true;
            if (__instance is LightFairy lf)
                lf.Spell();
            return false;
        }
    }
}
