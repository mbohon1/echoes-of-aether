# echoes-of-aether
Early-stage Unity game prototype planned for development with Devin, using CharacterController movement and a Cinemachine camera.


Echoes of Aether
An early-stage Unity game prototype planned for development with Devin.
Project Status
This repository initializes the project and documents its intended architecture.
The project is currently in the planning and setup phase. No playable build is available. The systems below are planned, not completed or tested.
Prototype Scope
The initial goal is a third-person exploration prototype featuring:
Camera-relative player movement
Walking and sprinting
Jumping, gravity, and grounded checks
A third-person follow and orbit camera
Architecture Decisions
Player Movement
Use Unity's CharacterController and CharacterController.Move.
The player will not use a Rigidbody-based movement controller.
Camera
Use Cinemachine for the third-person camera.
A dedicated camera target on the player will serve as the follow target. A separate input component will handle camera look input.
Component Responsibilities
PlayerInputReader: read movement, look, jump, and sprint input.
PlayerMovement: handle CharacterController movement, gravity, and grounded state.
PlayerCameraInput: connect look input to camera controls.
Keep input handling separate from movement logic and camera control.
Planned Folder Structure
Assets/
EchoesOfAether/
Scenes/
Scripts/
Input/
Player/
Camera/
Prefabs/
This is the intended structure; these folders have not necessarily been created yet.
Planned Use of Devin
Devin is intended to assist with project scaffolding, implementing the prototype components, and reviewing changes through this repository.
No completed Devin implementation is claimed at this stage.
First Milestone
Initialize the Unity project
Add and configure Cinemachine
Implement input handling
Implement CharacterController movement
Add jumping and grounded checks
Configure the third-person camera
Create a basic test scene
Test and document prototype behavior
