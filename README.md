# AloftModLoader
A mod loader for Aloft to facilitate BepInEx plugins and ThunderKit asset bundles

# Set up for development:

```
mkdir Libraries
```

Copy in these files from BepInEx:

- 0Harmony.dll
- BepInEx.Core.dll
- BepInEx.Unity.Common.dll
- BepInEx.Unity.Mono.dll

From Aloft copy these files in:
- Aloft.dll
- Assembly-CSharp.dll
- UnityEngine.dll
- UnityEngine.CoreModule.dll
- UnityEngineAssetBundleModule.dll

# Installation for use:

Set up for development, build and then copy the two .dll outputs into the `BepInEx/plugins` folder.
- AloftModLoader.dll
- AloftModFramework.dll

# For writing plugins:

Get Unity version 2021.3.12f1 from https://unity.com/releases/editor/archive

Install BepInEx (tested with https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.2)

Get thunderkit set up (used release 9.1.0): https://github.com/PassivePicasso/ThunderKit

Copy the `AloftModFramework.dll` file into the Thunderkit Unity project inside `Assets`. 

Asset bundles need to live inside `Aloft\Aloft_Data\StreamingAssets\amf` and must not end with `.manifest`.

Items created from `Create > AloftModFrameWork > Item` that have a `Post Loaded Item Id` set will automatically work for `give <post-loaded-item-id> <quantity>` commands inside Aloft.
