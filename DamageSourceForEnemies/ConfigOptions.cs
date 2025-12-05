using BepInEx.Configuration;
using MiscFixes.Modules;

namespace DamageSourceForEnemies
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> GiveDronesDamageSources;

        public static ConfigEntry<bool> MushroomDamageZoneDamageSource;
        public static ConfigEntry<bool> BeetleQueenDamageZoneDamageSource;
        public static ConfigEntry<bool> AlloyWorshipUnitDamageZoneDamageSource;
        public static ConfigEntry<bool> VoidlingDamageZoneDamageSource;
        public static ConfigEntry<bool> ScorchWurmDamageZoneDamageSource;

        internal static void BindConfigEntries(ConfigFile Config)
        {
            GiveDronesDamageSources = Config.BindOption<bool>(
                "Adding DamageSources",
                "Give drones DamageSources",
                "In case you want the drones to not have DamageSources due to possibly being OP with operator, use this config option.",
                true, Extensions.ConfigFlags.RestartRequired
            );


            MushroomDamageZoneDamageSource = Config.BindOption<bool>(
                "Damage zones from skills",
                "Mini Mushrum",
                "Should the damage zone left behind by the Mini Mushrum's spore attack have a damage source (Primary) set?",
                true
            );
            BeetleQueenDamageZoneDamageSource = Config.BindOption<bool>(
                "Damage zones from skills",
                "Beetle Queen",
                "Should the damage zones left behind by the Beetle Queen's spit attack have a damage source (Primary) set?",
                true
            );
            AlloyWorshipUnitDamageZoneDamageSource = Config.BindOption<bool>(
                "Damage zones from skills",
                "Alloy Worship Unit",
                "Should the damage zones left behind by the Alloy Worship Unit's projectiles have a damage source (Primary) set?",
                true
            );
            VoidlingDamageZoneDamageSource = Config.BindOption<bool>(
                "Damage zones from skills",
                "Voidling",
                "Should the damage zone left behind by the Voidling's big multi-laser attack have a damage source (Secondary) set?",
                true
            );
            ScorchWurmDamageZoneDamageSource = Config.BindOption<bool>(
                "Damage zones from skills",
                "Scorch Wurm",
                "Should the damage zone left behind by the Scorch Wurm's lava bomb attack have a damage source (Secondary) set?",
                true
            );

            Config.WipeConfig();
        }
    }
}