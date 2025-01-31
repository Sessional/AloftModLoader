using AloftModFramework.Items;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terrain.Platforms.Population;
using Terrain.Platforms.Types;
using UI;
using UI.Building;
using UnityEngine;
using Utilities;
using static Scriptable_Objects.SRecipeManager;

namespace AloftModLoader
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            //source.ThrowIfNull("source");
            //action.ThrowIfNull("action");
            foreach (T element in source)
            {
                action(element);
            }
            return source;
        }
    }

    static class ItemPatches
    {
        public static ScriptableInventoryItem RewriteItemIdResult(ScriptableInventoryItem __result, ItemID.ID id)
        {
            if (__result == null)
            {
                return AloftModLoader.Items.FirstOrDefault(x => x.ID == id);
            }

            return __result;
        }
    }

    static class RecipePatches
    {
        private static IList PatchedGroups = new List<CraftingStation>();
        public static ScriptableCraftRecipeGroup RewriteRecipeResult(ScriptableCraftRecipeGroup __result, CraftingStation stationType)
        {
            if (stationType == CraftingStation.All)
            {
                // this flow looks very similar to the station type filter below... But... it's `ALL` so it doesn't do a filter
                // it really just adds everything.
                PatchedGroups.Add(stationType);
                __result.Recipes = __result.Recipes.AddRangeToArray(AloftModLoader.Recipes.ToArray());
            }

            // TODO: inappropriate to unlock recipes here, but until a new way is plugged in/discovered/wired up, this will do.
            Level.CraftingManager.UnlockRecipe.UnlockRecipes(AloftModLoader.Recipes.Select(recipe => recipe.Output.ItemID).ToArray());

            if (__result == null)
            {
                // This assumes that any new stations have a pre-built recipe group that is wired up for us when constructing recipes
                // this may or may not preclude people from adding new recipes into another mods workbench...
                __result = AloftModLoader.RecipeGroups.Where(x => x.StationType == stationType).FirstOrDefault();
            }
            else if (__result != null && !PatchedGroups.Contains(stationType))
            {
                PatchedGroups.Add(stationType);
                __result.Recipes = __result.Recipes.AddRangeToArray(AloftModLoader.Recipes.Where(x => x.CraftingStation == stationType).ToArray());
            }

            return __result;
        }
    }

    static class LocalizationPatches
    {
        public static Dictionary<string, string> LocalizationValues = new Dictionary<string, string>();
        public static void SetLanguage(int index)
        {
            LocalizationValues.Clear();
            var localizationName = Localization.GetLanguageName(index);
            var selectedLocalization = AloftModLoader.Localizations.FirstOrDefault(x => x.Language.Equals(localizationName));
            // try to match the language picked
            // when none is matched, try to grab whatever the first one loaded
            // if that doesn't work then our best effort attempt is over
            var localizationResource = selectedLocalization == null ? AloftModLoader.Localizations.FirstOrDefault() : null;

            if (localizationResource != null)
            {
                foreach (var entry in localizationResource.LocalizationFile.text.Split('\n'))
                {
                    if (string.IsNullOrEmpty(entry)) continue;

                    var splitEntry = entry.Split('\t');
                    if (splitEntry.Length == 2)
                    {
                        LocalizationValues.Add(splitEntry[0], splitEntry[1]);
                    }
                    else
                    {
                        Console.WriteLine("Error with entry: " + entry);
                    }
                }
            }
        }

        public static string GetLocalizedValue(string __result, string key)
        {
            if (string.IsNullOrEmpty(__result))
            {
                if (LocalizationValues.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            return __result;
        }
    }

    static class BuildingHooks
    {

        public static bool PopulationDataAdded = false;
        public static void InitListExtension()
        {
            if (!PopulationDataAdded)
            {
                PopulationDataAdded = true;
                Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData = Level.TerrainManager.PopulationManager.PopulationDataManager.AllPopulationData.AddRangeToArray(AloftModLoader.Buildings.ToArray());
            }
        }

        private static bool Learned = false;
        public static ScriptableCrafting GetCrafting(ScriptableCrafting __result, PopulationID.ID id)
        {

            if (!Learned)
            {
                Learned = true;
                // TOOD: should have a different way of setting up learning blueprints for building?
                Console.WriteLine("Learning blueprints!");
                AloftModLoader.BuildingBlueprints.ForEach(blueprint => {
                    Console.WriteLine("Learning blueprint " + blueprint.name);
                    Level.CraftingManager.UnlockRecipe.UnlockNewRecipeBuilding(blueprint.PopData.PopulationID);
                });
            }

            InitListExtension(); // TOOD: why is this not getting called somewhere else..?

            var loadedBuildng = AloftModLoader.BuildingBlueprints.FirstOrDefault(x => x.ID == id);
            if (loadedBuildng != null)
            {
                return loadedBuildng;
            }
            return __result;
        }

        public static ScriptablePopulationData GetPopulationData(ScriptablePopulationData __result, PopulationID.ID populationID)
        {
            // stomp, looks like the other one alwasy returns something stupid? WaterL
            if (AloftModLoader.Buildings != null && AloftModLoader.Buildings.Count > 0)
            {
                var loadedPop = AloftModLoader.Buildings.FirstOrDefault(x => x.PopulationID == populationID);
                if (loadedPop != null)
                {
                    return loadedPop;
                }
            }

            return __result;
        }

        public static ScriptablePopulationData GetPopulationData2(ScriptablePopulationData __result, PopulationID.ID popID)
        {
            return GetPopulationData(__result, popID);
        }

        public static GameObject GetPrefabGameObject(GameObject __result, ScriptablePopulationData __instance)
        {
            if (__result == null)
            {
                if (__instance is AloftModFrameworkPopulationData)
                {
                    return ((AloftModFrameworkPopulationData)__instance).Prefab;
                }
            }
            return __result;
        }

        public static bool CustomTabsAdded = false;
        public static void AddMoreTabs(UI_BuildingMenu __instance)
        {
            if (!CustomTabsAdded)
            {
                CustomTabsAdded = true;

                for (int i = 0; i < AloftModLoader.BuildingTabs.Count; i++)
                {
                    var tab = AloftModLoader.BuildingTabs[i];

                    if (tab.HasParentCategory)
                    {
                        var parentCategory = __instance.ScriptableTabs.FirstOrDefault(x => x.Category == tab.ParentCategory);
                        if (parentCategory != null)
                        {
                            parentCategory.SubTabs = parentCategory.SubTabs.AddToArray(tab);
                        }
                    } else
                    {
                        __instance.ScriptableTabs = __instance.ScriptableTabs.AddToArray(tab);
                    }
                }
            }
        }
    }

    public static class Cheats {
        public static bool ProcessCommand(ref bool __result, string input)
        {
            var parts = input.Split(' ');

            if (parts.Length > 0)
            {
                Console.WriteLine(input);
                switch (parts[0])
                {
                    case "showpop":
                        var island = Level.TerrainManager.PlatformManager.HomeIsland;
                        Console.WriteLine("Is island null? " + (island == null).ToString());
                        var pop = island.PopulationSouls.Values.FirstOrDefault(x => x.PopulationID == (PopulationID.ID) 400003);
                        Console.WriteLine("Is bee pop null? " + (pop == null).ToString());
                        Console.WriteLine("Pop location is " + pop.LocalPosition);
                        __result = true;
                        return false;
                        break;
                    case "learn":
                        switch (parts[1])
                        {
                            case "building":
                                Console.WriteLine("Learning building!");
                                if (int.TryParse(parts[2], out var buildingId))
                                {
                                    Console.WriteLine("Learned building!");
                                    Level.CraftingManager.UnlockRecipe.UnlockNewRecipeBuilding((PopulationID.ID)buildingId);
                                    __result = true;
                                    return false;
                                }
                                break;
                            case "recipe":
                                if (int.TryParse(parts[2], out var recipeId))
                                {
                                    Level.CraftingManager.UnlockRecipe.LearnItemRecipe((ItemID.ID)recipeId);
                                    __result = true;
                                    return false;
                                }
                                break;
                        }
                        break;
                    case "spawn":
                        switch (parts[1])
                        {
                            case "pop":
                                if (int.TryParse(parts[2], out var popId))
                                {
                                    var population = AloftModLoader.Buildings.FirstOrDefault(x => x.PopulationID == (PopulationID.ID)popId);
                                    if (population != null)
                                    {
                                        PlatformAbstract cachedPlatform = Level.PlayerManager.AffectedByPlatform.CachedPlatform;
                                        var location = cachedPlatform.AffectedObjectParent.InverseTransformPoint(Level.PlayerManager.Anatomy.Pivot.position);
                                        var transform = Level.PlayerManager.Anatomy.Pivot;
                                        Console.WriteLine("Location is " + location.x + "," + location.y +"," + location.z);
                                        Console.WriteLine("Ghost prefab is" + population.ScriptableCrafting.CustomGhostPrefab.name);
                                        var spot = new Vector3(6010, 724, 26658);
                                        var spawnedObj = GameObject.Instantiate(population.ScriptableCrafting.CustomGhostPrefab, spot, Quaternion.identity);
                                        __result = true;
                                        return false;
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
            return true;
        }

        public static void TweakCommandsOnStart(CanvasConsole __instance)
        {

        }
    }

    public class AloftModFrameworkPopulationData : ScriptablePopulationData
    {
        public GameObject Prefab;
    }

    public class AloftModFrameworkInternalCraftingRecipe : ScriptableCraftRecipe
    {
        public SRecipeManager.CraftingStation CraftingStation;
    }

    public class AloftModFrameworkInternalBuildingTab : ScriptableBuildingTab
    {
        public bool HasParentCategory = false;
        public BuildingCategory ParentCategory;
    }

    [BepInPlugin(GUID, NAME, VERSION)]
    public class AloftModLoader : BaseUnityPlugin
    {
        const string GUID = "aloftmodloader.sessional.dev";
        const string NAME = "Aloft Mod Loader";
        const string VERSION = "0.1.1";

        public static Shader shaderEveryoneNeeds;

        static List<AssetBundle> bundles;
        static List<UnityEngine.Object> allAssets;
        public static List<ScriptableInventoryItem> Items;
        public static List<AloftModFrameworkInternalCraftingRecipe> Recipes;
        public static List<AloftModFrameworkInternalBuildingTab> BuildingTabs;
        public static List<ScriptableCraftRecipeGroup> RecipeGroups;
        public static List<ScriptableCrafting> BuildingBlueprints;
        public static List<AloftModFrameworkPopulationData> Buildings;
        public static List<AloftModFrameworkLocalization> Localizations;
        public static List<GameObject> GameObjects;

        public static Material hackyShader;

        static FieldInfo LocalizationField;

        public static BepInEx.Logging.ManualLogSource LoggerRef;
        public void Start()
        {
            Logger.LogInfo("Running!");
            string bundleDirectory = Path.Combine(Application.streamingAssetsPath, "amf");
            Logger.LogInfo("Bundle directory: " + bundleDirectory);
            var bundleNames = Directory.GetFiles(bundleDirectory);
            Logger.LogInfo("Bundle names: " + string.Join(",", bundleNames));

            shaderEveryoneNeeds = Shader.Find("Amplify/V2/DefaultPBR_Interactive");
            var whiteFallback = Resources.Load<Texture2D>("white");

            bundles = bundleNames
                .Where(x => !x.EndsWith(".manifest")) // TODO: order imports by this with dependencies rather than lucking out...
                .Distinct()
                .Select(x => {
                    Logger.LogInfo(string.Format("Loading asset bundle {0}", x));
                    return AssetBundle.LoadFromFile(x);
                }).ToList();
            allAssets = bundles.SelectMany(x => x.LoadAllAssets()).ForEach(x =>
            {
                DontDestroyOnLoad(x);
                x.hideFlags = HideFlags.HideAndDontSave;
            }).ToList();

            Items = allAssets
                .Where(x => x is AloftModFrameworkItem)
                .Cast<AloftModFrameworkItem>()
                .Select(x => {
                    var item = ScriptableObject.CreateInstance(typeof(ScriptableInventoryItem)) as ScriptableInventoryItem;
                    item.DisplayName = x.Name;
                    item.DisplayDescription = x.Description;
                    item.Category = x.Category;
                    item.DisplaySprite = x.DisplaySprite;
                    item.ID = (ItemID.ID)x.ItemId;
                    item.Type = x.Type;
                    item.Weight = x.Weight;
                    item.EquipType = x.EquipType;
                    item.AudioPickupID = x.AudioPickupID;
                    item.ItemTags = x.ItemTags;
                    item.hideFlags = HideFlags.HideAndDontSave;
                    return item;

                })
                .ToList();


            var aloftFrameworkRecipeGroups = allAssets
                .Where(x => x is AloftModFrameworkCraftingRecipeGroup)
                .Cast<AloftModFrameworkCraftingRecipeGroup>();

            RecipeGroups = aloftFrameworkRecipeGroups
                .Select(x =>
                {
                    var group = ScriptableObject.CreateInstance(typeof(ScriptableCraftRecipeGroup)) as ScriptableCraftRecipeGroup;

                    group.StationType = (CraftingStation) x.StationId;

                    return group;
                })
                .ToList();

            Recipes = allAssets
                .Where(x => x is AloftModFrameworkCraftingRecipe)
                .Cast<AloftModFrameworkCraftingRecipe>()
                .Select(x =>
                {
                    var recipe = ScriptableObject.CreateInstance(typeof(AloftModFrameworkInternalCraftingRecipe)) as AloftModFrameworkInternalCraftingRecipe;
                    Logger.LogInfo("Generating recipe: " + x.name);
                    recipe.Input = x.InputItems.Select(input => (ItemID.ID)input).ToArray();
                    recipe.hideFlags = HideFlags.HideAndDontSave;

                    ItemID.ID outputItemId;
                    if (x.OutputVanillaItem != ItemID.ID.Empty)
                    {
                        outputItemId = x.OutputVanillaItem;
                    }
                    else if (x.OutputModItem != null)
                    {
                        outputItemId = (ItemID.ID)x.OutputModItem.ItemId;
                    }
                    else
                    {
                        outputItemId = (ItemID.ID)x.OutputItemId;
                    }
                    recipe.Output = new ScriptableCrafting.CraftingCostClass(outputItemId, x.Quantity);
                    
                    if (x.AttachToExistingStation)
                    {
                        recipe.CraftingStation = x.Station;
                    }

                    var correspondingGroup = aloftFrameworkRecipeGroups.Where(group => group.Recipes.Contains(x)).FirstOrDefault();
                    if (correspondingGroup != null)
                    {
                        var recipeGroup = RecipeGroups.Where(group => group.StationType == (CraftingStation)correspondingGroup.StationId).FirstOrDefault();
                        recipeGroup.Recipes = recipeGroup.Recipes.AddItem(recipe).ToArray();
                        recipe.CraftingStation = (CraftingStation)correspondingGroup.StationId;
                    }

                    return recipe;
                })
                .ToList();


            BuildingTabs = allAssets
                .Where(x => x is AloftModFrameworkBuildingCategory)
                .Cast<AloftModFrameworkBuildingCategory>()
                .Select(x =>
                {
                    var buildingTab = ScriptableObject.CreateInstance<AloftModFrameworkInternalBuildingTab>();
                    buildingTab.Category = (ScriptableBuildingTab.BuildingCategory) x.BuildingCategoryId;
                    buildingTab.DisplayName = x.Name;
                    buildingTab.DisplayIcon = x.DisplayIcon;
                    buildingTab.SecondaryIcon = x.SecondaryIcon;
                    buildingTab.SubTabs = new ScriptableBuildingTab[0];

                    buildingTab.HasParentCategory = x.UseAParentCategory;
                    buildingTab.ParentCategory = x.ParentCategory;

                    return buildingTab;
                })
                .ToList();

            var buildingBlueprints = allAssets
                .Where(x => x is AloftModFrameworkBuildingBlueprint)
                .Cast<AloftModFrameworkBuildingBlueprint>()
                .ToList();

            //a building has no blueprint if and only if
            //there is no match of a building blueprint to our building data
            var BuildingsWithoutBlueprints = allAssets
                .Where(x => x is AloftModFrameworkBuildingData)
                .Cast<AloftModFrameworkBuildingData>()
                .Where(x => buildingBlueprints.FirstOrDefault(y => y.BuildingData.PopulationId == x.PopulationId) == null)
                .Select(x =>
                {
                    var populationData = ScriptableObject.CreateInstance(typeof(AloftModFrameworkPopulationData)) as AloftModFrameworkPopulationData;

                    populationData.PopulationID = (PopulationID.ID)x.PopulationId;
                    populationData.Prefab = x.InstancePrefab;
                    populationData.BehaviourType = x.BehaviourType;
                    populationData.MultiStepBehaviour = x.MultiStepBehaviour;
                    populationData.LoadDistance = x.LoadDistance;
                    populationData.PopDataTags = x.PopDataTags;

                    if (x.CanLearnViaSketchbook)
                    {
                        Logger.LogWarning(string.Format("A building {0} was marked with CanLearnViaSketchbook but it has no corresponding Blueprint to make it buildable.", x.name));
                    }

                    return populationData;
                })
                .ToList();

            var buildingsAndTheirBlueprints = allAssets
                .Where(x => x is AloftModFrameworkBuildingData)
                .Cast<AloftModFrameworkBuildingData>()
                .Where(x => buildingBlueprints.FirstOrDefault(y => y.BuildingData.PopulationId == x.PopulationId) != null)
                .Select(building =>
                {
                    Logger.LogInfo("Building building data:" + building.name);

                    var populationData = ScriptableObject.CreateInstance(typeof(AloftModFrameworkPopulationData)) as AloftModFrameworkPopulationData;
                    populationData.hideFlags = HideFlags.HideAndDontSave;
                    populationData.PopulationID = (PopulationID.ID)building.PopulationId;
                    populationData.Prefab = building.InstancePrefab;
                    populationData.BehaviourType = building.BehaviourType;
                    populationData.MultiStepBehaviour = building.MultiStepBehaviour;
                    populationData.LoadDistance = building.LoadDistance;
                    populationData.PopDataTags = building.PopDataTags;
                    populationData.PrefabPaths = new string[0];

                    var buildingBlueprint = buildingBlueprints.First(blueprint => blueprint.BuildingData == building);
                    Logger.LogInfo("Building corresponding blueprint data:" + buildingBlueprint.name);
                    var blueprintData = ScriptableObject.CreateInstance(typeof(ScriptableCrafting)) as ScriptableCrafting;
                    blueprintData.hideFlags = HideFlags.HideAndDontSave;
                    blueprintData.ID = populationData.PopulationID;
                    blueprintData.PopData = populationData;
                    blueprintData.DisplayName = buildingBlueprint.DisplayName;
                    blueprintData.DisplayDescription = buildingBlueprint.DisplayDescription;
                    blueprintData.DisplaySprite = buildingBlueprint.DisplaySprite;
                    blueprintData.HideInBuildMenu = buildingBlueprint.HideInBuildMenu;
                    blueprintData.Category = buildingBlueprint.GetCategory();
                    blueprintData.Variants = buildingBlueprint.Variants.Select(variant => (PopulationID.ID)variant.PopulationId).ToArray();
                    blueprintData.IsVariantOf = buildingBlueprint.IsVariantOf == null ? PopulationID.ID.Empty : (PopulationID.ID)buildingBlueprint.IsVariantOf.PopulationId;
                    blueprintData.DefaultScale = buildingBlueprint.DefaultScale;
                    blueprintData.CraftingCost = buildingBlueprint.CraftingCost;
                    blueprintData.HammerCost = buildingBlueprint.HammerCost;
                    blueprintData.PopToUnlockAsWell = buildingBlueprint.PopToUnlockAsWell;
                    blueprintData.AudioType = buildingBlueprint.AudioType;

                    populationData.ScriptableCrafting = blueprintData;
                    populationData.SketchbookCraftingRef = blueprintData;


                    return new
                    {
                        Blueprint = blueprintData,
                        Building = populationData,
                    };
                })
                .ToList();

            // TODO: is there blueprints that might not have been mapped to a building? If so, that's probably something we should say...
            BuildingBlueprints = buildingsAndTheirBlueprints.Select(x => x.Blueprint).ToList();

            var workbench = Resources.Load("Platform Builder/Constructions/Machines/Pre_Construction_Workbench") as GameObject;
            var workbenchMeshRenderer = workbench.GetComponentsInChildren<MeshRenderer>().First();
            var workbenchMaterial = workbenchMeshRenderer.material;
            Buildings = buildingsAndTheirBlueprints.Select(x => x.Building)//.Union(BuildingsWithoutBlueprints)
                .ForEach(x =>
                {
                    x.Prefab.GetComponentsInChildren<MeshRenderer>().ForEach(mesh =>
                    {
                        var mainTex = mesh.material.GetTexture("_MainTex");
                        var normalTex = mesh.material.GetTexture("_BumpMap");
                        var detailMask = mesh.material.GetTexture("_DetailMask");

                        var mat = new Material(workbenchMaterial);
                        mat.name = "AloftModFramework_DefaultPBR_Interactive_Material";
                        mat.hideFlags = HideFlags.HideAndDontSave;
                        mat.SetTexture("_TextureAlbedo", mainTex);
                        mat.SetTexture("_TextureNormals", normalTex);
                        mat.SetTexture("_TextureMask", detailMask);
                        mat.SetVector("_ColorSelect", new Vector4(1.6f, 1.3f, 0.5f, 1));
                        mat.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
                        mesh.material = mat;
                        for (int i = 0; i < mesh.materials.Length; i++)
                        {
                            // woulda been nice to go from shader, but IDK what's going on there... Lets you dodge the bullet on loading an object...
                            // var mats = new Material(Shader.Find("Amplify/V2/DefaultPBR_Interactive"));
                            var mat2 = new Material(workbenchMaterial);
                            mat2.name = "AloftModLoaderMaterial";
                            mat2.hideFlags = HideFlags.HideAndDontSave;
                            mat2.SetTexture("_TextureAlbedo", mainTex);
                            mat2.SetTexture("_TextureNormals", normalTex);
                            mat2.SetTexture("_TextureMask", detailMask);
                            mat2.SetVector("_ColorSelect", new Vector4(1.6f, 1.3f, 0.5f, 1));
                            mat2.SetColor("_Color", new Color(1f, 1f, 1f, 1f));
                            mesh.materials[i] = mat2;
                        }
                    });
                })
                .ToList();

            Localizations = allAssets
                .Where(x => x is AloftModFrameworkLocalization)
                .Cast<AloftModFrameworkLocalization>()
                .ToList();

            LoggerRef = Logger;

            Harmony harmony = new Harmony(GUID);
            var alternativeItemLoadPoint = AccessTools.Method(typeof(ScriptableInventoryManager), nameof(ScriptableInventoryManager.GetItem), new[] { typeof(ItemID.ID) });
            var customItemLoadHook = AccessTools.Method(typeof(ItemPatches), nameof(ItemPatches.RewriteItemIdResult));
            harmony.Patch(alternativeItemLoadPoint, null, new HarmonyMethod(customItemLoadHook));

            var recipeLoadPoint = AccessTools.Method(typeof(SRecipeManager), nameof(SRecipeManager.GetRecipes));
            var recipeLoadHook = AccessTools.Method(typeof(RecipePatches), nameof(RecipePatches.RewriteRecipeResult));
            harmony.Patch(recipeLoadPoint, null, new HarmonyMethod(recipeLoadHook));

            var localizationPoint = AccessTools.Method(typeof(Localization), nameof(Localization.SetLanguage));
            var localizationHook = AccessTools.Method(typeof(LocalizationPatches), nameof(LocalizationPatches.SetLanguage));
            harmony.Patch(localizationPoint, null, new HarmonyMethod(localizationHook));

            var localizationPoint2 = AccessTools.Method(typeof(Localization), nameof(Localization.GetLocalizedValue));
            var localizationHook2 = AccessTools.Method(typeof(LocalizationPatches), nameof(LocalizationPatches.GetLocalizedValue));
            harmony.Patch(localizationPoint2, null, new HarmonyMethod(localizationHook2));

            var craftingPoint = AccessTools.Method(typeof(ScriptableCraftingManager), nameof(ScriptableCraftingManager._GetCrafting));
            var craftingHook = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetCrafting));
            harmony.Patch(craftingPoint, null, new HarmonyMethod(craftingHook));

            var craftingPoint2 = AccessTools.Method(typeof(PopulationManager), nameof(PopulationManager.GetPopulationData));
            var craftingHook2 = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetPopulationData));
            harmony.Patch(craftingPoint2, null, new HarmonyMethod(craftingHook2));
            var craftingPoint3 = AccessTools.Method(typeof(ScriptablePopulationDataManager), nameof(ScriptablePopulationDataManager.GetPopulation));
            var craftingHook3 = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetPopulationData2));
            harmony.Patch(craftingPoint3, null, new HarmonyMethod(craftingHook3));
            var craftingPoint4 = AccessTools.Method(typeof(ScriptablePopulationDataManager), nameof(ScriptablePopulationDataManager.InitList));
            var craftingHook4 = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.InitListExtension));
            harmony.Patch(craftingPoint4, null, new HarmonyMethod(craftingHook4));
            var prefabPoint = AccessTools.Method(typeof(ScriptablePopulationData), nameof(ScriptablePopulationData.GetPrefabGameObject));
            var prefabHook = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.GetPrefabGameObject));
            harmony.Patch(prefabPoint, null, new HarmonyMethod(prefabHook));
            var buildingTabsHookPoint = AccessTools.Method(typeof(UI_BuildingMenu), nameof(UI_BuildingMenu.Tabs_Initialize));
            var buildingTabsHook = AccessTools.Method(typeof(BuildingHooks), nameof(BuildingHooks.AddMoreTabs));
            harmony.Patch(buildingTabsHookPoint, new HarmonyMethod(buildingTabsHook));
        }

    }
}
