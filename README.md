# Roll A Ball Unity Game

Built upon the foundation of Unity’s classic Roll-a-Ball tutorial, this project introduces custom gameplay mechanics and dynamic level progressions
<img width="1725" height="1116" alt="image" src="https://github.com/user-attachments/assets/83f5e759-7143-44ac-b4ef-5c82beae6db2" />

## Unity Version

This project uses Unity `6000.0.75f1 (LTS)`.

## Features

- Physics-based rolling ball movement using a `Rigidbody`
- Spacebar jump ability
- HUD with player name, round/count display, Retry button, and Exit button
- Four-round pickup challenge
- Stage reset between rounds
- Additional AI robot each round
- Different robot colors by round
- Chasing AI robots using Unity AI Navigation
- Added 3D level objects including a jump gate, ramp/platform, dynamic boxes, and corner pillars

## Controls

- `WASD` or arrow keys: roll the ball through physics force
- `Space`: jump while grounded
- `Retry`: restart the scene
- `Exit`: quit the built application

## Main Scene

Open:

```text
Assets/Scenes/MiniGame.unity
```

## Code

Main gameplay scripts are in:

```text
Assets/Scripts
```

Key files:

- `PlayerController.cs`: player movement, jumping, pickup collection, round progression, stage reset, Retry/Exit buttons, win/lose behavior
- `EnemyMovement.cs`: AI robot pursuit using `NavMeshAgent`
- `CameraController.cs`: camera follow behavior
- `Rotator.cs`: pickup rotation

## Notes

This repository is intended as a public source-code/project copy. Unity generated folders such as `Library`, `Logs`, `Temp`, `Obj`, and local build output are intentionally excluded.



