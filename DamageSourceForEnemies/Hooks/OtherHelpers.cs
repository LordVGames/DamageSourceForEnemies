using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
namespace DamageSourceForEnemies.Hooks;


internal static class OtherHelpers
{
    internal static void SetProjectileDamageComponentDamageSource(this GameObject gameObject, DamageSource damageSource)
    {
        gameObject.GetComponent<ProjectileDamage>().damageType.damageSource = damageSource;
    }
}