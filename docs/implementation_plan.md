# Lexting implementation plan (0.0.1 → 1.0.0)

This document is the **living roadmap** for reviving the Unity prototype, stabilizing XR integration, and shipping a **Meta Start** competition entry. It intentionally stays **out of** [`README.md`](../README.md); update this file as milestones firm up.

**Primary QA device:** Meta **Quest 2** (same hardware used for the original prototype).  
**Reference scene (today):** [`main_01_twozone.unity`](../Lexting_Unity/Assets/Lexting/_Scenes/main_01_twozone.unity) — dual-volume typing widget, dictionary, hotkeys, tutorial hooks. After **Milestone 2**, the same scene ships **inside** the single UPM package (reference scenes/prefabs folder — not a separate download or optional Samples import).

**Sprint window:** **2026-11-01** → **2026-11-19** (Meta Start deadline).  
**Polish cutoff:** **2026-11-15** — from this date forward, scope is demo/tutorial/presentation only unless a **P0** Quest blocker appears.

---

## Version map (intent, not release tags yet)

| Version | Meaning |
|--------|---------|
| **0.0.1** | Repository snapshot *as archived* (~2021 prototype + monolithic `Lexting_Unity/` tree). May not install or run on a Quest 2 without repair work. |
| **0.1.0** | **Milestone 1** — **Unity 6** upgrade on the legacy `Lexting_Unity/` tree + Quest 2 build; typing behavior matches the last known-good prototype (“as I left it”). Enables **`com.unity.pipeline`** (Cursor Unity MCP) on the monolith. |
| **0.2.0** | **Milestone 2** — Lexting ships as **one Unity package** (runtime, default dictionary, reference scenes, prefabs, tests). Monolithic `Lexting_Unity/` is deprecated or removed once parity is proven. Quest QA uses a **private local host project** (not in the open repo). |
| **0.5.0** | **Milestone 3** — Refreshed XR UPM stack; **OpenXR-first** Quest path documented and implemented; legacy Oculus Integration retired or quarantined (engine already on **Unity 6** from M1). |
| **1.0.0** | **Milestone 4** — Meta Start submission quality: stable demo, improved tutorial, 3-minute video ready, known limitations documented. |
| **1.1.0** | **Milestone 5 — Lexting Setup Wizard** — Editor **Lexting** menu command that configures the active scene for third-party integrators (auto rig bind + Lexting bootstrap where possible; validation errors/warnings when not). **Final milestone** in this roadmap. |

Patch releases (`0.1.x`, `0.2.x`, `0.5.x`, `1.0.x`) are expected during each milestone for Quest-side iteration.

---

## Target repository layout (after Milestone 2)

| Path | Role | In open repo |
|------|------|----------------|
| **`Lexting_Unity_Plugin/`** | Single UPM package: runtime + editor assemblies, `SpatialGestureText`, default dictionary Resources, vendored deps (SimpleJSON, Surge subset, etc.), reference **Scenes/** and **Prefabs/** (`main_01_twozone`, QWERTY sim, hand/XR tests). One install via Package Manager. | **Yes** |
| **Local Quest host** *(e.g. private `Lexting_Unity_Sample/` on disk)* | Maintainer-only Unity app: **`ProjectSettings`**, Android keystore, OpenXR/Oculus loader pins, build scripts, Meta Start branding. References the package via `file:` or Git URL. **Never published** in the open repo. | **No** |
| **`Lexting_Unity/`** | Monolithic prototype during Milestone 1. **Retire** after M2 exit criteria (tag/branch snapshot optional). | Remove from default branch when cut over |

**Package consumption:** add once in `Packages/manifest.json` or Package Manager UI, e.g. `"com.lexting.spatialgesturetext": "file:../Lexting_Unity_Plugin"` (final package id TBD). Reference scenes open from **Packages/** in the Project window.

---

## Cross-cutting assumptions

- **Keep:** [SimpleJSON](https://github.com/Bunny83/SimpleJSON) (vendored in plugin) — dictionary/metadata; low maintenance surface.
- **Keep (for now):** [Pixelplacement Surge](https://github.com/Pixelplacement/Surge) — tweens via `TweenUtils` and direct `Tween.*` calls; validate on **Unity 6** during **Milestone 1** reopen; replace if broken.
- **Defer / remove:** Leap Motion (`Assets/Plugins/LeapMotion/`, `LEXTING_FOUND_LEAP_SDK`) — remove during Milestone 3 SDK cleanup.
- **Engine baseline:** **Milestone 1** upgrades the monolith from archived **Unity 2022.3.19f1** to **Unity 6** LTS (exact 6.x patch recorded in [`ProjectVersion.txt`](../Lexting_Unity/ProjectSettings/ProjectVersion.txt) after upgrade). **Milestone 2** locks the same minimum Unity version in plugin `package.json`; device builds run from a **local host** project.
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

**M1 exit met** (Quest 2 smoke checklist passed, build **028** era). **Active milestone: M2 (`0.2.0`)** — plugin packaging. Optional M1 gap: **workstream 5** screen recording.

### Skeletal workstreams

1. **Unity 6 upgrade** — [x] Hub upgrade, package resolve, Android IL2CPP build green; Pipeline installed; Editor/`main_01_twozone` usable.
2. **Toolchain** — [~] Android/Quest deploy path works (Unity-bundled `adb`, sideload); package id still legacy `com.octest.qwerty_sim_16`; Android min SDK **24+** on OpenXR path (see baseline log).
3. **Build & deploy** — [x] Quest APK from Unity 6 (`build_quest.ps1` / `install_quest.ps1`); post-install QA capture (`bugs/tools/quest_install_qa.ps1`, `qa_close_session.ps1`); **`main_01_twozone`** enabled, **`QWERTY_Sim`** disabled in Build Settings.
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
**Goal:** Reformat the codebase so **Lexting is one installable Unity package**, not an app buried inside a mega-project. Typing UI, default dictionary, input adapters, utilities, **scenes, and prefabs** all live under **`Lexting_Unity_Plugin/`**. Day-to-day Quest APK builds use a **private local host** project (not in the open repo) that references the package.

**Suggested window:** **2026-11-08 → 2026-11-12** (start only after Milestone 1 exit criterion is met; may slip Meta Start calendar if split takes longer—prefer a **minimal** plugin cut that preserves behavior over a perfect asmdef graph).

**Prerequisite:** Milestone 1 Quest checklist passes on the **legacy** tree (so regressions from the move are attributable to packaging, not unknown drift).

### What moves into the plugin vs the private local host

| **Plugin (`Lexting_Unity_Plugin/`, in git)** | **Private local host (not in open repo)** |
|---------------------------------------------|-------------------------------------------|
| `SpatialGestureText` runtime + editor scripts | **`ProjectSettings`**, Android keystore, company/product IDs |
| English dictionary Resources, conjugation data | XR Management / OpenXR loader **project** configuration |
| Vendored compile-time deps (SimpleJSON, Surge, UniRx if retained) | Optional maintainer scripts (`build_quest`, `install_quest`) |
| **Reference content:** `main_01_twozone`, QWERTY_Sim, hand/XR test scenes, prefabs (XRRig reference, UI prefabs) | Meta Start one-off branding / capture settings |
| `package.json`, asmdefs, integrator README | `manifest.json` `file:` → plugin |

**Explicitly not in the plugin:** bulk **`Assets/Oculus/`** Integration (host pulls Meta/XR via UPM); MECH and unrelated monolith noise. Host project declares platform packages; plugin declares **API-level** XR/Input dependencies integrators need.

### Definition of done

- [ ] **`Lexting_Unity_Plugin/package.json`** exists with valid name, version `0.2.0`, Unity minimum, dependencies declared (Input System, XR Management, OpenXR, etc.—exact list documented in plugin README).
- [ ] Plugin compiles in isolation (no missing scripts under `Assets/Lexting/` paths in the monolith).
- [ ] Reference **scenes and prefabs** ship inside the package (visible under **Packages/** after install); includes **`main_01_twozone`** and other test scenes.
- [ ] Maintainer **local host** (gitignored path, e.g. `Lexting_Unity_Sample/`) documented privately or in `docs/local_quest_host.md` — **not** shipped in the open repo; host + imported samples pass the **Milestone 1 device checklist** on Quest 2.
- [ ] Documented **migration note**: which monolith paths map to which package folders; decision on whether **`Lexting_Unity/`** is deleted on main or kept read-only until `0.5.0`.
- [ ] Root [`README.md`](../README.md) reflects single-package install (done in spirit; refresh at cutover).

### Skeletal workstreams

1. **Inventory** — list every assembly under old `Assets/Lexting/`; mark plugin vs reference scenes/prefabs vs delete (deprecated letter cube, streaming, etc.).
2. **Package skeleton** — `package.json`, asmdef boundaries (Runtime / Editor), folder layout for Scenes/Prefabs/Resources.
3. **Move & fix references** — update namespaces/paths, Resources load paths; scene/prefab GUIDs live in the plugin tree.
4. **Local host bootstrap** — maintainer-only project with ProjectSettings/XR/keystore; import package samples for Quest builds.
5. **Cutover** — workflow = edit plugin in repo, test via imported samples in local host; stop editing monolith `Lexting_Unity/` once parity green.
6. **Retire monolith** — remove or archive `Lexting_Unity/` on default branch after sign-off (optional: git tag `pre-plugin-monolith`).

**Exit criterion:** Open repo contains **only** the one package; a maintainer local host references it and produces a Quest APK; **zero** typing logic remains only in the old monolith tree.

---

## Milestone 3 — XR refresh & OpenXR conformity

**Target version:** `0.5.0`  
**Goal:** Modernize **XR SDKs** on the **Unity 6** baseline from Milestone 1, then converge on a **single OpenXR-first path** on Quest 2—without changing Lexting typing semantics. Work happens in **`Lexting_Unity_Plugin`** and the **private local host** after Milestone 2; the old monolith is not the primary target.

**Suggested window:** **2026-11-10 → 2026-11-14** (overlap with late Milestone 2 is OK for *research*; land OpenXR cutover only after the plugin host compiles and Milestone 1 parity is green on the sample).

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
- **Leap** — delete plugin folder and `LEXTING_FOUND_LEAP_SDK` paths.
- **Third-party in plugin** — UniRx, Surge: re-verify on Unity 6 if M1 left gaps; log blockers in ADR if they force a deferral.

### OpenXR conformity (product + code)

- **`InputMode.OPEN_XR`** (or successor enum) is the **blessed** path for Milestone 4 builds and Meta Start demo; legacy `QUEST_*` modes removed or `#if`’d off for release configuration.
- **`SpatialGestureInput`** remains the **only** front door for typing logic; OpenXR/session/hand updates flow through **one adapter** (new or refactored class—name captured in ADR).
- **Gesture parity checklist** on Quest 2: pinch/grab, dual-hand word mode, wrist conjugation poses, hand cursor stability—same as Milestone 1, new stack underneath.
- **Honest README delta:** what is truly “OpenXR-first SDK” vs “OpenXR loader + legacy C# still migrating” at `0.5.0`.

### Questions this milestone must answer

- What actually drove **`SpatialGestureInput`** on the **2022/OVR** baseline, and what is the **minimal OpenXR data** to reproduce it?
- Which **`Assets/Oculus/`** assets are still required after UPM OpenXR + Meta runtime are refreshed?
- Which XR packages live in **plugin `package.json`** (API surface) vs **sample manifest only** (loaders, platform settings)?
- Does **Unity 6** require rendering/UI pipeline changes (e.g. URP settings, uGUI) in the sample scene for Quest?
- Where should **`LextingDefineSymbolsPostprocessor`** go—plugin editor, sample only, or deleted in favor of explicit asmdef/package dependencies?

### Definition of done

- [ ] **Sample + plugin compile on Unity 6** (inherited from M1/M2); XR-related package pins documented in plugin `package.json` + sample manifest.
- [ ] **UPM refresh complete** — manifest(s) updated; no stale 2022-only XR package pins unless documented with reason.
- [ ] **Quest 2 Android build** from **local host** (imported package samples) using **OpenXR** loader; hand tracking enabled; app reaches `main_01_twozone`.
- [ ] **Architecture decision record** ([`docs/xr_integration_adr.md`](xr_integration_adr.md) or appendix here): primary Quest path, packages + versions, adapter class diagram, fallback/disabled legacy modes.
- [ ] Input module enum + scene wiring **documented** (table: mode → classes → packages → defines).
- [ ] **Leap** and unused **Oculus Integration** cruft removed or isolated so they are not required for the OpenXR Quest build.
- [ ] Quest 2 **re-test** of full Milestone 1 checklist on **OpenXR** configuration; unintentional behavior changes filed in local [`bugs/`](../bugs/) (gitignored; see [`bugs/bugs_workflow.md`](../bugs/bugs_workflow.md)).
- [ ] **Known gaps** vs long-term **Lexting OpenXR SDK** listed for judges/integrators.

### Skeletal workstreams

1. **UPM / SDK inventory** — diff M1 manifest vs OpenXR-first target; upgrade in controlled commits; note Meta Quest setup doc links.
2. **OpenXR spike** — Quest 2 device session: loader, hand tracking, compare joint/wrist data to archived OVR baseline recordings from Milestone 1.
3. **Adapter & input manager** — implement or finish OpenXR path into **`SpatialGestureInput`**; simplify **`LextingInputModuleManager`**; default sample scene to OpenXR.
4. **Legacy retirement** — quarantine embedded **`Assets/Oculus/`** and remove **`LEXTING_FOUND_OCULUS_SDK`** auto-define for shipping Quest builds; **retain** **`OculusHandInput`** (rename optional) as the shared **XR Hands** gesture adapter — drop the **OVR skeleton** branch once no reference scene uses OVRCameraRig.
5. **Validation & docs** — Milestone 1 checklist on device; ADR + optional README platform section update.

**Exit criterion:** Quest 2 demo runs on **OpenXR**, refreshed UPM XR packages, single **package** in git, with a written map from runtime → adapter → **`SpatialGestureInput`** → typing UI.

**Schedule note:** If full OpenXR cutover cannot finish before **11/15**, Milestone 4 may ship on a **documented interim** stack (e.g. Unity 6 + temporary OVR adapter)—only with explicit ADR approval; default goal is OpenXR-primary for `0.5.0`.

---

## Milestone 4 — Meta Start delivery (polish mode)

**Target version:** `1.0.0`  
**Goal:** Competition-ready experience: **stable demo**, **clear onboarding**, **3-minute video**, optional **training/gamification** if schedule allows.

**Polish mode starts:** **2026-11-15**  
**Submission deadline:** **2026-11-19**

**Suggested window:** **2026-11-12 → 2026-11-19** (content prep can begin earlier; **feature freeze** for polish on 11/15).

### Definition of done

- [ ] **Demo script** — timed flow (~3 min) covering: problem → dual-volume typing → word finalize/conjugate → one hotkey → output destination; rehearsed on Quest 2.
- [ ] **Capture** — 3-minute video recorded (resolution/aspect per Meta Start rules); B-roll and fallback clips if live hand tracking fails.
- [ ] **Tutorial** — `TutorialManager` flow improved for first-time judges/users (copy, pacing, skip/restart, failure recovery); aligned with demo script beats.
- [ ] **Build label** — `1.0.0` Quest APK (local host + imported samples) matches video; changelog blurb for submission.
- [ ] **Known issues** — one-page list (environment lighting, dictionary size, PC keyboard mode out of scope, etc.).

### Skeletal workstreams (priority order)

1. **Demo & video** (P0) — script, scene state, lighting, UI legibility on capture.
2. **Tutorial pass** (P0) — fewer dead-ends; pop-gun / gesture hints match current input mode from Milestone 3.
3. **Submission packaging** (P0) — store/metadata assets, attribution (SimpleJSON, Surge, Unity, Meta), license pointers.
4. **Gamification / training** (P1, time-boxed) — e.g. speed-test hooks, streaks, or bullseye-style drills via existing `TutorialManager` / `SpeedTestManager`; **drop if not fun by 11/17**.
5. **Nice-to-have** (P2) — short “how to build Quest APK from sample” in `docs/`.

**Exit criterion:** You can submit on **11/19** without touching core typing logic unless a P0 crash appears.

---

## Milestone 5 — Lexting Setup Wizard (integrator DX)

**Target version:** `1.1.0`  
**Goal:** Third-party devs open **their** scene, run one **Lexting** menu command, and get a working Lexting stack without hand-wiring XR anchors or hunting prefabs. When automation cannot succeed, the tool reports **actionable validation errors and warnings** (Console + optional dialog / report window)—not silent failure.

**Suggested window:** **After Milestone 4** (post–Meta Start); not on the **11/19** critical path unless schedule allows a thin MVP.

**Prerequisite:** Milestone 2 plugin layout (Runtime + **Editor** asmdef), Milestone 3 OpenXR path preferred for default setup logic (legacy `QUEST_*` may remain as detected/fallback with warnings).

### Product shape

| Surface | Behavior |
|---------|----------|
| **Menu: Lexting → Setup Scene…** | Operates on the **active scene** (saved or dirty). Idempotent where possible: re-run updates wiring instead of duplicating `[INPUTS]` / duplicate rigs. |
| **Menu: Lexting → Validate Scene** *(optional split)* | Read-only pass; same rules as setup validation without mutating hierarchy. |
| **Auto setup (best effort)** | **Prerequisite:** integrator scene already has an **XR player rig** (documented in README). Discover or create: **`[INPUTS]`** + **`LextingInputModuleManager`** (blessed **`InputMode.OPEN_XR`** when XR stack supports it); bind **head / left hand / right hand** **Transform** references from **`XROrigin`** / XRI rig (**error** if no rig). **Do not** parent the dual-volume widget into the saved scene—widget comes from package prefabs at **runtime** (bootstrap wired by wizard via serialized config on **`[INPUTS]`** / manager; details in **`setup_wizard.md`**, not README Getting Started). Integrators never author the widget in **`Assets/`**. |
| **Manual integrator hooks (unchanged)** | **`ZoneFieldOutputReceiver`** on each **`InputField`** (or future attribute-driven auto-add with **opt-in** warning)—document that text output still requires explicit subscription unless wizard adds receivers user confirms. |

### Validation (errors vs warnings)

**Errors (setup aborts or incomplete — must fix):**

- No XR Management / OpenXR (or documented legacy path) configured for the active build target.
- No **`XROrigin`** / XR player rig / head transform in the scene (integrator must add their rig **before** running the wizard).
- Hand tracking required for chosen mode but subsystem / anchors missing after auto setup.
- Duplicate conflicting **`LextingInputModuleManager`** or broken references after partial run.
- Plugin assemblies or Resources prefabs missing (package not installed correctly).

**Warnings (scene may run; fix recommended):**

- Legacy **`QUEST_*`** mode or embedded Oculus Integration detected while OpenXR-first is recommended.
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
- [ ] **[`README.md`](../README.md) Getting Started** updated: primary path = install package → open your scene → **Lexting → Setup Scene** → add output receivers → build.
- [ ] **Decision log** entry + short **`docs/setup_wizard.md`** (integrator-facing): what the wizard does, what it cannot do, troubleshooting table.

### Skeletal workstreams

1. **Scene inventory & rules engine** — classify objects (XR rig, existing Lexting roots, UI fields); ordered mutation plan.
2. **Rig discovery & binding** — XRI / XROrigin heuristics on **integrator-owned** rig; populate **`LextingInputModuleManager`** slots for active mode; fail validation if ambiguous or missing.
3. **Runtime widget bootstrap** — **`Instantiate`** package prefab(s) (`TwoZoneCube`, output manager, hotkeys, etc.) when the app runs; head-relative spawn from existing controller offsets; wizard only ensures the serialized hook on **`[INPUTS]`** is present and valid.
4. **Validation UI** — Console + modal or **`EditorWindow`** report; copy-to-clipboard for support.
5. **Docs & README** — replace “wire your XR rig” maintainer flow with wizard-first narrative.

### Stretch goal (same milestone, not required for `1.1.0` exit)

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

**Exit criterion:** A developer with a stock **Unity 6 + OpenXR + XRI** template project, package installed, runs **Lexting → Setup Scene** once, adds **`ZoneFieldOutputReceiver`** to their **`InputField`**, and reaches typable volumes in Play Mode on device or in editor play (per supported mode)—without assigning nine transform fields by hand.

---

## Calendar (at a glance)

| Dates | Focus |
|--------|--------|
| **11/01 – 11/08** | Milestone 1 — **Unity 6** upgrade + Quest 2 baseline parity (`Lexting_Unity/`) |
| **11/08 – 11/12** | Milestone 2 — single Unity package; retire monolith |
| **11/10 – 11/14** | Milestone 3 — UPM XR refresh, OpenXR Quest path (plugin + local host) |
| **11/12 – 11/14** | Milestone 4 prep — demo outline, tutorial notes, capture dry runs |
| **11/15 – 11/19** | **Polish mode** — video, tutorial lock, submission build `1.0.0` |
| **Post–11/19** | **Milestone 5** — Lexting Setup Wizard (`1.1.0`); integrator menu + validation |

*Calendar is aggressive:* if Milestone 2 slips, compress XR work (Milestone 3) to what is required for the demo path only, or ship Meta Start from a **thin** sample while finishing plugin extraction on a branch. **Milestone 5 is explicitly last** and does not block Meta Start submission.

---

## Post–1.0.0 (after Milestone 5)

- Publish plugin to a registry (OpenUPM / Git URL) instead of `file:` only.
- Harden **Lexting OpenXR SDK** boundary inside the plugin (input adapter surface).
- Replace **UniRx** (`EventUtils` / `AsyncUtils`) with modern Unity patterns.
- Evaluate **Surge** replacement (DOTween, Unity animations, or minimal custom tweens).
- Unreal reference host, additional OEM runtimes from [README platform table](../README.md#platform-strategy).

*(Integrator scene setup is **in scope** as **Milestone 5**, not an open-ended post-1.0 item.)*

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
| | | |

---

## Related docs

- [`README.md`](../README.md) — product overview, architecture, OpenXR-first strategy  
- [`conjugation_algorithm.md`](conjugation_algorithm.md) — word mode and finalization (demo/tutorial reference)  
- [`setup_wizard.md`](setup_wizard.md) — *(Milestone 5)* integrator menu command, validation rules, troubleshooting *(create with M5)*  
- Local Quest QA + bug tracker: [`bugs/bugs_workflow.md`](../bugs/bugs_workflow.md), [`bugs/quest2_baseline_log.md`](../bugs/quest2_baseline_log.md) (gitignored `bugs/`)
