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
using HarmonyLib;
using EntityStates.ClayBruiser.Weapon;
using UnityEngine.Networking;
using EntityStates.BrotherMonster;
using EntityStates;
namespace DamageSourceForEnemies.Hooks;


/// <summary>
/// <para>
/// 
/// Lunar exploder is handled in Generic.cs under EditAndReturnFireProjectileInfo
/// <br />
/// Aurelionite's fist attack extends the normal one, including the DamageSource
/// <br />
/// Aurelionite's rock orb also extends the normal one
/// <br />
/// Her laser does too but we still need to hook it for the extra projectiles shot
/// 
/// </para>
/// </summary>
internal static class BaseGame
{
    [MonoDetourTargets(typeof(EntityStates.ArtifactShell.FireSolarFlares))]
    private static class ArtifactShell
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.ArtifactShell.FireSolarFlares.FixedUpdate.ILHook(DoILHook);
        }

        private static void DoILHook(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.BeetleGuardMonster.GroundSlam))]
    [MonoDetourTargets(typeof(EntityStates.BeetleGuardMonster.FireSunder))]
    private static class BeetleGuard
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.BeetleGuardMonster.GroundSlam.OnEnter.ILHook(GroundSlam_OnEnter);
            Mdh.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate.ILHook(FireSunder_FixedUpdate);
        }

        private static void GroundSlam_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.AddDamageSourceToOverlapAttack(DamageSource.Primary, info);
        }

        private static void FireSunder_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.BeetleMonster.HeadbuttState))]
    private static class Beetle
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.BeetleMonster.HeadbuttState.OnEnter.ILHook(HeadbuttState_OnEnter);
        }

        private static void HeadbuttState_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.AddDamageSourceToOverlapAttack(DamageSource.Primary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.BeetleQueenMonster.FireSpit))]
    private static class BeetleQueen
    {
        private static readonly AssetReferenceT<GameObject> _beetleQueenSpitGroundDamageZone = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_BeetleQueen.BeetleQueenAcid_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.BeetleQueenMonster.FireSpit.FireBlob.ILHook(FireSpit_FireBlob);

            if (ConfigOptions.BeetleQueenDamageZoneDamageSource.Value)
            {
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_beetleQueenSpitGroundDamageZone).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
            }
        }

        private static void FireSpit_FireBlob(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Bell.BellWeapon.ChargeTrioBomb))]
    private static class BrassContraption
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate.ILHook(ChargeTrioBomb_FixedUpdate);
        }

        private static void ChargeTrioBomb_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Bison.Charge))]
    [MonoDetourTargets(typeof(EntityStates.Bison.Headbutt))]
    private static class Bison
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Bison.Charge.ResetOverlapAttack.ILHook(Charge_ResetOverlapAttack);
            Mdh.EntityStates.Bison.Headbutt.OnEnter.ILHook(Headbutt_OnEnter);
        }

        private static void Charge_ResetOverlapAttack(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.AddDamageSourceToOverlapAttack(DamageSource.Primary, info);
        }

        private static void Headbutt_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.AddDamageSourceToOverlapAttack(DamageSource.Utility, info);
        }
    }



    private static class Mithrix
    {
        [MonoDetourTargets]
        private static class KarmaStuff
        {
            private static readonly AssetReferenceT<GameObject> _primaryKarmaLightning = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother_Karma.LunarCrawlerWeaponSlam_prefab);
            private static readonly AssetReferenceT<GameObject> _secondaryKarmaSpike1 = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother_Karma.KarmaSpikeSmall_prefab);
            private static readonly AssetReferenceT<GameObject> _secondaryKarmaSpike2 = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother_Karma.KarmaSpikeXSmall_prefab);
            private static readonly AssetReferenceT<GameObject> _utilityKarmaBomb = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother_Karma.KarmaBomb_prefab);
            private static readonly AssetReferenceT<GameObject> _primaryHurtLunarShardOnImpactSpike = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Brother_Karma.KarmaSpikeXXSmall_prefab);



            [MonoDetourHookInitialize]
            private static void SetupProjectileAssetDamageSources()
            {
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_primaryKarmaLightning).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_secondaryKarmaSpike1).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_secondaryKarmaSpike2).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_utilityKarmaBomb).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Utility);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_primaryHurtLunarShardOnImpactSpike).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
            }



            [MonoDetourTargets(typeof(EntityStates.BrotherMonster.KarmaSpire.FireProjectile))]
            private static class KarmaSpire
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.BrotherMonster.KarmaSpire.FireProjectile.OnEnter.ILHook(FireProjectile_OnEnter);
                }


                private static void FireProjectile_OnEnter(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
                }
            }



            [MonoDetourTargets(typeof(SprintBash))]
            [MonoDetourTargets(typeof(SkyLeapBomb))]
            [MonoDetourTargets(typeof(BaseSlideState))]
            private static class OtherKarmaProjectiles
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.BrotherMonster.WeaponSlam.FireCrystals.ILHook(WeaponSlam_FireCrystals);
                    Mdh.EntityStates.BrotherMonster.SprintBash.AuthorityFireAttack.ILHook(SprintBash_AuthorityFireAttack);
                    Mdh.EntityStates.BrotherMonster.SkyLeapBomb.FireRingAuthority.ILHook(SkyLeapBomb_FireRingAuthority);
                    Mdh.EntityStates.BrotherMonster.ExitSkyLeap.AttemptFireMeteorStormServer.ILHook(ExitSkyLeap_AttemptFireMeteorStormServer);
                    Mdh.EntityStates.BrotherMonster.ExitSkyLeap.FireCrawlersAuthority.ILHook(ExitSkyLeap_FireCrawlersAuthority);
                    Mdh.EntityStates.BrotherMonster.BaseSlideState.AttemptFireCrystalsAuthority.ILHook(SetUtilityDamageSource);
                    Mdh.EntityStates.BrotherMonster.BaseSlideState.AttemptFireHomingBombAuthority.ILHook(SetUtilityDamageSource);
                    Mdh.EntityStates.BrotherMonster.FistSlam.FireCrystalsAuthority.ILHook(FistSlam_FireCrystalsAuthority);
                }


                private static void FistSlam_FireCrystalsAuthority(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
                }


                private static void SetUtilityDamageSource(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericUtility, info);
                }


                private static void ExitSkyLeap_FireCrawlersAuthority(ILManipulationInfo info)
                {
                    ILWeaver w = new(info);
                    ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, w);
                    ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, w);
                }


                // need a custom IL hook here to change the MeteorStormController's blastDamageType
                private static void ExitSkyLeap_AttemptFireMeteorStormServer(ILManipulationInfo info)
                {
                    ILWeaver w = new(info);
                    w.MatchRelaxed(
                        x => x.MatchDup(),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallOrCallvirt<EntityState>("get_gameObject"),
                        x => x.MatchStfld<MeteorStormController>("owner") && w.SetCurrentTo(x)
                    ).ThrowIfFailure()
                    .InsertAfterCurrent(
                        w.Create(OpCodes.Dup),
                        w.CreateDelegateCall((MeteorStormController meteorStormController) =>
                        {
                            meteorStormController.blastDamageType.damageSource = DamageSource.Special;
                        })
                    );
                }


                private static void SkyLeapBomb_FireRingAuthority(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, info);
                }


                private static void SprintBash_AuthorityFireAttack(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
                }


                private static void WeaponSlam_FireCrystals(ILManipulationInfo info)
                {
                    // this overrides the secondary damagesource set on one of the karma spike assets
                    ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
                }
            }
        }




        [MonoDetourTargets(typeof(WeaponSlam))]
        [MonoDetourTargets(typeof(UltChannelState))]
        [MonoDetourTargets(typeof(ExitSkyLeap))]
        [MonoDetourTargets(typeof(BrotherUltUtils))]
        private static class Phase1And3
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                //Mdh.EntityStates.BrotherMonster.WeaponSlam.OnEnter.ILHook(WeaponSlam_OnEnter);
                Mdh.EntityStates.BrotherMonster.WeaponSlam.OnEnter.Postfix(ChangeWeaponSlamOverlapDamageSource);
                Mdh.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate.ILHook(WeaponSlam_FixedUpdate);
                Mdh.EntityStates.BrotherMonster.SprintBash.AuthorityModifyOverlapAttack.Postfix(SprintBash_AuthorityModifyOverlapAttack);
                // i can get away with using the same method now since they need the same single method call
                Mdh.EntityStates.BrotherMonster.ExitSkyLeap.FireRingAuthority.ILHook(OverrideSkyLeapSingleFireProjectile);
                Mdh.RoR2.BrotherUltUtils.EvaluatePattern.ILHook(BrotherUltUtils_EvaluatePattern);
            }


            private static void SprintBash_AuthorityModifyOverlapAttack(SprintBash self, ref OverlapAttack overlapAttack)
            {
                overlapAttack.damageType.damageSource = DamageSource.Secondary;
            }


            private static void BrotherUltUtils_EvaluatePattern(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSpecial, info);
            }


            private static void ChangeWeaponSlamOverlapDamageSource(EntityStates.BrotherMonster.WeaponSlam self)
            {
                self?.weaponAttack?.damageType.damageSource = DamageSource.Primary;
            }


            private static void WeaponSlam_FixedUpdate(ILManipulationInfo info)
            {
                ILWeaver w = new(info);

                ILHelpers.BlastAttacks.OverrideNextBlastAttackDamageSource(DamageSource.Primary, w);

                // phase 3 waves
                ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, w);

                // phase 3 pillar
                ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, w);
            }


            private static void OverrideSkyLeapSingleFireProjectile(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, info);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.BrotherMonster.Weapon.FireLunarShards))]
        [MonoDetourTargets(typeof(FistSlam))]
        [MonoDetourTargets(typeof(EntityStates.BrotherMonster.Weapon.LunarExploderMortarStorm))]
        private static class Phase4
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.BrotherMonster.FistSlam.FixedUpdate.ILHook(FistSlam_FixedUpdate);
                // both phase 1/3 and 4 use the same FireLunarShards but i'm putting it in here since it's phase 4 where it's more important
                Mdh.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter.ILHook(FireLunarShards_OnEnter);
                Mdh.EntityStates.BrotherMonster.Weapon.LunarExploderMortarStorm.FireMortarAuthority.ILHook(LunarExploderMortarStorm_FireMortarAuthority);
            }


            private static void LunarExploderMortarStorm_FireMortarAuthority(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericUtility, info);
            }


            private static void FireLunarShards_OnEnter(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
            }

            private static void FistSlam_FixedUpdate(ILManipulationInfo info)
            {
                // need to do all these because the fist slam also does a self damage and a blast attack along with the projectiles
                // creating a cursor here so it can remember it's position when using multiple match next methods
                ILWeaver w = new(info);

                ILHelpers.OverrideNextDamageInfoDamageSource(DamageSource.Secondary, w);

                //ILHelpers.BlastAttacks.SetDamageSourceForNextBlastAttack(DamageSource.Secondary, w);
                ILHelpers.BlastAttacks.OverrideNextBlastAttackDamageSource(DamageSource.Secondary, w);

                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
            }
        }



        [MonoDetourTargets(typeof(EntityStates.BrotherHaunt.FireRandomProjectiles))]
        private static class DetonationPhase
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.BrotherHaunt.FireRandomProjectiles.FireProjectile.ILHook(FireRandomProjectiles_FireProjectile);
            }


            private static void FireRandomProjectiles_FireProjectile(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
            }
        }
    }



    [MonoDetourTargets(typeof(EntityStates.ClayBoss.ClayBossWeapon.FireBombardment))]
    [MonoDetourTargets(typeof(EntityStates.ClayBoss.FireTarball))]
    [MonoDetourTargets(typeof(TarTetherController))]
    private static class ClayDunestrider
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade.ILHook(FireBombardment_FireGrenade);
            Mdh.EntityStates.ClayBoss.FireTarball.FireSingleTarball.ILHook(FireTarball_FireSingleTarball);
            Mdh.RoR2.TarTetherController.DoDamageTick.ILHook(TarTetherController_DoDamageTick);
        }

        private static void FireBombardment_FireGrenade(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void FireTarball_FireSingleTarball(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
        }

        private static void TarTetherController_DoDamageTick(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILHelpers.OverrideNextDamageInfoDamageSource(DamageSource.Special, w);
        }
    }



    [MonoDetourTargets(typeof(MinigunFire))]
    [MonoDetourTargets(typeof(MinigunState))]
    // the tar airblast extends from rex's airblast so we hook that
    [MonoDetourTargets(typeof(EntityStates.Treebot.Weapon.FireSonicBoom))]
    private static class ClayTemplar
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.ClayBruiser.Weapon.MinigunFire.OnFireAuthority.ILHook(MinigunFire_OnFireAuthority);
            // have to re-implement OnEnter and OnExit here bc any ILHook for MinigunFire stops the slow debuff from being applied/removed????
            Mdh.EntityStates.ClayBruiser.Weapon.MinigunState.OnEnter.Postfix(MinigunState_OnEnter);
            Mdh.EntityStates.ClayBruiser.Weapon.MinigunState.OnExit.Prefix(MinigunState_OnExit);
        }


        private static void MinigunFire_OnFireAuthority(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageTypeCombo.GenericPrimary, info);
        }


        private static void MinigunState_OnEnter(MinigunState self)
        {
            if (NetworkServer.active && self.characterBody != null)
            {
                self.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
            }
        }


        private static void MinigunState_OnExit(MinigunState self)
        {
            if (NetworkServer.active && self.characterBody != null)
            {
                self.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
            }
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Drone.DroneWeapon.FireGatling))]
    private static class GunnerTurret
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Drone.DroneWeapon.FireGatling.OnEnter.ILHook(FireGatling_FixedUpdate);
        }

        private static void FireGatling_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.GolemMonster.ClapState))]
    [MonoDetourTargets(typeof(EntityStates.GolemMonster.FireLaser))]
    private static class StoneGolem
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.GolemMonster.ClapState.FixedUpdate.ILHook(ClapState_FixedUpdate);
            Mdh.EntityStates.GolemMonster.FireLaser.OnEnter.ILHook(FireLaser_OnEnter);
        }

        private static void ClapState_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
        }

        private static void FireLaser_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.GrandParentBoss.GroundSwipe))]
    private static class GrandParentMonster
    {
        private static readonly AssetReferenceT<GameObject> _grandParentMiniBoulder = new (RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Grandparent.GrandparentMiniBoulder_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.GrandParentBoss.GroundSwipe.FixedUpdate.ILHook(GroundSwipe_FixedUpdate);

            AssetAsyncReferenceManager<GameObject>.LoadAsset(_grandParentMiniBoulder).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
            };
        }

        private static void GroundSwipe_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.GravekeeperMonster.Weapon.GravekeeperBarrage))]
    [MonoDetourTargets(typeof(EntityStates.GravekeeperBoss.FireHook))]
    private static class Grovetender
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.GravekeeperMonster.Weapon.GravekeeperBarrage.FireBlob.ILHook(GravekeeperBarrage_FireBlob);
            Mdh.EntityStates.GravekeeperBoss.FireHook.FireSingleHook.ILHook(FireHook_FireSingleHook);
        }

        private static void GravekeeperBarrage_FireBlob(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void FireHook_FireSingleHook(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.GreaterWispMonster.FireCannons))]
    private static class GreaterWisp
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.GreaterWispMonster.FireCannons.OnEnter.ILHook(FireCannons_OnEnter);
        }

        private static void FireCannons_OnEnter(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, w);
            ILHelpers.Projectiles.OverrideNextFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, w);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.HermitCrab.FireMortar))]
    private static class HermitCrab
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.HermitCrab.FireMortar.Fire.ILHook(FireMortar_Fire);
        }

        private static void FireMortar_Fire(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.ImpBossMonster.FireVoidspikes))]
    [MonoDetourTargets(typeof(EntityStates.ImpBossMonster.GroundPound))]
    [MonoDetourTargets(typeof(EntityStates.ImpBossMonster.BlinkState))]
    private static class ImpOverlord
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.ImpBossMonster.FireVoidspikes.FireSpikeAuthority.ILHook(FireVoidspikes_FireSpikeAuthority);
            Mdh.EntityStates.ImpBossMonster.FireVoidspikes.OnEnter.ILHook(FireVoidspikes_OnEnter);
            Mdh.EntityStates.ImpBossMonster.GroundPound.OnEnter.ILHook(GroundPound_OnEnter);
            Mdh.EntityStates.ImpBossMonster.BlinkState.ExitCleanup.ILHook(BlinkState_ExitCleanup);
        }

        private static void FireVoidspikes_FireSpikeAuthority(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void FireVoidspikes_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.OverrideDamageTypeCombo(DamageSource.Primary, info);
        }

        private static void GroundPound_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
        }

        private static void BlinkState_ExitCleanup(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.ImpMonster.DoubleSlash))]
    private static class Imp
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.ImpMonster.DoubleSlash.OnEnter.ILHook(DoubleSlash_OnEnter);
        }

        private static void DoubleSlash_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.OverrideDamageTypeCombo(DamageSource.Primary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.JellyfishMonster.JellyNova))]
    private static class Jellyfish
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.JellyfishMonster.JellyNova.Detonate.ILHook(JellyNova_Detonate);
        }

        private static void JellyNova_Detonate(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.LemurianBruiserMonster.FireMegaFireball))]
    [MonoDetourTargets(typeof(EntityStates.LemurianBruiserMonster.Flamebreath))]
    private static class ElderLemurian
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate.ILHook(FireMegaFireball_FixedUpdate);
            Mdh.EntityStates.LemurianBruiserMonster.Flamebreath.FireFlame.ILHook(Flamebreath_FireFlame);
        }

        private static void FireMegaFireball_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void Flamebreath_FireFlame(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Secondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.LemurianMonster.FireFireball))]
    [MonoDetourTargets(typeof(EntityStates.LemurianMonster.Bite))]
    private static class Lemurian
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.LemurianMonster.FireFireball.OnEnter.ILHook(FireFireball_OnEnter);
            Mdh.EntityStates.LemurianMonster.Bite.OnEnter.ILHook(Bite_OnEnter);
        }

        private static void FireFireball_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void Bite_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.OverlapAttacks.AddDamageSourceToOverlapAttack(DamageSource.Secondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.LunarGolem.FireTwinShots))]
    private static class LunarGolem
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.LunarGolem.FireTwinShots.FireSingle.ILHook(FireTwinShots_FireSingle);
        }

        private static void FireTwinShots_FireSingle(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.LunarWisp.FireLunarGuns))]
    [MonoDetourTargets(typeof(EntityStates.LunarWisp.SeekingBomb))]
    private static class LunarWisp
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.LunarWisp.FireLunarGuns.OnFireAuthority.ILHook(FireLunarGuns_OnFireAuthority);
            Mdh.EntityStates.LunarWisp.SeekingBomb.FireBomb.ILHook(SeekingBomb_FireBomb);
        }

        private static void FireLunarGuns_OnFireAuthority(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            ILHelpers.BulletAttacks.OverrideNextBulletAttackDamageSource(DamageSource.Primary, w);
            ILHelpers.BulletAttacks.OverrideNextBulletAttackDamageSource(DamageSource.Primary, w);
        }

        private static void SeekingBomb_FireBomb(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
        }
    }


    
    // setting special DamageSource for worm projectiles since their special ability changes their "stance" and stops them from firing projectiles for a bit
    [MonoDetourTargets(typeof(WormBodyPositions2))]
    private static class WormBosses
    {
        private static readonly AssetReferenceT<GameObject> _magmaWormBody = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_MagmaWorm.MagmaWormBody_prefab);
        private static readonly AssetReferenceT<GameObject> _overloadingWormBody = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_ElectricWorm.ElectricWormBody_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.RoR2.WormBodyPositions2.FireImpactBlastAttack.ILHook(WormBodyPositions2_FireImpactBlastAttack);
            Mdh.RoR2.WormBodyPositions2.FireMeatballs.ILHook(WormBodyPositions2_FireMeatballs);


            if (ConfigOptions.WormContactDamageSources.Value)
            {
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_magmaWormBody).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ContactDamage>().damageType.damageSource = DamageSource.Primary;
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_overloadingWormBody).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ContactDamage>().damageType.damageSource = DamageSource.Primary;
                };
            }
        }

        private static void WormBodyPositions2_FireImpactBlastAttack(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Special, info);
        }

        private static void WormBodyPositions2_FireMeatballs(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.MiniMushroom.SporeGrenade))]
    private static class MiniMushrum
    {
        private static readonly AssetReferenceT<GameObject> _mushrumGroundDamageZone = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_MiniMushroom.SporeGrenadeProjectileDotZone_prefab);
        private static readonly AssetReferenceT<GameObject> _mushrumGroundDamageZoneSimple = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_MiniMushroom.SporeGrenadeProjectileDotZoneSimple_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.MiniMushroom.SporeGrenade.FireGrenade.ILHook(SporeGrenade_FireGrenade);

            if (ConfigOptions.MushroomDamageZoneDamageSource.Value)
            {
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_mushrumGroundDamageZone).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_mushrumGroundDamageZoneSimple).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
            }
        }

        private static void SporeGrenade_FireGrenade(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.NullifierMonster.FirePortalBomb))]
    private static class VoidReaver
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.NullifierMonster.FirePortalBomb.FireBomb.ILHook(FirePortalBomb_FireBomb);
        }

        private static void FirePortalBomb_FireBomb(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.ParentMonster.GroundSlam))]
    private static class ParentMonster
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.ParentMonster.GroundSlam.FixedUpdate.ILHook(GroundSlam_FixedUpdate);
        }

        private static void GroundSlam_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.RoboBallBoss.Weapon.FireEyeBlast))]
    [MonoDetourTargets(typeof(EntityStates.RoboBallBoss.Weapon.FireDelayKnockup))]
    private static class VanillaSolusBosses
    {
        private static readonly AssetReferenceT<GameObject> _awuProjectileGroundDamageZone = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_RoboBallBoss.RoboBallDamageZone_prefab);

        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate.ILHook(FireEyeBlast_FixedUpdate);
            Mdh.EntityStates.RoboBallBoss.Weapon.FireDelayKnockup.OnEnter.ILHook(FireDelayKnockup_OnEnter);


            if (ConfigOptions.AlloyWorshipUnitDamageZoneDamageSource.Value)
            {
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_awuProjectileGroundDamageZone).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
            }
        }

        private static void FireEyeBlast_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void FireDelayKnockup_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.RoboBallMini.Weapon.FireEyeBeam))]
    private static class SolusProbe
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.RoboBallMini.Weapon.FireEyeBeam.ModifyBullet.Postfix(FireEyeBeam_ModifyBullet);
        }

        private static void FireEyeBeam_ModifyBullet(EntityStates.RoboBallMini.Weapon.FireEyeBeam self, ref BulletAttack bulletAttack)
        {
            bulletAttack.damageType.damageSource = DamageSource.Primary;
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Squid.SquidWeapon.FireSpine))]
    private static class SquidPolyp
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Squid.SquidWeapon.FireSpine.FireOrbArrow.ILHook(FireSpine_FireOrbArrow);
        }

        private static void FireSpine_FireOrbArrow(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            ILWeaverResult firstMatch = w.MatchRelaxed(
                x => x.MatchNewobj(out _),
                x => x.MatchStloc(3) && w.SetCurrentTo(x)
            );
            if (!firstMatch.IsValid)
            {
                w.MatchRelaxed(
                    x => x.MatchCallOrCallvirt(out _),
                    x => x.MatchStloc(3) && w.SetCurrentTo(x)
                ).ThrowIfFailure();
            }

            w.InsertAfterCurrent(
                w.Create(OpCodes.Ldloc_3),
                w.CreateDelegateCall((RoR2.Orbs.SquidOrb squidOrb) =>
                {
                    squidOrb.damageType.damageSource = DamageSource.Primary;
                })
            );
        }
    }



    [MonoDetourTargets(typeof(EntityStates.TitanMonster.FireFist))]
    [MonoDetourTargets(typeof(TitanRockController))]
    [MonoDetourTargets(typeof(EntityStates.TitanMonster.FireGoldMegaLaser))]
    private static class StoneTitanBosses
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.TitanMonster.FireFist.PlaceSingleDelayBlast.ILHook(FireFist_PlaceSingleDelayBlast);
            Mdh.RoR2.TitanRockController.Fire.ILHook(TitanRockController_Fire);
            Mdh.EntityStates.TitanMonster.FireGoldMegaLaser.FixedUpdate.ILHook(FireGoldMegaLaser_FixedUpdate);
        }

        private static void FireFist_PlaceSingleDelayBlast(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
        }

        private static void TitanRockController_Fire(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericUtility, info);
        }

        private static void FireGoldMegaLaser_FixedUpdate(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.UrchinTurret.Weapon.MinigunFire))]
    private static class MalachiteUrchin
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.UrchinTurret.Weapon.MinigunFire.OnFireAuthority.ILHook(MinigunFire_OnFireAuthority);
        }

        private static void MinigunFire_OnFireAuthority(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.VagrantMonster.Weapon.JellyBarrage))]
    [MonoDetourTargets(typeof(EntityStates.VagrantMonster.FireTrackingBomb))]
    private static class WanderingVagrant
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.VagrantMonster.Weapon.JellyBarrage.FireBlob.ILHook(JellyBarrage_FireBlob);
            Mdh.EntityStates.VagrantMonster.FireTrackingBomb.FireBomb.ILHook(FireTrackingBomb_FireBomb);
        }

        private static void JellyBarrage_FireBlob(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }

        private static void FireTrackingBomb_FireBomb(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Vulture.Weapon.FireWindblade))]
    private static class AlloyVulture
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Vulture.Weapon.FireWindblade.OnEnter.ILHook(FireWindblade_OnEnter);
        }

        private static void FireWindblade_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Wisp1Monster.FireEmbers))]
    private static class LesserWisp
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Wisp1Monster.FireEmbers.OnEnter.ILHook(FireEmbers_OnEnter);
        }

        private static void FireEmbers_OnEnter(ILManipulationInfo info)
        {
            ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
        }
    }
}