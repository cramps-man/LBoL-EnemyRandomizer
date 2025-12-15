using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
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

    }
}
