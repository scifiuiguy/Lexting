# Quest demo build

| File | Purpose |
|------|---------|
| **`tutorial_demo.apk`** | Ready-to-run Quest app: in-app tutorial and dual-volume typing demo (matches the public tutorial video). |

Download **`tutorial_demo.apk`** from this folder or from the repo’s **GitHub Releases** page when published. Sideload onto Quest 2 (or compatible headset) with your usual developer tools.

| Version | Date | Notes |
|---------|------|--------|
| **0.3.0** | 2026-11-04 | Greenfield build **005** — OpenXR-first path, **`OpenXRHandInput`** (current **`tutorial_demo.apk`**). |
| **0.2.0** | 2026-11-03 | Greenfield build **001** — first package-only Quest APK. |
| *(planned)* | — | **0.4.0** — Meta Start submission build (**beta**; tutorial video parity). |

## 0.3.0 (build 005)

- **Unity:** 6000.0.60f1 (IL2CPP, Android).
- **Package id:** `com.lexting.spatialgesturetext` (Player / UPM **`0.3.0`**).
- **Content:** UPM `com.lexting.spatialgesturetext` — reference scene **`main_01_twozone`** with **`InputMode.OPEN_XR`**, Meta Quest OpenXR + hand tracking, **`OpenXRHandInput`** adapter.
- **Origin:** Copied from greenfield `lexting_005.apk` after M3 Quest smoke (typing, hands, hover/z-sort fixes builds **002–005**).
- **Scope:** M3 shipping demo; full M1 / monolith build **028** parity checklist remains follow-up QA.

## 0.2.0 (build 001)

- First greenfield package-only APK; superseded by **0.3.0** for device testing.

Sideload: `adb install -r -d tutorial_demo.apk` (developer mode + USB debugging).
