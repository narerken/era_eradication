# DECISIONS.md

Dated log of what was checked, found broken, and fixed on this project.

2026-11-18  Jump worked from the keyboard but not the on-screen button. MobileInput.EndFrame() was resetting JumpPressedThisFrame before PlayerController2D read it. Moved the reset into a new ConsumeJumpPressed(), called only after the jump buffer consumes it.

2026-11-18  Player fell straight through the goal instead of stopping in it. Added PlayerState.LevelComplete; Goal now calls PlayerController2D.EnterLevelComplete(), which zeroes velocity and switches the Rigidbody2D to Kinematic.

2026-11-18  EnemyHealth.TakeDamage() existed but nothing ever called it - the player had no way to kill enemies. Added a stomp check in EnemyBase: landing on an enemy from above deals stomp damage and bounces the player; a side hit still damages the player instead.

2026-11-18  Enemy contact and Hazard damage only fired once per overlap, so standing in a hazard after invulnerability wore off dealt no further damage. Added OnCollisionStay2D / OnTriggerStay2D next to the existing Enter events in EnemyBase and Hazard.

2026-11-18  LevelData.patrolRadius was set per spawn in every level but never read by LevelSpawner. Added EnemyBase.Configure(radius), called right after each enemy is instantiated.

2026-11-18  Added a Hazard system (spike strips) and a real pickup sprite. New HazardSprite()/EmberCoinSprite() in ProceduralSprites, a Hazard component, LevelData.hazards, and LevelSpawner building them. Removed the placeholder fruit pickup (RealArt.OrangeSpin()).

2026-11-19  Swapped in 3 new enemy sprite sets (Skull/Ghosts/Mouth) for Patroller/Shooter/Charger. Pickups no longer spawn within 0.8 units of a stationary Shooter (LevelSpawner.IsNearStationaryEnemy).

2026-11-19  Removed assets nothing referenced anymore: the old MaskDude/NinjaFrog/PinkMan character sprites, the old fruit pickup frames, and the ProceduralSprites methods that generated sprites no longer used by anything (PlayerSprite, GroundTileSprite, PatrollerSprite, ShooterSprite, ChargerSprite, ProjectileSprite, EmberShardSprite, HeartSprite, ButtonSprite, RingSprite, SkyBackgroundSprite).

2026-11-19  Stripped comments from every script and switched this file to the dated-log format above.
