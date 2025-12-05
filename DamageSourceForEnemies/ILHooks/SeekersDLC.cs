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

namespace DamageSourceForEnemies.ILHooks
{
    internal static class SeekersDLC
    {
        [MonoDetourTargets(typeof(EntityStates.ChildMonster.SparkBallFire))]
        private static class ChildMonster
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.ChildMonster.SparkBallFire.FireBomb.ILHook(SparkBallFire_FireBomb);
            }

            private static void SparkBallFire_FireBomb(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
            }
        }



        private static class FalseSonBoss
        {
            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.FalseSonBossGenericStateWithSwing))]
            private static class FalseSonBossGenericStateWithSwing
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.FalseSonBoss.FalseSonBossGenericStateWithSwing.GenerateClubOverlapAttack.Postfix(FalseSonBossGenericStateWithSwing_GenerateClubOverlapAttack);
                }

                private static void FalseSonBossGenericStateWithSwing_GenerateClubOverlapAttack(ref GameObject attacker, ref float inDamageStat, ref HitBoxGroup hitBoxGroup, ref bool isCrit, ref TeamIndex team, ref float pushAwayForceOverride, ref OverlapAttack returnValue)
                {
                    returnValue.damageType.damageSource = DamageSource.Primary;
                }
            }


            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.FissureSlam))]
            private static class FissureSlam
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.FalseSonBoss.FissureSlam.DetonateAuthority.ILHook(FissureSlam_DetonateAuthority);
                    Mdh.EntityStates.FalseSonBoss.FissureSlam.FixedUpdate.ILHook(FissureSlam_FixedUpdate);
                }

                private static void FissureSlam_DetonateAuthority(ILManipulationInfo info)
                {
                    ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
                }

                private static void FissureSlam_FixedUpdate(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.LunarRain))]
            private static class LunarRain
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.FalseSonBoss.LunarRain.DetonateAuthority.ILHook(LunarRain_DetonateAuthority);
                }

                private static void LunarRain_DetonateAuthority(ILManipulationInfo info)
                {
                    ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.CorruptedPaths))]
            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.CorruptedPathsDash))]
            private static class CorruptedPaths
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.FalseSonBoss.CorruptedPaths.DetonateAuthority.ILHook(CorruptedPaths_DetonateAuthority);
                    Mdh.EntityStates.FalseSonBoss.CorruptedPathsDash.FixedUpdate.ILHook(CorruptedPathsDash_FixedUpdate);
                }

                private static void CorruptedPaths_DetonateAuthority(ILManipulationInfo info)
                {
                    ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
                }

                private static void CorruptedPathsDash_FixedUpdate(ILManipulationInfo info)
                {
                    ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.TaintedOffering))]
            private static class TaintedOffering
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.FalseSonBoss.TaintedOffering.FireProjectile.ILHook(TaintedOffering_FireProjectile);
                }

                private static void TaintedOffering_FireProjectile(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.FalseSonBoss.PrimeDevastator))]
            private static class PrimeDevastator
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.FalseSonBoss.PrimeDevastator.DetonateAuthority.ILHook(PrimeDevastator_DetonateAuthority);
                }

                private static void PrimeDevastator_DetonateAuthority(ILManipulationInfo info)
                {
                    ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Special, info);
                }
            }
        }



        // Halcyonite.GoldenSwipe and Halcyonite.GoldenSlash are handled in Generic.cs under SetMeleeAttackDamageSource
        [MonoDetourTargets(typeof(EntityStates.Halcyonite.TriLaser))]
        [MonoDetourTargets(typeof(EntityStates.Halcyonite.WhirlWindPersuitCycle))]
        private static class Halcyonite
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.Halcyonite.TriLaser.FireTriLaser.ILHook(TriLaser_FireTriLaser);
                Mdh.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateAttack.ILHook(WhirlWindPersuitCycle_UpdateAttack);
            }

            private static void TriLaser_FireTriLaser(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
            }

            private static void WhirlWindPersuitCycle_UpdateAttack(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.Scorchling.ScorchlingLavaBomb))]
        [MonoDetourTargets(typeof(EntityStates.Scorchling.ScorchlingBreach))]
        private static class ScorchWurm
        {
            private static readonly AssetReferenceT<GameObject> _scorchWurmDamageZone = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_DLC2_Scorchling.LavaBombHeatOrbProjectile_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.Scorchling.ScorchlingLavaBomb.Spit.ILHook(ScorchlingLavaBomb_Spit);
                Mdh.EntityStates.Scorchling.ScorchlingBreach.DetonateAuthority.ILHook(ScorchlingBreach_DetonateAuthority);


                if (ConfigOptions.ScorchWurmDamageZoneDamageSource.Value)
                {
                    AssetAsyncReferenceManager<GameObject>.LoadAsset(_scorchWurmDamageZone).Completed += (handle) =>
                    {
                        handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Secondary;
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(_scorchWurmDamageZone);
                    };
                }
            }

            private static void ScorchlingLavaBomb_Spit(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
            }

            private static void ScorchlingBreach_DetonateAuthority(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
            }
        }
    }
}