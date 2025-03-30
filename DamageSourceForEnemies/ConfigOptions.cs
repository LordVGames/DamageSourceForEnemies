using BepInEx.Configuration;

namespace DamageSourceForEnemies
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> MushroomDamageZoneDamageSource;
        public static ConfigEntry<bool> BeetleQueenDamageZoneDamageSource;
        public static ConfigEntry<bool> VoidlingDamageZoneDamageSource;
        public static ConfigEntry<bool> ScorchWurmDamageZoneDamageSource;

        internal static void BindConfigEntries(ConfigFile Config)
        {
            MushroomDamageZoneDamageSource = Config.Bind<bool>(
                "Damage zones from skills",
                "Mini Mushrum", true,
                "Should the damage zone left behind by the mini mushrum's spore attack have a damage source (Primary) set?"
            );
            BeetleQueenDamageZoneDamageSource = Config.Bind<bool>(
                "Damage zones from skills",
                "Beetle Queen", false,
                "Should the damage zones left behind by the beetle queen's spit attack have a damage source (Primary) set?"
            );
            VoidlingDamageZoneDamageSource = Config.Bind<bool>(
                "Damage zones from skills",
                "Voidling", false,
                "Should the damage zone left behind by the voidling's big multi-laser attack have a damage source (Secondary) set?"
            );
            ScorchWurmDamageZoneDamageSource = Config.Bind<bool>(
                "Damage zones from skills",
                "Scorch Wurm", true,
                "Should the damage zone left behind by the Scorch Wurm's lava bomb attack have a damage source (Secondary) set?"
            );
        }
    }
}