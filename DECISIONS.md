# DECISIONS.md

Dated log of what was checked, found broken, and fixed on this project.

2026-11-18  Jump worked from the keyboard but not the on-screen button. MobileInput.EndFrame() was resetting JumpPressedThisFrame before PlayerController2D read it. Moved the reset into a new ConsumeJumpPressed(), called only after the jump buffer consumes it.

2026-11-18  Player fell straight through the goal instead of stopping in it. Added PlayerState.LevelComplete; Goal now calls PlayerController2D.EnterLevelComplete(), which zeroes velocity and switches the Rigidbody2D to Kinematic.

2026-11-18  EnemyHealth.TakeDamage() existed but nothing ever called it - the player had no way to kill enemies. Added a stomp check in EnemyBase: landing on an enemy from above deals stomp damage and bounces the player; a side hit still damages the player instead.

2026-11-18  Enemy contact and Hazard damage only fired once per overlap, so standing in a hazard after invulnerability wore off dealt no further damage. Added OnCollisionStay2D / OnTriggerStay2D next to the existing Enter events in EnemyBase and Hazard.

2026-11-18  LevelData.patrolRadius was set per spawn in every level but never read by LevelSpawner. Added EnemyBase.Configure(radius), called right after each enemy is instantiated.

2026-11-18  Added a Hazard system (spike strips) and a real pickup sprite. New HazardSprite()/EmberCoinSprite() in ProceduralSprites, a Hazard component, LevelData.hazards, and LevelSpawner building them. Removed the placeholder fruit pickup (RealArt.OrangeSpin()).

2026-11-19  Swapped in 3 new enemy sprite sets (Skull/Ghosts/Mouth) for Patroller/Shooter/Charger. Pickups no longer spawn within 0.8 units of a stationary Shooter (LevelSpawner.IsNearStationaryEnemy).

2026-11-20  Sound pass. AudioManager self-wires via GameEvents.LevelCompleted/PlayerDied for win/lose and SceneManager.sceneLoaded for menu/level music, so no changes were needed in GameManager, Goal, or MainMenuController.

2026-11-20  Hooked the remaining events into existing code: jump in PlayerController2D, hit/death in PlayerHealth, hit in EnemyHealth, wind-up at the start of each enemy's telegraph state (PatrollerEnemy/ShooterEnemy/ChargerEnemy), pickup in Pickup, and UI click across every button handler (MainMenuController, LevelSelectController, SettingsPanel, ResetProgressPanel, HUDController, PauseController). PauseController now also toggles the Paused mixer snapshot.

2026-11-20  SettingsPanel already had the dB-conversion and mixer wiring from an earlier pass but no AudioMixer asset existed yet. So we create it step by step.

2026-11-20 Update README.md
