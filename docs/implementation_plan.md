# Lexting implementation plan (0.0.1 → 0.5.0 beta roadmap)

This document is the **living roadmap** for reviving the Unity prototype, stabilizing XR integration, and shipping a **Meta Start** competition entry. It intentionally stays **out of** [`README.md`](../README.md); update this file as milestones firm up.

**Primary QA device:** Meta **Quest 2** (same hardware used for the original prototype).  
**Reference scene (today):** [`main_01_twozone.unity`](../Lexting_Unity/Assets/Lexting/_Scenes/main_01_twozone.unity) — dual-volume typing widget, dictionary, hotkeys, tutorial hooks. After **Milestone 2**, the same scene ships **inside** the single UPM package (reference scenes/prefabs folder — not a separate download or optional Samples import).

**Sprint window:** **2026-11-01** → **2026-11-19** (Meta Start deadline).  
**Polish cutoff:** **2026-11-15** — from this date forward, scope is **tutorial / submission presentation** only unless a **P0** Quest blocker appears.

---

## Version map (intent, not release tags yet)

| Version | Meaning |
|--------|---------|
| **0.0.1** | Repository snapshot *as archived* (~2021 prototype + monolithic `Lexting_Unity/` tree). May not install or run on a Quest 2 without repair work. |
| **0.1.0** | **Milestone 1** — **Unity 6** upgrade on the legacy `Lexting_Unity/` tree + Quest 2 build; typing behavior matches the last known-good prototype (“as I left it”). Enables **`com.unity.pipeline`** (Cursor Unity MCP) on the monolith. |
| **0.2.0** | **Milestone 2** — Lexting ships as **one Unity package** (runtime, embedded SimpleJSON/Surge/UniRx, default dictionary, reference scenes/prefabs). **One-way extract** from monolith; Quest QA uses a **greenfield host** (new project, not in open repo)—never reintegrate the package into `Lexting_Unity/`. Delete monolith when stable. |
| **0.3.0** | **Milestone 3** — Refreshed XR UPM stack; **OpenXR-first** Quest path documented and implemented; legacy Oculus Integration retired or quarantined (engine already on **Unity 6** from M1). |
| **0.4.0** | **Milestone 4** — Meta Start submission quality (**beta**): in-app **`TutorialManager`** onboarding operational, stable **`tutorial_demo.apk`**, known limitations documented; **3-minute submission video** = **product demo** (widget / core typing), separate from in-app tutorial UI. |
| **0.5.0** | **Milestone 5 — Lexting Setup Wizard** — Editor **Lexting** menu command that configures the active scene for third-party integrators (auto rig bind + Lexting bootstrap where possible; validation errors/warnings when not). **Final milestone** in this roadmap. |

**Version policy:** Each milestone bumps **`Lexting_Unity_Plugin/package.json`** to its target **0.x** line. **`1.0.0` is not a milestone target**—Meta Start and this roadmap stay **beta** until a later **stable / GA** release (post-roadmap). Patch releases (`0.1.x` … `0.5.x`) are expected during each milestone for Quest-side iteration.

---

## Target repository layout (after Milestone 2)

| Path | Role | In open repo |
|------|------|----------------|
| **`Lexting_Unity_Plugin/`** | Single UPM package: runtime + editor assemblies, `SpatialGestureText`, default dictionary Resources, **embedded** third-party source (SimpleJSON, Surge subset, **UniRx 6.2.2**—not integrator UPM), reference **Scenes/** and **Prefabs/** (`main_01_twozone`, QWERTY sim, hand/XR tests). **`Releases/`** — Meta Start **`tutorial_demo.apk`** + release README (APK via GitHub Releases; gitignored). One install via Package Manager. | **Yes** |
| **Greenfield Quest host** *(e.g. private `Lexting_Unity_Sample/` on disk)* | **New** Unity project created from scratch to simulate a **3rd party** installing only the plugin (+ documented platform UPM). **`ProjectSettings`**, Android keystore, OpenXR/Oculus loader pins, build scripts, Meta Start branding. References the package via `file:` or Git URL. **Never** wire the package back into `Lexting_Unity/`. **Not** published in the open repo. | **No** |
| **`Lexting_Unity/`** | Monolithic donor during M1/M2 extract only. **One-way migration:** copy content out, validate in greenfield host, then **delete** from default branch—no reintegration, no dual-tree compile. Optional git tag `pre-plugin-monolith` before removal. | Remove after M2 exit criteria |

**Package consumption:** add once in `Packages/manifest.json` or Package Manager UI, e.g. `"com.lexting.spatialgesturetext": "file:../Lexting_Unity_Plugin"` (final package id TBD). Reference scenes open from **Packages/** in the Project window.

---

## Cross-cutting assumptions

- **Keep (embedded in plugin, pinned snapshots):** [SimpleJSON](https://github.com/Bunny83/SimpleJSON), [Pixelplacement Surge](https://github.com/Pixelplacement/Surge) (subset), [UniRx](https://github.com/neuecc/UniRx) **6.2.2** — **must ship inside `Lexting_Unity_Plugin/`** so integrators are not sent hunting UPM/Asset Store for matching versions. **`package.json` lists Unity/platform deps only** (Input System, XR Management, OpenXR, etc.), not UniRx/SimpleJSON/Surge. Integrator docs: **do not add a second UniRx** (or duplicate SimpleJSON/Surge) in the host project—embedded copies define the supported versions.
- **Surge:** tweens via `TweenUtils` and direct `Tween.*` calls; validate on **Unity 6** during **Milestone 1** reopen; replace if broken (still embedded until replaced).
- **Leap (reference only):** Monolith **`Assets/Plugins/LeapMotion/`** stays out of the package; vendored **`Runtime/Scripts/Input/Leap/`** + **`LEXTING_FOUND_LEAP_SDK`** paths are **deprecated in M3 but not deleted**—kept as a **template** for future non-OpenXR hand SDKs (see M3 workstream 6).
- **Engine baseline:** **Milestone 1** upgrades the monolith from archived **Unity 2022.3.19f1** to **Unity 6** LTS (exact 6.x patch recorded in [`ProjectVersion.txt`](../Lexting_Unity/ProjectSettings/ProjectVersion.txt) after upgrade). **Milestone 2** locks the same minimum Unity version in plugin `package.json`; device builds run from the **greenfield host** project.
- **Risk focus:** **Milestone 1** now includes the **editor upgrade** (highest one-shot risk); **Milestone 3** focuses on **UPM XR refresh** and **OpenXR conformity** on an already-Unity-6 tree. Prototype *behavior* stays locked across M1 device parity; **Milestone 2** changes packaging only (see [Platform targets in README](../README.md#platform-strategy)).
- **Agent tooling:** After Unity 6 + `unity pipeline install`, pin Cursor MCP to `Lexting_Unity` (`unity mcp configure cursor --project-path …`) for live Editor inspection during M1 Quest debugging.
- **Out of sprint scope unless time:** UniRx replacement, Surge → DOTween migration, deprecated letter-cube path, MLAPI/streaming revival.

---

## Milestone 1 — Unity 6 upgrade + Quest 2 baseline restore

**Target version:** `0.1.0`  
**Goal:** Open the project on **Unity 6**, restore a running **Android/Quest** build that **operates exactly as you left it** (~5 years ago): dual-volume word entry, dictionary filtering, conjugation/finalization, hotkeys, hand cursor, and in-scene tutorial *as it existed then* (polish comes in Milestone 4).

**Suggested window:** **2026-11-01 → 2026-11-08** (hard stop if slipping: cut everything except “opens on Unity 6, loads, tracks hands, types words”).

**Host project:** existing **`Lexting_Unity/`** only—no package split yet.

### Definition of done

- [x] Project upgraded to **Unity 6** LTS; [`ProjectVersion.txt`](../Lexting_Unity/ProjectSettings/ProjectVersion.txt) updated (**6000.0.60f1**); Editor + **Android** player compile (blocking build issues addressed: `OVRExternalComposition`, `OVRGradleGeneration`, duplicate `OVRPlugin.aar` on Android).
- [x] **`com.unity.pipeline`** installed; Cursor MCP pinned to `Lexting_Unity`; Pipeline **reachable** when the Editor is open (`unity command … editor status` / `build run`).
- [x] Project opens on Unity 6 without blocking compile errors for **Android** player.
- [x] APK installs on **Quest 2** (sideload); app **launches and loads** `main_01_twozone` (first verified **`lexting_002.apk`**; ongoing device QA through **`lexting_028.apk`**).
- [x] `LextingInputModuleManager` on **`QUEST_HANDS`** (enum `0`) in `main_01_twozone`; reference rig is **`XRRig`** + Unity **XR Hands** (`HandVisualizer`) — see local [`bugs/quest2_baseline_log.md`](../bugs/quest2_baseline_log.md) (gitignored).
- [x] End-to-end smoke test on device (signed off on Quest 2; see [`bugs/QA/`](../bugs/QA/) + baseline log):
  - [x] Hover **nodes** in both **volumes** via **hand cursor** (builds **025–028**; label tint + center-ring exit fixes landed)
  - [x] Grab/drag word assembly (up to four letters across hands) — acceptable for M1; path highlight polish tracked ([`BUG-20261102-c4d8`](../bugs/BUG-20261102-c4d8.md))
  - [x] Dictionary filter + word list behavior
  - [x] Finalization / conjugation gesture path ([`conjugation_algorithm.md`](conjugation_algorithm.md))
  - [x] Hotkeys (tab, backspace, shift/caps, etc.) via `LextingHotkeyManager` — Enter/Escape wired; many matrix hotkeys still non-operational on Quest (see README)
  - [x] Text reaches subscribed UI (`LextingOutputManager` / `InputField`) — grab/typing works; label lag vs output noted in baseline log
- [x] **Regression log:** local [`bugs/quest2_baseline_log.md`](../bugs/quest2_baseline_log.md) + per-build **`bugs/QA/QA-<build>-*.md`** ([`bugs/bugs_workflow.md`](../bugs/bugs_workflow.md); gitignored).

**M1 exit met** (Quest 2 smoke checklist passed, build **028** era). **M2 (`0.2.0`)** and **M3 (`0.3.0`)** exited (OpenXR-first plugin + greenfield smoke build **005**). **Active milestone: M4 (`0.4.0`)** — Meta Start delivery. Optional M1 gap: **workstream 5** screen recording.

### Skeletal workstreams

1. **Unity 6 upgrade** — [x] Hub upgrade, package resolve, Android IL2CPP build green; Pipeline installed; Editor/`main_01_twozone` usable.
2. **Toolchain** — [~] Android/Quest deploy path works (Unity-bundled `adb`, sideload); Quest APK **`applicationIdentifier`** **`com.lexting.spatialgesturetext`** (greenfield sample; monolith scripts may still reference legacy `com.octest.qwerty_sim_16` until updated); Android min SDK **24+** on OpenXR path (see baseline log).
3. **Build & deploy** — [x] Quest APK from Unity 6 (monolith: `Lexting_Unity/build_quest.ps1`; **M2 greenfield:** `tools/build_quest.ps1` → `Lexting_Unity_Sample`); install (`tools/install_quest.ps1`, package **`com.lexting.spatialgesturetext`**); post-install QA (`bugs/tools/quest_install_qa.ps1`, `qa_close_session.ps1`); **`main_01_twozone`** enabled in greenfield build settings.
4. **Runtime parity — XR Hands on XRRig** — [x] App boots on device; hands + grab/typing exercised on Quest through build **028**:
   - [x] **Keep:** [`XRRig.prefab`](../Lexting_Unity/Assets/Lexting/Prefabs/XRRig.prefab) (`LeftHand` / `RightHand` anchors), runtime **`HandMarker`**, sample **`HandVisualizer`**, gesture stack in **`OculusHandInput`** (transform-based grab/rotation, not OVR pinch APIs).
   - [x] **Fix:** **`OculusHandInput.BindHandNodes`** — runtime fallback: if no **`OVRCustomSkeleton`** under anchors → bind joint transforms from **`HandVisualizer/*Hand(Clone)`** (waits for clones); fixes left **`XRHandTrackingEvents`**, left palm path, joint **`Find`** roots. **`LEXTING_FOUND_OCULUS_SDK`** no longer forces the OVR-only branch when the scene uses XRRig.
   - [x] **Rotation / hover:** shared [`QuestHandRotationMapping.cs`](../Lexting_Unity/Assets/Lexting/Scripts/Input/QuestHandRotationMapping.cs); [`HandCursor`](../Lexting_Unity/Assets/Lexting/Scripts/TwoZoneCube/HandCursor.cs) center-ring exit; [`LetterSwitcher`](../Lexting_Unity/Assets/Lexting/Scripts/TwoZoneCube/LetterSwitcher.cs) hover label tint.
   - [x] **Verify:** **`XRHandSubsystem`** active with **OpenXR + Meta Quest Support** on device (**`010+`** builds); hands track and drive typing. Full OpenXR-first *documentation* and loader policy remain **M3**; bind path is validated for M1.
   - **Constraint (unchanged):** do not roll back to **`Lexting_OVRCameraRig` / OVRHandPrefab** for M1 unless fallback fails on device.
5. **Baseline capture** — [ ] Screen recording + comparison notes (text log + QA reports only so far).

**Exit criterion:** Project runs on **Unity 6**, and you would confidently say *“this is the same app I shipped on Quest 2 back then,”* even if the upgrade needed surgery to get there.

---

## Milestone 2 — Unity plugin packaging

**Target version:** `0.2.0`  
**Goal:** Reformat the codebase so **Lexting is one installable Unity package**, not an app buried inside a mega-project. Typing UI, default dictionary, input adapters, utilities, **scenes, and prefabs** all live under **`Lexting_Unity_Plugin/`**. Day-to-day Quest APK builds move to a **greenfield host** project (not in the open repo) that references **only** the package—simulating a 3rd-party install.

**Suggested window:** **2026-11-08 → 2026-11-12** (start only after Milestone 1 exit criterion is met; may slip Meta Start calendar if split takes longer—prefer a **minimal** plugin cut that preserves behavior over a perfect asmdef graph).

**Prerequisite:** Milestone 1 Quest checklist passes on the **legacy** tree (so regressions from the move are attributable to packaging, not unknown drift).

### Migration strategy (one-way only)

This milestone is **extract → validate → delete**, not “turn the monolith into a package consumer.”

1. **Extract** — Copy (or move in git) Lexting content from **`Lexting_Unity/`** into **`Lexting_Unity_Plugin/`** UPM layout (`package.json`, Runtime/Editor asmdefs, Scenes/Prefabs/Resources). The monolith is a **donor** during extract; do **not** add a `file:` package reference to **`Lexting_Unity/Packages/manifest.json`** or attempt to run both trees in one project (duplicate types / double refs).
2. **Greenfield host** — Create a **new** Unity project from scratch (maintainer-local, e.g. `Lexting_Unity_Sample/`). Install **only** the plugin plus documented platform packages (XR, Input System, Meta/OpenXR as in integrator README). Use this host for compile fixes and **Quest smoke**—it stands in for “someone cloned the repo and added the package.”
3. **Retire monolith** — When the greenfield host is **100% stable** (M1 device checklist on Quest 2), **remove `Lexting_Unity/`** from the default branch. Optional tag **`pre-plugin-monolith`** before deletion for forensic archive. **No** merge-back of the package into the old project at any point.

### What moves into the plugin vs the greenfield host

| **Plugin (`Lexting_Unity_Plugin/`, in git)** | **Greenfield host (not in open repo)** |
|---------------------------------------------|----------------------------------------|
| `SpatialGestureText` runtime + editor scripts | **`ProjectSettings`**, Android keystore, company/product IDs |
| English dictionary Resources, conjugation data | XR Management / OpenXR loader **project** configuration |
| **Embedded third-party source** (see below)—SimpleJSON, Surge subset, UniRx **6.2.2** | Optional maintainer scripts (`build_quest`, `install_quest`) |
| **Reference content:** `main_01_twozone`, QWERTY_Sim, hand/XR test scenes, prefabs (XRRig reference, UI prefabs) | Meta Start one-off branding / capture settings |
| `package.json`, asmdefs, integrator README, **`ThirdPartyNotices.md`** | `manifest.json` `file:` → plugin (or Git URL) |

**Monolith paths that must land in the plugin (not only `Assets/Lexting/`):**

| Monolith source | Plugin destination (typical) |
|-----------------|------------------------------|
| `Assets/Lexting/` (scripts, scenes, prefabs, Resources, Surge, SimpleJSON) | `Runtime/`, `Editor/`, `Scenes/`, `Prefabs/`, `Runtime/ThirdParty/…` |
| `Assets/Plugins/UniRx/` (Scripts + asmdef; **exclude** Examples and `Assets/UniRxTests/`) | `Runtime/ThirdParty/UniRx/` |
| **`Assets/Wispy Sky/`** (see **Wispy Skybox** below)—materials + textures referenced by reference scenes | `Runtime/ThirdParty/WispySky/` or `Content/Sky/` (subset only if embed approved) |

**Explicitly not in the plugin:** bulk **`Assets/Oculus/`** Integration (host pulls Meta/XR via UPM); MECH and unrelated monolith noise; **`Wispy Sky/Extras/`** (206MB PSD/EXR source art) unless explicitly needed. Host project declares platform packages; plugin declares **API-level** XR/Input dependencies integrators need.

### Embedded third-party dependencies & licenses

Integrators **must not** be required to resolve UniRx, SimpleJSON, or Surge from UPM or the Asset Store. Those libraries ship **inside the package** as pinned source snapshots so behavior and compile surface stay under Lexting control (avoid “latest UniRx” integration hell).

| Library | Pin / location today | License | Ship in plugin |
|---------|----------------------|---------|----------------|
| SimpleJSON | `Assets/Lexting/Scripts/ThirdParty/` | MIT ([Bunny83/SimpleJSON](https://github.com/Bunny83/SimpleJSON)) | Source + upstream **LICENSE** file |
| Surge (subset) | `Assets/Lexting/Scripts/ThirdParty/Pixelplacement/Surge/` | MIT (`License.txt` already in tree) | Subset used by Lexting + **License.txt** |
| UniRx | `Assets/Plugins/UniRx/` — **6.2.2** per `ReadMe.txt` | MIT ([neuecc/UniRx](https://github.com/neuecc/UniRx)); portions from Rx.NET/mono | Runtime scripts + asmdef; **LICENSE** text; no Examples/tests in shipping tree |

#### Wispy Skybox (reference-scene look — **license gate**, not MIT)

**Product need:** `main_01_twozone` (and some other reference scenes) use **`WispySkyboxMat`** from [`Assets/Wispy Sky/`](Lexting_Unity/Assets/Wispy%20Sky/) (`guid` on `WispySkyboxMat.mat`). Integrators should open those scenes with the same sky look without hunting Asset Store dependencies—**if** redistribution is permitted.

**What is in the repo today:** [`ReadMe.txt`](Lexting_Unity/Assets/Wispy%20Sky/ReadMe.txt) only (publisher link). **No** standalone LICENSE/EULA file ships with the folder; rights come from the [Unity Asset Store listing](https://assetstore.unity.com/packages/2d/textures-materials/sky/wispy-skybox-21737) (**Mundus**, free, **Standard Unity Asset Store EULA**, store label **Extension Asset**).

**Redistribution vs code libraries:** This is **not** like SimpleJSON/Surge/UniRx. The [standard EULA](https://unity.com/legal/as-terms) allows use when the asset is **incorporated into a Licensed Product** (a game/app with substantial original content) and **not** when end users can **extract or download the raw asset** separately ([Asset Store EULA FAQ](https://assetstore.unity.com/browse/eula-faq)). Publishing skybox `.tif`/materials inside an **open UPM package on GitHub** is **not clearly allowed** without publisher/Unity written permission—it is closer to sublicensing art than embedding in a shipped APK.

| Outcome | Action |
|---------|--------|
| **Embed in plugin (preferred UX)** | Obtain **written redistribution OK** from **Mundus** (or Unity Asset Store support) for including a **minimal subset** in `Lexting_Unity_Plugin/` (e.g. `WispySkyboxMat` + cubemap/mobile mats + `Textures/` actually referenced; **no** `Demo/`, **no** `Extras/` PSD). Record permission + EULA pointer in **`ThirdPartyNotices.md`**. |
| **Embed not granted (default integrator path)** | **Do not** commit Wispy files to the open repo. Integrators install **[Wispy Skybox](https://assetstore.unity.com/packages/2d/textures-materials/sky/wispy-skybox-21737)** via **Package Manager → My Assets** (documented in README Getting Started). Alternatives: CC0/self-authored sky in scenes, or maintainer-only local copy for builds. |
| **Quest APK / store build** | Using Wispy **inside a built player** you ship is the normal “Licensed Product” path **for licensees who obtained the asset**—distinct from **repackaging source art in the SDK**. |

**M2 checklist item:** resolve Wispy **before** treating reference scenes as done; do not assume embed until legal gate passes.

**Attribution:** Add **`ThirdPartyNotices.md`** (or `LICENSE-THIRD-PARTY`) at plugin root: name, pinned version, license, source URL, and “included as vendored snapshot / subset” where applicable. Plugin README links to it; store/submission packaging (M3) reuses the same file.

### Definition of done

- [x] **`Lexting_Unity_Plugin/package.json`** exists with valid name, version `0.2.0`, Unity minimum; **dependencies = Unity/platform packages only** (Input System, XR Management, OpenXR, etc.—exact list in plugin README). **No** UniRx/SimpleJSON/Surge entries in `dependencies`.
- [ ] Plugin compiles in the **greenfield host** with **no** `Assets/Lexting/` or monolith-only paths referenced.
- [x] Reference **scenes and prefabs** ship inside the package (visible under **Packages/** after install); **`main_01_twozone`** only (legacy test scenes stay in monolith until retired).
- [ ] **Greenfield host** documented in `docs/local_quest_host.md` (or private maintainer notes): Unity version, manifest snippet, platform UPM list, Quest build flow — **not** shipped in the open repo; host passes the **Milestone 1 device checklist** on Quest 2.
- [x] **`ThirdPartyNotices.md`** complete; integrator README warns against adding duplicate UniRx/SimpleJSON/Surge.
- [ ] **Wispy Skybox** — legal gate closed: either **embedded subset** with documented Asset Store EULA + permission (if obtained), or **documented substitute / integrator install step** so `main_01_twozone` is valid in the greenfield host.
- [x] Documented **migration map**: [`docs/plugin_migration_map.md`](plugin_migration_map.md) (Wispy **not** embedded—My Assets).
- [ ] **`Lexting_Unity/`** monolith retired **when maintainer chooses** (optional tag `pre-plugin-monolith` before local delete). Not required for **`0.2.0`** exit if the plugin + greenfield host build Quest.
- [ ] Root [`README.md`](../README.md) reflects single-package install (refresh at cutover).

### Skeletal workstreams

1. **Inventory** — list every assembly under `Assets/Lexting/` **and** `Assets/Plugins/UniRx/`; trace **scene skybox refs** to `Assets/Wispy Sky/`; mark plugin vs reference scenes/prefabs vs delete (deprecated letter cube, streaming, UniRx tests, Wispy Demo/Extras, etc.).
2. **Package skeleton** — `package.json`, asmdef boundaries (Runtime / Editor / embedded UniRx), folder layout for Scenes/Prefabs/Resources/ThirdParty.
3. **Extract & fix references** — one-way copy into plugin; update Resources load paths; scene/prefab GUIDs live in the plugin tree; wire Runtime asmdefs to embedded UniRx only.
4. **Greenfield host bootstrap** — **new** Unity project (not the monolith): ProjectSettings/XR/keystore, `manifest.json` → plugin; Quest APK smoke.
5. **Stabilize** — all feature work happens in **`Lexting_Unity_Plugin/`**; monolith receives **no** further edits after extract starts (except emergency M1 hotfix until host parity).
6. **Retire monolith** — delete **`Lexting_Unity/`** on default branch when greenfield host is stable; tag archive if desired.

**Exit criterion:** Open repo contains a complete **`Lexting_Unity_Plugin/`**; greenfield host references **only** the package and produces a Quest APK; **new feature work** lands in the plugin, not the monolith. Monolith may remain on branch until maintainer deletes it locally.

---

## Milestone 3 — XR refresh & OpenXR conformity

**Target version:** `0.3.0`  
**Goal:** Modernize **XR SDKs** on the **Unity 6** baseline from Milestone 1, then converge on a **single OpenXR-first path** on Quest 2—without changing Lexting typing semantics. Work happens in **`Lexting_Unity_Plugin`** and the **greenfield host** after Milestone 2; the monolith is gone after M2 exit.

**Suggested window:** **2026-11-10 → 2026-11-14** (overlap with late Milestone 2 is OK for *research*; land OpenXR cutover only after the plugin host compiles and Milestone 1 parity is green on the sample).

### M3 go-ahead decisions (2026-11-04)

| # | Decision |
|---|----------|
| 1 | Reference scene + shipping path use **`InputMode.OPEN_XR`** once the adapter is wired. **`OPEN_XR`** is the mode for **any OpenXR-conformant rig** (Meta Quest, PC VR, etc.)—including third-party **Meta/Oculus-style XRRig** prefabs when the host uses the **OpenXR** loader, not a separate Quest-only enum. **M5** wizard defaults to **`OPEN_XR`** and **drops `QUEST_*` from the mode picker**; docs tell devs to choose **`OPEN_XR`** for Meta and other OpenXR runtimes. |
| 2 | Keep **`QUEST_*`** enum values **until M5** (obsolete, not removed in M3); **collapse/remove `QUEST_*` in M5** with wizard + enum cleanup. |
| 3 | **Migrate** **`LextingInputModuleManager`** to a **single** head / left / right **Transform** trio (scene + prefab save). |
| 4 | Rename adapter **`OculusHandInput`** → **`OpenXRHandInput`** (move out of `OculusIntegration/`). |
| 5 | **OVR skeleton** fallback: **deprecate, do not delete** (same policy as Leap reference)—not required for greenfield Quest APKs. |
| 6 | **M3 exit:** adapter wired + **maintainer greenfield smoke** on Quest; **full M1 / build 028 checklist not required** to close M3. |
| 7 | Ship **`v0.3.0`** GitHub Release + bump **`package.json`** after smoke passes. |
| 8 | **Docs:** update **`README.md` version badge only** (no milestone/platform section refresh in M3). Known gaps live in **`docs/`** / ADR. |
| 9 | **Scope:** **`Lexting_Unity_Plugin`** + local greenfield host only; monolith not kept green for M3. |
| 10 | **Setup wizard:** **M5** only (confirmed). |
| 11 | **Leap (and reference adapters) off Quest APK:** ensure **no Leap IL** in Android player—**`LEXTING_FOUND_LEAP_SDK` off** on Android; wrap files in **`#if`**, and **comment out entire classes/files** if anything still pulls Leap into the build. Verify with a greenfield APK build (optional: inspect player assembly / logcat). |

### Baseline → target

| Layer | After Milestone 1 | Milestone 3 target |
|--------|-------------------|---------------------|
| **Unity Editor** | **Unity 6** LTS (monolith, then plugin + sample) | Same; minimum version locked in plugin `package.json` |
| **Quest runtime** | Meta loader + legacy **`Assets/Oculus/`** Integration (`OculusHandInput`, `QUEST_*` modes) — whatever still boots post–Unity 6 upgrade | **OpenXR** as primary loader; Quest validated on **Meta’s OpenXR runtime** |
| **Hand data** | OVR / mixed samples (`UnityEngine.XR.Hands` in places) | One **documented adapter** into **`SpatialGestureInput`** (poses, pinch/grab, wrist orientation for conjugation) |
| **UPM XR stack** | Whatever Unity 6 migration resolved in M1 ([`manifest.json`](../Lexting_Unity/Packages/manifest.json)) | Refreshed **OpenXR**, **XR Management**, **XR Hands**, **XR Interaction Toolkit**, and Meta/Oculus UPM packages aligned to OpenXR-first Quest builds |
| **Legacy defines** | `LEXTING_FOUND_OCULUS_SDK`, auto symbol postprocessor | Replaced or simplified—no dependency on embedded `OVRSkeleton` asset search for daily builds |

### SDK refresh scope

Refresh and document versions together (plugin `package.json` **dependencies** + sample **manifest**; avoid duplicate/conflicting pins):

- **`com.unity.xr.openxr`** — current feature set for Android/Quest; enable hand-tracking / interaction extensions needed for Lexting gestures.
- **`com.unity.xr.management`** — loader order: OpenXR primary for Quest Android builds.
- **`com.unity.xr.hands`** — align with OpenXR hand subsystem where used (e.g. visualizers, future adapter).
- **`com.unity.xr.interaction.toolkit`** — bump to Unity-6-compatible release; keep only what the sample rig actually uses.
- **`com.unity.xr.oculus`** (and/or **Meta XR** all-in-one package if adopted) — **host-only** support packages, not a second parallel typing stack.
- **`com.unity.inputsystem`** — align with Unity 6; QWERTY demo / any new Input System usage.
- **Embedded `Assets/Oculus/` Integration** — inventory → **remove or quarantine** from sample once OpenXR path covers hand tracking; no new features on OVR-only code.
- **Leap (in package)** — **deprecate, do not delete:** keep **`LeapHandInput`**, **`LextingLeap*Marker`**, and **`LEAP_*`** `InputMode` values behind **`#if LEXTING_FOUND_LEAP_SDK`**; document in ADR as the **reference adapter pattern** (bind SDK → **`LextingHand`** → events → **`SpatialGestureInput`**). Not required for Quest/OpenXR shipping; may never ship for Leap again.
- **Third-party in plugin** — UniRx, Surge: re-verify on Unity 6 if M1 left gaps; log blockers in ADR if they force a deferral.

### OpenXR conformity (product + code)

- **`InputMode.OPEN_XR`** is the **blessed** path for reference scene, M3 exit, and Milestone 4 Meta Start demo; **`QUEST_*`** stays in enum until **M5** collapse (obsolete in M3).
- **`SpatialGestureInput`** remains the **only** front door for typing logic; OpenXR/session/hand updates flow through **one adapter** (new or refactored class—name captured in ADR).
- **Gesture parity checklist** on Quest 2: pinch/grab, dual-hand word mode, wrist conjugation poses, hand cursor stability—same as Milestone 1, new stack underneath.
- **Honest README delta:** what is truly “OpenXR-first SDK” vs “OpenXR loader + legacy C# still migrating” at `0.3.0`.

### Questions this milestone must answer

- What actually drove **`SpatialGestureInput`** on the **2022/OVR** baseline, and what is the **minimal OpenXR data** to reproduce it?
- Which **`Assets/Oculus/`** assets are still required after UPM OpenXR + Meta runtime are refreshed?
- Which XR packages live in **plugin `package.json`** (API surface) vs **sample manifest only** (loaders, platform settings)?
- Does **Unity 6** require rendering/UI pipeline changes (e.g. URP settings, uGUI) in the sample scene for Quest?
- Where should **`LextingDefineSymbolsPostprocessor`** go—plugin editor, sample only, or deleted in favor of explicit asmdef/package dependencies?

### Definition of done

- [x] **Sample + plugin compile on Unity 6** (inherited from M1/M2); XR-related package pins documented in plugin [`package.json`](../Lexting_Unity_Plugin/package.json) (Input System, XR Management, OpenXR, XR Hands, XRI, Oculus UPM). Maintainer host `Packages/manifest.json` mirrors the same XR pins (host gitignored; see [`docs/plugin_migration_map.md`](plugin_migration_map.md)).
- [x] **UPM refresh complete** — plugin + host on Unity **6000.0.60f1** pins (e.g. OpenXR **1.15.1**, XR Management **4.5.1**, XR Hands **1.5.0**, XRI **3.0.8**); no stale 2022-only XR package pins on the shipping plugin path.
- [x] **Quest 2 Android build** from **local host** (package-only import) using **OpenXR** loader (+ Meta Quest Support / hand tracking on host); **`main_01_twozone`** in build settings; first greenfield APK **`tutorial_demo.apk`** (build **001**, release **v0.2.0**). Full M1 gesture checklist on this stack not yet signed off (see below).
- [x] **Architecture decision record** ([`docs/xr_integration_adr.md`](xr_integration_adr.md)): primary Quest path, packages + versions, adapter diagram, **`InputMode`** wiring table, legacy/disabled modes, validation status (**`OPEN_XR`** mode gaps explicitly open for M3).
- [x] Input module enum + scene wiring **documented** — ADR **`InputMode` → wiring** table; reference scene **`OPEN_XR`** + **`head_OXR`** trio.
- [x] **Leap reference adapter — deprecated, retained** — **`[Obsolete]`** on **`LeapHandInput`**; Android strips **`LEXTING_FOUND_LEAP_SDK`**; sources remain **`#if`**-gated.
- [x] **Oculus Integration cruft — isolated for OpenXR Quest** — **`OpenXRHandInput`**; OVR skeleton + **`SimpleOculusHandInput`** deprecated, **`#if LEXTING_FOUND_OCULUS_SDK`**; **`QUEST_*`** enum retained until M5.
- [x] Quest 2 **greenfield smoke** on **`OPEN_XR`** + **`OpenXRHandInput`** (typing, hands, load scene)—file issues in local [`bugs/`](../bugs/) as needed. Full M1 / build **028** parity checklist is **follow-up**, not a gate for M3 exit.
- [x] **Known gaps** — [`docs/known_gaps.md`](known_gaps.md). **README:** version badge **`0.3.0`** only.

### Skeletal workstreams

1. **UPM / SDK inventory** — diff M1 manifest vs OpenXR-first target; upgrade in controlled commits; note Meta Quest setup doc links.
2. **OpenXR spike** — Quest 2 device session: loader, hand tracking, compare joint/wrist data to archived OVR baseline recordings from Milestone 1.
3. **Adapter & input manager** — **rename** to **`OpenXRHandInput`**; wire **`OPEN_XR`** (attach adapter, **`InitHandEventBindings`** / generalize subscriptions); **migrate** manager to **one** head/hand Transform trio; set **`main_01_twozone`** to **`OPEN_XR`**.
4. **Legacy deprecate (not delete)** — **OVR skeleton** + **`SimpleOculusHandInput`**: mark deprecated like Leap; keep **`#if LEXTING_FOUND_OCULUS_SDK`** for Standalone/monolith reference only. No embedded **`Assets/Oculus/`** in package. **`QUEST_*`** enum retained until M5.
5. **Validation & docs** — Milestone 1 checklist on device; ADR + optional README platform section update.
6. **Leap → template** — **`[Obsolete]`** + ensure Quest APK excludes Leap (**`#if`**, **comment out whole classes** if needed); ADR template section (done).
7. [x] **Release** — plugin **`0.3.0`**, greenfield APK build **005** → **`Releases/tutorial_demo.apk`**, **`gh release v0.3.0`** after smoke.

**Exit criterion:** Greenfield Quest smoke on **`OPEN_XR`** + **`OpenXRHandInput`**; map in ADR; **`0.3.0`** published. Full **028** parity and **`QUEST_*` removal** deferred to post-smoke QA and **M5**.

**Schedule note:** If full OpenXR cutover cannot finish before **11/15**, Milestone 4 may ship on a **documented interim** stack (e.g. Unity 6 + temporary OVR adapter)—only with explicit ADR approval; default goal is OpenXR-primary for `0.3.0`.

---

## Milestone 4 — Meta Start delivery (polish mode)

**Doc note:** Keep **`implementation_plan.md` changes local** until **M4 exit** (commit with **`0.4.0`** release), unless you explicitly ask for an interim docs commit.

**Target version:** `0.4.0` (**beta** — Meta Start submission; not `1.0.0`)  
**Goal:** Competition-ready **`tutorial_demo.apk`**: **`TutorialManager`** reliably onboards new users on Quest. **Primary engineering focus is the in-app tutorial coroutine/UI** — **not** re-recording the Meta Start **3-minute product demo video** (see below). **Authoritative design (22 steps, indices 0–21, copy, validation):** [`in_app_tutorial.md`](in_app_tutorial.md).

**Two different “tutorials”:**

| Artifact | Purpose |
|----------|---------|
| **In-app `TutorialManager`** | Short onboarding per [`in_app_tutorial.md`](in_app_tutorial.md) (grab demos, typing + word mode, checkmarks, skip → menu / typing). **M4 P0.** |
| **~3-minute Meta Start video** | **Marketing / judges demo** of **core widget behavior** (dual-volume typing, word mode, etc.) — **not** a capture of the in-app onboarding slides. Existing **~2021 footage** may be **re-submitted or lightly re-edited** for 2026 rules; **not** tied to fixing `TutorialManager`. |

**Polish mode starts:** **2026-11-15**  
**Submission deadline:** **2026-11-19**

**Suggested window:** **2026-11-04 → 2026-11-19** (tutorial repair + smoke can start immediately after M3; **feature freeze** for polish on 11/15).

### M4 go-ahead decisions (2026-11-04)

| # | Decision |
|---|----------|
| 1 | **`TutorialManager` is P0** — **walk the full in-app flow on device as built today** (including timer auto-pages and any dev **`UnitTest_Trigger_*`** pop-guns); fix state-machine / stuck bugs found there. **Unmapped gestures stay unmapped for now**; making steps fully manual-only is **follow-up** after the walkthrough. |
| 2 | **Meta Start video** — **product demo** (~2021 capture); submit/re-edit for 2026 rules if needed. **Not** the in-app onboarding sequence. |
| 3 | **Exit path** — in-app tutorial smoke → UX (skip, don’t-show-again) → **full Quest smoke** → ship **`0.4.0`**. Real-hand pop-gun wiring vs unit tests decided **after** walkthrough. |
| 4 | **Skip** — **skip only** (no restart): **stop pending tutorial coroutines**, **dismiss overlay**, fire same completion path as finish so **typing / post-tutorial menu** can proceed. |
| 5 | **Don’t show again** — **`PlayerPrefs`** (or equivalent) OK for `0.4.0`; may revisit persistence later. |
| 6 | **Speed test** — **hide or disable** middle menu option for M4 (implementation was never finished; see below). |
| 7 | **Tutorial demo hands** — **`0.3.0` / build 005:** animated **non-tracked** **`TutorialHand_Left` / `TutorialHand_Right`** (gesture pose hints) **missing on device** — prefab hierarchy may still be intact; investigate **canvas motion / scale** (below) and monolith diff. **Restore / fix visibility** before tutorial walkthrough sign-off. |
| 8 | **Output UI distance (M3)** — Scene override placed **`[EXAMPLE_OUTPUT_UI]`** **`~12–13.5 m`** on Z, but **`ZoneTextingDemoCanvas`** still **lerps `_motionRoot`** (holds **`TutorialGroup` + demo hands**) toward **`head + flatForward × _cameraDepthDistance`** when the user is **not** looking at the UI; may read as **snapping closer** at start. **M4:** reconcile editor placement vs runtime follow for **tutorial phase**. |

### Definition of done

- [ ] **Tutorial — demo hands restored** — Reconcile scene/prefab vs monolith; confirm **`TutorialHand_*`** under **`TutorialGroup`** (not deleted from **`TextOutputCanvas.prefab`**). Fix **visibility** (layer, scale, materials) and **parent motion** so **`FadeHands` / `MoveThumb`** read on Quest.
- [ ] **Tutorial — output canvas distance** — **`ZoneTextingDemoCanvas`** on **`TextOutputCanvas`**: **`_cameraDepthDistance`** (**12** in prefab), **`_rootMotionDampeningFactor`** (**0.1**), **`_motionRoot`** = rect that owns **`TutorialManager`**. M3 scene sets **`m_LocalPosition.z ≈ 13.56`** on that root, but **`Update`** still **lerps position** when **`_lookConfidence < 0`** and **always lerps rotation** toward the user. **M4:** decide whether tutorial should **disable follow**, use a **fixed world pose**, or tune distance so demo hands stay in view (likely related to “missing hands”).
- [ ] **Tutorial — walkthrough smoke (as built)** — Quest 2 pass the **designed** coroutine flow end-to-end; document what triggers each page today (pop-gun events, timers, unit-test injection). Fix P0 stuck states / dead-ends in local [`bugs/`](../bugs/) QA. **Defer** “all manual, no timers” and **new gesture mapping** until after this pass unless a step is already broken without it.
- [ ] **Tutorial — skip** — user **skip** **stops active tutorial coroutines**, **dismisses** UI, invokes **`OnTutorialCompleted`** (or equivalent) so **`HighlightableZoneController` / typing** path matches normal completion.
- [ ] **Tutorial — don’t show again** — **`PlayerPrefs`** (or equivalent) suppresses in-app tutorial on later launches when enabled.
- [ ] **Tutorial — state machine & bugs** — no double-fires or stuck `currentPage` after walkthrough fixes; handlers (`TutorialHandHandler`, checkmarks, bullseye) consistent with whatever trigger source is active during M4 testing.
- [ ] **Tutorial — UX pass** — copy, pacing, legibility after walkthrough; **no full restart** flow in `0.4.0`.
- [ ] **Tutorial — triggers (follow-up)** — after walkthrough: decide on **OpenXR pop-gun** vs **`SpatialGestureInput` unit tests**, timer auto-pages, and stream-receiver fast-dismiss for submission builds.
- [ ] **Speed test — hide for M4** — remove or disable **`HighlightableZoneController`** menu index **1** (`OnActivateSpeedTest`); no live subscribers today (see **Speed test inventory** below).
- [ ] **Full build smoke** — after tutorial fixes: dual-volume typing, word mode, hotkeys, output receivers, load **`main_01_twozone`** on greenfield host; no P0 regressions vs M3 **`0.3.0`** baseline.
- [ ] **Build label** — **`0.4.0`** Quest APK → repo-root **`Releases/tutorial_demo.apk`** (greenfield host + package); **`Releases/README.md`** + GitHub Release **`v0.4.0`**; submission metadata (beta labeling as required).
- [ ] **Known issues** — one-page list (lighting, dictionary, PC keyboard / passthrough out of scope, tutorial edge cases deferred, etc.).
- [ ] **3-minute product demo video (submission)** — **~2021 widget demo** re-submitted or lightly edited for 2026 Meta Start rules; update README / submission link when final URL is set (**independent** of in-app tutorial work).

### Skeletal workstreams (priority order)

1. **Tutorial demo hands + canvas pose** (P0) — monolith diff; **`ZoneTextingDemoCanvas`** vs M3 **~12 m** scene placement; restore/fix **`TutorialHand_*`** visibility on device (**blocks** gesture-hint UX).
2. **Tutorial walkthrough QA** (P0) — device pass **as built**; fix state machine bugs; inventory triggers vs timers vs unit tests.
3. **Tutorial UX** (P0) — **skip** (coroutine cancel + dismiss + complete), **don’t-show-again** (`PlayerPrefs`), copy/pacing tweaks.
4. **Speed test menu** (P0) — hide/disable unfinished **`SpeedTestManager`** entry from floater menu.
5. **Reference build smoke** (P0) — end-to-end Quest pass (tutorial + typing stack); same greenfield path as M3 releases.
6. **Submission packaging** (P0) — store/metadata, attribution (SimpleJSON, Surge, Unity, Meta), license pointers, release notes.
7. **Product demo video** (P1) — Meta Start **~3 min widget demo** (2021 asset); not in-app tutorial capture.
8. **Nice-to-have** (P2) — short “how to build Quest APK from sample” in `docs/`; finish **`SpeedTestManager`** post-M4.

**Exit criterion:** First-time user can **complete or skip** the in-app tutorial on Quest 2, then use dual-volume typing in **`main_01_twozone`**; speed-test menu hidden; **`0.4.0`** published; Meta Start submission assets ready by **11/19** without core typing refactors unless a P0 crash appears.

### Tutorial smoke checklist (device — fill in during M4)

| Step | Trigger / expectation | Pass |
|------|------------------------|------|
| Launch | Tutorial overlay visible (`Introduce`); **demo `TutorialHand_*` meshes** visible (not user tracked hands) | |
| Page 0 | Right **single pop-gun** → right checkmark | |
| Page 0 | Left **single pop-gun** → left checkmark → success audio / bullseye | |
| Page 0→1 | Page turn (+ note if **timer** auto-advances) | |
| Mid tutorial | Thumb / hand hint animations if applicable | |
| Page 2 | **Dual pop-gun** → both checkmarks → advance | |
| Final pages | Last transitions → **`tutorialCompleted`** | |
| Dismiss | Overlay fades; typing widget usable | |
| Regression | Typing + word mode after tutorial | |
| Skip | Pending coroutines stopped; overlay dismissed; same post-tutorial path as finish | |

### Output UI motion (M3 note — tutorial hands)

**Components:** **`TextOutputCanvas`** root → **`ZoneTextingDemoCanvas`** (`Runtime/Scripts/Zones/ZoneTextingDemoCanvas.cs`); **`_motionRoot`** → **`TutorialGroup`** / **`TutorialManager`** / **`TutorialHand_*`** nested prefabs.

**M3 editor change:** **`main_01_twozone`** prefab instance **`[EXAMPLE_OUTPUT_UI]`** overrides **`m_LocalPosition.z`** to **~13.56** (intent: ~**12 m** from rig at start).

**Runtime behavior (still active):** **`ZoneTextingDemoCanvas`** only **lerps to the true 12 m world target** while **`_isMoving`** — set when **`_lookConfidence < 0`** (user **not** looking at the UI). If the user **faces the tutorial at launch**, **`_isMoving` stays false** and the canvas **never** gets the **`head + horizontalForward × 12`** snap on that frame.

**Observed on Quest (build 005):** **Start distance ~3–5 m**, not 12 m. **Turn ~90°** (look away → **`_isMoving`**) → canvas **repositions to ~12 m**; turn back → **another ~12 m** reset, **noticeably farther** than the initial pose. So **editor `m_LocalPosition.z ≈ 13.56`** on the world-space **`RectTransform`** does **not** match the script’s **12 m world** formula (scale / rect / anchor vs **`transform.position`** lerp).

**Likely link to “missing” demo hands:** Tutorial + **`TutorialHand_*`** ride **`_motionRoot`**; wrong **startup** distance/scale vs post-reposition **12 m** can hide skinned hint meshes until after a gaze-triggered reset.

**M4 fix (code):** On first frame with **`hasHead`**, **snap `_motionRoot`** with the **same** world formula as reposition (or call **`OnRefreshPosition`** once); optional **freeze follow during `TutorialManager`**; stop relying on scene **local Z** for distance.

### Speed test inventory (plugin — M4: hide menu item)

- **`SpeedTestManager`** (`Runtime/Scripts/SpeedTests/`) — WPM timed test UI: scrollable prompt text, `Compare()` vs `InputField`, **`RunTest()` / `StopTest()`**; **`Awake` does not auto-start** (init coroutines commented out).
- **`HighlightableZoneController`** — post-tutorial floater menu: index **0** typing tutorial, **1** **`OnActivateSpeedTest`**, **2** freestyle. **`OnActivateSpeedTest` has no active subscribers** (only commented hook in deprecated **`CubeController`**).
- **`TextOutputCanvas.prefab`** — **`SpeedTestPanel`** / **`SpeedTestDisplay`** UI exists but is not wired into a complete M4 flow.
- **M4 intent:** **Hide/disable menu option 1** so judges only see working paths.

---

## Milestone 5 — Lexting Setup Wizard (integrator DX)

**Target version:** `0.5.0`  
**Goal:** Third-party devs open **their** scene, run one **Lexting** menu command, and get a working Lexting stack without hand-wiring XR anchors or hunting prefabs. When automation cannot succeed, the tool reports **actionable validation errors and warnings** (Console + optional dialog / report window)—not silent failure.

**Suggested window:** **After Milestone 4** (post–Meta Start); not on the **11/19** critical path unless schedule allows a thin MVP.

**Prerequisite:** Milestone 2 plugin layout (Runtime + **Editor** asmdef), Milestone 3 **`OpenXRHandInput`** + **`OPEN_XR`** reference scene.

**Input mode (M5):** Setup wizard **only offers `OPEN_XR`** for XR hand tracking (Meta Quest, Steam Frame, etc.—any OpenXR-conformant host). **`QUEST_*` modes removed from wizard UI** and **collapsed/removed from `InputMode` enum** in M5; until then M3/M4 use **`OPEN_XR`** in reference scene while obsolete **`QUEST_*`** values remain for serialized compatibility.

### Product shape

| Surface | Behavior |
|---------|----------|
| **Menu: Lexting → Setup Scene…** | Operates on the **active scene** (saved or dirty). Idempotent where possible: re-run updates wiring instead of duplicating `[INPUTS]` / duplicate rigs. |
| **Menu: Lexting → Validate Scene** *(optional split)* | Read-only pass; same rules as setup validation without mutating hierarchy. |
| **Auto setup (best effort)** | **Prerequisite:** integrator scene already has an **XR player rig** (OpenXR loader configured). Discover or create: **`[INPUTS]`** + **`LextingInputModuleManager`** with **`InputMode.OPEN_XR`** (including Meta/Oculus-branded XRRig prefabs—OpenXR session, not **`QUEST_*`**). Bind **head / left / right hand** from **`XROrigin`** / XRI (**error** if no rig). **Do not** parent the dual-volume widget into the saved scene—runtime bootstrap only. **No `QUEST_*` in wizard mode list (M5).** |
| **Manual integrator hooks (unchanged)** | **`ZoneFieldOutputReceiver`** / **`ZoneTextOutputReceiver`** on each output target—integrator **enables/disables the receiver component** (or GameObject) to choose which field or label is live; see **Widget & output lifecycle** below. No automatic **`InputField`** focus wiring ( **`Text`** has no focus callbacks). |

### Widget & output lifecycle (integrator UX — ship in M5)

**Problem (prototype today):** Reference scenes keep the dual-volume widget in-scene and “summon” via gesture (`ControllerActivated`); there is **no** hide-at-idle or **InputField**-driven show flow. **`LextingOutputManager`** **broadcasts** the same string to every enabled receiver—integrators need a documented, supported pattern for real apps.

**Widget bootstrap (serialized on `LextingInputModuleManager`, wizard sets default):**

| Option | Behavior |
|--------|----------|
| **At scene load** | Runtime bootstrap **`Instantiate`**s the package widget prefab(s) **disabled / hidden** (no typing interaction, cursors off) until the integrator or Lexting API shows it. |
| **On first output receiver `OnEnable`** | Defer **`Instantiate`** until the **first** **`ZoneFieldOutputReceiver`** or **`ZoneTextOutputReceiver`** enables—still starts **hidden/disabled** until show rules fire. |

Integrator picks one mode via **`LextingInputModuleManager`** (enum or bool pair—exact API TBD in **`setup_wizard.md`**). Wizard + reference sample should demonstrate **at scene load (hidden)** as the default for new projects.

**Show / hide / dismiss:** Replace reliance on index-finger summon alone for 3rd-party apps—expose a small runtime API (and/or events) to **show**, **hide**, and **dismiss** the widget, wired to **`ControllerActivated`**, calibration, and cursor/volume visibility. Reference demo may keep gesture summon; integrator docs treat **explicit show** as primary.

**Output targeting (manual per object — intentional):**

- **Do not** auto-enable/disable receivers from **`InputField`** select/deselect callbacks in the SDK—**`Text`** labels will not get parallel focus events.
- **Do** document that devs **toggle each receiver’s enabled state** (or GameObject active) when the user focuses a field—only **one** enabled **`InputField`** receiver at a time unless they intentionally mirror text to multiple targets.
- Optional **Getting Started** snippet: pseudo-code “on field A focused → enable receiver A, disable B/C” (maintainer updates README **in M5 only** when this ships—not during M2).

### Quest passthrough → PC text (Netcode for GameObjects)

**Context:** Legacy **`QUEST_*_PASSTHROUGH`** modes were wired in **`main_01_twozone`** via **`LextingNetworkManager`** (MLAPI **`NetworkManager`** + **`LextingMLAPIManager`** + **`LextingServerStatus`**). MLAPI predates **Unity Netcode for GameObjects (NGO)**; the old stack is **commented out** in source and the scene object is **optional / often disabled** for local Quest work. **`LextingStream_Quest.unity`** is a separate UDP gesture lab—not the two-zone passthrough product path.

**Monolith retention (until Netcode ships):** Keep **`Lexting_Unity/`** in the repo as reference for the old MLAPI implementation (`Assets/Lexting/Scripts/Streaming/MLAPI/`, **`MLAPI_*`** test scenes, serialized **`LextingNetworkManager`** on **`main_01_twozone`**) until NGO replacements are implemented and validated. Do **not** delete the monolith solely for MLAPI cleanup before that milestone completes.

**Target behavior (unchanged product intent):** Quest passthrough app renders the dual-volume widget locally; finalized text (and hotkeys where applicable) is delivered to a **Windows host** on the LAN—today’s commented **`LextingStreamClient` `ServerRpc`s** + **`windowsKeySim`** on the host. Reconcile with README **UDP Streaming Service** direction (Alpha) so integrators get one supported PC path, not two competing stacks.

**PC host side:** The working prototype included a **separate PC server** (maintainer has an **archive** to restore when M5 networking starts—not in this repo today). M5 must **migrate that server** into the delivery (repo layout TBD: standalone Windows build, plugin sample, or companion project) and wire it to the same protocol as the Quest NGO client.

**M5 tasks (networking delivery):**

- [ ] Add **`com.unity.netcode.gameobjects`** (and transport, e.g. Unity Transport) to the **plugin** package dependencies; define minimum Unity / NGO versions in **`package.json`**.
- [ ] **Restore archived PC server** from maintainer archive; **migrate** host/receiver logic to NGO (or unified transport chosen above); document build/run, IP/port, and pairing with Quest client.
- [ ] Implement NGO replacements for the MLAPI roles: **network manager / host–client lifecycle**, **stream client** (Quest → host keystrokes), **server/host status** (IP, ready/off)—names TBD; avoid resurrecting MLAPI APIs.
- [ ] Port commented send sites in **`LextingOutputManager`** and **`CubeWordAssemblyController`** (`isDesktop` / passthrough final word) to the new client API (RPC or approved message pattern).
- [ ] Ship a **reference scene hook** (e.g. on **`[GameModule]`** or dedicated root): NGO **`NetworkManager`** + Lexting stream components; **wizard optional wiring** for passthrough mode when **`QUEST_HANDS_PASSTHROUGH`** is selected (baseline: documented manual setup).
- [ ] Document **PC host vs Quest client** build targets, firewall/port, and enable/disable of networking (replacing “toggle **`LextingNetworkManager`** active”).
- [ ] Manual QA: passthrough mode + two-zone widget + text appearing on Windows host; regression on **`QUEST_HANDS`** with networking **off**.
- [ ] After NGO path passes QA: remove MLAPI scene leftovers from **plugin** reference scene; archive or trim monolith MLAPI folder only when reference is no longer needed.

**Safe to remove now (pre-NGO):** Deleting the **`LextingNetworkManager`** GameObject from **`main_01_twozone`** causes **no runtime null refs** in the plugin or monolith—nothing in active C# resolves that object or **`LextingMLAPIManager.Singleton`**; MLAPI calls exist only in **comments**. **`LextingServerStatus`** is an empty stub and was only on that object.

### Validation (errors vs warnings)

**Errors (setup aborts or incomplete — must fix):**

- No XR Management / OpenXR (or documented legacy path) configured for the active build target.
- No **`XROrigin`** / XR player rig / head transform in the scene (integrator must add their rig **before** running the wizard).
- Hand tracking required for chosen mode but subsystem / anchors missing after auto setup.
- Duplicate conflicting **`LextingInputModuleManager`** or broken references after partial run.
- Plugin assemblies or Resources prefabs missing (package not installed correctly).

**Warnings (scene may run; fix recommended):**

- Legacy **`QUEST_*`** mode still set on **`[INPUTS]`** (wizard should set **`OPEN_XR`**); embedded **`Assets/Oculus/`** Integration detected.
- **`InputField`** / UI text targets present without **`ZoneFieldOutputReceiver`**.
- Multiple XR rigs in scene; wizard bound the first match—user should confirm.
- Tutorial / demo-only objects in a production integrator scene.
- Scene not saved after mutation.

Validation output should name **GameObject paths**, **component types**, and **fix hints** (e.g. “Enable hand tracking in OpenXR settings”, “Add Package X”).

### Definition of done

- [ ] **Editor assembly** under **`Lexting_Unity_Plugin/Editor/`** (or equivalent) with **`LextingSetupWizard`** (name TBD) and menu items registered via **`MenuItem("Lexting/…")`**.
- [ ] **Setup Scene** command implements discovery + bind + runtime widget bootstrap wiring per ADR from Milestone 3; **no** requirement to open **`main_01_twozone`** for basic integration.
- [ ] **Validate Scene** (combined or separate) lists all errors/warnings; CI-friendly optional **exit code** or log format documented for future automation.
- [ ] **Unit / Edit Mode tests** where feasible (transform discovery, idempotent second run); manual QA checklist on blank XRI template + reference sample scene.
- [ ] **[`README.md`](../README.md) Getting Started** updated: primary path = install package → open your scene → **Lexting → Setup Scene** → add output receivers → build; **plus** widget bootstrap mode, **manual receiver enable/disable on focus**, and show/hide widget API (no **`InputField`** auto-focus magic in the SDK).
- [ ] **Decision log** entry + short **`docs/setup_wizard.md`** (integrator-facing): what the wizard does, what it cannot do, troubleshooting table.
- [ ] **`LextingInputModuleManager`** exposes **widget bootstrap timing** (scene load vs first receiver **`OnEnable`**) and default **hidden/disabled** instantiate; wizard persists choice on **`[INPUTS]`**.
- [ ] **Runtime widget bootstrap** creates widget **hidden/disabled** per manager setting; **show / hide / dismiss** API documented (replace gesture-only summon for integrators).
- [ ] **`setup_wizard.md` + Getting Started:** integrators **enable one `ZoneFieldOutputReceiver` / `ZoneTextOutputReceiver` at a time** via component **`enabled`** (or GameObject active)—manual focus routing; explain broadcast behavior when multiple receivers stay enabled.
- [ ] **Passthrough / PC typing (NGO):** Netcode stack replaces MLAPI **`LextingNetworkManager`**; passthrough send path wired from output manager / word assembly; monolith retained until port validated (see **Quest passthrough → PC text** above).

### Skeletal workstreams

1. **Scene inventory & rules engine** — classify objects (XR rig, existing Lexting roots, UI fields); ordered mutation plan.
2. **Rig discovery & binding** — XRI / XROrigin heuristics on **integrator-owned** rig; populate **`LextingInputModuleManager`** slots for active mode; fail validation if ambiguous or missing.
3. **Runtime widget bootstrap** — **`Instantiate`** package prefab(s) (`TwoZoneCube`, output manager, hotkeys, etc.) **hidden/disabled** per **`LextingInputModuleManager`** (scene load vs first receiver **`OnEnable`**); head-relative spawn from existing controller offsets; **show/hide/dismiss** API; wizard ensures serialized hook on **`[INPUTS]`** is valid.
4. **Output & focus contract** — document **manual receiver toggle** per focused **`InputField`** / active **`Text`**; no SDK **`InputField`** focus auto-wiring (integrators control **`enabled`** per receiver; simultaneous multi-target output is intentional broadcast behavior if they enable multiple receivers).
5. **Validation UI** — Console + modal or **`EditorWindow`** report; copy-to-clipboard for support.
6. **Docs & README** — wizard-first narrative + widget lifecycle + receiver enable/disable pattern (**Getting Started** update **only when M5 ships**).
7. **Passthrough networking (NGO)** — Netcode host/client + keystroke relay for **`QUEST_*_PASSTHROUGH`**; reference scene wiring; align with or supersede README UDP Streaming Service alpha.

### Stretch goal (same milestone, not required for `0.5.0` exit)

**OpenXR-direct poses — no rig Transform wiring on the integrator’s player controller.**

Assuming **OpenXR** as the runtime, it should be feasible for Lexting to **poll view (head) and hand (or controller) poses from the active OpenXR session**—via Unity’s OpenXR / **XR Hands** (and/or documented extensions)—and feed **`SpatialGestureInput`** without serializing **head / left hand / right hand** references into **`LextingInputModuleManager`** or requiring the wizard to discover anchors on the dev’s **XR Origin** hierarchy.

| If delivered | Integrator scene after wizard |
|--------------|-------------------------------|
| **Baseline M5** | **`[INPUTS]`** + manager with **Transform** refs into **their** rig (README path today). |
| **Stretch** | **`[INPUTS]`** + **`InputMode.OPEN_XR`** (and output receivers only); Lexting owns pose read each frame/tick from OpenXR—**no** Lexting components on the player rig beyond what their template already has for rendering/local XR. |

**Engineering notes (planning only—no README change until shipped):**

- Reuse / extend Milestone 3 **OpenXR adapter** so **`LextingInputModuleManager`** can operate in a **“OpenXR session poses”** mode with null or ignored rig **Transform** slots when the stretch path is active.
- Wizard validation shifts: require **OpenXR loader + hand tracking capable session** (project/scene), not “bind these three transforms.”
- Fallback: keep rig-reference mode for non-OpenXR, legacy **`QUEST_*`**, or runtimes where joint data is only exposed on tracked **`Transform`**s.
- Risk: pose space (tracking vs. player vs. stage), latency vs. existing **HandMarker** / cursor smoothing, and parity with conjugation wrist orientation—needs Quest 2 checklist on stretch path before calling it default.

**Stretch definition of done (optional checkboxes):**

- [ ] Runtime path documented in ADR + **`setup_wizard.md`**; README Getting Started updated **only when** stretch ships as default.
- [ ] **`OPEN_XR`** pose-only mode passes Milestone 1 gesture checklist on Quest 2 without **`LextingInputModuleManager`** head/hand **Transform** assignments.
- [ ] Setup wizard **Setup Scene** succeeds on XRI template with **no** rig binding step when stretch mode is enabled.

**Exit criterion:** A developer with a stock **Unity 6 + OpenXR + XRI** template project, package installed, runs **Lexting → Setup Scene** once, adds **`ZoneFieldOutputReceiver`** to their **`InputField`**, **enables that receiver when the field is focused** (others disabled), **shows the widget** via documented API when typing should begin, and reaches typable volumes in Play Mode on device or in editor play (per supported mode)—without assigning nine transform fields by hand and without leaving the widget visible at scene load unless they chose otherwise.

---

## Calendar (at a glance)

| Dates | Focus |
|--------|--------|
| **11/01 – 11/08** | Milestone 1 — **Unity 6** upgrade + Quest 2 baseline parity (`Lexting_Unity/`) |
| **11/08 – 11/12** | Milestone 2 — single Unity package; retire monolith |
| **11/10 – 11/14** | Milestone 3 — UPM XR refresh, OpenXR Quest path (plugin + local host) |
| **11/04 – 11/14** | Milestone 4 — **`TutorialManager`** manual mode, trigger smoke, UX pass |
| **11/15 – 11/19** | **Polish mode** — full build smoke, submission packaging, **`0.4.0` beta** release; video confirm/reuse |
| **Post–11/19** | **Milestone 5** — Lexting Setup Wizard (`0.5.0`); integrator menu + validation |

*Calendar is aggressive:* if Milestone 2 slips, compress XR work (Milestone 3) to what is required for the demo path only, or ship Meta Start from a **thin** sample while finishing plugin extraction on a branch. **Milestone 5 is explicitly last** and does not block Meta Start submission.

---

## After roadmap milestones (future stable `1.0.0`)

Work below is **out of scope** for Milestones 1–5 (`0.1.0`–`0.5.0` beta). **`1.0.0`** means a later **stable / GA** SDK—not Meta Start submission.

- Publish plugin to a registry (OpenUPM / Git URL) instead of `file:` only.
- Harden **Lexting OpenXR SDK** boundary inside the plugin (input adapter surface).
- Replace **UniRx** (`EventUtils` / `AsyncUtils`) with modern Unity patterns.
- Evaluate **Surge** replacement (DOTween, Unity animations, or minimal custom tweens).
- Unreal reference host, additional OEM runtimes from [README platform table](../README.md#platform-strategy).

*(Integrator scene setup is **in scope** as **Milestone 5**, not an open-ended post-roadmap item.)*

---

## Decision log (append as you go)

| Date | Decision | Notes |
|------|----------|--------|
| 2026-11-01 | Plan created | Initial three milestones: Quest parity → XR conformity → Meta Start polish. |
| 2026-11-01 | Four milestones | Insert **Milestone 2 — Unity plugin packaging** after baseline restore; renumber XR → M3, Meta Start → M4. Target: one `Lexting_Unity_Plugin` in git; private local Quest host only; retire `Lexting_Unity/`. |
| 2026-11-01 | Milestone 3 scope | **UPM XR SDK refresh** + **OpenXR-first** Quest conformity (not OVR-only patch). |
| 2026-11-01 | Unity 6 → M1 | **`unity pipeline install` requires Unity 6+**; editor upgrade moved from M3 into **Milestone 1** so Quest baseline + Cursor MCP share one reopen. |
| 2026-11-01 | Quest boot on `002` | Disabled embedded **`AndroidOpenXR/OVRPlugin.aar`** for Android; **`lexting_002.apk`** loads **`main_01_twozone`** on Quest 2; hands still broken (see baseline log). |
| 2026-11-01 | CLI build/install | Added **`build_quest.ps1`** (Pipeline `build run`) and **`install_quest.ps1`** (latest `lexting_NNN.apk` sideload). |
| 2026-11-01 | Package-only public repo | Scenes/prefabs inside one plugin package; **no** public `Lexting_Unity_Sample`—Quest host project is maintainer-local only. |
| 2026-11-01 | No Samples~ import step | Reference scenes ship in the package tree; install once via Package Manager (not Unity’s optional **Samples → Import** copy workflow). |
| 2026-11-01 | Milestone 5 — Setup Wizard | **Final roadmap milestone** (`1.1.0`): **Lexting** Editor menu (**Setup Scene** + validation) for third-party integrators; auto rig/Lexting bootstrap when possible; errors/warnings when not. Post–Meta Start; README becomes wizard-first. |
| 2026-11-01 | M5 stretch: OpenXR poses | **Optional:** poll head/hand from **OpenXR session** (XR Hands / extensions) so integrators need **no** Lexting **Transform** wiring on their player controller; baseline M5 still rig-bind via wizard; README unchanged until stretch ships. |
| 2026-11-02 | Quest QA workflow | `bugs/QA/QA-<build>-*.md` naming, per-bug Description/Status blocks, carry-forward + `qa_close_session.ps1`; install script warns if QA Enter skipped. |
| 2026-11-02 | Device fixes (M1) | Hover label tint (`LetterSwitcher`); center hover stuck (`HandCursor` threshold exit); compile fixes for player build (`OculusHandInput` / serialization). Verified through **`lexting_028.apk`**. |
| 2026-11-04 | Milestone version map | **Sequential 0.x beta:** M3 **`0.3.0`**, M4 Meta Start **`0.4.0`**, M5 wizard **`0.5.0`**. **`1.0.0`** reserved for post-roadmap stable/GA—not Meta Start. (Earlier plan used M3=`0.5.0`, M4=`1.0.0` to mirror “launch” semver.) |
| 2026-11-04 | M3 Leap scope | **Deprecate, do not delete** in-package Leap adapter/marker code; keep as **reference template** for future hand-tracking SDKs; Quest shipping excludes Leap via defines. |
| 2026-11-04 | M3 go-ahead | **`OPEN_XR`** + **`OpenXRHandInput`**; single Transform trio; **`QUEST_*` until M5**; smoke exit + **`v0.3.0`** release; README badge only; Leap/OVR **deprecated not deleted**; Leap off APK via **`#if`** + comment-out classes if needed. |
| 2026-11-04 | M4 go-ahead | **Tutorial-first** Meta Start path: fix **`TutorialManager`** (manual progression, trigger/state bugs), structured tutorial smoke, UX pass, full build smoke, **`0.4.0`**; **legacy ~2021 demo video may suffice**; no new demo script / capture unless required. |
| 2026-11-04 | M4 UX / video | **Skip only** (no restart); optional **don’t-show-again** on load; video = **reuse + re-upload/edit** for 2026 rules. **Baseline review:** tutorial pop-guns today come from **`SpatialGestureInput` unit-test coroutines**; **`OpenXRHandInput` pop-gun detection is commented out** — M4 must wire real gestures before manual smoke sign-off. |
| 2026-11-04 | M4 scope clarify | **Meta Start video ≠ in-app tutorial** (product/widget demo). Walk through **`TutorialManager` as built** before manual-only/timer changes; **skip** cancels coroutines + dismisses; **`PlayerPrefs`** for don’t-show-again; **speed test** menu hide (unfinished **`SpeedTestManager`**). **Don’t commit** this plan until M4 done. |
| 2026-11-04 | M4 tutorial hands | Build **005** / **`0.3.0`**: animated **`TutorialHand_Left`/`Right`** hint hands **missing** (likely M3 scene cleanup). M4 P0: diff monolith **`main_01_twozone`** + **`TextOutputCanvas`** and restore prefab instances. |
| 2026-11-04 | M4 canvas vs hands | M3 scene **local Z ≈ 13.56** does **not** yield **12 m** at runtime; **~3–5 m** at start until **`_isMoving`** (look away) runs **`ZoneTextingDemoCanvas`** world snap — explains **farther reset** vs start and possibly **missing demo hands**. |
| 2026-11-03 | M5 widget & output lifecycle | Hidden/disabled widget bootstrap (scene load vs first receiver **`OnEnable`**) toggle on **`LextingInputModuleManager`**; show/hide API; integrators **manually enable/disable** output receivers per focus—no **`InputField`** auto-focus in SDK. README Getting Started updated when M5 ships. |
| 2026-11-03 | M5 passthrough / Netcode | Replace MLAPI **`LextingNetworkManager`** stack with **Unity Netcode for GameObjects** for **`QUEST_*_PASSTHROUGH`** PC typing; **migrate archived PC server** into delivery; keep **`Lexting_Unity`** monolith until NGO port references old **`Streaming/MLAPI/`**; removing scene **`LextingNetworkManager`** is safe (no active code refs). |
| | | |

---

## Related docs

- [`README.md`](../README.md) — product overview, architecture, OpenXR-first strategy  
- [`conjugation_algorithm.md`](conjugation_algorithm.md) — word mode and finalization (demo/tutorial reference)  
- [`setup_wizard.md`](setup_wizard.md) — *(Milestone 5)* integrator menu command, validation rules, troubleshooting *(create with M5)*  
- Local Quest QA + bug tracker: [`bugs/bugs_workflow.md`](../bugs/bugs_workflow.md), [`bugs/quest2_baseline_log.md`](../bugs/quest2_baseline_log.md) (gitignored `bugs/`)
