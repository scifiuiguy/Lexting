# Quest demo build

| File | Purpose |
|------|---------|
| **`tutorial_demo.apk`** | Ready-to-run Quest app: in-app tutorial and dual-volume typing demo (matches the public tutorial video). |

Download **`tutorial_demo.apk`** from this folder or from the repo’s **GitHub Releases** page when published. Sideload onto Quest 2 (or compatible headset) with your usual developer tools.

| Version | Date | Notes |
|---------|------|--------|
| **0.4.0** | 2026-11-07 | Meta Start submission build — **22-step** in-app tutorial (`TutorialManager`), **SKIP**, checkmark stroke polish, speed-test menu hidden, hover letter display fix. Rebuild on greenfield host with `tools/build_quest.ps1` to refresh the APK after package **`0.4.0`**. |
| **0.3.0** | 2026-11-04 | Greenfield build **005** — OpenXR-first path, **`OpenXRHandInput`**. |
| **0.2.0** | 2026-11-03 | Greenfield build **001** — first package-only Quest APK. |

## 0.4.0

- **Unity:** 6000.0.60f1 (IL2CPP, Android).
- **Package id:** `com.lexting.spatialgesturetext` (Player / UPM **`0.4.0`**).
- **Content:** UPM `com.lexting.spatialgesturetext` — reference scene **`main_01_twozone`** with **`InputMode.OPEN_XR`**, Meta Quest OpenXR + hand tracking, full onboarding per [`docs/in_app_tutorial.md`](../docs/in_app_tutorial.md).
- **Scope:** M4 Meta Start **beta**; tutorial complete or skip → dual-volume typing + floater menu (typing tutorial / freestyle).

## 0.3.0 (build 005)

- **Content:** OpenXR-first shipping demo; full M1 / monolith build **028** parity checklist remains follow-up QA.

## 0.2.0 (build 001)

- First greenfield package-only APK; superseded by **0.3.0** for device testing.

Sideload: `adb install -r -d tutorial_demo.apk` (developer mode + USB debugging).
