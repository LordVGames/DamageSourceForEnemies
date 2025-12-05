using BepInEx;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using RoR2;
using System;
using System.Reflection;
using AugmentedVoidReaverMod = DamageSourceForEnemies.ILHooks.Mods.AugmentedVoidReaverMod;

namespace DamageSourceForEnemies
{
    [BepInAutoPlugin]
    [BepInDependency(AugmentedVoidReaverMod.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(EnemiesPlus.EnemiesPlusPlugin.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    public partial class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            Log.Init(Logger);
            ConfigOptions.BindConfigEntries(Config);
            MonoDetourManager.InvokeHookInitializers(typeof(Plugin).Assembly, reportUnloadableTypes: false);
        }


#if DEBUG
        [MonoDetourTargets(typeof(CharacterMaster))]
        private static class DebugLogDamageSourceOnHit
        {
            [MonoDetourHookInitialize]
            internal static void Setup()
            {
                Mdh.RoR2.CharacterMaster.OnBodyDamaged.Postfix(LogDamageSource);
            }

            private static void LogDamageSource(CharacterMaster self, ref DamageReport damageReport)
            {
                Log.Debug($"damageType == {damageReport.damageInfo.damageType}");
            }
        }
#endif
    }
}