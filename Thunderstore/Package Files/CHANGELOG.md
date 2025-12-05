## 2.0.0

- Fixed for AC release
- Added DamageSources for AC enemies, bosses and drones
- Huge internal refactor
- - All hooks have been redone (somewhat) & ported to MonoDetour
- Made drone DamageSources a config option
- - This is incase it's too OP with operator's nanobugged debuff
- Fixed a bunch of things from before not having a DamageSource
- - Clay Templar tar airblast
- - Imp Overlord teleport
- - Alloy Worship Unit leftover ground damage zones from it's projectiles
- - Both worm bosses contact damage
- - Void Jailer tether damage tick
- - Void Devastator sticky bomb shots explosions
- - Scorch wurm ground breach blast
- Added support for [EnemiesPlusPlus](https://thunderstore.io/c/riskofrain2/p/score/EnemiesPlusPlus/)
- Made all damage zone config options default to true

## 1.2.1

- Add support for [AugmentedVoidReaver](https://thunderstore.io/c/riskofrain2/p/Nuxlar/AugmentedVoidReaver/)

## 1.2.0

- Changed the config options' section name
- - Probably means the config options have been reset so double check them if you changed any
- Fixed FlatItemBuff's squid polyp edit breaking this mod's squid polyp edit
- Added a DamageSource to the scorch wurm shot's damage zone
- - This also configurable just like the other damage zones (this one is on by default)


## 1.1.0

- Fixed for SOTS phase 2

- Gave the flamethrower drone the correct DamageSource
- - It was basing itself off of Artificer's, so it had the "Special" DamageSource before this. Now it has the "Primary" DamageSource as it should.

- Added a few config options for some enemies' ground damage zones getting a DamageSource
- - Mini Mushrum's spore attack (on by default)
- - Beetle Queen's spit attack (off by default)
- - Voidling's one attack that leaves a big damage zone (off by default)

## 1.0.0

- First release