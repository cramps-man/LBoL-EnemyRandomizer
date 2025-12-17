using LBoL.Core;
using LBoLEntitySideloader.PersistentValues;
using System;
using System.Collections.Generic;

namespace EnemyRandomizer
{
    public sealed class PreviousEncounterSaveData : CustomGameRunSaveData
    {
        public List<Type> previousEncounter = new List<Type>();
        public override string Name => GetType().Name;
        public override void Restore(GameRunController gameRun)
        {
            EnemyRandomizer.previousEncounter.Clear();
            EnemyRandomizer.previousEncounter.AddRange(previousEncounter);
        }

        public override void Save(GameRunController gameRun)
        {
            previousEncounter.Clear();
            previousEncounter.AddRange(EnemyRandomizer.previousEncounter);
        }
    }
}
