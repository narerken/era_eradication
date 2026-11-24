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

## Sound

Every sound and both music tracks are synthesized at runtime in
`Assets/Scripts/Audio/ProceduralAudio.cs` (sine/square waves + noise, built with
`AudioClip.Create`) — nothing downloaded, so there's no external source or license to list.
`AudioManager` (same folder) plays them, crossfades the two music loops on scene change, and
switches to a lowpass "Paused" snapshot while the game is paused.

Events: jump, hit (player + enemies), enemy wind-up (plays at the start of each enemy's
telegraph, before the attack lands), death, pickup, win, lose, UI click. Pitch is randomized
slightly per play so repeats don't sound identical.

`Assets/Audio/MasterMixer.mixer` has `Master` with two child groups, `Music` and `SFX`, routed
from `AudioManager`'s music/SFX `AudioSource`s via `outputAudioMixerGroup`. A `Paused` snapshot
(selectable alongside the default `Snapshot`) drops `Master`'s volume a few dB and adds a
Lowpass effect at an ~800 Hz cutoff; `AudioManager.SetPaused()` transitions to it when the game
pauses and back to the default snapshot on resume.

The Music/SFX sliders in Settings don't go through the mixer's exposed parameters — Unity's
"Expose to script" step has to be done by hand in the Editor UI per-parameter, which doesn't
round-trip well through a one-click rebuild. Instead, `AudioManager.SetVolumes(music, sfx)` sets
the music/SFX `AudioSource.volume` directly (scaled into the crossfade and `PlayOneShot` calls),
and the save data persists both as linear 0–1 values. Routing/snapshots still go through the
real mixer group and snapshot objects, just not through named float parameters.

Both `Main Camera`s (`MainMenu` and `Game` scenes) carry an `Audio Listener` component so SFX
and music are actually audible — Unity's default 2D camera prefab/template used here didn't
include one, and the project would otherwise spam "There are no audio listeners in the scene"
and play nothing.

## Data & saves

- `Assets/Data/*.asset` — stats assets + three `LevelData` assets. One `Game` scene and
  `LevelSpawner` build whichever level the player picked.
- `SaveManager` writes JSON to `Application.persistentDataPath/save.json` (score, unlocked
  levels, volume). A missing or corrupted file falls back to defaults instead of crashing.

## Project layout

```
Assets/
  Art/            generated + real sprites
  Audio/          MasterMixer.mixer (Master/Music/SFX groups, Paused snapshot — see Sound above)
  Data/           ScriptableObject balance + level assets
  Prefabs/
  Scenes/         MainMenu.unity, Game.unity
  Scripts/
    Audio/        AudioManager, ProceduralAudio
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
