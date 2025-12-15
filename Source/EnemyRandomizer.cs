using HarmonyLib;
using LBoL.Base.Extensions;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Normal.Shenlings;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnemyRandomizer
{
    [HarmonyPatch]
    internal class EnemyRandomizer
    {
        private const int MAX_ENEMIES = 5;
        private const int MAX_ROLLS = 10;

        [HarmonyPatch(typeof(BattleStation), nameof(BattleStation.OnEnter))]
        private static void Postfix(BattleStation __instance)
        {
            if (!(__instance is EnemyStation) && !(__instance is EliteEnemyStation))
            {
                BepinexPlugin.log.LogInfo("Not enemy or elite station");
                return;
            }
            if (!__instance.GameRun.HasJadeBox<EnemyRandomizerJadebox>())
            {
                BepinexPlugin.log.LogInfo("Jadebox not enabled");
                return;
            }

            BepinexPlugin.log.LogInfo("Act: " + __instance.Stage.Level + " - Act section: " + __instance.Act);
            int maxActWeight = GetActWeight(__instance.Stage.Level, __instance.Act, __instance is EliteEnemyStation);
            BepinexPlugin.log.LogInfo("Max Act weight: " + maxActWeight);
            var potentialEnemies = new List<List<ValueTuple<Type, int>>>();
            do
            {
                var candidates = new List<ValueTuple<Type, int>>();
                do
                {
                    var cand = enemyWeights.Where(w => w.Value <= maxActWeight).Sample(__instance.GameRun.StationRng);
                    candidates.Add((cand.Key, cand.Value));
                    BepinexPlugin.log.LogInfo("chosen cand: " + cand.Key + " - " + cand.Value);
                    BepinexPlugin.log.LogInfo("sum: " + candidates.Sum(c => c.Item2) + " - count: " + candidates.Count);
                } while (candidates.Sum(c => c.Item2) < maxActWeight && candidates.Count < MAX_ENEMIES);
                if (candidates.Sum(c => c.Item2) > maxActWeight)
                {
                    var toRemove = candidates.Last();
                    BepinexPlugin.log.LogInfo("To remove: " + toRemove);
                    candidates.Remove(toRemove);
                }
                BepinexPlugin.log.LogInfo("Total weight: " + candidates.Sum(c => c.Item2));
                BepinexPlugin.log.LogInfo("===Added enemy group===");
                potentialEnemies.Add(candidates);
            } while (potentialEnemies.Count < MAX_ROLLS);

            var chosenEnemies = new List<EnemyGroupEntry.EntrySource>();
            int index = 4;
            foreach (var (enemyType, value) in potentialEnemies.OrderByDescending(e => e.Sum(e2 => e2.Item2)).First())
            {
                chosenEnemies.Add(new EnemyGroupEntry.EntrySource(enemyType, index));
                index--;
            };

            var enemyGroup = __instance.EnemyGroup;
            __instance.EnemyGroup = new EnemyGroup(enemyGroup.Id, chosenEnemies, enemyGroup.EnemyType,
                "Five", enemyGroup.PlayerRootV2, enemyGroup.PreBattleDialogName, enemyGroup.PostBattleDialogName,
                enemyGroup.Hidden, enemyGroup.DebutTime, enemyGroup.Environment);
            foreach (var enemy in __instance.EnemyGroup)
            {
                enemy.EnterGameRun(__instance.GameRun);
            }
        }
        private static int GetActWeight(int act, int actSection, bool isElite)
        {
            return isElite ? actWeights[act + "-e"] : actWeights[act + "-" + actSection];
        }

        private static Dictionary<string, int> actWeights = new Dictionary<string, int>()
        {
            { "1-1", 40 },
            { "1-2", 50 },
            { "1-3", 60 },
            { "1-e", 80 },
            { "2-1", 80 },
            { "2-2", 95 },
            { "2-3", 110 },
            { "2-e", 150 },
            { "3-1", 150 },
            { "3-2", 175 },
            { "3-3", 200 },
            { "3-e", 300 },
        };

        private static Dictionary<Type, int> enemyWeights = new Dictionary<Type, int>()
        {
            { typeof(WhiteFairy), 30 },
            { typeof(RavenWen), 15 },
            { typeof(RavenGuo), 15 },
            { typeof(GuihuoBlue), 10 },
            { typeof(GuihuoGreen), 10 },
            { typeof(GuihuoRed), 10 },
            { typeof(SickGirl), 20 },
            { typeof(YinyangyuRed), 20 },
            { typeof(YinyangyuBlue), 20 },
            { typeof(DollBlue), 40 },
            { typeof(DollPurple), 40 },
            { typeof(FraudRabbit), 20 },
            { typeof(BlackFairy), 50 },
            { typeof(Bat), 15 },
            { typeof(MaoyuBlue), 10 },
            { typeof(Maoyu), 10 },
            { typeof(MaoyuRed), 15 },
            { typeof(MaoyuBlack), 25 },
            //{ typeof(Sunny), 30 },
            { typeof(Luna), 30 },
            { typeof(Star), 30 },
            { typeof(Aya), 80 },
            { typeof(Rin), 80 },
            { typeof(Purifier), 40 },
            { typeof(Scout), 40 },
            { typeof(WaterGirl), 70 },
            { typeof(Yaoshi), 30 },
            { typeof(Fox), 120 },
            { typeof(BatLord), 40 },
            { typeof(HetongKailang), 30 },
            { typeof(HetongYinchen), 30 },
            { typeof(ShenlingPurple), 30 },
            { typeof(ShenlingWhite), 30 },
            { typeof(Nitori), 130 },
            { typeof(Youmu), 130 },
            { typeof(Kokoro), 130 },
            { typeof(YaTiangou), 120 },
            { typeof(LangTiangou), 130 },
            { typeof(LoveGirl), 150 },
            { typeof(Terminator), 80 },
            { typeof(HardworkRabbit), 150 },
            { typeof(LazyRabbit), 150 },
            { typeof(KanakoLimao), 100 },
            { typeof(SuwakoLimao), 120 },
            { typeof(Clownpiece), 250 },
            { typeof(Siji), 250 },
            { typeof(Doremy), 250 },
        };
    }
}
