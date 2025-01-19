using Scriptable_Objects;
using UnityEngine;

namespace AloftModFramework.Items
{

    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Item")]
    public class AloftModFrameworkItem : ScriptableInventoryItem
    {
        public int postLoadedItemId;
        public string postLoadedLocalizedName;
    }
}
