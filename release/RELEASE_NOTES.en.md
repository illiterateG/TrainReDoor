# Train ReDoor 1.2.1

- Deduplicates `Bound` messages per mesh and resolved set of door bindings. Identical bindings stay quiet when the editor replaces a render entity. A new binding layout is still reported, even when the number of door bones stays the same.
- The existing door animation, binding configuration and ECS synchronization retain their 1.2.0 behavior.

The official build and automated checks passed on Cities: Skylines II 1.6.4f1. An instrumented build handled door bindings during same-save on/off profiling. Repeated reimport logging was covered by a regression test against the built DLL; an in-game editor reimport comparison remains unverified.

For manual installation, download `TrainReDoor_1.2.1.zip` and use the [installation guide](../docs/INSTALL.md).
