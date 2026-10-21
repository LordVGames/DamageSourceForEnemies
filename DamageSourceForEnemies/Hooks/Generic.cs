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
namespace DamageSourceForEnemies.Hooks;


[MonoDetourTargets(typeof(BasicMeleeAttack))]
[MonoDetourTargets(typeof(GenericProjectileBaseState))]
[MonoDetourTargets(typeof(BaseState))]
[MonoDetourTargets(typeof(MissileUtils))]
[MonoDetourTargets(typeof(ProjectileFireChildren))]
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
        Mdh.RoR2.Projectile.ProjectileFireChildren.FireChild.ILHook(ProjectileFireChildren_FireChild);
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
            case EntityStates.Vermin.Weapon.TongueLash:
                overlapAttack.damageType.damageSource = DamageSource.Primary;
                break;
            case EntityStates.Mimic.Monster.Devour:
                overlapAttack.damageType.damageSource = DamageSource.Secondary;
                break;
        }
    }



    private static void GenericProjectileBaseState_FireProjectile(ILManipulationInfo info)
    {
        ILWeaver w = new(info);
        w.MatchRelaxed(
            x => x.MatchCallOrCallvirt<ProjectileManager>("FireProjectile") && w.SetCurrentTo(x)
        ).ThrowIfFailure()
        .InsertBeforeCurrent(
            w.Create(OpCodes.Ldarg_0),
            w.CreateCall(EditAndReturnFireProjectileInfo)
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
            case EntityStates.LynxArcher.FireArrow:
            case EntityStates.LynxArcher.FireArrowMultiple:
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



    // this shit is so ASS!!!!!! why does damage source not transfer!!!!
    private static void ProjectileFireChildren_FireChild(ILManipulationInfo info)
    {
        ILWeaver w = new(info);
        w.MatchRelaxed(
            x => x.MatchLdloc(3),
            x => x.MatchLdarg(0) && w.SetCurrentTo(x),
            x => x.MatchLdfld<ProjectileFireChildren>("projectileDamage"),
            x => x.MatchLdfld<ProjectileDamage>("crit"),
            x => x.MatchStfld<ProjectileDamage>("crit")
        ).ThrowIfFailure()
        .InsertAfterCurrent(
            w.CreateDelegateCall((ProjectileDamage projectileDamage, ProjectileFireChildren projectileFireChildren) =>
            {
                projectileDamage.damageType.damageSource = projectileFireChildren.projectileDamage.damageType.damageSource;
                return projectileDamage;
            }),
            w.Create(OpCodes.Ldarg_0)
        );
    }
}