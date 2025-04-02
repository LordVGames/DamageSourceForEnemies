using BepInEx;
using RoR2;
using R2API.Utils;
using HarmonyLib;

namespace DamageSourceForEnemies
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(ModSupport.AugmentedVoidReaverMod.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "DamageSourceForEnemies";
        public const string PluginVersion = "1.2.1";
        public void Awake()
        {
            Log.Init(Logger);
            ConfigOptions.BindConfigEntries(Config);
            AssetEdits.EditAssetsBasedOnConfig();
            ILHooks.SetupILHooks();
            if (ModSupport.AugmentedVoidReaverMod.ModIsRunning)
            {
                Harmony harmony = new(PluginGUID);
                harmony.CreateClassProcessor(typeof(ModSupport.AugmentedVoidReaverMod.HarmonyPatches)).Patch();
            }
#if DEBUG
            On.RoR2.CharacterMaster.OnBodyDamaged += CharacterMaster_OnBodyDamaged;
#endif
        }

#if DEBUG
        private void CharacterMaster_OnBodyDamaged(On.RoR2.CharacterMaster.orig_OnBodyDamaged orig, CharacterMaster self, DamageReport damageReport)
        {
            orig(self, damageReport);
            Log.Debug($"damageSource == {damageReport.damageInfo.damageType.damageSource}");
        }
#endif
    }
}