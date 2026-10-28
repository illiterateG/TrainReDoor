# Validation status

[English](VALIDATION.md) · [日本語](DEVELOPMENT.ja.md) · [简体中文](VALIDATION.zh-CN.md)

## Recorded engineering checks

The 1.2.0 delivery records an official build with source generation and post-processing, zero build warnings/errors, 73 protocol/assembly checks, four synthetic cases with 16 door bindings, and runtime parsing of default/example JSON. Its recorded compilation environment is game 1.6.2f1. Hashes identify the exact delivered artifacts.

Synthetic fixtures exercise names, side selection and travel. They are hand-authored inputs, not imported model records. These checks do not establish model skinning or gameplay correctness.

## Recorded publication

The local official-publisher receipt dated 2026-10-25 confirms a successful NewVersion upload for Mod ID 164831, version 1.2.0, recommended pattern 1.6.*, with Unlisted visibility. This is a historical publication record, not a fresh remote visibility check.

Remote upload success and subscribed-package operation are separate checks. No game boarding or subscribed-package verification has been recorded for this release.

## Gameplay checklist

- [ ] Record game, asset and controller versions and the test line/save.
- [ ] Confirm one controller copy and the expected Loaded/configuration path.
- [ ] Check Bound logs for body and separate glass; matching alone does not prove skinning.
- [ ] Test left/right platforms, reversed consists and all car variants.
- [ ] Inspect panel/glass alignment, closing after boarding and pause/resume.
- [ ] Test independent dual-side boarding when a reproducible scenario exists.
- [ ] Check unrelated bones/assets, save/reload and restart.
- [ ] Inspect multiple views for geometry, skinning, normals, materials and glass.
- [ ] Install the platform-subscribed package and test it independently.

Only mark checks actually performed. The recommended version pattern is not a claim that every matching patch was tested. No departure interlock, sound, collision or passenger-routing feature should be advertised.
