# Third-party notices (Lexting Unity package)

Embedded snapshots ship **inside** this package. Integrators must **not** add duplicate copies from UPM or the Asset Store unless migrating off these pinned versions.

| Component | Version / pin | License | Location | Source |
|-----------|---------------|---------|----------|--------|
| **UniRx** | 6.2.2 | MIT | `Runtime/ThirdParty/UniRx/` | [neuecc/UniRx](https://github.com/neuecc/UniRx) |
| **SimpleJSON** | vendored | MIT | `Runtime/Scripts/ThirdParty/SimpleJSON/` | [Bunny83/SimpleJSON](https://github.com/Bunny83/SimpleJSON) |
| **Surge** (subset) | vendored | MIT | `Runtime/Scripts/ThirdParty/Pixelplacement/Surge/` | [Pixelplacement/Surge](https://github.com/Pixelplacement/Surge) |
| **Windows Input Simulator** | 0.2.x (Codeplex) | Permissive (see nuspec) | `Runtime/ThirdParty/WindowsInput/` | [InputSimulator / WindowsInput](http://inputsimulator.codeplex.com) |
| **XR Hands Hand Visualizer sample** | Unity sample 1.3.0 | Unity Companion License | `Runtime/ThirdParty/XRHandsHandVisualizer/` | `com.unity.xr.hands` sample (vendored for `XRRig.prefab`) |

**Not included in this package (integrator-provided):**

- **Wispy Skybox** — import from [Unity Asset Store My Assets](https://assetstore.unity.com/packages/2d/textures-materials/sky/wispy-skybox-21737) if reference scene skybox should match the demo APK.

Unity and Meta/OpenXR packages are declared in `package.json` `dependencies` only.
