using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;

namespace EnemyRandomizer
{
    public sealed class EnemyRandomizerJadeboxDef : JadeBoxTemplate
    {
        public override IdContainer GetId()
        {
            return nameof(EnemyRandomizerJadebox);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.jadeboxLoc.AddEntity(this);
        }

        public override JadeBoxConfig MakeConfig()
        {
            var config = new JadeBoxConfig(
                Index: 0,
                Id: "",
                Order: 10,
                Group: new List<string>() { },
                Value1: null,
                Value2: null,
                Value3: null,
                Mana: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
                );

            return config;
        }
    }
    [EntityLogic(typeof(EnemyRandomizerJadeboxDef))]
    public sealed class EnemyRandomizerJadebox : JadeBox
    {
        protected override void OnEnterBattle()
        {
            HandleBattleEvent(Battle.BattleStarted, OnBattleStarted);
            ReactBattleEvent(Battle.Player.StatusEffectAdding, OnPlayerStatusEffectAdding);
            HandleBattleEvent(Battle.EnemyPointGenerating, OnEnemyPointGenerating);
        }

        private void OnEnemyPointGenerating(DieEventArgs args)
        {
            if (!(GameRun.CurrentStation is EnemyStation) && !(GameRun.CurrentStation is EliteEnemyStation))
                return;

            int maxWeightForAct = EnemyRandomizer.GetActWeight(GameRun.CurrentStage.Level, 0, true);
            if (!EnemyRandomizer.enemyWeights.TryGetValue(args.Unit.GetType(), out EnemyRandomizer.EnemyActWeights enemyWeights))
                return;
            int currentWeight = enemyWeights.GetWeight(GameRun.CurrentStage.Level);
            if (currentWeight < 0)
                return;
            float percent = (float)currentWeight / maxWeightForAct;
            BepinexPlugin.log.LogInfo("Act max weight: " + maxWeightForAct + " - Enemy weight: " + currentWeight + " - Percentage: " + percent);
            int bonusPower = GameRun.BattleRng.NextInt(1, 3);
            int newPower = (int)(30 * percent);
            BepinexPlugin.log.LogInfo("New enemy power: " + newPower + " - Bonus power: " + bonusPower);
            args.Power = newPower + bonusPower;
        }

        private IEnumerable<BattleAction> OnPlayerStatusEffectAdding(StatusEffectApplyEventArgs args)
        {
            if (!(args.Effect is LockedOn))
                yield break;
            if (!(args.ActionSource is Star s))
                yield break;

            args.ForceCancelBecause(CancelCause.Reaction);
            yield return new ApplyStatusEffectAction<EnemyLockedOn>(s, args.Level);
        }

        private void OnBattleStarted(GameEventArgs args)
        {
            foreach (var enemy in Battle.AllAliveEnemies)
            {
                ReactBattleEvent(enemy.StatusEffectAdding, OnEnemyStatusEffectAdding);
            }
            HandleBattleEvent(Battle.EnemySpawned, OnEnemySpawned);
        }

        private void OnEnemySpawned(UnitEventArgs args)
        {
            ReactBattleEvent(args.Unit.StatusEffectAdding, OnEnemyStatusEffectAdding);
        }

        private IEnumerable<BattleAction> OnEnemyStatusEffectAdding(StatusEffectApplyEventArgs args)
        {
            if (!(args.Effect is Firepower))
                yield break;
            if (args.ActionSource is EndOfTurnFpSe)
                yield break;
            args.ForceCancelBecause(CancelCause.Reaction);
            yield return new ApplyStatusEffectAction<EndOfTurnFpSe>(args.Unit, args.Level);
        }
    }
}
