using System;
using RoR2;
using RoR2.Projectile;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace DamageSourceForEnemies
{
    internal static class ILHooks
    {
        private static class Util
        {
            internal static void LogCurrentIL(ILContext il, ILCursor c)
            {
#if DEBUG
                Log.Warning($"cursor is {c}");
                Log.Warning($"il is {il}");
#endif
            }

            internal static void LogILError(string methodName, ILContext il, ILCursor c)
            {
                Log.Error($"COULD NOT IL HOOK {methodName}!");
                Log.Error($"cursor is {c}");
                Log.Error($"il is {il}");
            }
        }

        private static class ILEdits
        {
            internal static void AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo newGenericDamageTypeCombo, string hookedMethodName, int localILVariableInt, ILContext il, ILCursor c)
            {
                // add our DamageTypeCombo
                if (!c.TryGotoNext(MoveType.Before,
                    x => x.MatchCallvirt<ProjectileManager>("FireProjectileWithoutDamageType")
                ))
                {
                    Util.LogILError(hookedMethodName, il, c);
                    return;
                }
                c.EmitDelegate<Func<DamageTypeCombo?>>(() =>
                {
                    return new DamageTypeCombo?(newGenericDamageTypeCombo);
                });


                // now we replace FireProjectileWithoutDamageType with the original FireProjectile that still exists for some reason
                c.Index = 0;
                if (!c.TryGotoNext(MoveType.AfterLabel,
                    x => x.MatchCallvirt<ProjectileManager>("FireProjectileWithoutDamageType")
                ))
                {
                    Util.LogILError(hookedMethodName, il, c);
                    return;
                }
                
                // so I KNOW doing .Remove is bad but my other option is to somehow duplicate 10 variables of different types and load them back twice
                // and not only am i not sure how to do that but that happens 42 times
                // plus the original FireProjectile is entirely the same aside from an extra parameter that's null by default anyways
                // so im just gonna make this easier on myself and remove the singular line
                c.Remove();
                c.Emit<ProjectileManager>(OpCodes.Callvirt, "FireProjectile");
            }

            internal static void AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo damageTypeCombo, string methodName, int localILVariableInt, ILContext il, ILCursor c)
            {
                if (!c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdloc(localILVariableInt),
                    x => x.MatchCallvirt<ProjectileManager>("FireProjectile")
                ))
                {
                    Util.LogILError(methodName, il, c);
                    return;
                }
                c.Emit(OpCodes.Ldloc, localILVariableInt);
                c.EmitDelegate<Func<FireProjectileInfo, FireProjectileInfo>>((fireProjectileInfo) =>
                {
                    fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(damageTypeCombo);
                    return fireProjectileInfo;
                });
                c.Emit(OpCodes.Stloc, localILVariableInt);
            }

            internal enum BulletAttackMatchType
            {
                OwnerIsGameObject = 0,
                ExistingDamageType
            }

            internal static void SetupForBulletAttackDelegate(bool isAttackInEntityState, BulletAttackMatchType bulletAttackMatchType, string methodName, ILContext il, ILCursor c)
            {
                bool couldNotMatch = false;
                switch (bulletAttackMatchType)
                {
                    case BulletAttackMatchType.OwnerIsGameObject:
                        if (!c.TryGotoNext(MoveType.After,
                            x => x.MatchLdarg(0),
                            x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                            x => x.MatchStfld<BulletAttack>("owner")
                        ))
                        {
                            couldNotMatch = true;
                        }
                        break;
                    case BulletAttackMatchType.ExistingDamageType:
                        if (!c.TryGotoNext(MoveType.After,
                            x => x.MatchLdcI4(out _),
                            x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                            x => x.MatchStfld<BulletAttack>("damageType")
                        ))
                        {
                            couldNotMatch = true;
                        }
                        break;
                }
                if (couldNotMatch)
                {
                    Util.LogILError(methodName, il, c);
                    return;
                }


                if (isAttackInEntityState)
                {
                    c.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    c.Emit(OpCodes.Dup);
                }
            }

            internal enum OverlapAttackMatchType
            {
                BasicDamageCalculation = 0,
                ExistingDamageType
            }

            /// <remarks>
            /// This logs it's own error if this fails, so don't log another error yourself.
            /// </remarks>
            /// 
            /// <returns>
            /// true if the match was successful, false if not
            /// </returns>
            internal static bool SetupForOverlapAttackDelegate(bool isAttackInEntityState, OverlapAttackMatchType overlapAttackMatchType, string methodName, ILContext il, ILCursor c)
            {
                bool couldNotMatch = false;
                switch (overlapAttackMatchType)
                {
                    case OverlapAttackMatchType.BasicDamageCalculation:
                        if (!c.TryGotoNext(MoveType.After,
                            x => x.MatchLdfld<EntityStates.BaseState>("damageStat"),
                            x => x.MatchMul(),
                            x => x.MatchStfld<OverlapAttack>("damage")
                        ))
                        {
                            couldNotMatch = true;
                        }
                        break;
                    case OverlapAttackMatchType.ExistingDamageType:
                        if (!c.TryGotoNext(MoveType.After,
                            x => x.MatchLdcI4(out _),
                            x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                            x => x.MatchStfld<OverlapAttack>("damageType")
                        ))
                        {
                            couldNotMatch = true;
                        }
                        break;
                }
                if (couldNotMatch)
                {
                    Util.LogILError(methodName, il, c);
                    return false;
                }


                if (isAttackInEntityState)
                {
                    c.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    c.Emit(OpCodes.Dup);
                }
                return true;
            }

            internal enum BlastAttackMatchType
            {
                AttackerIsGameObject = 0,
                BeforeFalloffModel,
                ExistingDamageType
            }

            /// <remarks>
            /// Made this one a method since it's used twice
            /// </remarks>
            private static bool MatchBasedOnBlastAttackMatchType(BlastAttackMatchType blastAttackMatchType, ILCursor c)
            {
                switch (blastAttackMatchType)
                {
                    case BlastAttackMatchType.AttackerIsGameObject:
                        if (c.TryGotoNext(MoveType.After,
                            x => x.MatchLdarg(0),
                            x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                            x => x.MatchStfld<BlastAttack>("attacker")
                        ))
                        {
                            return true;
                        }
                        break;
                    case BlastAttackMatchType.BeforeFalloffModel:
                        if (c.TryGotoNext(MoveType.Before,
                            x => x.MatchDup(),
                            x => x.MatchLdcI4(0),
                            x => x.MatchStfld<BlastAttack>("falloffModel")
                        ))
                        {
                            return true;
                        }
                        break;
                    case BlastAttackMatchType.ExistingDamageType:
                        if (c.TryGotoNext(MoveType.After,
                            x => x.MatchLdcI4(out _),
                            x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                            x => x.MatchStfld<BlastAttack>("damageType")
                        ))
                        {
                            return true;
                        }
                        break;
                }
                return false;
            }

            /// <remarks>
            /// Cannot be used when the BlastAttack is attached to an EntityState, use <see cref="SetupForBlastAttackDelegate"/> instead when doing that
            /// </remarks>
            internal static void SetDamageSourceForBlastAttack(DamageSource damageSource, BlastAttackMatchType blastAttackMatchType, string methodName, ILContext il, ILCursor c)
            {
                if (!MatchBasedOnBlastAttackMatchType(blastAttackMatchType, c))
                {
                    Util.LogILError(methodName, il, c);
                    return;
                }

                c.Emit(OpCodes.Dup);
                c.EmitDelegate<Action<BlastAttack>>((blastAttack) =>
                {
                    if (blastAttack != null)
                    {
                        blastAttack.damageType.damageSource = damageSource;
                    }
                });
            }

            /// <remarks>
            /// Use this instead of <see cref="SetDamageSourceForBlastAttack"/> when modifying a BlastAttack that's attached to an EntityState
            /// </remarks>
            /// <returns>
            /// true if the match was successful, false if not
            /// </returns>
            internal static bool SetupForBlastAttackDelegate(BlastAttackMatchType blastAttackMatchType, string methodName, ILContext il, ILCursor c)
            {
                if (!MatchBasedOnBlastAttackMatchType(blastAttackMatchType, c))
                {
                    Util.LogILError(methodName, il, c);
                    return false;
                }

                c.Emit(OpCodes.Ldarg_0);
                return true;
            }
        }

        internal static void SetupILHooks()
        {
            // welcome to hell, but not really
            // it's a lot of hooks and the majority of them just use one of the methods from ILEdits


            // a bunch of enemy attacks inherit BasicMeleeAttack, some of which don't have an OnEnter to hook into
            // so im just gonna hook into the base class and check for the attacks i need there
            IL.EntityStates.BasicMeleeAttack.OnEnter += BasicMeleeAttack_OnEnter;
            // same situation here
            IL.EntityStates.GenericProjectileBaseState.FireProjectile += GenericProjectileBaseState_FireProjectile;

            // wolfo's simulacrum mod has an augment with the artifact shell so i'm going to assume it can get items that way
            IL.EntityStates.ArtifactShell.FireSolarFlares.FixedUpdate += ArtifactShell_FireSolarFlares_FixedUpdate;
            IL.EntityStates.AcidLarva.LarvaLeap.DetonateAuthority += AcidLarva_LarvaLeap_DetonateAuthority;
            IL.EntityStates.BeetleGuardMonster.GroundSlam.OnEnter += BeetleGuardMonster_GroundSlam_OnEnter;
            IL.EntityStates.BeetleGuardMonster.FireSunder.FixedUpdate += BeetleGuardMonster_FireSunder_FixedUpdate;
            IL.EntityStates.BeetleMonster.HeadbuttState.OnEnter += BeetleMonster_HeadbuttState_OnEnter;
            IL.EntityStates.BeetleQueenMonster.FireSpit.FireBlob += BeetleQueenMonster_FireSpit_FireBlob;
            IL.EntityStates.Bell.BellWeapon.ChargeTrioBomb.FixedUpdate += Bell_BellWeapon_ChargeTrioBomb_FixedUpdate;
            IL.EntityStates.Bison.Headbutt.OnEnter += Bison_Headbutt_OnEnter;
            IL.EntityStates.Bison.Charge.ResetOverlapAttack += Bison_Charge_ResetOverlapAttack;
            // BrotherHaunt can get items from wolfo's simulcrum mod via a specific augment where it's used
            IL.EntityStates.BrotherHaunt.FireRandomProjectiles.FireProjectile += BrotherHaunt_FireRandomProjectiles_FireProjectile;
            IL.EntityStates.BrotherMonster.ExitSkyLeap.FireRingAuthority += BrotherMonster_ExitSkyLeap_FireRingAuthority;
            IL.EntityStates.BrotherMonster.FistSlam.FixedUpdate += BrotherMonster_FistSlam_FixedUpdate;
            IL.EntityStates.BrotherMonster.WeaponSlam.OnEnter += BrotherMonster_WeaponSlam_OnEnter;
            IL.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += BrotherMonster_WeaponSlam_FixedUpdate;
            IL.EntityStates.BrotherMonster.UltChannelState.FireWave += BrotherMonster_UltChannelState_FireWave;
            // FireLunarShardsHurt inherits the normal version changes
            IL.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += BrotherMonster_Weapon_FireLunarShards_OnEnter;
            IL.EntityStates.ChildMonster.SparkBallFire.FireBomb += ChildMonster_SparkBallFire_FireBomb;
            IL.EntityStates.ClayBoss.FireTarball.FireSingleTarball += ClayBoss_FireTarball_FireSingleTarball;
            // below is for the dunestrider suck attack
            IL.RoR2.TarTetherController.DoDamageTick += TarTetherController_DoDamageTick;
            IL.EntityStates.ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade += ClayBoss_ClayBossWeapon_FireBombardment_FireGrenade;
            IL.EntityStates.ClayBruiser.Weapon.MinigunFire.OnFireAuthority += ClayBruiser_Weapon_MinigunFire_OnFireAuthority;
            // surprisingly the devs tried to set the templar secondary's damagesource correctly, but it still says utility because it's based on rex's utility and it's damagetypecombo is protected
            // and even when i try to IL hook it it still says utility, even if the hook is successful
            // it doesn't do damage so it doesn't really matter anyways
            IL.EntityStates.ClayGrenadier.FaceSlam.FixedUpdate += ClayGrenadier_FaceSlam_FixedUpdate;
            IL.EntityStates.Drone.DroneWeapon.FireGatling.OnEnter += Drone_DroneWeapon_FireGatling_OnEnter;
            IL.EntityStates.Drone.DroneWeapon.FireMegaTurret.FireBullet += Drone_DroneWeapon_FireMegaTurret_FireBullet;
            IL.EntityStates.Drone.DroneWeapon.FireMissileBarrage.FireMissile += Drone_DroneWeapon_FireMissileBarrage_FireMissile;
            IL.EntityStates.Drone.DroneWeapon.FireTurret.OnEnter += Drone_DroneWeapon_FireTurret_OnEnter;
            IL.EntityStates.Drone.DroneWeapon.FireTwinRocket.FireProjectile += Drone_DroneWeapon_FireTwinRocket_FireProjectile;
            // flamethrower drone's flamethrower is a primary but it inherits artificer's flamethrower
            // so we need to modify the damagesource through that
            IL.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += Mage_Weapon_Flamethrower_FireGauntlet;
            IL.EntityStates.FalseSonBoss.CorruptedPaths.DetonateAuthority += FalseSonBoss_CorruptedPaths_DetonateAuthority;
            IL.EntityStates.FalseSonBoss.CorruptedPathsDash.FixedUpdate += FalseSonBoss_CorruptedPathsDash_FixedUpdate;
            IL.EntityStates.FalseSonBoss.FissureSlam.FixedUpdate += FalseSonBoss_FissureSlam_FixedUpdate;
            IL.EntityStates.FalseSonBoss.FissureSlam.DetonateAuthority += FalseSonBoss_FissureSlam_DetonateAuthority;
            IL.EntityStates.FalseSonBoss.LunarRain.DetonateAuthority += FalseSonBoss_LunarRain_DetonateAuthority;
            IL.EntityStates.FalseSonBoss.PrimeDevastator.DetonateAuthority += FalseSonBoss_PrimeDevastator_DetonateAuthority;
            IL.EntityStates.FalseSonBoss.FalseSonBossGenericStateWithSwing.GenerateClubOverlapAttack += FalseSonBoss_FalseSonBossGenericStateWithSwing_GenerateClubOverlapAttack;
            IL.EntityStates.FalseSonBoss.TaintedOffering.FireProjectile += FalseSonBoss_TaintedOffering_FireProjectile;
            IL.EntityStates.FlyingVermin.Weapon.Spit.FireProjectile += FlyingVermin_Weapon_Spit_FireProjectile;
            IL.EntityStates.GolemMonster.ClapState.FixedUpdate += GolemMonster_ClapState_FixedUpdate;
            IL.EntityStates.GolemMonster.FireLaser.OnEnter += GolemMonster_FireLaser_OnEnter;
            // unlike everything else under GrandParentBoss, this is used for the modern grandparent
            IL.EntityStates.GrandParentBoss.GroundSwipe.FixedUpdate += GrandParentBoss_GroundSwipe_FixedUpdate;
            IL.EntityStates.GravekeeperBoss.FireHook.FireSingleHook += GravekeeperBoss_FireHook_FireSingleHook;
            IL.EntityStates.GravekeeperMonster.Weapon.GravekeeperBarrage.FireBlob += GravekeeperMonster_Weapon_GravekeeperBarrage_FireBlob;
            IL.EntityStates.GreaterWispMonster.FireCannons.OnEnter += GreaterWispMonster_FireCannons_OnEnter;
            IL.EntityStates.Halcyonite.TriLaser.FireTriLaser += Halcyonite_TriLaser_FireTriLaser;
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateAttack += Halcyonite_WhirlWindPersuitCycle_UpdateAttack;
            IL.EntityStates.HermitCrab.FireMortar.Fire += HermitCrab_FireMortar_Fire;
            IL.EntityStates.ImpBossMonster.FireVoidspikes.FireSpikeAuthority += ImpBossMonster_FireVoidspikes_FireSpikeAuthority;
            IL.EntityStates.ImpBossMonster.FireVoidspikes.OnEnter += ImpBossMonster_FireVoidspikes_OnEnter;
            IL.EntityStates.ImpBossMonster.GroundPound.OnEnter += ImpBossMonster_GroundPound_OnEnter;
            IL.EntityStates.ImpMonster.DoubleSlash.OnEnter += ImpMonster_DoubleSlash_OnEnter;
            IL.EntityStates.JellyfishMonster.JellyNova.Detonate += JellyfishMonster_JellyNova_Detonate;
            IL.EntityStates.LemurianBruiserMonster.FireMegaFireball.FixedUpdate += LemurianBruiserMonster_FireMegaFireball_FixedUpdate;
            IL.EntityStates.LemurianBruiserMonster.Flamebreath.FireFlame += LemurianBruiserMonster_Flamebreath_FireFlame;
            IL.EntityStates.LemurianMonster.Bite.OnEnter += LemurianMonster_Bite_OnEnter;
            IL.EntityStates.LemurianMonster.FireFireball.OnEnter += LemurianMonster_FireFireball_OnEnter;
            IL.EntityStates.LunarGolem.FireTwinShots.FireSingle += LunarGolem_FireTwinShots_FireSingle;
            IL.EntityStates.LunarWisp.FireLunarGuns.OnFireAuthority += LunarWisp_FireLunarGuns_OnFireAuthority;
            IL.EntityStates.LunarWisp.SeekingBomb.FireBomb += LunarWisp_SeekingBomb_FireBomb;
            IL.EntityStates.MajorConstruct.Weapon.FireLaser.ModifyBullet += MajorConstruct_Weapon_FireLaser_ModifyBullet;
            // this doesn't affect the stuff on the floor the projectile leaves, just the initial hit
            IL.EntityStates.MiniMushroom.SporeGrenade.FireGrenade += MiniMushroom_SporeGrenade_FireGrenade;
            IL.EntityStates.NullifierMonster.FirePortalBomb.FireBomb += NullifierMonster_FirePortalBomb_FireBomb;
            IL.EntityStates.ParentMonster.GroundSlam.FixedUpdate += ParentMonster_GroundSlam_FixedUpdate;
            IL.EntityStates.RoboBallBoss.Weapon.FireDelayKnockup.OnEnter += RoboBallBoss_Weapon_FireDelayKnockup_OnEnter;
            IL.EntityStates.RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate += RoboBallBoss_Weapon_FireEyeBlast_FixedUpdate;
            IL.EntityStates.RoboBallMini.Weapon.FireEyeBeam.ModifyBullet += RoboBallMini_Weapon_FireEyeBeam_ModifyBullet;
            IL.EntityStates.ScavMonster.FireEnergyCannon.OnEnter += ScavMonster_FireEnergyCannon_OnEnter;
            IL.EntityStates.ScavMonster.ThrowSack.Fire += ScavMonster_ThrowSack_Fire;
            IL.EntityStates.Scorchling.ScorchlingLavaBomb.Spit += Scorchling_ScorchlingLavaBomb_Spit;
            // you never know if someone somehow is giving squid turrets items
            IL.EntityStates.Squid.SquidWeapon.FireSpine.FireOrbArrow += Squid_SquidWeapon_FireSpine_FireOrbArrow;
            IL.EntityStates.TitanMonster.FireFist.PlaceSingleDelayBlast += TitanMonster_FireFist_PlaceSingleDelayBlast;
            // aurelionite fist attack inherits the normal fist attack changes
            IL.EntityStates.TitanMonster.FireGoldMegaLaser.FixedUpdate += TitanMonster_FireGoldMegaLaser_FixedUpdate;
            IL.EntityStates.TitanMonster.FireMegaLaser.FireBullet += TitanMonster_FireMegaLaser_FireBullet;
            // below is for the stone titan & aurelionite utilities that spawn a thing that shoots automatically
            IL.RoR2.TitanRockController.Fire += TitanRockController_Fire;
            IL.EntityStates.UrchinTurret.Weapon.MinigunFire.OnFireAuthority += UrchinTurret_Weapon_MinigunFire_OnFireAuthority;
            IL.EntityStates.VagrantMonster.FireMegaNova.Detonate += VagrantMonster_FireMegaNova_Detonate;
            IL.EntityStates.VagrantMonster.FireTrackingBomb.FireBomb += VagrantMonster_FireTrackingBomb_FireBomb;
            IL.EntityStates.VagrantMonster.Weapon.JellyBarrage.FireBlob += VagrantMonster_Weapon_JellyBarrage_FireBlob;
            IL.EntityStates.VoidInfestor.Infest.OnEnter += VoidInfestor_Infest_OnEnter;
            IL.EntityStates.VoidJailer.Weapon.Capture2.OnEnter += VoidJailer_Weapon_Capture2_OnEnter;
            IL.EntityStates.VoidMegaCrab.BackWeapon.FireVoidMissiles.FireMissile += VoidMegaCrab_BackWeapon_FireVoidMissiles_FireMissile;
            IL.EntityStates.VoidMegaCrab.Weapon.FireCrabCannonBase.FireProjectile += VoidMegaCrab_Weapon_FireCrabCannonBase_FireProjectile;
            IL.EntityStates.VoidRaidCrab.SpinBeamAttack.FireBeamBulletAuthority += VoidRaidCrab_SpinBeamAttack_FireBeamBulletAuthority;
            IL.EntityStates.VoidRaidCrab.Weapon.FireMissiles.FixedUpdate += VoidRaidCrab_Weapon_FireMissiles_FixedUpdate;
            IL.EntityStates.VoidRaidCrab.Weapon.BaseFireMultiBeam.OnEnter += VoidRaidCrab_Weapon_BaseFireMultiBeam_OnEnter;
            IL.EntityStates.Vulture.Weapon.FireWindblade.OnEnter += Vulture_Weapon_FireWindblade_OnEnter;
            IL.EntityStates.Wisp1Monster.FireEmbers.OnEnter += Wisp1Monster_FireEmbers_OnEnter;
        }



        private static void BasicMeleeAttack_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(0),
                x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                x => x.MatchStfld<OverlapAttack>("damageType")
            ))
            {
                Util.LogILError("GenericProjectileBaseState.FireProjectile", il, c);
                return;
            }

            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<OverlapAttack, EntityStates.BasicMeleeAttack>>((overlapAttack, entityState) =>
            {
                switch (entityState)
                {
                    // im assuming HeroRelicSwing is just the survivor primary but on the boss
                    case EntityStates.FalseSonBoss.HeroRelicSwing:
                    case EntityStates.FalseSonBoss.HeroRelicSwingLeft:
                    case EntityStates.Gup.GupSpikesState:
                    case EntityStates.Halcyonite.GoldenSwipe:
                    case EntityStates.Vermin.Weapon.TongueLash:
                        overlapAttack.damageType.damageSource = DamageSource.Primary;
                        break;
                    case EntityStates.BrotherMonster.SprintBash:
                        overlapAttack.damageType.damageSource = DamageSource.Secondary;
                        break;
                    case EntityStates.Halcyonite.GoldenSlash:
                        overlapAttack.damageType.damageSource = DamageSource.Special;
                        break;
                }
            });
        }

        private static void GenericProjectileBaseState_FireProjectile(ILContext il)
        {
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchStfld<FireProjectileInfo>("crit"),
                x => x.MatchLdloc(2),
                x => x.MatchStloc(1)
            ))
            {
                Util.LogILError("GenericProjectileBaseState.FireProjectile", il, c);
                return;
            }

            c.Emit(OpCodes.Ldloc_1);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<FireProjectileInfo, EntityStates.GenericProjectileBaseState, FireProjectileInfo>>((fireProjectileInfo, entityState) =>
            {
                switch (entityState)
                {
                    case EntityStates.MinorConstruct.Weapon.FireConstructBeam:
                    case EntityStates.VoidJailer.Weapon.Fire:
                    case EntityStates.LunarExploderMonster.Weapon.FireExploderShards:
                    case EntityStates.VoidBarnacle.Weapon.Fire:
                        fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericPrimary);
                        break;
                    case EntityStates.ClayGrenadier.ThrowBarrel:
                        fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericSecondary);
                        break;
                }
                return fireProjectileInfo;
            });
            c.Emit(OpCodes.Stloc_1);
        }



        private static void ArtifactShell_FireSolarFlares_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            // i couldn't get it to shoot what spawning as the artifact shell, but it had a primary skill so i'm just gonna assume it shoots with that
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericPrimary,
                "ArtifactShell.FireSolarFlares.FixedUpdate",
                1,
                il, c
            );
        }

        private static void AcidLarva_LarvaLeap_DetonateAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Primary,
                ILEdits.BlastAttackMatchType.BeforeFalloffModel,
                "AcidLarva.LarvaLeap.DetonateAuthority",
                il, c
            );
        }

        private static void BeetleGuardMonster_GroundSlam_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.BasicDamageCalculation,
                    "BeetleGuardMonster.GroundSlam.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.BeetleGuardMonster.GroundSlam>>((bgGroundSlam) =>
            {
                // need to null-check overlap attacks because multiplayer clients can get an NRE in without it
                if (bgGroundSlam != null && bgGroundSlam.attack != null)
                {
                    bgGroundSlam.attack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void BeetleGuardMonster_FireSunder_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSecondary,
                "BeetleGuardMonster.FireSunder.FixedUpdate",
                1,
                il, c
            );
        }

        private static void BeetleMonster_HeadbuttState_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.BasicDamageCalculation,
                    "BeetleMonster.HeadbuttState.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.BeetleMonster.HeadbuttState>>((beetleHeadbutt) =>
            {
                if (beetleHeadbutt != null && beetleHeadbutt.attack != null)
                {
                    beetleHeadbutt.attack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void BeetleQueenMonster_FireSpit_FireBlob(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "BeetleQueenMonster.FireSpit.FireBlob",
                1,
                il, c
            );
        }

        private static void Bell_BellWeapon_ChargeTrioBomb_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "Bell.BellWeapon.ChargeTrioBomb.FixedUpdate",
                7,
                il, c
            );
        }

        private static void Bison_Headbutt_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.BasicDamageCalculation,
                    "Bison.Headbutt.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.Bison.Headbutt>>((bisonHeadbutt) =>
            {
                if (bisonHeadbutt != null && bisonHeadbutt.attack != null)
                {
                    bisonHeadbutt.attack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void Bison_Charge_ResetOverlapAttack(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.BasicDamageCalculation,
                    "Bison.Charge.ResetOverlapAttack",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.Bison.Charge>>((bisonChargeAttack) =>
            {
                if (bisonChargeAttack != null && bisonChargeAttack.attack != null)
                {
                    bisonChargeAttack.attack.damageType.damageSource = DamageSource.Utility;
                }
            });
        }

        private static void BrotherHaunt_FireRandomProjectiles_FireProjectile(ILContext il)
        {
            ILCursor c = new(il);
            // yes it does the ending explosions via a skill and it's apparently a primary skill
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericPrimary,
                "BrotherHaunt.FireRandomProjectiles.FireProjectile",
                4,
                il, c
            );
        }

        private static void BrotherMonster_FistSlam_FixedUpdate(ILContext il)
        {
            string methodName = "BrotherMonster.FistSlam.FixedUpdate";
            ILCursor c = new(il);
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.AttackerIsGameObject, methodName, il, c))
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.BrotherMonster.FistSlam>>((fistSlamAttack) =>
            {
                fistSlamAttack.attack.damageType.damageSource = DamageSource.Secondary;
            });

            c.Index = 0;
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, methodName, 7, il, c);
        }

        private static void BrotherMonster_ExitSkyLeap_FireRingAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, "BrotherMonster.ExitSkyLeap.FireRingAuthority", 5, il, c);
        }

        private static void BrotherMonster_WeaponSlam_OnEnter(ILContext il)
        {
            string methodName = "BrotherMonster.WeaponSlam.OnEnter";
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchDup(),
                x => x.MatchLdcI4(0),
                x => x.MatchCall(out _),
                x => x.MatchStfld<OverlapAttack>("damageType")
            ))
            {
                Util.LogILError(methodName, il, c);
                return;
            }
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<OverlapAttack>>((mithrixWeaponSlamOverlapAttack) =>
            {
                if (mithrixWeaponSlamOverlapAttack != null)
                {
                    mithrixWeaponSlamOverlapAttack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void BrotherMonster_WeaponSlam_FixedUpdate(ILContext il)
        {
            string methodName = "BrotherMonster.WeaponSlam.FixedUpdate";
            ILCursor c = new(il);


            #region Extra BlastAttack that the attack does
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.AttackerIsGameObject, methodName, il, c))
            {
                return;
            }
            c.EmitDelegate<Action<EntityStates.BrotherMonster.WeaponSlam>>((weaponSlamAttack) =>
            {
                weaponSlamAttack.blastAttack.damageType.damageSource = DamageSource.Primary;
            });
            #endregion


            #region Phase 3 waves
            c.Index = 0;
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCall<ProjectileManager>("get_instance"),
                x => x.MatchLdsfld<EntityStates.BrotherMonster.WeaponSlam>("waveProjectilePrefab")
            ))
            {
                Util.LogILError(methodName, il, c);
            }
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, methodName, 7, il, c);
            #endregion


            #region Phase 3 damaging pillar
            c.Index = 0;
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCall<ProjectileManager>("get_instance"),
                x => x.MatchLdsfld<EntityStates.BrotherMonster.WeaponSlam>("pillarProjectilePrefab")
            ))
            {
                Util.LogILError(methodName, il, c);
            }
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, methodName, 7, il, c);
            #endregion
        }

        private static void BrotherMonster_UltChannelState_FireWave(ILContext il)
        {
            // this isn't on mithrix's normal skill selection so i'm just gonna say it's from the special
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, "BrotherMonster.UltChannelState.FireWave", 7, il, c);
        }

        private static void BrotherMonster_Weapon_FireLunarShards_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericPrimary,
                "BrotherMonster.Weapon.FireLunarShards.OnEnter",
                2,
                il, c
            );
        }

        private static void ChildMonster_SparkBallFire_FireBomb(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, "ChildMonster.SparkBallFire.FireBomb", 3, il, c);
        }

        private static void ClayBoss_FireTarball_FireSingleTarball(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, "ClayBoss.FireTarball.FireSingleTarball", 3, il, c);
        }

        private static void TarTetherController_DoDamageTick(ILContext il)
        {
            // for the dunestrider suck attack
            // also this is almost the exact same as SetupForOverlapAttackDelegate but with DamageInfo instead of OverlapAttack
            string methodName = "TarTetherController.DoDamageTick";
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(0),
                x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                x => x.MatchStfld<DamageInfo>("damageType")
            ))
            {
                Util.LogILError(methodName, il, c);
                return;
            }

            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<DamageInfo>>((damageInfo) =>
            {
                damageInfo.damageType.damageSource = DamageSource.Special;
            });
        }

        private static void ClayBoss_ClayBossWeapon_FireBombardment_FireGrenade(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, "ClayBoss.ClayBossWeapon.FireBombardment.FireGrenade", 20, il, c);
        }

        private static void ClayBruiser_Weapon_MinigunFire_OnFireAuthority(ILContext il)
        {
            string methodName = "ClayBruiser.Weapon.MinigunFire.OnFireAuthority";
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(0),
                x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                x => x.MatchStfld<BulletAttack>("damageType")
            ))
            {
                Util.LogILError(methodName, il, c);
                return;
            }

            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void ClayGrenadier_FaceSlam_FixedUpdate(ILContext il)
        {
            // not going to modify the first damageInfo since that's for self damage
            // and im guessing if i do modify it then it'll be able to waste luminous shot charges
            string methodName = "ClayGrenadier.FaceSlam.FixedUpdate";
            ILCursor c = new(il);
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.ExistingDamageType, methodName, il, c))
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.ClayGrenadier.FaceSlam>>((faceSlamAttack) =>
            {
                faceSlamAttack.attack.damageType.damageSource = DamageSource.Primary;
            });

            c.Index = 0;
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, methodName, 7, il, c);
        }

        private static void Drone_DroneWeapon_FireGatling_OnEnter(ILContext il)
        {
            // this is only for the gunner turret
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                false,
                ILEdits.BulletAttackMatchType.OwnerIsGameObject,
                "Drone.DroneWeapon.FireGatling.OnEnter",
                il, c
            );
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void Drone_DroneWeapon_FireMegaTurret_FireBullet(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                false,
                ILEdits.BulletAttackMatchType.OwnerIsGameObject,
                "Drone.DroneWeapon.FireMegaTurret.OnEnter",
                il, c
            );
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void Drone_DroneWeapon_FireMissileBarrage_FireMissile(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, "Drone.DroneWeapon.FireMissileBarrage.FireMissile", 11, il, c);
        }

        private static void Drone_DroneWeapon_FireTurret_OnEnter(ILContext il)
        {
            // this is actually not for the gunner turret, rather it's for other drones except the mega drone
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                false,
                ILEdits.BulletAttackMatchType.OwnerIsGameObject,
                "Drone.DroneWeapon.FireTurret.OnEnter",
                il, c
            );
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void Drone_DroneWeapon_FireTwinRocket_FireProjectile(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericSecondary, "Drone.DroneWeapon.FireTwinRocket.FireProjectile", 7, il, c);
        }

        private static void Mage_Weapon_Flamethrower_FireGauntlet(ILContext il)
        {
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdflda<BulletAttack>("damageType"),
                x => x.MatchLdcI4(8),
                x => x.MatchStfld<DamageTypeCombo>("damageSource")
            ))
            {
                Util.LogILError("Mage.Weapon.Flamethrower.FireGauntlet", il, c);
                return;
            }

            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<BulletAttack, EntityStates.Mage.Weapon.Flamethrower>>((bulletAttack, entityState) =>
            {
                if (entityState.GetType() != typeof(EntityStates.Drone.DroneWeapon.Flamethrower))
                {
                    return;
                }

                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void FalseSonBoss_CorruptedPaths_DetonateAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Utility,
                ILEdits.BlastAttackMatchType.AttackerIsGameObject,
                "FalseSonBoss.CorruptedPaths.DetonateAuthority",
                il, c
            );
        }

        private static void FalseSonBoss_CorruptedPathsDash_FixedUpdate(ILContext il)
        {
            string methodName = "FalseSonBoss.CorruptedPathsDash.FixedUpdate";
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                x => x.MatchStfld<BlastAttack>("attacker")
            ))
            {
                Util.LogILError(methodName, il, c);
                return;
            }

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EntityStates.FalseSonBoss.CorruptedPathsDash>>((dashAttack) =>
            {
                dashAttack.explosionAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void FalseSonBoss_FissureSlam_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, "FalseSonBoss.FissureSlam.FixedUpdate", 4, il, c);
        }

        private static void FalseSonBoss_FissureSlam_DetonateAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Primary,
                ILEdits.BlastAttackMatchType.AttackerIsGameObject,
                "FalseSonBoss.FissureSlam.DetonateAuthority",
                il, c
            );
        }

        private static void FalseSonBoss_LunarRain_DetonateAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Secondary,
                ILEdits.BlastAttackMatchType.AttackerIsGameObject,
                "FalseSonBoss.LunarRain.DetonateAuthority",
                il, c
            );
        }

        private static void FalseSonBoss_TaintedOffering_FireProjectile(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericSpecial, "FalseSonBoss.TaintedOffering.FireProjectile", 1, il, c);
        }

        private static void FalseSonBoss_PrimeDevastator_DetonateAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Special,
                ILEdits.BlastAttackMatchType.AttackerIsGameObject,
                "FalseSonBoss.PrimeDevastator.DetonateAuthority",
                il, c
            );
        }

        private static void FalseSonBoss_FalseSonBossGenericStateWithSwing_GenerateClubOverlapAttack(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    false,
                    ILEdits.OverlapAttackMatchType.ExistingDamageType,
                    "FalseSonBoss.SwatAwayPlayersSlam.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<OverlapAttack>>((overlapAttack) =>
            {
                if (overlapAttack != null)
                {
                    overlapAttack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void FlyingVermin_Weapon_Spit_FireProjectile(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, "FlyingVermin.Weapon.Spit.FireProjectile", 3, il, c);
        }

        private static void GolemMonster_ClapState_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.AttackerIsGameObject, "GolemMonster.ClapState.FixedUpdate", il, c))
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.GolemMonster.ClapState>>((clappingCheeks) =>
            {
                clappingCheeks.attack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void GolemMonster_FireLaser_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(DamageSource.Secondary, ILEdits.BlastAttackMatchType.AttackerIsGameObject, "GolemMonster.FireLaser.OnEnter", il, c);
        }

        private static void GrandParentBoss_GroundSwipe_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericPrimary,
                "GrandParentBoss.GroundSwipe.FixedUpdate",
                8,
                il, c
            );
        }

        private static void GravekeeperBoss_FireHook_FireSingleHook(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSecondary,
                "GravekeeperBoss.FireHook.FireSingleHook",
                1,
                il, c
            );
        }

        private static void GravekeeperMonster_Weapon_GravekeeperBarrage_FireBlob(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "GravekeeperMonster.Weapon.GravekeeperBarrage.FireBlob",
                0,
                il, c
            );
        }

        private static void GreaterWispMonster_FireCannons_OnEnter(ILContext il)
        {
            // there's a separate FireProjectile for each hand, so 2 separate edits need to happen
            string methodName = "GreaterWispMonster.FireCannons.OnEnter";
            ILCursor c = new(il);


            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(6),
                x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrfalse(out _)
            ))
            {
                Util.LogILError(methodName, il, c);
            }
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                methodName,
                8,
                il, c
            );


            c.Index = 0;
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(7),
                x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrfalse(out _)
            ))
            {
                Util.LogILError(methodName, il, c);
            }
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                methodName,
                8,
                il, c
            );
        }

        private static void Halcyonite_TriLaser_FireTriLaser(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Secondary,
                ILEdits.BlastAttackMatchType.AttackerIsGameObject,
                "Halcyonite.TriLaser.FireTriLaser",
                il, c
            );
        }

        private static void Halcyonite_WhirlWindPersuitCycle_UpdateAttack(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Utility,
                ILEdits.BlastAttackMatchType.ExistingDamageType,
                "Halcyonite.WhirlWindPersuitCycle.UpdateAttack",
                il, c
            );
        }

        private static void HermitCrab_FireMortar_Fire(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "HermitCrab.FireMortar.Fire",
                18,
                il, c
            );
        }

        private static void ImpBossMonster_FireVoidspikes_FireSpikeAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "ImpBossMonster.FireVoidspikes.FireSpikeAuthority",
                1,
                il, c
            );
        }

        private static void ImpBossMonster_FireVoidspikes_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.ExistingDamageType,
                    "ImpBossMonster.FireVoidspikes.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.ImpBossMonster.FireVoidspikes>>((overlordRangedAttack) =>
            {
                if (overlordRangedAttack != null && overlordRangedAttack.attack != null)
                {
                    overlordRangedAttack.attack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void ImpBossMonster_GroundPound_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.AttackerIsGameObject, "ImpBossMonster.GroundPound.OnEnter", il, c))
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.ImpBossMonster.GroundPound>>((groundPound) =>
            {
                groundPound.attack.damageType.damageSource = DamageSource.Secondary;
            });
        }

        private static void ImpMonster_DoubleSlash_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.ExistingDamageType,
                    "ImpMonster.DoubleSlash.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.ImpMonster.DoubleSlash>>((doubleSlash) =>
            {
                if (doubleSlash != null && doubleSlash.attack != null)
                {
                    doubleSlash.attack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void JellyfishMonster_JellyNova_Detonate(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.AttackerIsGameObject, "JellyfishMonster.JellyNova.Detonate", il, c))
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.JellyfishMonster.JellyNova>>((explosion) =>
            {
                explosion.attack.damageType.damageSource = DamageSource.Secondary;
            });
        }

        private static void LemurianBruiserMonster_FireMegaFireball_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "LemurianBruiserMonster.FireMegaFireball.FixedUpdate",
                6,
                il, c
            );
        }

        private static void LemurianBruiserMonster_Flamebreath_FireFlame(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                true,
                ILEdits.BulletAttackMatchType.ExistingDamageType,
                "LemurianBruiserMonster.Flamebreath.FireFlame",
                il, c
            );
            c.EmitDelegate<Action<EntityStates.LemurianBruiserMonster.Flamebreath>>((flameBreath) =>
            {
                flameBreath.bulletAttack.damageType.damageSource = DamageSource.Secondary;
            });
        }

        private static void LemurianMonster_Bite_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.BasicDamageCalculation,
                    "LemurianMonster.Bite.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.LemurianMonster.Bite>>((biteAttack) =>
            {
                if (biteAttack != null && biteAttack.attack != null)
                {
                    biteAttack.attack.damageType.damageSource = DamageSource.Secondary;
                }
            });
        }

        private static void LemurianMonster_FireFireball_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "LemurianMonster.FireFireball.OnEnter",
                2,
                il, c
            );
        }

        private static void LunarGolem_FireTwinShots_FireSingle(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "LunarGolem.FireTwinShots.FireSingle",
                2,
                il, c
            );
        }

        private static void LunarWisp_FireLunarGuns_OnFireAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                false,
                ILEdits.BulletAttackMatchType.ExistingDamageType,
                "LunarWisp.FireLunarGuns.OnFireAuthority",
                il, c
            );
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void LunarWisp_SeekingBomb_FireBomb(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSecondary,
                "LunarWisp.SeekingBomb.FireBomb",
                3,
                il, c
            );
        }

        private static void MajorConstruct_Weapon_FireLaser_ModifyBullet(ILContext il)
        {
            // copying what EngiTurretWeapon FireBeam does to set the DamageTypeCombo to GenericPrimary
            // which funny enough the xi construct laser inherits from, but overrides the ModifyBullet with nothing
            ILCursor c = new(il);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit<BulletAttack>(OpCodes.Ldflda, "damageType");
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit<DamageTypeCombo>(OpCodes.Stfld, "damageSource");
        }

        private static void MiniMushroom_SporeGrenade_FireGrenade(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "MiniMushroom.SporeGrenade.FireGrenade",
                18,
                il, c
            );
        }

        private static void NullifierMonster_FirePortalBomb_FireBomb(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericPrimary,
                "NullifierMonster.FirePortalBomb.FireBomb",
                4,
                il, c
            );
        }

        private static void ParentMonster_GroundSlam_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForBlastAttackDelegate(ILEdits.BlastAttackMatchType.AttackerIsGameObject, "ParentMonster.GroundSlam.FixedUpdate", il, c))
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.ParentMonster.GroundSlam>>((groundSlamAttack) =>
            {
                groundSlamAttack.attack.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void RoboBallBoss_Weapon_FireDelayKnockup_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSpecial,
                "RoboBallBoss.Weapon.FireDelayKnockup.OnEnter",
                10,
                il, c
            );
        }

        private static void RoboBallBoss_Weapon_FireEyeBlast_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "RoboBallBoss.Weapon.FireEyeBlast.FixedUpdate",
                7,
                il, c
            );
        }

        private static void RoboBallMini_Weapon_FireEyeBeam_ModifyBullet(ILContext il)
        {
            // doing the same thing as MajorConstruct_Weapon_FireLaser_ModifyBullet
            ILCursor c = new(il);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit<BulletAttack>(OpCodes.Ldflda, "damageType");
            c.Emit(OpCodes.Ldc_I4_1);
            c.Emit<DamageTypeCombo>(OpCodes.Stfld, "damageSource");
        }

        private static void ScavMonster_FireEnergyCannon_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "ScavMonster.FireEnergyCannon.OnEnter",
                4,
                il, c
            );
        }

        private static void ScavMonster_ThrowSack_Fire(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSecondary,
                "ScavMonster.ThrowSack.Fire",
                17,
                il, c
            );
        }

        private static void Scorchling_ScorchlingLavaBomb_Spit(ILContext il)
        {
            // why is this a secondary?
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSecondary,
                "Scorchling.ScorchlingLavaBomb.Spit",
                18,
                il, c
            );
        }

        private static void Squid_SquidWeapon_FireSpine_FireOrbArrow(ILContext il)
        {
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchNewobj(out _),
                    x => x.MatchStloc(3)
                ))
            {
                Util.LogILError("Squid.SquidWeapon.FireSpine.FireOrbArrow", il, c);
            }

            c.Emit(OpCodes.Ldloc_3);
            c.EmitDelegate<Action<RoR2.Orbs.SquidOrb>>((squidOrb) =>
            {
                squidOrb.damageType.damageSource = DamageSource.Primary;
            });
        }

        private static void TitanMonster_FireFist_PlaceSingleDelayBlast(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericSecondary,
                "TitanMonster.FireFist.PlaceSingleDelayBlast",
                0,
                il, c
            );
        }

        private static void TitanMonster_FireGoldMegaLaser_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSpecial,
                "TitanMonster.FireGoldMegaLaser.FixedUpdate",
                1,
                il, c
            );
        }

        private static void TitanMonster_FireMegaLaser_FireBullet(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                true,
                ILEdits.BulletAttackMatchType.OwnerIsGameObject,
                "TitanMonster.FireMegaLaser.FireBullet",
                il, c
            );
            c.EmitDelegate<Action<EntityStates.TitanMonster.FireMegaLaser>>((laserAttack) =>
            {
                laserAttack.bulletAttack.damageType.damageSource = DamageSource.Special;
            });
        }

        // for stone titan & aurelionite
        private static void TitanRockController_Fire(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericUtility,
                "RoR2.TitanRockController.Fire",
                5,
                il, c
            );
        }

        private static void UrchinTurret_Weapon_MinigunFire_OnFireAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "UrchinTurret.Weapon.MinigunFire.OnFireAuthority",
                4,
                il, c
            );
        }

        private static void VagrantMonster_FireMegaNova_Detonate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Special,
                ILEdits.BlastAttackMatchType.ExistingDamageType,
                "VagrantMonster.FireMegaNova.Detonate",
                il, c
            );
        }

        private static void VagrantMonster_FireTrackingBomb_FireBomb(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSecondary,
                "VagrantMonster.FireTrackingBomb.FireBomb",
                3,
                il, c
            );
        }

        private static void VagrantMonster_Weapon_JellyBarrage_FireBlob(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "VagrantMonster.Weapon.JellyBarrage.FireBlob",
                0,
                il, c
            );
        }

        private static void VoidInfestor_Infest_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!ILEdits.SetupForOverlapAttackDelegate(
                    true,
                    ILEdits.OverlapAttackMatchType.ExistingDamageType,
                    "VoidInfestor.Infest.OnEnter",
                    il, c
                )
            )
            {
                return;
            }

            c.EmitDelegate<Action<EntityStates.VoidInfestor.Infest>>((infestAttack) =>
            {
                if (infestAttack != null && infestAttack.attack != null)
                {
                    infestAttack.attack.damageType.damageSource = DamageSource.Primary;
                }
            });
        }

        private static void VoidJailer_Weapon_Capture2_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall<EntityStates.EntityState>("get_gameObject"),
                x => x.MatchStfld<DamageInfo>("attacker")
            ))
            {
                Util.LogILError("VoidJailer.Weapon.Capture2.OnEnter", il, c);
            }

            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<DamageInfo>>((damageInfo) =>
            {
                damageInfo.damageType.damageSource = DamageSource.Secondary;
            });
        }

        private static void VoidMegaCrab_BackWeapon_FireVoidMissiles_FireMissile(ILContext il)
        {
            string methodName = "VoidMegaCrab.BackWeapon.FireVoidMissiles.FireMissile";
            ILCursor c = new(il);


            if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(0),
                    x => x.MatchCallvirt<UnityEngine.Transform>("get_forward"),
                    x => x.MatchCall(out _)
                ))
            {
                Util.LogILError(methodName, il, c);
            }
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSpecial,
                methodName,
                3,
                il, c
            );


            c.Index = 0;
            if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloc(1),
                    x => x.MatchCallvirt<UnityEngine.Transform>("get_forward"),
                    x => x.MatchCall(out _)
                ))
            {
                Util.LogILError(methodName, il, c);
            }
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericSpecial,
                methodName,
                3,
                il, c
            );
        }

        private static void VoidMegaCrab_Weapon_FireCrabCannonBase_FireProjectile(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "VoidMegaCrab.Weapon.FireCrabCannonBase.FireProjectile",
                5,
                il, c
            );
        }

        private static void VoidRaidCrab_SpinBeamAttack_FireBeamBulletAuthority(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                false,
                ILEdits.BulletAttackMatchType.ExistingDamageType,
                "VoidRaidCrab.SpinBeamAttack.FireBeamBulletAuthority",
                il, c
            );
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Utility;
            });
        }

        private static void VoidRaidCrab_Weapon_FireMissiles_FixedUpdate(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileInfo(
                DamageTypeCombo.GenericPrimary,
                "VoidRaidCrab.Weapon.FireMissiles.FixedUpdate",
                2,
                il, c
            );
        }

        private static void VoidRaidCrab_Weapon_BaseFireMultiBeam_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetDamageSourceForBlastAttack(
                DamageSource.Secondary,
                ILEdits.BlastAttackMatchType.ExistingDamageType,
                "VoidRaidCrab.Weapon.BaseFireMultiBeam.OnEnter",
                il, c
            );
        }

        private static void Vulture_Weapon_FireWindblade_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.AddDamageTypeComboToFireProjectileWithoutDamageType(
                DamageTypeCombo.GenericPrimary,
                "Vulture.Weapon.FireWindblade.OnEnter",
                3,
                il, c
            );
        }

        private static void Wisp1Monster_FireEmbers_OnEnter(ILContext il)
        {
            ILCursor c = new(il);
            ILEdits.SetupForBulletAttackDelegate(
                false,
                ILEdits.BulletAttackMatchType.OwnerIsGameObject,
                "Wisp1Monster.FireEmbers.OnEnter",
                il, c
            );
            c.EmitDelegate<Action<BulletAttack>>((bulletAttack) =>
            {
                bulletAttack.damageType.damageSource = DamageSource.Primary;
            });
        }
    }
}