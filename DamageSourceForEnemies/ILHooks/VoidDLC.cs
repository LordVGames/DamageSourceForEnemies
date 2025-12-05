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
using UnityEngine.UIElements.StyleSheets;

namespace DamageSourceForEnemies.ILHooks
{
    // gup is handled in Generic.cs under SetMeleeAttackDamageSource
    internal static class VoidDLC
    {
        [MonoDetourTargets(typeof(EntityStates.AcidLarva.LarvaLeap), GenerateControlFlowVariants = true)]
        private static class Larva
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.AcidLarva.LarvaLeap.GetBlastDamageType.ControlFlowPrefix(ReplaceDamageSource);
            }

            private static ReturnFlow ReplaceDamageSource(EntityStates.AcidLarva.LarvaLeap self, ref DamageTypeCombo returnValue)
            {
                // bruh
                returnValue = DamageTypeCombo.GenericPrimary;
                return ReturnFlow.SkipOriginal;
            }
        }



        [MonoDetourTargets(typeof(EntityStates.ClayGrenadier.FaceSlam))]
        // the tar throw is handled in Generic.cs under EditAndReturnFireProjectileInfo
        private static class ClayApothecary
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.ClayGrenadier.FaceSlam.FixedUpdate.ILHook(FaceSlam_FixedUpdate);
            }

            private static void FaceSlam_FixedUpdate(ILManipulationInfo info)
            {
                ILWeaver w = new(info);

                ILHelpers.OverrideNextDamageInfoDamageSource(DamageSource.Primary, w);
                ILHelpers.BlastAttacks.OverrideNextBlastAttackDamageSource(DamageSource.Primary, w);
                ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, w);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.FlyingVermin.Weapon.Spit))]
        private static class BlindPest
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.FlyingVermin.Weapon.Spit.FireProjectile.ILHook(Spit_FireProjectile);
            }

            private static void Spit_FireProjectile(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.Vermin.Weapon.TongueLash))]
        private static class BlindVermin
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.Vermin.Weapon.TongueLash.AuthorityModifyOverlapAttack.Postfix(TongueLash_AuthorityModifyOverlapAttack);
            }

            private static void TongueLash_AuthorityModifyOverlapAttack(EntityStates.Vermin.Weapon.TongueLash self, ref OverlapAttack overlapAttack)
            {
                overlapAttack.damageType.damageSource = DamageSource.Primary;
            }
        }



        // alpha construct is handled in Generic.cs under EditAndReturnFireProjectileInfo



        [MonoDetourTargets(typeof(EntityStates.MajorConstruct.Weapon.FireLaser))]
        [MonoDetourTargets(typeof(EntityStates.MajorConstruct.Weapon.TerminateLaser))]
        private static class ConstructBosses
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.MajorConstruct.Weapon.FireLaser.ModifyBullet.Postfix(FireLaser_ModifyBullet);
                Mdh.EntityStates.MajorConstruct.Weapon.TerminateLaser.OnEnter.ILHook(TerminateLaser_OnEnter);
            }

            private static void FireLaser_ModifyBullet(EntityStates.MajorConstruct.Weapon.FireLaser self, ref BulletAttack bulletAttack)
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            }

            private static void TerminateLaser_OnEnter(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.VoidInfestor.Infest))]
        private static class VoidInfestor
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.VoidInfestor.Infest.OnEnter.ILHook(Infest_OnEnter);
            }

            private static void Infest_OnEnter(ILManipulationInfo info)
            {
                ILHelpers.OverlapAttacks.AddDamageSourceToOverlapAttack(DamageSource.Primary, info);
            }
        }



        // void jailer shotgun shot is handled in Generic.cs under EditAndReturnFireProjectileInfo
        [MonoDetourTargets(typeof(EntityStates.VoidJailer.Weapon.Capture2))]
        [MonoDetourTargets(typeof(JailerTetherController))]
        private static class VoidJailer
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.VoidJailer.Weapon.Capture2.OnEnter.ILHook(Capture2_OnEnter);
                Mdh.RoR2.JailerTetherController.DoDamageTick.ILHook(JailerTetherController_DoDamageTick);
            }

            private static void Capture2_OnEnter(ILManipulationInfo info)
            {
                ILHelpers.OverrideDamageInfoDamageSource(DamageSource.Secondary, info);
            }

            private static void JailerTetherController_DoDamageTick(ILManipulationInfo info)
            {
                ILHelpers.OverrideDamageInfoDamageSource(DamageSource.Secondary, info);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.VoidMegaCrab.Weapon.FireCrabCannonBase))]
        [MonoDetourTargets(typeof(MegacrabProjectileController))]
        [MonoDetourTargets(typeof(EntityStates.VoidMegaCrab.BackWeapon.FireVoidMissiles))]
        private static class VoidDevastator
        {
            private static readonly AssetReferenceT<GameObject> _devastatorStickyBomb = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_DLC1_VoidMegaCrab.MegaCrabWhiteCannonStuckProjectile_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.VoidMegaCrab.Weapon.FireCrabCannonBase.FireProjectile.ILHook(FireCrabCannonBase_FireProjectile);
                Mdh.RoR2.Projectile.MegacrabProjectileController.OnDestroy.ILHook(MegacrabProjectileController_OnDestroy);
                Mdh.EntityStates.VoidMegaCrab.BackWeapon.FireVoidMissiles.FireMissile.ILHook(FireVoidMissiles_FireMissile);


                AssetAsyncReferenceManager<GameObject>.LoadAsset(_devastatorStickyBomb).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Secondary;
                    AssetAsyncReferenceManager<GameObject>.UnloadAsset(_devastatorStickyBomb);
                };
            }

            private static void FireCrabCannonBase_FireProjectile(ILManipulationInfo info)
            {
                ILWeaver w = new(info);

                w.MatchRelaxed(
                    x => x.MatchCallOrCallvirt<ProjectileManager>("FireProjectileWithoutDamageType") && w.SetCurrentTo(x)
                ).ThrowIfFailure();
                w.InsertBeforeCurrent(
                    w.Create(OpCodes.Ldarg_0),
                    w.CreateDelegateCall((EntityStates.VoidMegaCrab.Weapon.FireCrabCannonBase fireCrabCannonBase) =>
                    {
                        DamageTypeCombo damageTypeCombo = DamageTypeCombo.Generic;
                        switch (fireCrabCannonBase)
                        {
                            case EntityStates.VoidMegaCrab.Weapon.FireCrabWhiteCannon:
                                damageTypeCombo = DamageTypeCombo.GenericPrimary;
                                break;
                            case EntityStates.VoidMegaCrab.Weapon.FireCrabBlackCannon:
                                damageTypeCombo = DamageTypeCombo.GenericSecondary;
                                break;
                        }
                        return new DamageTypeCombo?(damageTypeCombo);
                    })
                );
                w.ReplaceCurrent(
                    w.Create<ProjectileManager>(OpCodes.Callvirt, "FireProjectile")
                );
            }

            private static void MegacrabProjectileController_OnDestroy(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
            }

            private static void FireVoidMissiles_FireMissile(ILManipulationInfo info)
            {
                ILWeaver w = new(info);

                ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, w);
                ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, w);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.VoidRaidCrab.Weapon.FireMissiles))]
        [MonoDetourTargets(typeof(EntityStates.VoidRaidCrab.Weapon.BaseFireMultiBeam))]
        [MonoDetourTargets(typeof(EntityStates.VoidRaidCrab.SpinBeamAttack))]
        private static class Voidling
        {
            private static readonly AssetReferenceT<GameObject> _laserShotGroundDamageZone = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_DLC1_VoidRaidCrab.VoidRaidCrabMultiBeamDotZone_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.VoidRaidCrab.Weapon.FireMissiles.FixedUpdate.ILHook(FireMissiles_FixedUpdate);
                Mdh.EntityStates.VoidRaidCrab.Weapon.BaseFireMultiBeam.OnEnter.ILHook(BaseFireMultiBeam_OnEnter);
                Mdh.EntityStates.VoidRaidCrab.SpinBeamAttack.FireBeamBulletAuthority.ILHook(SpinBeamAttack_FireBeamBulletAuthority);


                if (ConfigOptions.VoidlingDamageZoneDamageSource.Value)
                {
                    AssetAsyncReferenceManager<GameObject>.LoadAsset(_laserShotGroundDamageZone).Completed += (handle) =>
                    {
                        handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Secondary;
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(_laserShotGroundDamageZone);
                    };
                }
            }

            private static void FireMissiles_FixedUpdate(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
            }

            private static void BaseFireMultiBeam_OnEnter(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
            }

            private static void SpinBeamAttack_FireBeamBulletAuthority(ILManipulationInfo info)
            {
                ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Utility, info);
            }
        }
    }
}