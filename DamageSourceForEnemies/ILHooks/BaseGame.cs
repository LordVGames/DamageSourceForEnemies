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
            private static readonly AssetReferenceT<GameObject> _beetleQueenSpitGroundDamageZone = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_BeetleQueen.BeetleQueenAcid_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.BeetleQueenMonster.FireSpit.FireBlob.ILHook(FireSpit_FireBlob);

                if (ConfigOptions.BeetleQueenDamageZoneDamageSource.Value)
                {
                    AssetAsyncReferenceManager<GameObject>.LoadAsset(_beetleQueenSpitGroundDamageZone).Completed += (handle) =>
                    {
                        handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Primary;
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(_beetleQueenSpitGroundDamageZone);
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



        // class contains other classes for the different phases of mithrix
        private static class Mithrix
        {
            // for reference: mithrix hammer swipe attack is handled in:
            // Generic.cs under BasicMeleeAttack_OnEnter
            [MonoDetourTargets(typeof(EntityStates.BrotherMonster.WeaponSlam))]
            [MonoDetourTargets(typeof(EntityStates.BrotherMonster.UltChannelState))]
            [MonoDetourTargets(typeof(EntityStates.BrotherMonster.ExitSkyLeap))]
            private static class Phase1And3
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    //Mdh.EntityStates.BrotherMonster.WeaponSlam.OnEnter.ILHook(WeaponSlam_OnEnter);
                    Mdh.EntityStates.BrotherMonster.WeaponSlam.OnEnter.Postfix(ChangeWeaponSlamOverlapDamageSource);
                    Mdh.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate.ILHook(WeaponSlam_FixedUpdate);
                    // i can get away with using the same method now since they need the same single method call
                    Mdh.EntityStates.BrotherMonster.ExitSkyLeap.FireRingAuthority.ILHook(OverrideSkyLeapSingleFireProjectile);
                    Mdh.EntityStates.BrotherMonster.UltChannelState.FireWave.ILHook(OverrideSkyLeapSingleFireProjectile);
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
            [MonoDetourTargets(typeof(EntityStates.BrotherMonster.FistSlam))]
            private static class Phase4
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    Mdh.EntityStates.BrotherMonster.FistSlam.FixedUpdate.ILHook(FistSlam_FixedUpdate);
                    // both phase 1/3 and 4 use the same FireLunarShards but i'm putting it in here since it's phase 4 where it's more important
                    Mdh.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter.ILHook(FireLunarShards_OnEnter);
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



        [MonoDetourTargets(typeof(EntityStates.ClayBruiser.Weapon.MinigunFire))]
        // the tar airblast extends from rex's airblast so we hook that
        [MonoDetourTargets(typeof(EntityStates.Treebot.Weapon.FireSonicBoom))]
        private static class ClayTemplar
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.ClayBruiser.Weapon.MinigunFire.OnFireAuthority.ILHook(MinigunFire_OnFireAuthority);
                // the damagesource for the clay templar tar airblast really doesn't want to change no matter what i do
                // so i gotta replace it in the entitystate it extends from
                Mdh.EntityStates.Treebot.Weapon.FireSonicBoom.OnEnter.ILHook(FireSonicBoom_OnEnter);
            }


            private static void FireSonicBoom_OnEnter(ILManipulationInfo info)
            {
                ILWeaver w = new(info);

                OverrideNextDamageSourceIfNeeded(w);
                OverrideNextDamageSourceIfNeeded(w);
            }
            private static void OverrideNextDamageSourceIfNeeded(ILWeaver w)
            {
                w.MatchNextRelaxed(
                    x => x.MatchStfld<DamageInfo>("damageType") && w.SetCurrentTo(x)
                );
                w.InsertBeforeCurrent(
                    w.Create(OpCodes.Ldarg_0),
                    w.CreateDelegateCall((DamageTypeCombo damageTypeCombo, EntityStates.Treebot.Weapon.FireSonicBoom entityState) =>
                    {
                        if (entityState is EntityStates.ClayBruiser.Weapon.FireSonicBoom)
                        {
                            damageTypeCombo.damageSource = DamageSource.Secondary;
                        }
                        return damageTypeCombo;
                    })
                );
            }


            private static void MinigunFire_OnFireAuthority(ILManipulationInfo info)
            {
                ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageTypeCombo.GenericPrimary, info);
            }
        }



        private static class Drones
        {
            [MonoDetourTargets(typeof(EntityStates.Drone.DroneWeapon.FireGatling))]
            private static class GunnerTurret
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    if (!ConfigOptions.GiveDronesDamageSources.Value)
                    {
                        return;
                    }

                    Mdh.EntityStates.Drone.DroneWeapon.FireGatling.OnEnter.ILHook(FireGatling_FixedUpdate);
                }

                private static void FireGatling_FixedUpdate(ILManipulationInfo info)
                {
                    ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.Drone.DroneWeapon.FireTurret))]
            private static class GunnerDrone
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    if (!ConfigOptions.GiveDronesDamageSources.Value)
                    {
                        return;
                    }

                    Mdh.EntityStates.Drone.DroneWeapon.FireTurret.OnEnter.ILHook(FireTurret_FixedUpdate);
                }

                private static void FireTurret_FixedUpdate(ILManipulationInfo info)
                {
                    ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.Mage.Weapon.Flamethrower))]
            private static class IncineratorDrone
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    if (!ConfigOptions.GiveDronesDamageSources.Value)
                    {
                        return;
                    }

                    Mdh.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet.ILHook(Flamethrower_FireGauntlet);
                }

                private static void Flamethrower_FireGauntlet(ILManipulationInfo info)
                {
                    ILWeaver w = new(info);

                    w.MatchRelaxed(
                        x => x.MatchCallOrCallvirt<BulletAttack>("Fire") && w.SetCurrentTo(x)
                    ).ThrowIfFailure();

                    w.InsertBeforeCurrent(
                        w.Create(OpCodes.Ldarg_0),
                        w.CreateDelegateCall((BulletAttack bulletAttack, EntityStates.Mage.Weapon.Flamethrower flamethrower) =>
                        {
                            if (flamethrower is EntityStates.Drone.DroneWeapon.Flamethrower)
                            {
                                bulletAttack?.damageType.damageSource = DamageSource.Primary;
                            }

                            return bulletAttack;
                        })
                    );
                }
            }


            [MonoDetourTargets(typeof(EntityStates.Drone.DroneWeapon.FireMissileBarrage))]
            private static class MissileDrone
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    if (!ConfigOptions.GiveDronesDamageSources.Value)
                    {
                        return;
                    }

                    Mdh.EntityStates.Drone.DroneWeapon.FireMissileBarrage.FireMissile.ILHook(FireMissileBarrage_FireMissile);
                }

                private static void FireMissileBarrage_FireMissile(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
                }
            }


            [MonoDetourTargets(typeof(EntityStates.Drone.DroneWeapon.FireMegaTurret))]
            [MonoDetourTargets(typeof(EntityStates.Drone.DroneWeapon.FireTwinRocket))]
            private static class TC280
            {
                [MonoDetourHookInitialize]
                private static void Setup()
                {
                    if (!ConfigOptions.GiveDronesDamageSources.Value)
                    {
                        return;
                    }

                    Mdh.EntityStates.Drone.DroneWeapon.FireMegaTurret.FireBullet.ILHook(FireMegaTurret_FireBullet);
                    Mdh.EntityStates.Drone.DroneWeapon.FireTwinRocket.FireProjectile.ILHook(FireTwinRocket_FireProjectile);
                }

                private static void FireMegaTurret_FireBullet(ILManipulationInfo info)
                {
                    ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Primary, info);
                }

                private static void FireTwinRocket_FireProjectile(ILManipulationInfo info)
                {
                    ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
                }
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
            private static readonly AssetReferenceT<GameObject> _grandParentMiniBoulder = new (RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_Grandparent.GrandparentMiniBoulder_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.GrandParentBoss.GroundSwipe.FixedUpdate.ILHook(GroundSwipe_FixedUpdate);

                AssetAsyncReferenceManager<GameObject>.LoadAsset(_grandParentMiniBoulder).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Primary;
                    AssetAsyncReferenceManager<GameObject>.UnloadAsset(_grandParentMiniBoulder);
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



        // lunar exploder is handled in Generic.cs under EditAndReturnFireProjectileInfo



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



        [MonoDetourTargets(typeof(WormBodyPositions2))]
        private static class WormBosses
        {
            private static readonly AssetReferenceT<GameObject> _magmaWormBody = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_MagmaWorm.MagmaWormBody_prefab);
            private static readonly AssetReferenceT<GameObject> _overloadingWormBody = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_ElectricWorm.ElectricWormBody_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.RoR2.WormBodyPositions2.FireImpactBlastAttack.ILHook(WormBodyPositions2_FireImpactBlastAttack);
                Mdh.RoR2.WormBodyPositions2.FireMeatballs.ILHook(WormBodyPositions2_FireMeatballs);


                AssetAsyncReferenceManager<GameObject>.LoadAsset(_magmaWormBody).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ContactDamage>().damageType.damageSource = DamageSource.Primary;
                    AssetAsyncReferenceManager<GameObject>.UnloadAsset(_magmaWormBody);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_overloadingWormBody).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ContactDamage>().damageType.damageSource = DamageSource.Primary;
                    AssetAsyncReferenceManager<GameObject>.UnloadAsset(_overloadingWormBody);
                };
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
            private static readonly AssetReferenceT<GameObject> _mushrumGroundDamageZone = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_MiniMushroom.SporeGrenadeProjectileDotZone_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.MiniMushroom.SporeGrenade.FireGrenade.ILHook(SporeGrenade_FireGrenade);

                if (ConfigOptions.MushroomDamageZoneDamageSource.Value)
                {
                    AssetAsyncReferenceManager<GameObject>.LoadAsset(_mushrumGroundDamageZone).Completed += (handle) =>
                    {
                        handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Primary;
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(_mushrumGroundDamageZone);
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
            private static readonly AssetReferenceT<GameObject> _awuProjectileGroundDamageZone = new(RoR2BepInExPack.GameAssetPaths.Version_1_35_0.RoR2_Base_RoboBallBoss.RoboBallDamageZone_prefab);

            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate.ILHook(FireEyeBlast_FixedUpdate);
                Mdh.EntityStates.RoboBallBoss.Weapon.FireDelayKnockup.OnEnter.ILHook(FireDelayKnockup_OnEnter);


                if (ConfigOptions.AlloyWorshipUnitDamageZoneDamageSource.Value)
                {
                    AssetAsyncReferenceManager<GameObject>.LoadAsset(_awuProjectileGroundDamageZone).Completed += (handle) =>
                    {
                        handle.Result.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Primary;
                        AssetAsyncReferenceManager<GameObject>.UnloadAsset(_awuProjectileGroundDamageZone);
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



        [MonoDetourTargets(typeof(EntityStates.ScavMonster.FireEnergyCannon))]
        [MonoDetourTargets(typeof(EntityStates.ScavMonster.ThrowSack))]
        private static class Scavenger
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.ScavMonster.FireEnergyCannon.OnEnter.ILHook(FireEnergyCannon_OnEnter);
                Mdh.EntityStates.ScavMonster.ThrowSack.Fire.ILHook(ThrowSack_Fire);
            }

            private static void FireEnergyCannon_OnEnter(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
            }

            private static void ThrowSack_Fire(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
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
        [MonoDetourTargets(typeof(EntityStates.TitanMonster.FireMegaLaser))]
        // aurelionite fist attack extends the normal one, including the damagesource
        // aurelionite rock orb also extends the normal one
        // laser does too but we still need to hook it for the extra projectiles shot
        [MonoDetourTargets(typeof(EntityStates.TitanMonster.FireGoldMegaLaser))]
        private static class StoneTitanBosses
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.TitanMonster.FireFist.PlaceSingleDelayBlast.ILHook(FireFist_PlaceSingleDelayBlast);
                Mdh.RoR2.TitanRockController.Fire.ILHook(TitanRockController_Fire);
                Mdh.EntityStates.TitanMonster.FireMegaLaser.FireBullet.ILHook(FireMegaLaser_FireBullet);
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

            private static void FireMegaLaser_FireBullet(ILManipulationInfo info)
            {
                ILHelpers.BulletAttacks.OverrideBulletAttackDamageSource(DamageSource.Special, info);
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
        [MonoDetourTargets(typeof(EntityStates.VagrantMonster.FireMegaNova))]
        private static class WanderingVagrant
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.VagrantMonster.Weapon.JellyBarrage.FireBlob.ILHook(JellyBarrage_FireBlob);
                Mdh.EntityStates.VagrantMonster.FireTrackingBomb.FireBomb.ILHook(FireTrackingBomb_FireBomb);
                Mdh.EntityStates.VagrantMonster.FireMegaNova.Detonate.ILHook(FireMegaNova_Detonate);
            }

            private static void JellyBarrage_FireBlob(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
            }

            private static void FireTrackingBomb_FireBomb(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, info);
            }

            private static void FireMegaNova_Detonate(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Special, info);
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
}