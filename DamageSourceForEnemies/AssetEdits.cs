using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;
using RoR2.Projectile;
using BepInEx.Configuration;

namespace DamageSourceForEnemies
{
    internal static class AssetEdits
    {
        internal static void EditAssetsBasedOnConfig()
        {
            // mini mushrum shot
            SetDamageZoneAssetDamageSource(
                "RoR2/Base/MiniMushroom/SporeGrenadeProjectileDotZone.prefab",
                ConfigOptions.MushroomDamageZoneDamageSource,
                DamageSource.Primary
            );

            // beetle queen spit
            SetDamageZoneAssetDamageSource(
                "RoR2/Base/Beetle/BeetleQueenAcid.prefab",
                ConfigOptions.BeetleQueenDamageZoneDamageSource,
                DamageSource.Primary
            );

            // voidling multi-shot laser (not the one that shoots plasma-shrimp-like projectiles)
            SetDamageZoneAssetDamageSource(
                "RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab",
                ConfigOptions.VoidlingDamageZoneDamageSource,
                DamageSource.Secondary
            );

            // scorch wurm shot
            SetDamageZoneAssetDamageSource(
                "RoR2/DLC2/Scorchling/LavaBombHeatOrbProjectile.prefab",
                ConfigOptions.ScorchWurmDamageZoneDamageSource,
                DamageSource.Secondary
            );

        }

        private static void SetDamageZoneAssetDamageSource(string assetPath, ConfigEntry<bool> associatedConfigEntry, DamageSource damageSource)
        {
            if (!associatedConfigEntry.Value)
            {
                return;
            }

            GameObject damageZone = Addressables.LoadAssetAsync<GameObject>(assetPath).WaitForCompletion();
            ProjectileDamage projectileDamage = damageZone.GetComponent<ProjectileDamage>();
            projectileDamage.damageType.damageSource = damageSource;
        }
    }
}