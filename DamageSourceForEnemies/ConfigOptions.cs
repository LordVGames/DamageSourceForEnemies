using BepInEx.Configuration;

namespace DamageSourceForEnemies
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> EnableMushroomDamageZoneDamageSource;
        public static ConfigEntry<bool> EnableBeetleQueenDamageZoneDamageSource;
        public static ConfigEntry<bool> EnableVoidlingDamageZoneDamageSource;

        internal static void BindConfigEntries(ConfigFile Config)
        {
            EnableMushroomDamageZoneDamageSource = Config.Bind<bool>(
                "Ground damage zones",
                "Mini Mushrum", true,
                "Should the damage zone left behind by the mini mushrum's spore attack have a damage source (Primary) set?"
            );
            EnableBeetleQueenDamageZoneDamageSource = Config.Bind<bool>(
                "Ground damage zones",
                "Beetle Queen", false,
                "Should the damage zones left behind by the beetle queen's spit attack have a damage source (Primary) set?"
            );
            EnableVoidlingDamageZoneDamageSource = Config.Bind<bool>(
                "Ground damage zones",
                "Voidling", false,
                "Should the damage zone left behind by the voidling's big multi-laser attack have a damage source (Secondary) set?"
            );
        }
    }
}