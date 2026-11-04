# Known gaps (`0.3.0`)

Living list for post–Milestone 3 follow-up. Not duplicated in README per M3 scope.

| Area | Status | Notes |
|------|--------|--------|
| M1 / build **028** gesture parity | Open | Greenfield smoke on **`OPEN_XR`** + **`OpenXRHandInput`** is the M3 exit gate; full checklist is follow-up QA. |
| **`QUEST_*` `InputMode`** | Obsolete | Enum + legacy transform columns remain for serialized scenes; **M5** wizard drops picker entries and collapses enum. |
| OVR skeleton fallback | Deprecated | **`#if LEXTING_FOUND_OCULUS_SDK`** only; stripped on Android Quest via define postprocessor. |
| Leap adapter | Deprecated | **`#if LEXTING_FOUND_LEAP_SDK`**; stripped on Android Quest. |
| Setup wizard | M5 | Integrators hand-wire **`LextingInputModuleManager`** + **`OPEN_XR`** until **0.5.0**. |
| Monolith `Lexting_Unity/` | Local | Not updated in M3; greenfield sample + UPM package are the shipping path. |

See [`xr_integration_adr.md`](xr_integration_adr.md) for architecture and input-mode wiring.
