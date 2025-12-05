using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace DamageSourceForEnemies.ILHooks.Mods
{
    [MonoDetourTargets(typeof(AugmentedVoidReaver.BetterPortalBomb))]
    internal static class AugmentedVoidReaverMod
    {
        // it doesn't provide it's own plugin GUID
        internal const string GUID = "com.Nuxlar.AugmentedVoidReaver";

        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            Mdh.AugmentedVoidReaver.BetterPortalBomb.FireBomb.ILHook(BetterPortalBomb_FireBomb);
        }

        private static void BetterPortalBomb_FireBomb(ILManipulationInfo info)
        {
            ILHelpers.Projectiles.AddDamageTypeComboToFireProjectileInfo(DamageTypeCombo.GenericPrimary, info);
        }
    }
}