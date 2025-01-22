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


    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Crafting Recipe")]
    public class AloftModFrameworkCraftingRecipe : ScriptableObject
    {
        public int[] InputItems;
        public ItemID.ID OutputVanillaItem;
        public int OutputItemId;
        public AloftModFrameworkItem OutputModItem;
        public int Quantity;
    }

    /*
     *     [Serializable]
    public class CraftingCostClass
    {
        [FormerlySerializedAs("itemID")]
        public ItemID.ID ItemID;

        [FormerlySerializedAs("qty")]
        public int Qty;

        public CraftingCostClass()
        {
        }

        public CraftingCostClass(ItemID.ID itemID, int qty)
        {
            ItemID = itemID;
            Qty = qty;
        }
    }*/

    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Localization Resource")]
    public class AloftModFrameworkLocalization : ScriptableObject
    {
        public TextAsset LocalizationFile;
        public string Language;
    }

    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Building")]
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

    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Building Blueprint")]
    public class AloftModFrameworkBuildingBlueprint : ScriptableObject
    {
        public bool HideInBuildMenu;
        public string DisplayName;
        public string DisplayDescription;
        public Sprite DisplaySprite;

        // TOOD: some day we should allow this to be set without the enum (kinda like for items), so we can add custom categories.
        public ScriptableBuildingTab.BuildingCategory Category;
        
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
    }
}
