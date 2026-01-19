using LBoL.Core;
using LBoLEntitySideloader.PersistentValues;
using System;
using System.Collections.Generic;

namespace EnemyRandomizer
{
    public sealed class PreviousEncounterSaveData : CustomGameRunSaveData
    {
        public List<Type> previousEncounter = new List<Type>();
        public List<List<Type>> allSeenEncounters = new List<List<Type>>();
        public override string Name => GetType().Name;
        public override void Restore(GameRunController gameRun)
        {
            EnemyRandomizer.previousEncounter.Clear();
            EnemyRandomizer.previousEncounter.AddRange(previousEncounter);
            EnemyRandomizer.allSeenEncounters.Clear();
            EnemyRandomizer.allSeenEncounters.AddRange(allSeenEncounters);
        }

        public override void Save(GameRunController gameRun)
        {
            previousEncounter.Clear();
            previousEncounter.AddRange(EnemyRandomizer.previousEncounter);
            allSeenEncounters.Clear();
            allSeenEncounters.AddRange(EnemyRandomizer.allSeenEncounters);
        }

        public override void OnGamerunEnded()
        {
            EnemyRandomizer.previousEncounter.Clear();
            EnemyRandomizer.allSeenEncounters.Clear();
        }
    }
}
