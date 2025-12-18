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
            BepinexPlugin.log.LogInfo("Previous encounter: " + string.Join(", ", previousEncounter.ConvertAll<string>(t => t.Name)));
            var potentialEnemies = new List<List<ValueTuple<Type, int>>>();
            int numRolls = 0;
            do
            {
                var candidates = new List<ValueTuple<Type, int>>();
                int firstEnemyMinWeight = __instance.GameRun.StationRng.NextInt(0, maxActWeight);
                int minEnemies = __instance.GameRun.StationRng.NextInt(1, 2);
                int maxEnemies = __instance.GameRun.StationRng.NextInt(3, 5);
                BepinexPlugin.log.LogInfo("First enemy min weight: " + firstEnemyMinWeight + " - min enemies: " + minEnemies + " - max enemies: " + maxEnemies + " - num roll: " + numRolls);
                do
                {
                    var cand = enemyWeights.Where(w => w.Value >= firstEnemyMinWeight && w.Value <= maxActWeight).SampleOrDefault(__instance.GameRun.StationRng);
                    firstEnemyMinWeight = 0;
                    if (cand.Key == null)
                        continue;
                    candidates.Add((cand.Key, cand.Value));
                    BepinexPlugin.log.LogInfo("chosen cand: " + cand.Key.Name + " - " + cand.Value + " - sum: " + candidates.Sum(c => c.Item2));
                } while (candidates.Sum(c => c.Item2) < maxActWeight && candidates.Count < maxEnemies);
                if (candidates.Sum(c => c.Item2) > maxActWeight)
                {
                    var toRemove = candidates.Last();
                    BepinexPlugin.log.LogInfo("To remove: " + toRemove.Item1.Name);
                    candidates.Remove(toRemove);
                }
                if (++numRolls > MAX_ROLLS)
                    break;

                BepinexPlugin.log.LogInfo("Total weight: " + candidates.Sum(c => c.Item2));
                if (candidates.Sum(c => c.Item2) < weightLeeway)
                {
                    BepinexPlugin.log.LogInfo("INVALID: Below min weight");
                    continue;
                }
                if (candidates.Count < minEnemies)
                {
                    BepinexPlugin.log.LogInfo("INVALID: Less than min enemies");
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
            int index = 7;
            previousEncounter.Clear();
            foreach (var (enemyType, value) in potentialEnemies.Sample(__instance.GameRun.StationRng))
            {
                previousEncounter.Add(enemyType);
                chosenEnemies.Add(new EnemyGroupEntry.EntrySource(enemyType, index));
                index--;
            };

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
            { "1-2", 50 },
            { "1-3", 60 },
            { "1-e", 90 },
            { "2-1", 100 },
            { "2-2", 115 },
            { "2-3", 130 },
            { "2-e", 175 },
            { "3-1", 190 },
            { "3-2", 210 },
            { "3-3", 230 },
            { "3-e", 300 },
        };

        internal static Dictionary<Type, int> enemyWeights = new Dictionary<Type, int>()
        {
            { typeof(WhiteFairy), 30 },
            { typeof(RavenWen), 15 },
            { typeof(RavenGuo), 15 },
            { typeof(GuihuoBlue), 25 }, //spirit
            { typeof(GuihuoGreen), 25 },
            { typeof(GuihuoRed), 35 },
            { typeof(SickGirl), 25 },
            { typeof(YinyangyuRed), 20 }, //yingyang orb
            { typeof(YinyangyuBlue), 30 },
            { typeof(DollBlue), 50 },
            { typeof(DollPurple), 50 },
            { typeof(FraudRabbit), 40 },
            { typeof(BlackFairy), 50 },
            { typeof(Bat), 35 },
            { typeof(MaoyuBlue), 10 }, //kedama
            { typeof(Maoyu), 15 }, //angy firepower kedama
            { typeof(MaoyuRed), 10 },
            { typeof(MaoyuBlack), 25 },
            { typeof(Sunny), 45 },
            { typeof(Luna), 60 },
            { typeof(Star), 60 },
            { typeof(Aya), 60 },
            { typeof(Rin), 60 },
            { typeof(Purifier), 50 },
            { typeof(Scout), 50 },
            { typeof(WaterGirl), 100 },
            { typeof(Yaoshi), 45 }, //floating rock
            { typeof(Fox), 150 },
            { typeof(BatLord), 70 },
            { typeof(HetongKailang), 60 }, //joy kappa
            { typeof(HetongYinchen), 40 }, //gloomy kappa
            { typeof(ShenlingPurple), 30 }, //gold spirit
            { typeof(ShenlingWhite), 30 },
            { typeof(Nitori), 120 },
            { typeof(Youmu), 120 },
            { typeof(Kokoro), 120 },
            { typeof(YaTiangou), 120 }, //crow tengu
            { typeof(LangTiangou), 130 }, //wolf tengu
            { typeof(LoveGirl), 200 },
            { typeof(Terminator), 100 },
            { typeof(HardworkRabbit), 150 },
            { typeof(LazyRabbit), 150 },
            { typeof(KanakoLimao), 120 },
            { typeof(SuwakoLimao), 120 },
            { typeof(Clownpiece), 230 },
            { typeof(Siji), 230 },
            { typeof(Doremy), 230 },
        };

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

        private static bool IsValidEncounter(List<ValueTuple<Type, int>> candidates)
        {
            if (previousEncounter.Count != 0 && candidates.Where(c => previousEncounter.Contains(c.Item1)).Count() == previousEncounter.Count)
            {
                BepinexPlugin.log.LogInfo("INVALID: matches previous encounter");
                return false;
            }
            if (candidates.Count == 1 && candidates.Any(c => supportEnemies.Contains(c.Item1)))
            {
                BepinexPlugin.log.LogInfo("INVALID: solo support unit");
                return false;
            }
            if (candidates.Where(c => summonerEnemies.Contains(c.Item1)).Count() > 1)
            {
                BepinexPlugin.log.LogInfo("INVALID: more than 1 summoner");
                return false;
            }
            if (candidates.Any(c => c.Item1 == typeof(HetongYinchen)) && !candidates.Any(c => gloomyKappaRequirements.Contains(c.Item1)))
            {
                BepinexPlugin.log.LogInfo("INVALID: gloomy kappa without drone or nitori");
                return false;
            }
            return true;
        }
    }
}
