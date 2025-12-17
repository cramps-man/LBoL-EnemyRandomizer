using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
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
