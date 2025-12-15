using HarmonyLib;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnemyRandomizer
{
    [HarmonyPatch]
    internal class EnemyRandomizer
    {

        [HarmonyPatch(typeof(BattleStation), nameof(BattleStation.OnEnter))]
        private static void Postfix(BattleStation __instance)
        {
            if (!(__instance is EnemyStation) && !(__instance is EliteEnemyStation))
                return;
            
            var stage = __instance.Stage;
            var allEnemyGroups = stage.EnemyPoolAct1._entries.Select(e => e.Elem).Concat(stage.EnemyPoolAct2._entries.Select(e => e.Elem))
                .Concat(stage.EnemyPoolAct3._entries.Select(e => e.Elem)).Concat(stage.EliteEnemyPool._entries.Select(e => e.Elem));
            var allEnemyEntries = allEnemyGroups.Select(id => Library.GetEnemyGroupEntry(id));
            HashSet<Type> enemyTypes = new HashSet<Type>();
            foreach (var entry in allEnemyEntries)
            {
                if (!entry.Config.IsSub && entry.Config.Subs.Any())
                {
                    foreach (var sub in entry.Config.Subs)
                    {
                        EnemyGroupConfig.FromId(sub).Enemies.Where(e => e != "Empty").Do(e => enemyTypes.Add(TypeFactory<EnemyUnit>.GetType(e)));
                    }
                }
                else
                {
                    entry.Do(es => enemyTypes.Add(es.Type));
                }
            }
            foreach (var type in enemyTypes)
            {
                BepinexPlugin.log.LogInfo(type.Name);
            }
            enemyTypes.RemoveWhere(t => t == typeof(Sunny));

            var chosenEnemies = new List<EnemyGroupEntry.EntrySource>();
            const int maxEnemies = 5;
            int numEnemies = __instance.GameRun.StationRng.NextInt(1, maxEnemies);
            for (int i = maxEnemies - 1; i >= maxEnemies - numEnemies; i--)
                chosenEnemies.Add(new EnemyGroupEntry.EntrySource(enemyTypes.Sample(__instance.GameRun.StationRng), i));
            var enemyGroup = __instance.EnemyGroup;
            __instance.EnemyGroup = new EnemyGroup(enemyGroup.Id, chosenEnemies, enemyGroup.EnemyType,
                "Five", enemyGroup.PlayerRootV2, enemyGroup.PreBattleDialogName, enemyGroup.PostBattleDialogName,
                enemyGroup.Hidden, enemyGroup.DebutTime, enemyGroup.Environment);
            foreach (var enemy in __instance.EnemyGroup)
            {
                enemy.EnterGameRun(__instance.GameRun);
            }
        }
    }
}
