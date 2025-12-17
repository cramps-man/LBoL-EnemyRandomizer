using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.CustomHandlers;
using System;
using System.Collections.Generic;

namespace EnemyRandomizer
{
    internal static class EnemyHiddenDamageHandler
    {
        public static void RegisterHandlers()
        {
            CHandlerManager.RegisterBattleEventHandler(b => b.BattleStarting, OnBattleStarted);
            CHandlerManager.RegisterBattleEventHandler(b => b.Player.StatusEffectAdding, OnPlayerStatusEffectAdding);
        }

        private static void OnPlayerStatusEffectAdding(StatusEffectApplyEventArgs args)
        {
            if (!GameMaster.Instance.CurrentGameRun.HasJadeBox<EnemyRandomizerJadebox>())
                return;
            var battle = GameMaster.Instance.CurrentGameRun.Battle;
            if (battle == null)
                return;
            if (!(args.Effect is LockedOn))
                return;
            if (!(args.ActionSource is Star s))
                return;

            args.ForceCancelBecause(CancelCause.Reaction);
            battle.React(new Reactor(new ApplyStatusEffectAction<EnemyLockedOn>(s, args.Level)), null, ActionCause.None);
        }

        private static void OnBattleStarted(GameEventArgs args)
        {
            if (!GameMaster.Instance.CurrentGameRun.HasJadeBox<EnemyRandomizerJadebox>())
                return;
            var battle = GameMaster.Instance.CurrentGameRun.Battle;
            if (battle == null)
                return;

            foreach (var enemy in battle.AllAliveEnemies)
            {
                RegisterEventHandler<StatusEffectApplyEventArgs, BattleController>(b => enemy.StatusEffectAdding, OnEnemyStatusAdding, GameEventPriority.ConfigDefault, UniqueTracker.Instance.cHandlerManager.battleEventHandlers).RegisterHandler(battle);
                //CHandlerManager.RegisterBattleEventHandler(b => enemy.StatusEffectAdding, OnEnemyStatusAdding);
            }
            RegisterEventHandler<UnitEventArgs, BattleController>(b => b.EnemySpawned, OnEnemySpawned, GameEventPriority.ConfigDefault, UniqueTracker.Instance.cHandlerManager.battleEventHandlers).RegisterHandler(battle);
            //CHandlerManager.RegisterBattleEventHandler(b => b.EnemySpawned, OnEnemySpawned);
        }

        internal static HandlerHolder<T, PT> RegisterEventHandler<T, PT>(EventProvider<T, PT> eventProvider, GameEventHandler<T> handler, GameEventPriority priority, Dictionary<Type, HashSet<IHandleHolder>> handlers) where T : GameEventArgs where PT : class
        {
            handlers.TryAdd(typeof(T), new HashSet<IHandleHolder>());
            HandlerHolder<T, PT> handlerHolder = new HandlerHolder<T, PT>
            {
                eventProvider = eventProvider,
                handler = handler,
                priority = priority
            };
            if (!handlers[typeof(T)].Add(handlerHolder))
            {
                BepinexPlugin.log.LogWarning($"Custom handler (event holder: {typeof(PT)}, args: {typeof(T)}) was already registered thus not registered again.");
            }

            return handlerHolder;
        }

        private static void OnEnemySpawned(UnitEventArgs args)
        {
            var battle = GameMaster.Instance.CurrentGameRun.Battle;
            if (battle == null)
                return;
            RegisterEventHandler<StatusEffectApplyEventArgs, BattleController>(b => args.Unit.StatusEffectAdding, OnEnemyStatusAdding, GameEventPriority.ConfigDefault, UniqueTracker.Instance.cHandlerManager.battleEventHandlers).RegisterHandler(battle);
            //CHandlerManager.RegisterBattleEventHandler(b => args.Unit.StatusEffectAdding, OnEnemyStatusAdding);
        }

        private static void OnEnemyStatusAdding(StatusEffectApplyEventArgs args)
        {
            var battle = GameMaster.Instance.CurrentGameRun.Battle;
            if (battle == null)
                return;
            if (!(args.Effect is Firepower))
                return;
            if (args.ActionSource is EndOfTurnFpSe)
                return;
            args.ForceCancelBecause(CancelCause.Reaction);
            battle.React(new Reactor(EndOfTurnFp(args)), null, ActionCause.None);
        }
        private static IEnumerable<BattleAction> EndOfTurnFp(StatusEffectApplyEventArgs args)
        {
            yield return new ApplyStatusEffectAction<EndOfTurnFpSe>(args.Unit, args.Level);
        }
    }
}
