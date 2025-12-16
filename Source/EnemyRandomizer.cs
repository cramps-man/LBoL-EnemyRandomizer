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
        private const int MAX_ROLLS = 25;

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
                int firstEnemyMinWeight = __instance.GameRun.StationRng.NextInt(0, maxActWeight);
                int maxEnemies = __instance.GameRun.StationRng.NextInt(3, 5);
                BepinexPlugin.log.LogInfo("First enemy min weight: " + firstEnemyMinWeight + " - max enemies: " + maxEnemies);
                do
                {
                    var cand = enemyWeights.Where(w => w.Value >= firstEnemyMinWeight && w.Value <= maxActWeight).SampleOrDefault(__instance.GameRun.StationRng);
                    firstEnemyMinWeight = 0;
                    if (cand.Key == null)
                        continue;
                    candidates.Add((cand.Key, cand.Value));
                    BepinexPlugin.log.LogInfo("chosen cand: " + cand.Key + " - " + cand.Value + " - sum: " + candidates.Sum(c => c.Item2));
                } while (candidates.Sum(c => c.Item2) < maxActWeight && candidates.Count < maxEnemies);
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
            foreach (var (enemyType, value) in potentialEnemies.OrderByDescending(e => e.Sum(e2 => e2.Item2)).Take(5).Sample(__instance.GameRun.StationRng))
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
            { "2-1", 90 },
            { "2-2", 105 },
            { "2-3", 120 },
            { "2-e", 160 },
            { "3-1", 175 },
            { "3-2", 200 },
            { "3-3", 225 },
            { "3-e", 300 },
        };

        private static Dictionary<Type, int> enemyWeights = new Dictionary<Type, int>()
        {
            { typeof(WhiteFairy), 30 },
            { typeof(RavenWen), 15 },
            { typeof(RavenGuo), 15 },
            { typeof(GuihuoBlue), 15 }, //spirit
            { typeof(GuihuoGreen), 15 },
            { typeof(GuihuoRed), 25 },
            { typeof(SickGirl), 20 },
            { typeof(YinyangyuRed), 25 }, //yingyang orb
            { typeof(YinyangyuBlue), 25 },
            { typeof(DollBlue), 50 },
            { typeof(DollPurple), 50 },
            { typeof(FraudRabbit), 25 },
            { typeof(BlackFairy), 50 },
            { typeof(Bat), 20 },
            { typeof(MaoyuBlue), 10 }, //kedama
            { typeof(Maoyu), 10 },
            { typeof(MaoyuRed), 15 },
            { typeof(MaoyuBlack), 25 },
            //{ typeof(Sunny), 30 },
            { typeof(Luna), 30 },
            { typeof(Star), 30 },
            { typeof(Aya), 60 },
            { typeof(Rin), 60 },
            { typeof(Purifier), 45 },
            { typeof(Scout), 45 },
            { typeof(WaterGirl), 90 },
            { typeof(Yaoshi), 45 }, //floating rock
            { typeof(Fox), 150 },
            { typeof(BatLord), 50 },
            { typeof(HetongKailang), 30 }, //joy kappa
            { typeof(HetongYinchen), 20 }, //gloomy kappa
            { typeof(ShenlingPurple), 30 }, //gold spirit
            { typeof(ShenlingWhite), 30 },
            { typeof(Nitori), 120 },
            { typeof(Youmu), 120 },
            { typeof(Kokoro), 120 },
            { typeof(YaTiangou), 120 }, //crow tengu
            { typeof(LangTiangou), 130 }, //wolf tengu
            { typeof(LoveGirl), 150 },
            { typeof(Terminator), 100 },
            { typeof(HardworkRabbit), 150 },
            { typeof(LazyRabbit), 150 },
            { typeof(KanakoLimao), 120 },
            { typeof(SuwakoLimao), 100 },
            { typeof(Clownpiece), 225 },
            { typeof(Siji), 225 },
            { typeof(Doremy), 225 },
        };
    }
}
