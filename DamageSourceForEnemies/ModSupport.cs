using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.Projectile;

namespace DamageSourceForEnemies
{
    internal static class ModSupport
    {
        internal static class AugmentedVoidReaverMod
        {
            // it doesn't provide it's own plugin GUID
            internal const string GUID = "com.Nuxlar.AugmentedVoidReaver";
            private static bool? _modexists;
            internal static bool ModIsRunning
            {
                get
                {
                    if (_modexists == null)
                    {
                        _modexists = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(GUID);
                    }
                    return (bool)_modexists;
                }
            }

            [HarmonyPatch]
            public class HarmonyPatches
            {
                [HarmonyPatch(typeof(AugmentedVoidReaver.BetterPortalBomb), nameof(AugmentedVoidReaver.BetterPortalBomb.FireBomb))]
                [HarmonyILManipulator]
                public static void AddDamageSource(ILContext il)
                {
                    ILCursor c = new(il);

                    if (!c.TryGotoNext(MoveType.AfterLabel,
                        x => x.MatchLdloc(1),
                        x => x.MatchCallvirt<ProjectileManager>("FireProjectile")
                    ))
                    {
                        Log.Error("COULD NOT IL HOOK AugmentedVoidReaver.BetterPortalBomb.FireBomb");
                        Log.Warning($"cursor is {c}");
                        Log.Warning($"il is {il}");
                    }

                    c.Emit(OpCodes.Ldloc, 1);
                    c.EmitDelegate<Func<FireProjectileInfo, FireProjectileInfo>>((fireProjectileInfo) =>
                    {
                        fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericPrimary);
                        return fireProjectileInfo;
                    });
                    c.Emit(OpCodes.Stloc_1);
                }
            }
        }
    }
}