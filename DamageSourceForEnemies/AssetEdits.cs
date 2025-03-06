using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2;
using RoR2.Projectile;

namespace DamageSourceForEnemies
{
    internal static class AssetEdits
    {
        internal static void EditAssetsBasedOnConfig()
        {
            EditMushroomSporeDotZone();
            EditBeetleQueenAcidDotZone();
            EditVoidlingMultiLaserDotZone();
        }

        private static void EditMushroomSporeDotZone()
        {
            if (!ConfigOptions.EnableMushroomDamageZoneDamageSource.Value)
            {
                return;
            }

            GameObject mushroomSporeDamageZone = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectileDotZone.prefab").WaitForCompletion();
            ProjectileDamage mushroomSporeProjectileDamage = mushroomSporeDamageZone.GetComponent<ProjectileDamage>();
            mushroomSporeProjectileDamage.damageType.damageSource = DamageSource.Primary;
        }

        private static void EditBeetleQueenAcidDotZone()
        {
            if (!ConfigOptions.EnableBeetleQueenDamageZoneDamageSource.Value)
            {
                return;
            }

            GameObject beetleQueenAcidDamageZone = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleQueenAcid.prefab").WaitForCompletion();
            ProjectileDamage beetleQueenAcidProjectileDamage = beetleQueenAcidDamageZone.GetComponent<ProjectileDamage>();
            beetleQueenAcidProjectileDamage.damageType.damageSource = DamageSource.Primary;
        }

        private static void EditVoidlingMultiLaserDotZone()
        {
            if (!ConfigOptions.EnableVoidlingDamageZoneDamageSource.Value)
            {
                return;
            }

            GameObject voidlingMultiBeamDamageZone = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab").WaitForCompletion();
            ProjectileDamage voidlingMultiBeamProjectileDamage = voidlingMultiBeamDamageZone.GetComponent<ProjectileDamage>();
            voidlingMultiBeamProjectileDamage.damageType.damageSource = DamageSource.Secondary;
        }
    }
}