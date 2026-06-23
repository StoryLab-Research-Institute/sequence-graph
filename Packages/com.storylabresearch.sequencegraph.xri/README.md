# StoryLab Sequence Graph — XRI Nodes

XR Interaction Toolkit (XRI) nodes for [com.storylabresearch.sequencegraph](../com.storylabresearch.sequencegraph). This package depends on the XR-agnostic core and adds action/condition nodes that drive an XRI rig.

> **Status: stub.** The asmdef, package.json, and dependency wiring are in place, but the nodes themselves have not yet been written. They are being ported from an UltimateXR original; the table below tracks the work. `Runtime/AssemblyInfo.cs` is an empty placeholder that only exists so the assembly compiles — delete it once real nodes land.

## Nodes to implement

These are rewrites of the original UltimateXR-based SequenceGraph nodes, re-targeted at XRI. Effort estimates from the porting analysis:

| Original UXR node | Behaviour | XRI mapping | Effort |
|---|---|---|---|
| `UXRFadeNode` | Fade camera to/from black | Screen-fade overlay / tunnelling vignette on the XRI camera | Moderate |
| `UXRTeleportNode` | Teleport rig to a transform | `XROrigin.MoveCameraToWorldLocation` + `MatchOriginUpCameraForward`, or `TeleportationProvider.QueueTeleportRequest` | Trivial–Moderate |
| `UXRDisableTeleportNode` | Enable/disable teleport locomotion | Toggle the XRI `TeleportationProvider` / locomotion provider component | Trivial |
| `UXRSceneLoadNode` | Fade then load/reload/next/prev scene | Plain `SceneManager` + the fade above (see core scene-load helper if ported) | Moderate |
| `UXRIgnoreControllerInputNode` | Gate controller input | Enable/disable XRI input action maps / interactors | Moderate |
| `UXRCompassSetNode` | Point UXR compass guidance at a target | No direct XRI equivalent — needs a custom guidance UI, or drop | Hard |
| `UXRCompassClearNode` | Clear the compass guidance | As above | Hard |
| `StoryLabUxrHeartbeatNode` | Drive a heartbeat haptic/audio effect | Depends on porting the StoryLab heartbeat effector; reimplement against XRI haptics | Hard |

See the original implementations under
`ia_p1_primary/Assets/StoryLabResearch/Experimental/SequenceGraph/Nodes/UltimateXR/`
in the source project for reference behaviour.

## Consuming this package

This package's `package.json` declares a dependency on the core package, but **git
dependencies are not transitive in UPM** — a consumer project must add *both* git URLs
to its `manifest.json`. See the repository root `README.md` for the manifest snippet.
