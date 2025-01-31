using Audio;
using Crafting.MultiStep_Construction;
using Player.Player_Equip;
using Scriptable_Objects;
using System;
using System.Collections.Generic;
using System.IO;
using Terrain.Platforms.Population.Population_Soul;
using UnityEngine;
using Utilities;
using static Scriptable_Objects.ScriptableInventoryItem;
using static System.Collections.Specialized.BitVector32;

namespace AloftModFramework.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Item")]
    public class AloftModFrameworkItem : ScriptableObject
    {
        public int ItemId;
        public string Name;
        public string Description;
        public Sprite DisplaySprite;

        public ItemID.ItemCatergory Category;
        public ItemID.ItemType Type;
        public ItemID.ItemWeight Weight;
        public PlayerEquip.Equipable EquipType;
        public Audio_Pickup.PickUpAudioTagID AudioPickupID;
        public ItemTagID[] ItemTags;
    }

    // TODO: ideally item references would look something like this, but Unity doesn't appear to serialize these
    // when loading from an asset bundle... It's there in the file but comes out null when you load the asset.. :(
    //[Serializable]
    //public class AloftModFrameworkItemReference
    //{
    //    public int ItemId;
    //    public ItemID.ID VanillaItem;
    //    public AloftModFrameworkItem ModItem;

    //    public AloftModFrameworkItemReference() { }
    //    public AloftModFrameworkItemReference(int itemId, ItemID.ID vanillaItem, AloftModFrameworkItem modItem)
    //    {
    //        ItemId = itemId;
    //        VanillaItem = vanillaItem;
    //        ModItem = modItem;
    //    }

    //    public int GetItemIdAsInt()
    //    {
    //        if (VanillaItem != ItemID.ID.Empty) return (int)VanillaItem;
    //        if (ModItem != null) return ModItem.ItemId;
    //        else return ItemId;
    //    }

    //    public ItemID.ID GetItemId()
    //    {
    //        return (ItemID.ID)GetItemIdAsInt();
    //    }
    //}

    [CreateAssetMenu(fileName = "RecipeGroup", menuName = "AloftModFramework/Crafting Recipe Group")]
    public class AloftModFrameworkCraftingRecipeGroup : ScriptableObject
    {
        public int StationId;
        public string StationName;
        public AloftModFrameworkCraftingRecipe[] Recipes;
    }

    [CreateAssetMenu(fileName = "Recipe", menuName = "AloftModFramework/Crafting Recipe")]
    public class AloftModFrameworkCraftingRecipe : ScriptableObject
    {
        public int[] InputItems;
        public ItemID.ID OutputVanillaItem;
        public int OutputItemId;
        public AloftModFrameworkItem OutputModItem;
        public int Quantity;

        public bool AttachToExistingStation;
        public SRecipeManager.CraftingStation Station;
    }

    [CreateAssetMenu(fileName = "Localization", menuName = "AloftModFramework/Localization Resource")]
    public class AloftModFrameworkLocalization : ScriptableObject
    {
        public TextAsset LocalizationFile;
        public string Language;
    }

    [CreateAssetMenu(fileName = "Building", menuName = "AloftModFramework/Building")]
    public class AloftModFrameworkBuildingData : ScriptableObject
    {
        public int PopulationId;
        public GameObject InstancePrefab;
        
        // TODO: replace with a different construct to permit custom behavior types
        public PopulationSoul.BehaviourTypeEnum BehaviourType;
        public MultiStepConstructionManager.MultiStepBehaviour MultiStepBehaviour;

        public ScriptablePopulationData.SpawnDistance LoadDistance = ScriptablePopulationData.SpawnDistance.Medium;
        public ScriptablePopulationData.PopDataTagID[] PopDataTags;

        public bool CanLearnViaSketchbook;
    }

    [CreateAssetMenu(fileName = "Building Blueprint", menuName = "AloftModFramework/Building Blueprint")]
    public class AloftModFrameworkBuildingBlueprint : ScriptableObject
    {
        public bool HideInBuildMenu;
        public string DisplayName;
        public string DisplayDescription;
        public Sprite DisplaySprite;

        public ScriptableBuildingTab.BuildingCategory Category;
        public AloftModFrameworkBuildingCategory ModCategory;
        public int CategoryId;
        
        // This is really the "LOOK" of the construction thing.
        public AloftModFrameworkBuildingData BuildingData;

        // TOOD: these should become smarter to allow binding to EXISTING data -- (we deleted the PopulationID.ID enum reference because it only works for vanilla recipes then
        public AloftModFrameworkBuildingData IsVariantOf;
        public AloftModFrameworkBuildingData[] Variants;

        public float DefaultScale = 1f;

        // TODO: we should make it so these are smart enough to allow new resources
        public ScriptableCrafting.CraftingCostClass[] CraftingCost;
        public ScriptableCrafting.CraftingCostClass[] HammerCost;


        // TODO: we should make it so these are smart enough to allow new "pops"
        public PopulationID.ID[] PopToUnlockAsWell;


        public ScriptableCrafting.ConstructionMaterial AudioType;

        public ScriptableBuildingTab.BuildingCategory GetCategory()
        {
            if (ModCategory != null)
            {
                return (ScriptableBuildingTab.BuildingCategory) ModCategory.BuildingCategoryId;
            }
            
            if (CategoryId != 0)
            {
                return (ScriptableBuildingTab.BuildingCategory)CategoryId;
            }

            return Category;
        }
    }

    [CreateAssetMenu(fileName = "Building Category", menuName = "AloftModFramework/Building Category")]
    public class AloftModFrameworkBuildingCategory : ScriptableObject
    {
        public int BuildingCategoryId;
        public string Name;
        public Sprite DisplayIcon;
        public Sprite SecondaryIcon;
        public bool UseAParentCategory = false;
        public ScriptableBuildingTab.BuildingCategory ParentCategory;
    }
}
