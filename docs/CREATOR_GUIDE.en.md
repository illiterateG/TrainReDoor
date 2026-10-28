# Creator integration guide

[English](CREATOR_GUIDE.en.md) · [日本語](CREATOR_GUIDE.ja.md) · [简体中文](CREATOR_GUIDE.zh-CN.md)

For release 1.2.0 / assembly 1.2.0.0. This documents the implementation. See the [validation record](VALIDATION.md) for the distinction between code checks, publication and gameplay.

## Model and import requirements

The runtime entity must carry Train, PrefabRef, InterpolatedTransform, Bone and Skeleton data. Each controlled RenderPrefab needs valid ProceduralAnimationProperties/ProceduralBone data and bind indices. Door bones must have purpose None=0. Keep bogie, pantograph and other bone purposes intact.

Skin moving door panels, glass, seals and trims to their door bones. Fixed frames remain on the body. Start from the fully closed bind pose. Separate body and glass meshes must both retain matching bones and bind positions. A static mesh with a Door object name is not enough. Keep authoring dimensions/units consistent and verify exported scale; the controller never changes gauge or geometry.

## Automatic convention

Use `ReDoor_L_01_0_850` and `ReDoor_L_01_1_850` for a pair, with R labels for the opposite side. `TrainReDoor_` is an accepted alternative prefix. Matching is case-sensitive. Group is a positive Int32; travel is 1–10000 millimetres, using a dot for decimals. Direction 0 is parent +Z, direction 1 is parent -Z. These are game-native parent axes, not guaranteed Blender axes.

Physical platform side comes from native object X: negative is Left, positive is Right. Automatic matching rejects abs(X)<0.0001 m. The L/R text label does not override physical side after exporter conversion. Parent-space rotation must be suitable for longitudinal translation; otherwise correct the skeleton or use JSON.

## Existing bones and JSON

See [the complete example](../examples/CustomTrain.example.json). Each rule identifies the exact game RenderPrefab name and bone name, case-sensitively. Add separate rules for separate body/glass meshes. `side` is Left, Right or Auto; Auto uses native X. `deltaMeters` has three parent-space components and a total length of 0.001–10 metres. `[0,0,0.85]` means 850 mm. A diagonal translation is possible; staged plug-door motion is not.

Only one `TrainReDoor.json` next to the loaded controller is read at load time. The Loaded log prints its path. Asset-side configuration discovery/merging and a registration API are not implemented. Restart after editing. Controller updates may replace hand-edited adjacent configuration. Prefer automatic names for published assets.

Global openSeconds/closeSeconds range from 0.05 to 60 simulated seconds. Excluded exact mesh names override JSON mappings, which override the standard convention. Invalid or duplicate mappings invalidate the custom settings as a whole and fall back to defaults.

## Test and distribute

Enable one controller copy and run the train on a real line. A static preview has no boarding signals. Confirm Loaded and Bound logs for both body and glass, then test left/right platforms, reversed consists, pause/resume and closing after boarding. Bound logs establish matching bones, not correct skinning.

| Symptom | Check |
| --- | --- |
| No Loaded log | Active playset, DLL installation and overlapping controllers |
| No Bound log | Runtime component eligibility, exact mesh/bone names, purpose and bind indices |
| Glass does not move | Separate glass skeleton, bind pose and skin weights |
| Wrong side | Native object X or explicit JSON side |
| Wrong direction | Direction 0/1, parent rotation and displacement vector |
| Deformation | Skin weights and closed bind pose |
| Configuration unchanged | Loaded configuration path, restart and parser errors |

Document only checks you executed and the tested game/asset/controller versions. No departure interlock, audio, collisions, passenger routing, rotating doors or staged plug doors are implemented. Vehicle names alone do not establish entity eligibility.

Use the existing Train ReDoor listing (Mod ID 164831) as the dependency and link the creator guide. Do not copy a sample ID or distribute another controller DLL inside your train asset. Creators following the convention do not need C# or a per-vehicle code mod.

Automatic matching uses the public ReDoor/TrainReDoor convention only. Other bone names require exact JSON mappings or renaming. Global open/close times are 0.05–60 simulated seconds; exclusion overrides JSON, then standard naming. Use one controller copy and test a running line, not only a static preview.
