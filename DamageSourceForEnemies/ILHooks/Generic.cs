using EntityStates;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DamageSourceForEnemies.ILHooks
{
    
    [MonoDetourTargets(typeof(BasicMeleeAttack))]
    [MonoDetourTargets(typeof(GenericProjectileBaseState))]
    [MonoDetourTargets(typeof(BaseState))]
    internal static class Generic
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            // a bunch of enemy attacks inherit BasicMeleeAttack, some of which don't have an OnEnter to hook into
            // so im just gonna hook into the base class and check for the attacks i need there
            Mdh.EntityStates.BasicMeleeAttack.OnEnter.ILHook(BasicMeleeAttack_OnEnter);
            // same goes for generic projectiles
            Mdh.EntityStates.GenericProjectileBaseState.FireProjectile.ILHook(GenericProjectileBaseState_FireProjectile);
            Mdh.EntityStates.BaseState.InitMeleeOverlap.Postfix(BaseState_InitMeleeOverlap);
        }



        private static void BasicMeleeAttack_OnEnter(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            w.MatchRelaxed(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BasicMeleeAttack>("damageType"),
                x => x.MatchStfld<OverlapAttack>("damageType") && w.SetCurrentTo(x)
            ).ThrowIfFailure();
            w.InsertAfterCurrent(
                w.Create(OpCodes.Dup),
                w.Create(OpCodes.Ldarg_0),
                w.CreateCall(SetMeleeAttackDamageSource)
            );
        }
        private static void SetMeleeAttackDamageSource(OverlapAttack overlapAttack, BasicMeleeAttack entityState)
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
        }



        private static void GenericProjectileBaseState_FireProjectile(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            w.MatchRelaxed(
                x => x.MatchStfld<FireProjectileInfo>("crit"),
                x => x.MatchLdloc(2),
                x => x.MatchStloc(1) && w.SetCurrentTo(x)
            ).ThrowIfFailure();
            w.InsertAfterCurrent(
                w.Create(OpCodes.Ldloc_1),
                w.Create(OpCodes.Ldarg_0),
                w.CreateCall(EditAndReturnFireProjectileInfo),
                w.Create(OpCodes.Stloc_1)
            );
        }
        private static FireProjectileInfo EditAndReturnFireProjectileInfo(FireProjectileInfo fireProjectileInfo, GenericProjectileBaseState entityState)
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
        }


        private static void BaseState_InitMeleeOverlap(BaseState self, ref float damageCoefficient, ref GameObject hitEffectPrefab, ref Transform modelTransform, ref string hitboxGroupName, ref OverlapAttack returnValue)
        {
            switch (self)
            {
                case EntityStates.ExtractorUnit.ExtractLunge:
                case EntityStates.SolusAmalgamator.Thruster:
                    returnValue.damageType.damageSource = DamageSource.Primary;
                    break;
                case EntityStates.FalseSonBoss.CorruptedPathsDash:
                    returnValue.damageType.damageSource = DamageSource.Utility;
                    break;
            }
        }

    }
}