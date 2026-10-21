using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace DamageSourceForEnemies.Hooks;


/// <summary>
/// <para>
/// 
/// AKA Alloyed Collective
/// <br />
/// Thruster attack for Solus Amalgamator and Solus Extractor are handled in Generic.cs under BaseState_InitMeleeOverlap
/// <br />
/// Solus heart is completely unplayable when spawning in as them, so I can't really test any of these aside from making sure the hooks don't error out
/// <br />
/// I know the sources of damage for it's skills though because the errors from the skills themselves reveal their names
/// <br />
/// Solus distributor doesn't need hooks because it never directly attacks
/// 
/// </para>
/// </summary>
internal static class DLC3
{
    [MonoDetourTargets(typeof(EntityStates.FriendUnit.FinalSacrifice))]
    private static class BestBuddy
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.FriendUnit.FinalSacrifice.FixedUpdate.ILHook(FinalSacrifice_FixedUpdate);
        }

        private static void FinalSacrifice_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSpecial, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.DefectiveUnit.Denial))]
    [MonoDetourTargets(typeof(EntityStates.DefectiveUnit.DenialProjectile))]
    [MonoDetourTargets(typeof(EntityStates.DefectiveUnit.Detonate))]
    private static class SolusInvalidator
    {
        private static readonly AssetReferenceT<GameObject> _invalidatorProjectileImpactExplosion = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_DefectiveUnit.ArtilleryLandedChildProjectile_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.DefectiveUnit.Denial.FixedUpdate.ILHook(Denial_FixedUpdate);
            Mdh.EntityStates.DefectiveUnit.DenialProjectile.FireProjectile.ILHook(DenialProjectile_FireProjectile);
            Mdh.EntityStates.DefectiveUnit.Detonate.FixedUpdate.ILHook(Detonate_FixedUpdate);


            AssetAsyncReferenceManager<GameObject>.LoadAsset(_invalidatorProjectileImpactExplosion).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
            };
        }

        private static void Denial_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }

        private static void DenialProjectile_FireProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }

        private static void Detonate_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.IronHauler.Weapon.ShootProjectile))]
    private static class SolusTransporter
    {
        private static readonly AssetReferenceT<GameObject> _transporterGroundAttack = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_IronHauler.IronHaulerGravityWellProjectile_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            // this method uses the normal FireProjectile and the IL says it set a DamageSource as secondary, but the C# says it's null...
            // doesn't matter, the damagesource is handled by the projectile's ProjectileDamage anyways


            AssetAsyncReferenceManager<GameObject>.LoadAsset(_transporterGroundAttack).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
            };
        }

    }



    [MonoDetourTargets(typeof(EntityStates.SolusAmalgamator.FlamethrowerCannon))]
    [MonoDetourTargets(typeof(EntityStates.SolusAmalgamator.FlamethrowerTurret))]
    [MonoDetourTargets(typeof(EntityStates.SolusAmalgamator.ShockArmor))]
    [MonoDetourTargets(typeof(EntityStates.SolusAmalgamator.ArtilleryStrike))]
    private static class SolusAmalgamator
    {
        private static readonly AssetReferenceT<GameObject> _trackingBomb = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_SolusAmalgamator.SolusAmalgamatorTrackingBomb_prefab);
        private static readonly AssetReferenceT<GameObject> _groundAttack = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_SolusAmalgamator.SolusAmalgamatorLiftAoEVFX_prefab);


        [MonoDetourHookInitialize]
        private static void Setup()
        {
            // these 2 also affect the severed scorcher, but that's fine since the flamethrower is also a primary on that
            Mdh.EntityStates.SolusAmalgamator.FlamethrowerCannon.FireFlamethrower.ILHook(SetFlamethrowerDamageSource);
            Mdh.EntityStates.SolusAmalgamator.FlamethrowerTurret.FireFlamethrower.ILHook(SetFlamethrowerDamageSource);
            Mdh.EntityStates.SolusAmalgamator.ShockArmor.ApplyShock.ILHook(ShockArmor_ApplyShock);
            // idk when this is used but i'll give it a damagesource anyways
            Mdh.EntityStates.SolusAmalgamator.ArtilleryStrike.FireMissiles.ILHook(ArtilleryStrike_FireMissiles);


            AssetAsyncReferenceManager<GameObject>.LoadAsset(_trackingBomb).Completed += (handle) =>
            {
                // this also affects the missile pod body that fires this from it's primary, but whatever that's it's only skill anyways
                // it's detached from the boss that uses it as a secondary anyways so it still makes sense
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
            };
            AssetAsyncReferenceManager<GameObject>.LoadAsset(_groundAttack).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Special);
            };
        }

        private static void SetFlamethrowerDamageSource(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
        }

        private static void ShockArmor_ApplyShock(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
        }

        private static void ArtilleryStrike_FireMissiles(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.SolusHeart.DDOS))]
    [MonoDetourTargets(typeof(EntityStates.SolusHeart.FireOrbitalStrike))]
    [MonoDetourTargets(typeof(EntityStates.SolusHeart.Underclock))]
    [MonoDetourTargets(typeof(EntityStates.SolusHeart.GroundSlamState))]
    private static class SolusHeart
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.SolusHeart.DDOS.FireProjectile.ILHook(DDOS_FireProjectile);
            Mdh.EntityStates.SolusHeart.FireOrbitalStrike.ModifyFireProjectileInfo.Postfix(FireOrbitalStrike_ModifyFireProjectileInfo);
            Mdh.EntityStates.SolusHeart.Underclock.ModifyFireProjectileInfo.Postfix(Underclock_ModifyFireProjectileInfo);
            Mdh.EntityStates.SolusHeart.GroundSlamState.Detonate.ILHook(GroundSlamState_Detonate);
        }

        private static void DDOS_FireProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }

        private static void FireOrbitalStrike_ModifyFireProjectileInfo(EntityStates.SolusHeart.FireOrbitalStrike self, ref FireProjectileInfo fireProjectileInfo)
        {
            if (!fireProjectileInfo.damageTypeOverride.HasValue)
            {
                return;
            }

            DamageTypeCombo temp = fireProjectileInfo.damageTypeOverride.Value;
            temp.damageSource = DamageSource.Secondary;
            fireProjectileInfo.damageTypeOverride = temp;
        }

        private static void Underclock_ModifyFireProjectileInfo(EntityStates.SolusHeart.Underclock self, ref FireProjectileInfo fireProjectileInfo)
        {
            if (!fireProjectileInfo.damageTypeOverride.HasValue)
            {
                return;
            }

            DamageTypeCombo temp = fireProjectileInfo.damageTypeOverride.Value;
            temp.damageSource = DamageSource.Utility;
            fireProjectileInfo.damageTypeOverride = temp;
        }

        private static void GroundSlamState_Detonate(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Special, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.SolusMine.Detonate))]
    private static class SolusMine
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.SolusMine.Detonate.OnEnter.ILHook(Detonate_OnEnter);
        }

        private static void Detonate_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.SolusWing.GravPulse))]
    [MonoDetourTargets(typeof(EntityStates.SolusWing.ExpandingLaserBase))]
    [MonoDetourTargets(typeof(EntityStates.SolusWing.SuppressionFire))]
    private static class SolusWing
    {
        private static readonly AssetReferenceT<GameObject> _trackingShots = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_SolusWing.SolusWing_LaserBurstBlastProjectile_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.SolusWing.GravPulse.FireBlastAttack.ILHook(GravPulse_FireBlastAttack);
            Mdh.EntityStates.SolusWing.GravPulse.FireProjectile.ILHook(GravPulse_FireProjectile);
            Mdh.EntityStates.SolusWing.SuppressionFire.SpawnDamageFieldProjectile.ILHook(SuppressionFire_SpawnDamageFieldProjectile);
            Mdh.EntityStates.SolusWing.ExpandingLaserBase.DoDamage.ILHook(ExpandingLaserBase_DoDamage);


            AssetAsyncReferenceManager<GameObject>.LoadAsset(_trackingShots).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
            };
        }

        private static void GravPulse_FireBlastAttack(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
        }

        private static void GravPulse_FireProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }

        private static void SuppressionFire_SpawnDamageFieldProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericUtility, info);
        }

        private static void ExpandingLaserBase_DoDamage(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Special, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Tanker.Ignite))]
    [MonoDetourTargets(typeof(EntityStates.Tanker.Accelerant))]
    [MonoDetourTargets(typeof(EntityStates.Tanker.GreasePuddle.IgniteGrease))]
    private static class SolusScorcher
    {
        private static readonly AssetReferenceT<GameObject> _gooPuddleProjectile = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_Tanker.TankerAccelerantPuddleBodyProjectile_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Tanker.Ignite.FireFlamethrower.ILHook(Ignite_FireFlamethrower);
            Mdh.EntityStates.Tanker.Accelerant.FireGooProjectile.ILHook(Accelerant_FireGooProjectile);
            Mdh.EntityStates.Tanker.GreasePuddle.IgniteGrease.OnEnter.ILHook(IgniteGrease_OnEnter);


            AssetAsyncReferenceManager<GameObject>.LoadAsset(_gooPuddleProjectile).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
            };
        }

        private static void Ignite_FireFlamethrower(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
        }

        private static void Accelerant_FireGooProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }

        private static void IgniteGrease_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.WorkerUnit.FireDrillDash))]
    private static class SolusProspector
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.WorkerUnit.FireDrillDash.OnEnter.ILHook(FireDrillDash_OnEnter);
        }

        private static void FireDrillDash_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.OverrideDamageTypeCombo(DamageSource.Primary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.VultureHunter.Body.BombingRun))]
    [MonoDetourTargets(typeof(EntityStates.VultureHunter.Body.BombingRunLaser))]
    [MonoDetourTargets(typeof(EntityStates.VultureHunter.Body.ThrowSpear))]
    [MonoDetourTargets(typeof(EntityStates.VultureHunter.Body.ThrowTeleportSpear))]
    [MonoDetourTargets(typeof(EntityStates.VultureHunter.Weapon.FireSolusLaser))]
    [MonoDetourTargets(typeof(EntityStates.VultureHunter.Weapon.Calldown))]
    private static class AlloyHunter
    {
        private static readonly AssetReferenceT<GameObject> _bombingRunExplosionProjectile = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_VultureHunter.BombingRunDelayedExplosionProjectile_prefab);
        private static readonly AssetReferenceT<GameObject> _spearExplosionProjectile = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC3_VultureHunter.SpearImpactDelayedExplosionProjectile_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.VultureHunter.Body.BombingRun.BombingRunFixedUpdate.ILHook(BombingRun_BombingRunFixedUpdate);
            Mdh.EntityStates.VultureHunter.Body.BombingRunLaser.FireLaserProjectile.ILHook(BombingRunLaser_FireLaserProjectile);
            Mdh.EntityStates.VultureHunter.Body.ThrowSpear.ModifyProjectileInfo.Postfix(ThrowSpear_ModifyProjectileInfo);
            Mdh.EntityStates.VultureHunter.Body.ThrowTeleportSpear.FireSpearProjectile.ILHook(ThrowTeleportSpear_FireSpearProjectile);
            Mdh.EntityStates.VultureHunter.Weapon.FireSolusLaser.FireBullet.ILHook(FireSolusLaser_FireBullet);
            Mdh.EntityStates.VultureHunter.Weapon.Calldown.FireProjectile.ILHook(Calldown_FireProjectile);


            AssetAsyncReferenceManager<GameObject>.LoadAsset(_bombingRunExplosionProjectile).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
            };
            AssetAsyncReferenceManager<GameObject>.LoadAsset(_spearExplosionProjectile).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Utility);
            };
        }

        private static void BombingRun_BombingRunFixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }

        private static void BombingRunLaser_FireLaserProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }

        private static void ThrowSpear_ModifyProjectileInfo(EntityStates.VultureHunter.Body.ThrowSpear self, ref FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.damageTypeOverride.HasValue)
            {
                fireProjectileInfo.damageTypeOverride = DamageTypeCombo.GenericUtility;
            }
            else
            {
                DamageTypeCombo temp = fireProjectileInfo.damageTypeOverride.Value;
                temp.damageSource = DamageSource.Utility;
                fireProjectileInfo.damageTypeOverride = temp;
            }
        }

        private static void ThrowTeleportSpear_FireSpearProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericUtility, info);
        }

        private static void FireSolusLaser_FireBullet(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
        }

        private static void Calldown_FireProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }
    }
}