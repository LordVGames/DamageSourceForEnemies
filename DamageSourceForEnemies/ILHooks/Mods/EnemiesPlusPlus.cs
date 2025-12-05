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

namespace DamageSourceForEnemies.ILHooks.Mods
{
    internal static class EnemiesPlusPlus
    {
        [MonoDetourTargets(typeof(EnemiesPlus.Content.Beetle.BeetleSpit))]
        internal static class Beetle
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EnemiesPlus.Content.Beetle.BeetleSpit.Fire.ILHook(BeetleSpit_Fire);


                EnemiesPlus.Content.Beetle.BeetleSpit.projectilePrefab.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Secondary;
            }

            private static void BeetleSpit_Fire(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Secondary, info);
            }
        }



        [MonoDetourTargets(typeof(EnemiesPlus.Content.Wisp.FireBlast))]
        internal static class LesserWisp
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EnemiesPlus.Content.Wisp.FireBlast.Fire.ILHook(FireBlast_Fire);


                EnemiesPlus.Content.Wisp.FireBlast.projectilePrefab.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Primary;
            }

            private static void FireBlast_Fire(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Primary, info);
            }
        }


        [MonoDetourTargets(typeof(EnemiesPlus.Content.Imp.ImpVoidSpike))]
        internal static class Imp
        {
            [MonoDetourHookInitialize]
            private static void Setup()
            {
                Mdh.EnemiesPlus.Content.Imp.ImpVoidSpike.HandleSlash.ILHook(ImpVoidSpike_HandleSlash);


                EnemiesPlus.Content.Imp.ImpVoidSpike.projectilePrefab.GetComponent<ProjectileDamage>().damageType.damageSource = DamageSource.Secondary;
            }

            private static void ImpVoidSpike_HandleSlash(ILManipulationInfo info)
            {
                ILHelpers.Projectiles.ReplaceNullDamageSourceInFireProjectile(DamageSource.Secondary, info);
            }
        }
    }
}