using BepInEx.Configuration;
using MiscFixes.Modules;
namespace DamageSourceForEnemies;


public static class ConfigOptions
{
    public static ConfigEntry<bool> MushroomDamageZoneDamageSource;
    public static ConfigEntry<bool> BeetleQueenDamageZoneDamageSource;
    public static ConfigEntry<bool> AlloyWorshipUnitDamageZoneDamageSource;
    public static ConfigEntry<bool> VoidlingDamageZoneDamageSource;
    public static ConfigEntry<bool> ScorchWurmDamageZoneDamageSource;
    public static ConfigEntry<bool> WormContactDamageSources;


    internal static void BindConfigEntries(ConfigFile Config)
    {
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
        WormContactDamageSources = Config.BindOption<bool>(
            "Adding DamageSources",
            "Give worm contact damage a DamageSource",
            "Worm contact damage is not tied to a skill, but if you want it to still have a DamageSource (primary in this case) then enable this.",
            true, Extensions.ConfigFlags.RestartRequired
        );
    }
}