# Quest demo build

| File | Purpose |
|------|---------|
| **`tutorial_demo.apk`** | Ready-to-run Quest app: in-app tutorial and dual-volume typing demo (matches the public tutorial video). |

Download **`tutorial_demo.apk`** from this folder or from the repo’s **GitHub Releases** page when published. Sideload onto Quest 2 (or compatible headset) with your usual developer tools.

| Version | Date | Notes |
|---------|------|--------|
| **0.2.0** | 2026-11-03 | Greenfield build **001** — first package-only Quest APK (see below). |
| *(planned)* | — | **1.0.0** — Meta Start submission build (tutorial video parity). |

## 0.2.0 (build 001)

- **Unity:** 6000.0.60f1 (IL2CPP, Android).
- **Package id:** `com.lexting.spatialgesturetext` (Player Settings / bundle **0.2.0**).
- **Content:** UPM package `com.lexting.spatialgesturetext` only — reference scene **`main_01_twozone`**, OpenXR + Meta Quest Support + hand tracking (same stack as maintainer greenfield host).
- **Origin:** Copied from greenfield `lexting_001.apk` (not monolith `Lexting_Unity/`).
- **Scope:** Milestone 2 demo binary; device QA ongoing. Full M1 checklist re-run on greenfield is tracked separately from monolith build **028**.

Sideload: `adb install -r -d tutorial_demo.apk` (developer mode + USB debugging).
