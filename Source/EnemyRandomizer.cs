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
        private const int MAX_POTENTIAL_ENCOUNTERS = 25;
        private const int MAX_ROLLS = 500;
        internal static List<Type> previousEncounter = new List<Type>();
        internal static List<List<Type>> allSeenEncounters = new List<List<Type>>();

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

            BepinexPlugin.log.LogInfo("===NEW COMBAT===");
            BepinexPlugin.log.LogInfo("Act: " + __instance.Stage.Level + " - Act section: " + __instance.Act);
            int maxActWeight = GetActWeight(__instance.Stage.Level, __instance.Act, __instance is EliteEnemyStation);
            BepinexPlugin.log.LogInfo("Max Act weight: " + maxActWeight);
            int weightLeeway = maxActWeight - 10 - maxActWeight / 8;
            BepinexPlugin.log.LogInfo("Min weight leeway: " + weightLeeway);
            BepinexPlugin.log.LogInfo("Previous encounter: " + string.Join(", ", previousEncounter.ConvertAll(t => t.Name)));
            BepinexPlugin.log.LogInfo("Previous formations: ");
            allSeenEncounters.ForEach(se => BepinexPlugin.log.LogInfo(" - " + string.Join(", ", se.ConvertAll(t => t.Name))));
            var potentialEnemies = new List<List<(Type type, int weight)>>();
            int numRolls = 0;
            do
            {
                var candidates = new List<(Type type, int weight)>();
                int firstEnemyMinWeight = __instance.GameRun.StationRng.NextInt(0, maxActWeight);
                int minEnemies = __instance.GameRun.StationRng.NextInt(1, 3);
                int maxEnemies = __instance.GameRun.StationRng.NextInt(3, 5);
                BepinexPlugin.log.LogInfo("First enemy min weight: " + firstEnemyMinWeight + " - min enemies: " + minEnemies + " - max enemies: " + maxEnemies + " - num roll: " + numRolls);
                do
                {
                    var cand = enemyWeights.Where(w => w.Value.GetWeight(__instance.Stage.Level) >= firstEnemyMinWeight && w.Value.GetWeight(__instance.Stage.Level) <= maxActWeight).SampleOrDefault(__instance.GameRun.StationRng);
                    firstEnemyMinWeight = 0;
                    if (cand.Key == null)
                        continue;
                    int candWeight = cand.Value.GetWeight(__instance.Stage.Level);
                    candidates.Add((cand.Key, candWeight));
                    BepinexPlugin.log.LogInfo("chosen cand: " + cand.Key.Name + " - " + candWeight + " - sum: " + candidates.Sum(c => c.weight));
                } while (candidates.Sum(c => c.weight) < maxActWeight && candidates.Count < maxEnemies);
                if (candidates.Sum(c => c.weight) > maxActWeight)
                {
                    var toRemove = candidates.Last();
                    BepinexPlugin.log.LogInfo("To remove: " + toRemove.type.Name);
                    candidates.Remove(toRemove);
                }
                if (++numRolls > MAX_ROLLS)
                    break;

                BepinexPlugin.log.LogInfo("Total weight: " + candidates.Sum(c => c.weight));
                if (candidates.Sum(c => c.weight) < weightLeeway)
                {
                    BepinexPlugin.log.LogInfo("===INVALID: Below min weight===");
                    continue;
                }
                if (candidates.Count < minEnemies)
                {
                    BepinexPlugin.log.LogInfo("===INVALID: Less than min enemies===");
                    continue;
                }
                if (!IsValidEncounter(candidates))
                    continue;
                BepinexPlugin.log.LogInfo("===Added enemy group===");
                potentialEnemies.Add(candidates);
            } while (potentialEnemies.Count < MAX_POTENTIAL_ENCOUNTERS);
            if (potentialEnemies.Count <= 0)
            {
                BepinexPlugin.log.LogInfo("===Potential encounters empty===");
                return;
            }

            /*var chosenEnemies = new List<EnemyGroupEntry.EntrySource>()
            {
                new EnemyGroupEntry.EntrySource(typeof(Nitori), 2),
            };*/
            var chosenEnemies = new List<EnemyGroupEntry.EntrySource>();
            List<int> possibleIndexes = new List<int>() { 3,4,5,6,7 };
            previousEncounter.Clear();
            foreach (var (enemyType, value) in potentialEnemies.Sample(__instance.GameRun.StationRng))
            {
                previousEncounter.Add(enemyType);
                int chosenIndex = possibleIndexes.SampleOrDefault(__instance.GameRun.StationRng);
                if (chosenIndex == 0)
                    break;
                possibleIndexes.Remove(chosenIndex);
                chosenEnemies.Add(new EnemyGroupEntry.EntrySource(enemyType, chosenIndex));
            };
            allSeenEncounters.Add(chosenEnemies.Select(es => es.Type).ToList());

            var enemyGroup = __instance.EnemyGroup;
            __instance.EnemyGroup = new EnemyGroup(enemyGroup.Id, chosenEnemies, enemyGroup.EnemyType,
                BepinexPlugin.CUSTOM_EIGHT_FORMATION, enemyGroup.PlayerRootV2, enemyGroup.PreBattleDialogName, enemyGroup.PostBattleDialogName,
                enemyGroup.Hidden, enemyGroup.DebutTime, enemyGroup.Environment);
            foreach (var enemy in __instance.EnemyGroup)
            {
                enemy.EnterGameRun(__instance.GameRun);
            }
        }
        internal static int GetActWeight(int act, int actSection, bool isElite)
        {
            return isElite ? actWeights[act + "-e"] : actWeights[act + "-" + actSection];
        }

        internal static Dictionary<string, int> actWeights = new Dictionary<string, int>()
        {
            { "1-1", 40 },
            { "1-2", 60 },
            { "1-3", 70 },
            { "1-e", 100 },
            { "2-1", 110 },
            { "2-2", 125 },
            { "2-3", 140 },
            { "2-e", 190 },
            { "3-1", 210 },
            { "3-2", 230 },
            { "3-3", 250 },
            { "3-e", 360 },
        };

        internal static Dictionary<Type, EnemyActWeights> enemyWeights = new Dictionary<Type, EnemyActWeights>()
        {
            { typeof(WhiteFairy), new EnemyActWeights(40, 40, 40) },
            { typeof(RavenWen), new EnemyActWeights(15, 20, 25) },
            { typeof(RavenGuo), new EnemyActWeights(15, 20, 25) },
            { typeof(GuihuoBlue), new EnemyActWeights(20, 25, 30) }, //spirit
            { typeof(GuihuoGreen), new EnemyActWeights(20, 25, 30) },
            { typeof(GuihuoRed), new EnemyActWeights(30, 35, 40) },
            { typeof(SickGirl), new EnemyActWeights(20, 25, 30) },
            { typeof(YinyangyuRed), new EnemyActWeights(20, 20, 20) }, //yingyang orb
            { typeof(YinyangyuBlue), new EnemyActWeights(30, 35, 40) },
            { typeof(DollBlue), new EnemyActWeights(50, 75, 100) },
            { typeof(DollPurple), new EnemyActWeights(50, 75, 100) },
            { typeof(FraudRabbit), new EnemyActWeights(40, 60, 80) },
            { typeof(BlackFairy), new EnemyActWeights(60, 70, 80) },
            { typeof(Bat), new EnemyActWeights(25, 40, 55) },
            { typeof(MaoyuBlue), new EnemyActWeights(10, 10, 10) }, //kedama
            { typeof(Maoyu), new EnemyActWeights(15, 15, 15) }, //angy firepower kedama
            { typeof(MaoyuRed), new EnemyActWeights(10, 10, 10) },
            { typeof(MaoyuBlack), new EnemyActWeights(25, 25, 25) },
            { typeof(Sunny), new EnemyActWeights(40, 40, 40) },
            { typeof(Luna), new EnemyActWeights(45, 60, 75) },
            { typeof(Star), new EnemyActWeights(45, 75, 100) },
            { typeof(Aya), new EnemyActWeights(70, 85, 100) },
            { typeof(Rin), new EnemyActWeights(70, 70, 70) },
            { typeof(Purifier), new EnemyActWeights(50, 65, 80) },
            { typeof(Scout), new EnemyActWeights(50, 65, 80) },
            { typeof(WaterGirl), new EnemyActWeights(-1, 100, 120) },
            { typeof(Yaoshi), new EnemyActWeights(45, 60, 75) }, //floating rock
            { typeof(Fox), new EnemyActWeights(-1, 120, 180) },
            { typeof(BatLord), new EnemyActWeights(60, 80, 100) },
            { typeof(HetongKailang), new EnemyActWeights(60, 80, 100) }, //joy kappa
            { typeof(HetongYinchen), new EnemyActWeights(40, 40, 40) }, //gloomy kappa
            { typeof(ShenlingPurple), new EnemyActWeights(30, 30, 30) }, //gold spirit
            { typeof(ShenlingWhite), new EnemyActWeights(30, 30, 30) },
            { typeof(Nitori), new EnemyActWeights(-1, 130, 150) },
            { typeof(Youmu), new EnemyActWeights(-1, 130, 150) },
            { typeof(Kokoro), new EnemyActWeights(-1, 130, 150) },
            { typeof(YaTiangou), new EnemyActWeights(-1, 130, 150) }, //crow tengu
            { typeof(LangTiangou), new EnemyActWeights(-1, 130, 150) }, //wolf tengu
            { typeof(LoveGirl), new EnemyActWeights(-1, 190, 230) },
            { typeof(Terminator), new EnemyActWeights(-1, 100, 120) },
            { typeof(HardworkRabbit), new EnemyActWeights(-1, 150, 180) },
            { typeof(LazyRabbit), new EnemyActWeights(-1, 150, 180) },
            { typeof(KanakoLimao), new EnemyActWeights(-1, 140, 160) },
            { typeof(SuwakoLimao), new EnemyActWeights(-1, 120, 150) },
            { typeof(Clownpiece), new EnemyActWeights(-1, -1, 240) },
            { typeof(Siji), new EnemyActWeights(-1, -1, 240) },
            { typeof(Doremy), new EnemyActWeights(-1, -1, 240) },
        };
        internal struct EnemyActWeights
        {
            public int Act1;
            public int Act2;
            public int Act3;
            public EnemyActWeights(int act1, int act2, int act3)
            {
                Act1 = act1;
                Act2 = act2;
                Act3 = act3;
            }
            public int GetWeight(int act)
            {
                return act switch
                {
                    1 => Act1,
                    2 => Act2,
                    3 => Act3,
                    _ => -1
                };
            }
        }

        private static List<Type> supportEnemies = new List<Type>()
        {
            typeof(GuihuoBlue),
            typeof(GuihuoGreen),
            typeof(GuihuoRed),
            typeof(SickGirl),
            typeof(YinyangyuBlue),
            typeof(FraudRabbit),
            typeof(Bat),
            typeof(Luna),
            typeof(Star),
            typeof(Purifier),
            typeof(Scout),
            typeof(Fox),
            typeof(BatLord),
            typeof(HetongKailang),
            typeof(HetongYinchen),
            typeof(SuwakoLimao),
        };
        private static List<Type> summonerEnemies = new List<Type>()
        {
            typeof(Rin),
            typeof(Nitori),
            typeof(Kokoro),
            typeof(KanakoLimao),
            typeof(Clownpiece),
            typeof(Siji),
            typeof(Doremy),
        };
        private static List<Type> gloomyKappaRequirements = new List<Type>()
        {
            typeof(Purifier),
            typeof(Scout),
            typeof(Terminator),
            typeof(Nitori),
        };

        private static bool IsValidEncounter(List<(Type type, int weight)> candidates)
        {
            if (previousEncounter.Any(pe => candidates.Select(c => c.type).Contains(pe)))
            {
                BepinexPlugin.log.LogInfo("===INVALID: matches 1 or more enemy types from previous encounter===");
                return false;
            }
            if (candidates.Count == 1 && candidates.Any(c => supportEnemies.Contains(c.type)))
            {
                BepinexPlugin.log.LogInfo("===INVALID: solo support unit===");
                return false;
            }
            if (candidates.Where(c => summonerEnemies.Contains(c.type)).Count() > 1)
            {
                BepinexPlugin.log.LogInfo("===INVALID: more than 1 summoner===");
                return false;
            }
            if (candidates.Any(c => c.type == typeof(HetongYinchen)) && !candidates.Any(c => gloomyKappaRequirements.Contains(c.type)))
            {
                BepinexPlugin.log.LogInfo("==INVALID: gloomy kappa without drone or nitori===");
                return false;
            }
            if (allSeenEncounters.Any(se => Enumerable.SequenceEqual(se.OrderBy(se2 => se2.Name), candidates.Select(c => c.type).OrderBy(c => c.Name))))
            {
                BepinexPlugin.log.LogInfo("==INVALID: matches a formation seen previously this run===");
                return false;
            }
            return true;
        }
    }
}
