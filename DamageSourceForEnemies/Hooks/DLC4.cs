using EntityStates;
using EntityStates.BigMush;
using EntityStates.ChampRushReaper;
using EntityStates.Lynx2Monster;
using EntityStates.LynxAmbusher;
using EntityStates.Mimic.Monster;
using EntityStates.MiniLynx;
using EntityStates.Swift;
using EntityStates.Totem;
using Mdh.EntityStates.BigMush.Regrowth;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.HookGen;
using MonoMod.Cil;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace DamageSourceForEnemies.Hooks;


/// <summary>
/// 
/// AKA Hallowed Concepts
/// Lynx archer's attacks are entirely handled by EditAndReturnFireProjectileInfo in Generic.cs
/// 
/// </summary>
internal static class DLC4
{
    [MonoDetourTargets(typeof(Proliferate))]
    [MonoDetourTargets(typeof(Regrowth))]
    [MonoDetourTargets(typeof(SporeCoat))]
    // Shruum's secondary projectiles already have their DamageSource as secondary but none of it's other attacks do???
    private static class Shruum
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.BigMush.Proliferate.FireBlastAttack.ILHook(Proliferate_FireBlastAttack);
            Mdh.EntityStates.BigMush.Proliferate.FireHeadSmashMeleeAttack.Prefix(Proliferate_FireHeadSmashMeleeAttack);
            Mdh.EntityStates.BigMush.Proliferate.FireMushProjectile.ILHook(Proliferate_FireMushProjectile);
            Mdh.EntityStates.BigMush.Regrowth.FireBlastAttack.ILHook(Regrowth_FireBlastAttack);
            Mdh.EntityStates.BigMush.Regrowth.FireJumpAttack.Prefix(Regrowth_FireJumpAttack);
            Mdh.EntityStates.BigMush.Regrowth.FireLandingBlastAttack.Prefix(Regrowth_FireLandingBlastAttack);
            Mdh.EntityStates.BigMush.Regrowth.FireMushrumProjectiles.ILHook(Regrowth_FireMushrumProjectiles);
            Mdh.EntityStates.BigMush.SporeCoat.FireBlastAttack.ILHook(SporeCoat_FireBlastAttack);
        }


        private static void SporeCoat_FireBlastAttack(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
        }


        private static void Regrowth_FireMushrumProjectiles(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Utility, info);
        }


        private static void Regrowth_FireLandingBlastAttack(Regrowth self)
        {
            self._landingBlastAttack?.damageType.damageSource = DamageSource.Utility;
        }


        private static void Regrowth_FireJumpAttack(Regrowth self)
        {
            self._jumpAttack?.damageType.damageSource = DamageSource.Utility;
        }


        private static void Regrowth_FireBlastAttack(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Utility, info);
        }


        private static void Proliferate_FireMushProjectile(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Primary, info);
        }


        private static void Proliferate_FireHeadSmashMeleeAttack(Proliferate self)
        {
            self._smashAttack?.damageType.damageSource = DamageSource.Primary;
        }


        private static void Proliferate_FireBlastAttack(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
        }
    }



    // Gilded wurm projectiles are already handled by hooks in the WormBosses class in BaseGame.cs
    [MonoDetourTargets]
    private static class GildedWurms
    {
        private static readonly AssetReferenceT<GameObject> _laserGildedWurmBody = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4.GildedWurmLaserBody_prefab);
        private static readonly AssetReferenceT<GameObject> _orbGildedWurmBody = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4.GildedWurmOrbBody_prefab);


        [MonoDetourHookInitialize]
        private static void Setup()
        {
            if (ConfigOptions.WormContactDamageSources.Value)
            {
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_laserGildedWurmBody).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ContactDamage>().damageType.damageSource = DamageSource.Primary;
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_orbGildedWurmBody).Completed += (handle) =>
                {
                    handle.Result.GetComponent<ContactDamage>().damageType.damageSource = DamageSource.Primary;
                };
            }
        }
    }



    private static class Lynxes
    {
        [MonoDetourTargets(typeof(FrenziedUpsetPoundSlamState))]
        [MonoDetourTargets(typeof(TwoSwipes))]
        private static class LynxBerserker
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.Lynx2Monster.FrenziedUpsetPoundSlamState.DetonateAuthority.ILHook(FrenziedUpsetPoundSlamState_DetonateAuthority);
                Mdh.EntityStates.Lynx2Monster.TwoSwipes.HandleSlash.Prefix(TwoSwipes_HandleSlash);
            }


            private static void TwoSwipes_HandleSlash(TwoSwipes self, ref string animatorParamName, ref string muzzleName, ref string hitBoxGroupName)
            {
                self.attack?.damageType.damageSource = DamageSource.Primary;
            }


            private static void FrenziedUpsetPoundSlamState_DetonateAuthority(ILManipulationInfo info)
            {
                ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Secondary, info);
            }
        }



        [MonoDetourTargets(typeof(SpearStrike))]
        [MonoDetourTargets(typeof(SpearStrikeStationary))]
        // idk if this next one is used but im just gonna cover it anyways
        [MonoDetourTargets(typeof(HunterHurlThrow))]
        private static class LynxAmbusher
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.LynxAmbusher.SpearStrike.FireSpearStrike.Prefix(SpearStrike_FireSpearStrike);
                Mdh.EntityStates.LynxAmbusher.SpearStrikeStationary.FireSpearStrike.Prefix(SpearStrikeStationary_FireSpearStrike);
                Mdh.EntityStates.LynxAmbusher.HunterHurlThrow.FixedUpdate.ILHook(HunterHurlThrow_FixedUpdate);
            }


            private static void HunterHurlThrow_FixedUpdate(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
            }


            private static void SpearStrikeStationary_FireSpearStrike(SpearStrikeStationary self)
            {
                self.attack?.damageType.damageSource = DamageSource.Primary;
            }


            private static void SpearStrike_FireSpearStrike(SpearStrike self)
            {
                self.attack?.damageType.damageSource = DamageSource.Primary;
            }
        }



        [MonoDetourTargets(typeof(EntityStates.LynxGhostTurret.GhostArrow))]
        private static class LynxGhostTurret
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.LynxGhostTurret.GhostArrow.FixedUpdate.ILHook(GhostArrow_FixedUpdate);
            }


            private static void GhostArrow_FixedUpdate(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
            }
        }



        [MonoDetourTargets(typeof(Nibble))]
        private static class LynxPup
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.MiniLynx.Nibble.AuthorityModifyOverlapAttack.Postfix(Nibble_AuthorityModifyOverlapAttack);
            }


            private static void Nibble_AuthorityModifyOverlapAttack(Nibble self, ref OverlapAttack overlapAttack)
            {
                overlapAttack.damageType.damageSource = DamageSource.Primary;
            }
        }



        // this seems unused but idk
        [MonoDetourTargets(typeof(EntityStates.SpiritMonster.RingOfFire))]
        private static class Spirit
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.SpiritMonster.RingOfFire.FireProjectile.ILHook(RingOfFire_FireProjectile);
            }


            private static void RingOfFire_FireProjectile(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Primary, info);
            }
        }



        [MonoDetourTargets(typeof(GroundPound))]
        [MonoDetourTargets(typeof(SacrificeSuccess))]
        [MonoDetourTargets(typeof(SoulBlast))]
        [MonoDetourTargets(typeof(SoulBlast))]
        // sacrifice ally skill (secondary) damage source is already set
        private static class Totem
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.Totem.GroundPound.FireLandingAOEAttackServer.ILHook(GroundPound_FireLandingAOEAttackServer);
                Mdh.EntityStates.Totem.GroundPound.OnHitGround.Prefix(GroundPound_OnHitGround);
                Mdh.EntityStates.Totem.SacrificeSuccess.FireProjectilesAuthority.ILHook(SacrificeSuccess_FireProjectilesAuthority);
                Mdh.EntityStates.Totem.SoulBlast.FixedUpdate.ILHook(SoulBlast_FixedUpdate);
            }


            private static void SoulBlast_FixedUpdate(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericUtility, info);
            }


            private static void SacrificeSuccess_FireProjectilesAuthority(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
            }


            private static void GroundPound_OnHitGround(GroundPound self)
            {
                self._attack?.damageType.damageSource = DamageSource.Primary;
            }


            private static void GroundPound_FireLandingAOEAttackServer(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.OverrideFireProjectileWithoutDamageType(DamageTypeCombo.GenericPrimary, info);
            }
        }
    }



    [MonoDetourTargets(typeof(EntityStates.Mimic.Devour.DevouredBody))]
    [MonoDetourTargets(typeof(Lick))]
    // Mimic devour attack is handled in SetMeleeAttackDamageSource in Generic.cs
    // SpitOutDevouredBody's projectile's DamageSource is already set to SkillMask
    private static class Mimic
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Mimic.Devour.DevouredBody.FixedUpdate.ILHook(DevouredBody_FixedUpdate);
            Mdh.EntityStates.Mimic.Monster.Lick.AuthorityModifyOverlapAttack.Postfix(Lick_AuthorityModifyOverlapAttack);
        }


        private static void Lick_AuthorityModifyOverlapAttack(Lick self, ref OverlapAttack overlapAttack)
        {
            overlapAttack.damageType.damageSource = DamageSource.Primary;
        }


        private static void DevouredBody_FixedUpdate(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            w.MatchRelaxed(
                x => x.MatchDup(),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EntityState>("get_gameObject"),
                x => x.MatchStfld<DamageInfo>("inflictor"),
                x => x.MatchDup() && w.SetCurrentTo(x)
            ).ThrowIfFailure()
            .InsertAfterCurrent(
                w.CreateDelegateCall((DamageInfo damageInfo) =>
                {
                    damageInfo.damageType.damageSource = DamageSource.Primary;
                    return damageInfo;
                })
            );
        }
    }



    /*
     * green lemurian takes the damagesources of the normal lemurian (which are none, except with this mod)
     * BUT the skills are swapped around on the green lemurian????? so the damagesources are wrong
     * sooo we'll just switch the skills back around
    */
    [MonoDetourTargets]
    private static class PolymorphGreenLemurian
    {
        private static readonly AssetReferenceT<SkillFamily> _polymorphLemurianPrimarySkillFamily = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_ShrineCompulsion.PolyLemurianPrimaryFamily_asset);
        private static SkillFamily _primarySkillFamily = null;
        private static readonly AssetReferenceT<SkillFamily> _polymorphLemurianSecondarySkillFamily = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_ShrineCompulsion.PolyLemurianSecondaryBody_asset);
        private static SkillFamily _secondarySkillFamily = null;


        [MonoDetourHookInitialize]
        private static void Setup()
        {
            AssetAsyncReferenceManager<SkillFamily>.LoadAsset(_polymorphLemurianPrimarySkillFamily).Completed += (handle) =>
            {
                _primarySkillFamily = handle.Result;
                OnBothSkillFamiliesLoaded();
            };
            AssetAsyncReferenceManager<SkillFamily>.LoadAsset(_polymorphLemurianSecondarySkillFamily).Completed += (handle) =>
            {
                _secondarySkillFamily = handle.Result;
                OnBothSkillFamiliesLoaded();
            };
        }


        private static void OnBothSkillFamiliesLoaded()
        {
            if (_primarySkillFamily == null || _secondarySkillFamily == null)
            {
                return;
            }


            // ty visual studio quick actions
            (_secondarySkillFamily.variants, _primarySkillFamily.variants) = (_primarySkillFamily.variants, _secondarySkillFamily.variants);
        }
    }



    // providence and a most of his projectiles already have DamageSources (ty dlc devs!)
    // only this that doesn't are his malachite energy shots, and those are used in a lot of his attacks
    // so for once we'll use the SkillMask DamageSource, since it counts as skill damage but not from any specific one
    [MonoDetourTargets(typeof(ProvidenceController))]
    private static class Providence
    {
        private static readonly AssetReferenceT<GameObject> _malachiteEnergyProjectile = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4.MalachiteEnergyProjectile_prefab);


        [MonoDetourHookInitialize]
        private static void Setup()
        {
            AssetAsyncReferenceManager<GameObject>.LoadAsset(_malachiteEnergyProjectile).Completed += (handle) =>
            {
                handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.SkillMask);
            };
        }
    }



    // this guy wasn't the easiest to test and find the right things to add damagesources to, could possibly be some things from em missing a damage source
    private static class AnimaHarvester
    {
        [MonoDetourTargets(typeof(AssailState))]
        [MonoDetourTargets(typeof(FireBrambles))]
        [MonoDetourTargets(typeof(Reap2))]
        [MonoDetourTargets(typeof(Haunt4CastEnter))]
        private static class Phase1
        {
            private static readonly AssetReferenceT<GameObject> _swipeProjectileLeft = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_ReaperBoss.ReaperBossAssailSlashProjectile_prefab);
            private static readonly AssetReferenceT<GameObject> _swipeProjectileRight = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_ReaperBoss.ReaperBossAssailSlashProjectileRightVariant_prefab);
            private static readonly AssetReferenceT<GameObject> _swipeProjectileLeftPhase2 = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4.ReaperBossAssailSlashMalachiteProjectile_prefab);
            private static readonly AssetReferenceT<GameObject> _swipeProjectileRightPhase2 = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4.ReaperBossAssailSlashMalachiteProjectileRightVariant_prefab);
            private static readonly AssetReferenceT<GameObject> _tornadoRecastProjectile = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_ReaperBoss.SoulStormRecastBossProjectile_prefab);
            private static readonly AssetReferenceT<GameObject> _brambleStormGroundProjectile = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC4_ReaperBoss.BrambleStormSpikeProjectileGround_prefab);
            // idk where tf in the code these are spawned man


            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EntityStates.ChampRushReaper.AssailState.FireMeleeSlash.Prefix(AssailState_FireMeleeSlash);
                Mdh.EntityStates.ChampRushReaper.AssailState.FireSwipeProjectileAuthority.ILHook(AssailState_FireSwipeProjectileAuthority);
                Mdh.EntityStates.ChampRushReaper.FireBrambles.FireBrambleAuthority.ILHook(FireBrambles_FireBrambleAuthority);
                Mdh.EntityStates.ChampRushReaper.Reap2.FireReapOverlapAttack.Prefix(Reap2_FireReapOverlapAttack);
                Mdh.EntityStates.ChampRushReaper.Haunt4CastEnter.FixedUpdate.ILHook(Haunt4CastEnter_FixedUpdate);


                AssetAsyncReferenceManager<GameObject>.LoadAsset(_swipeProjectileLeft).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_swipeProjectileRight).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_swipeProjectileLeftPhase2).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_swipeProjectileRightPhase2).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Primary);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_tornadoRecastProjectile).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Special);
                };
                AssetAsyncReferenceManager<GameObject>.LoadAsset(_brambleStormGroundProjectile).Completed += (handle) =>
                {
                    handle.Result.SetProjectileDamageComponentDamageSource(DamageSource.Secondary);
                };
            }


            private static void Haunt4CastEnter_FixedUpdate(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSpecial, info);
            }


            private static void Reap2_FireReapOverlapAttack(Reap2 self, ref string hitBoxGroupName)
            {
                self._swingAttack?.damageType.damageSource = DamageSource.Utility;
            }


            private static void FireBrambles_FireBrambleAuthority(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericSecondary, info);
            }


            private static void AssailState_FireSwipeProjectileAuthority(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Primary, info);
            }


            private static void AssailState_FireMeleeSlash(AssailState self, ref string hitBoxGroupName, ref string effectMuzzleName)
            {
                self._swingAttack?.damageType.damageSource = DamageSource.Primary;
            }
        }
    }



    // these guys are unused but just in case these guys get used at some point
    [MonoDetourTargets(typeof(FlyingToDive))]
    [MonoDetourTargets(typeof(Stuck))]
    // they set a damage source on the swift's other attacks????
    private static class Swift
    {
        [MonoDetourHookInitialize]
        private static void Setup()
        {
            Mdh.EntityStates.Swift.FlyingToDive.ExecuteSlamEffect.ILHook(FlyingToDive_ExecuteSlamEffect);
            Mdh.EntityStates.Swift.FlyingToDive.OnEnter.Postfix(FlyingToDive_OnEnter);
            Mdh.EntityStates.Swift.Stuck.OnEnter.ILHook(Swift_Stuck);
        }


        private static void Swift_Stuck(ILManipulationInfo info)
        {
            ILHelpers.OverrideDamageInfoDamageSource(DamageSource.Primary, info);
        }


        private static void FlyingToDive_OnEnter(FlyingToDive self)
        {
            self.grabOverlapAttack?.damageType.damageSource = DamageSource.Primary;
        }


        private static void FlyingToDive_ExecuteSlamEffect(ILManipulationInfo info)
        {
            ILHelpers.BlastAttacks.OverrideBlastAttackDamageSource(DamageSource.Primary, info);
        }
    }
}