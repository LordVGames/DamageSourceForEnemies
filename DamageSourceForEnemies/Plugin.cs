using BepInEx;
using R2API.Utils;

namespace DamageSourceForEnemies
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "DamageSourceForEnemies";
        public const string PluginVersion = "1.1.0";
        public void Awake()
        {
            Log.Init(Logger);
            ConfigOptions.BindConfigEntries(Config);
            AssetEdits.EditAssetsBasedOnConfig();
            ILHooks.SetupILHooks();
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