# Echoes of Aether

A third-person exploration prototype for Unity, in early development.

> **Status: prototype draft.** The scripts in this repo are written but have **not yet been compiled or play-tested in the Unity Editor**. The untested items are listed under [Known limitations](#known-limitations).

## Architecture

| Concern | Decision |
|---|---|
| Player movement | Unity `CharacterController` + `CharacterController.Move` (no Rigidbody) |
| Camera | Cinemachine 3.x: `CinemachineCamera` with **Third Person Follow** + **Rotate With Follow Target** |
| Input | Unity Input System, with actions created in code (no `.inputactions` asset needed) |

Each script has one job:

```
PlayerInputReader  ──► PlayerMovement      (CharacterController.Move, gravity, jump, grounding)
        │
        └────────────► PlayerCameraInput   (rotates CameraTarget) ──► Cinemachine follows it
```

- `PlayerInputReader`: reads move, look, jump and sprint input. It never moves anything.
- `PlayerMovement`: handles camera-relative movement, walk/sprint, turning toward the movement direction, jumping (with coyote time and a jump buffer), gravity and grounding.
- `PlayerCameraInput`: applies yaw/pitch to the camera target, with a pitch clamp. Cinemachine does the follow, offset, damping and collision work.

## Target versions (chosen for this draft)

- **Unity 6 LTS** (6000.0.x)
- **Input System** `com.unity.inputsystem` 1.11+
- **Cinemachine** `com.unity.cinemachine` 3.1+ (uses the `Unity.Cinemachine` namespace and `CinemachineCamera`)

These versions are the target for this draft. They have not been verified in a running project yet.

## Folder structure

```
Assets/
  EchoesOfAether/
    Scripts/
      Input/PlayerInputReader.cs
      Player/PlayerMovement.cs
      Camera/PlayerCameraInput.cs
PLAN.md
README.md
.gitignore
```

## Setup

1. Create a new **Unity 6** project using the **3D (URP or Built-in)** template, then copy the `Assets/EchoesOfAether` folder into it (or clone this repo into the project root).
2. **Window → Package Manager**: install **Input System** and **Cinemachine** (3.x).
   When Unity asks to enable the new input backend, click **Yes**. Alternatively, set **Project Settings → Player → Active Input Handling** to *Input System Package* or *Both*.
3. Build a test scene:
   - Add a large **Plane** for the ground, plus a few **Cubes** as steps or obstacles.
   - Create an empty GameObject **Player** at (0, 1, 0) and add:
     - **Character Controller**: Height 2, Radius 0.4, Center (0, 0, 0), Step Offset 0.3
     - **PlayerInputReader**
     - **PlayerMovement**
     - **PlayerCameraInput**
   - Optionally, add a **Capsule** as a visual child of Player and remove its Capsule Collider.
   - Create an empty child of Player named **CameraTarget** at local position (0, 0.6, 0).
4. Set up the camera:
   - Make sure the **Main Camera** has a **CinemachineBrain**. Cinemachine adds one automatically when you create a Cinemachine camera.
   - **GameObject → Cinemachine → Cinemachine Camera**.
   - On the `CinemachineCamera`, set **Tracking Target** to `CameraTarget`.
   - Set **Position Control** to **Third Person Follow**. Suggested values: Shoulder Offset (0.5, 0.2, 0), Camera Distance 4, Camera Side 0.6.
   - Set **Rotation Control** to **Rotate With Follow Target**.
5. Connect the Inspector references:
   - `PlayerMovement`: **Input** → Player (`PlayerInputReader`). **Camera Transform** → Main Camera. You can leave it empty to fall back to `Camera.main`.
   - `PlayerCameraInput`: **Input** → Player. **Camera Target** → `CameraTarget`. **Cinemachine Camera** → the CinemachineCamera (optional).
6. Press **Play**.

## Controls

| Action | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | WASD / Arrow keys | Left stick |
| Look | Mouse | Right stick |
| Sprint | Left Shift (hold) | Left stick press (hold) |
| Jump | Space | South button (A / Cross) |
| Release cursor | Escape | — |
| Re-lock cursor | Left click | — |

Mouse sensitivity is set in **degrees per pixel** of mouse movement. Stick sensitivity is set in **degrees per second** at full deflection.

## Manual test checklist

- [ ] The project compiles with no errors after installing the packages
- [ ] WASD moves the player relative to the camera's facing direction
- [ ] The player turns smoothly toward the movement direction
- [ ] Holding Shift increases speed
- [ ] Space jumps only when grounded, or just after walking off a ledge (coyote time)
- [ ] Pressing Space just before landing still triggers a jump (jump buffer)
- [ ] Gravity pulls the player down, and the player lands cleanly on cubes and slopes
- [ ] Hitting a ceiling during a jump stops upward motion
- [ ] The mouse rotates the camera, and vertical look stays within the pitch limits
- [ ] Escape frees the cursor and left click re-locks it
- [ ] Missing references produce a clear error in the Console

## Known limitations

- Not yet compiled or play-tested in Unity.
- Grounding relies on `CharacterController.isGrounded`. There is no slope sliding or extra ground probe yet.
- No animation, audio or moving-platform support yet.
- Input bindings are hard-coded and cannot be rebound at runtime.

## Roadmap

See [PLAN.md](PLAN.md).
