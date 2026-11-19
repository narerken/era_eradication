# The Era of Eradication

2D mobile platformer. A lantern-robot crosses a collapsing ruin, dodging three types of
guardian machines on the way to each gate.

## Unity version

`6000.6.0f1` (Unity 6), 2D, new Input System.

## Controls

- On-screen stick (bottom-left): move.
- JUMP button (bottom-right): jump, with coyote time + jump buffering.
- Keyboard in the Editor: A/D or arrow keys to move, Space/W/Up to jump.
- II button (top-right, in-level): pause.

## Enemies

| Enemy | States | Tell |
|---|---|---|
| Sentinel (Patroller) | Patrol → Chase → Attack → Return | Freezes + flashes red before lunging |
| Turret (Shooter) | Idle → Aim → Shoot → Cooldown | Barrel glows during the aim |
| Brute (Charger) | Patrol → WindUp → Charge → Cooldown | Shakes before a straight dash |

All three read the player with distance + vision cone + a raycast blocked by walls. Tuning
numbers live in `EnemyStats` / `PlayerStats` under `Assets/Data/`, not in code.

## Data & saves

- `Assets/Data/*.asset` — stats assets + three `LevelData` assets. One `Game` scene and
  `LevelSpawner` build whichever level the player picked.
- `SaveManager` writes JSON to `Application.persistentDataPath/save.json` (score, unlocked
  levels, volume). A missing or corrupted file falls back to defaults instead of crashing.

## Project layout

```
Assets/
  Art/            generated + real sprites
  Data/           ScriptableObject balance + level assets
  Editor/         ProjectBuilder.cs - one-click project build
  Prefabs/
  Scenes/         MainMenu.unity, Game.unity
  Scripts/
    Core/         GameManager, GameEvents
    Camera/       CameraFollow2D
    Data/         ScriptableObject class definitions
    Enemies/      EnemyBase + Patroller/Shooter/Charger + Projectile + EnemyHealth
    Input/        MobileInput, OnScreenStick, OnScreenButton
    Player/       PlayerController2D, PlayerHealth
    Save/         SaveData, SaveManager
    UI/           Menu/HUD/Pause/LevelSelect/Settings/ResetProgress/SafeArea
    World/        LevelSpawner, Pickup, Goal, Hazard
```

See `DECISIONS.md` for how the AI assistant works on this project.
