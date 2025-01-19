using AloftModFramework.Items;
using BepInEx;
using BepInEx.Unity.Mono;
using HarmonyLib;
using Level_Manager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
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
        const string GUID = "github.com/sessional/AloftModLoader";
        const string NAME = "Aloft Mod Loader";
        const string VERSION = "0.1";

        static List<AssetBundle> bundles;
        static List<AloftModFrameworkItem> Items;
        public void Awake()
        {
            string bundleDirectory = Path.Combine(Application.streamingAssetsPath, "amf");
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
                .ForEach(x => x.ID = (ID) x.postLoadedItemId)
                .ToList();
            Harmony harmony = new Harmony(GUID);

            var loadTimeMethod = AccessTools.Method(typeof(ManagerAbstract), "LateInitialize");
            var loadTimeHook = AccessTools.Method(typeof(AloftModLoader), "OverrideLateInitialize");
            harmony.Patch(loadTimeMethod, new HarmonyMethod(loadTimeHook));

            //var originalScriptableItem = AccessTools.Method(typeof(CanvasConsole), "EnterCommand");
            //var secondScriptableItem = AccessTools.Method(typeof(Class1), "OverrideCommands");
            //harmony.Patch(originalScriptableItem, new HarmonyMethod(secondScriptableItem));
        }

        public static void OverrideLateInitialize()
        {
            if (Items.Count > 0 && !Level.ConstantManager.ConstantManagers.InventoryManager.Items.Contains(Items.First()))
            {
                Level.ConstantManager.ConstantManagers.InventoryManager.Items = Level.ConstantManager.ConstantManagers.InventoryManager.Items.AddRangeToArray(Items.ToArray());
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
