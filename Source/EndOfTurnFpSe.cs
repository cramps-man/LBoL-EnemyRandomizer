using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyRandomizer
{
    public sealed class EndOfTurnFpSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId()
        {
            string id = this.GetType().Name;
            return id.Remove(id.Length - 3);
        }

        public override LocalizationOption LoadLocalization()
        {
            return BepinexPlugin.StatusEffectsBatchLoc.AddEntity(this);
        }

        public override Sprite LoadSprite()
        {
            var sprite = ResourceLoader.LoadSprite(GetId() + ".png", BepinexPlugin.embeddedSource);
            if (sprite == null)
            {
                return ResourceLoader.LoadSprite("dummyicon.png", BepinexPlugin.embeddedSource);
            }
            return sprite;
        }

        public override StatusEffectConfig MakeConfig()
        {
            return new StatusEffectConfig(
                Id: "",
                Index: 0,
                Order: 17,
                Type: StatusEffectType.Positive,
                IsVerbose: false,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: true,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: StackType.Add,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default",
                ImageId: null
            );
        }
    }

    [EntityLogic(typeof(EndOfTurnFpSeDef))]
    public sealed class EndOfTurnFpSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            ReactOwnerEvent(Battle.AllEnemyTurnEnded, this.OnAllEnemyTurnEnded);
        }

        private IEnumerable<BattleAction> OnAllEnemyTurnEnded(GameEventArgs args)
        {
            if (!Battle.BattleShouldEnd)
            {
                NotifyActivating();
                yield return new ApplyStatusEffectAction<Firepower>(Owner, Level);
                yield return new RemoveStatusEffectAction(this);
            }
        }
    }
}
