# Monolith → `Lexting_Unity_Plugin` path map (M2)

| Monolith (`Lexting_Unity/Assets/…`) | Package (`Lexting_Unity_Plugin/…`) |
|-------------------------------------|-------------------------------------|
| `Lexting/Scripts/` | `Runtime/Scripts/` |
| `Lexting/Resources/` | `Runtime/Resources/` |
| `Lexting/Prefabs/` | `Runtime/Prefabs/` |
| `Lexting/_Scenes/main_01_twozone*` | `Scenes/main_01_twozone*` |
| `Lexting/Scripts/EnglishDictionaries/Resources/` | `Runtime/Scripts/EnglishDictionaries/Resources/` |
| `Lexting/Scripts/ThirdParty/SimpleJSON`, `…/Surge/` | Same under `Runtime/Scripts/ThirdParty/` |
| `Lexting/ThirdParty/WindowsInput/` | `Runtime/ThirdParty/WindowsInput/` |
| `Plugins/UniRx/` (no Examples) | `Runtime/ThirdParty/UniRx/` |
| `Samples/XR Hands/…/HandVisualizer/` | `Runtime/ThirdParty/XRHandsHandVisualizer/` |
| `Lexting/Scripts/**/Editor/` | `Editor/` (mirrored subpaths) |

**Not copied:** `Lexting/_Scenes/` (except main), `Streaming/MLAPI/`, `ThirdParty/EdgeDetect*`, `OutlineEffect`, `Wireframe`, `Assets/Oculus/`, `Assets/Wispy Sky/`.

**One-way rule:** do not add a `file:` Lexting package reference to **`Lexting_Unity/Packages/manifest.json`**.

**Greenfield host** (`Lexting_Unity_Sample/` next to the plugin): in `Packages/manifest.json` use  
`"com.lexting.spatialgesturetext": "file:../../Lexting_Unity_Plugin"`  
(paths are relative to the host **`Packages/`** folder, so two levels up to repo root).  
Windows absolute path also works: `"file:F:/Lexting/Lexting_Unity_Plugin"`.

For **`tools/build_quest.ps1`**, also add **`com.unity.pipeline`** (same version as monolith, e.g. `0.9.1-exp.1`). Pipeline is per open project; you do not need to close the monolith Editor—each Unity instance registers its own project path.
