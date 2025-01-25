# AloftModLoader
A mod loader for Aloft to facilitate content-only asset bundle mods for Aloft. AloftModLoader aims to erase the boiler plate of creating a content only and plugs content directly into Aloft dynamically at run time.

## Installation for playing

Install BepInEx-Unity.Mono. Tested and built against with [v6.0.0-pre.2](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.2)
Copy the contents of the `plugins` directory into `Aloft/BepInEx/plugins`.

## Installation for creating content from this framework

- Get Unity version 2021.3.12f1 from [unity's download archive](https://unity.com/releases/editor/archive)
- Install BepInEx Tested and built against with [v6.0.0-pre.2](https://github.com/BepInEx/BepInEx/releases/tag/v6.0.0-pre.2)
- Get [thunderkit](https://github.com/PassivePicasso/ThunderKit) set up (used release 9.1.0): https://github.com/PassivePicasso/ThunderKit

- Copy the `plugins/AloftModFramework.dll` file into the Thunderkit Unity project inside `Assets`. 

- Create your thunderkit project (pipeline and manifest). Pipeline output (asset bundle locations) must output to `Aloft\Aloft_Data\StreamingAssets\amf` to be loaded automatically.

- View [the frameworks github page](https://github.com/Sessional/AloftModLoader?tab=readme-ov-file#creating-your-first-recipe) for help creating your first recipe.
