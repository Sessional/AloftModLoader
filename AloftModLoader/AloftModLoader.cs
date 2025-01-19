using AloftModFramework.Items;
using BepInEx;
using BepInEx.Unity.Mono;
using Crafting.Inventory;
using HarmonyLib;
using Level_Manager;
using Scriptable_Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilities;
using static Utilities.ItemID;

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

    [BepInPlugin(GUID, NAME, VERSION)]
    public class AloftModLoader : BaseUnityPlugin
    {
        const string GUID = "aloftmodloader.sessional.dev";
        const string NAME = "Aloft Mod Loader";
        const string VERSION = "1.0";

        static List<AssetBundle> bundles;
        static List<AloftModFrameworkItem> Items;

        static FieldInfo LocalizationField;
        public void Awake()
        {
            Logger.LogInfo("Running!");
            string bundleDirectory = Path.Combine(Application.streamingAssetsPath, "amf");
            Logger.LogInfo("Bundle directory: " + bundleDirectory);
            var bundleNames = Directory.GetFiles(bundleDirectory);
            Logger.LogInfo("Bundle names: " + string.Join(",", bundleNames));

            bundles = bundleNames
                .Where(x => !x.EndsWith(".manifest"))
                .Distinct()
                .Select(x => {
                Logger.LogInfo(string.Format("Loading asset {0}", x));
                return AssetBundle.LoadFromFile(x);
            }).ToList();
            var allAssets = bundles.SelectMany(x => x.LoadAllAssets());

            Items = allAssets
                .Where(x => x is AloftModFrameworkItem)
                .Cast<AloftModFrameworkItem>()
                .ForEach(x => {
                    x.ID = (ID)x.ItemId;
                    //localAsDict.Add(x.name, x.postLoadedLocalizedName);
                })
                .ToList();
            
            Harmony harmony = new Harmony(GUID);
            PatchItemLoading(harmony);


            var type = typeof(Localization);
            var dictionary = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            LocalizationField = dictionary.Where(x => x.Name.Equals("_locDictionary")).First();

            //var originalScriptableItem = AccessTools.Method(typeof(CanvasConsole), "EnterCommand");
            //var secondScriptableItem = AccessTools.Method(typeof(Class1), "OverrideCommands");
            //harmony.Patch(originalScriptableItem, new HarmonyMethod(secondScriptableItem));
        }

        /// <summary>
        /// Patching item loading needs to happen during the world initialization
        /// 
        /// When the level loads, it hooks up static instances of things, and those
        /// instances involve loading item pouches that might be on the floor. If 
        /// hooking into the LateInitialize method here doesn't occur, those item
        /// pouches cause the scripting engine to error out and those item pouches
        /// to break (permanently?).
        /// 
        /// The crux of the issue is that you need
        /// </summary>
        /// <param name="harmony"></param>
        public void PatchItemLoading(Harmony harmony)
        {
            var itemLoadHookPoint = AccessTools.Method(typeof(ManagerAbstract), "LateInitialize");
            var customItemLoadMethod = AccessTools.Method(typeof(AloftModLoader), "OverrideLateInitialize");
            harmony.Patch(itemLoadHookPoint, new HarmonyMethod(customItemLoadMethod));
        }

        
        public static void OverrideLateInitialize()
        {
            if (Level.ConstantManager != null)
            {
                if (Items.Count > 0 && !Level.ConstantManager.ConstantManagers.InventoryManager.Items.Contains(Items.First()))
                {
                    var localization = LocalizationField.GetValue(null);
                    var localAsDict = (Dictionary<string, string>)localization;
                    Items.ForEach(x =>
                    {
                        localAsDict.Add(x.DisplayName, x.DisplayName);
                        localAsDict.Add(x.DisplayDescription, x.DisplayDescription);
                    });
                    Level.ConstantManager.ConstantManagers.InventoryManager.Items = Level.ConstantManager.ConstantManagers.InventoryManager.Items.AddRangeToArray(Items.ToArray());
                }
            }
        }

        public static bool OverrideCommands(string input)
        {
            //if (input.StartsWith("yes"))
            //{
            //    Crafting.Inventory.Inventory.AddItem(newItem.ID, 1);
            //    return false;
            //}

            return true;
        }

    }
}
