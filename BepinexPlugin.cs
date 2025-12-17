using BepInEx;
using HarmonyLib;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace EnemyRandomizer
{
    [BepInPlugin(global::EnemyRandomizer.PInfo.GUID, global::EnemyRandomizer.PInfo.Name, global::EnemyRandomizer.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = global::EnemyRandomizer.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());
        internal static DirectorySource directorySource = new DirectorySource(global::EnemyRandomizer.PInfo.GUID, "");

        internal static BatchLocalization jadeboxLoc = new BatchLocalization(directorySource, typeof(JadeBoxTemplate), "jadebox");

        internal static string CUSTOM_EIGHT_FORMATION = "8custom";
        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            EntityManager.RegisterSelf();

            harmony.PatchAll();

            EnemyGroupTemplate.AddFormation(CUSTOM_EIGHT_FORMATION, new Dictionary<int, Vector2>()
            {
                {0, new Vector2(-1,2) },
                {1, new Vector2(-1,-1) },
                {2, new Vector2(0.5f,0.5f) },
                {3, new Vector2(2,2) },
                {4, new Vector2(2,-1) },
                {5, new Vector2(3.5f,0.5f) },
                {6, new Vector2(5,2) },
                {7, new Vector2(5,-1) },
            });
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }
    }
}
