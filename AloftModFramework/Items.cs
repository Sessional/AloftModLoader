using Scriptable_Objects;
using System.Collections.Generic;
using System.Security.Policy;
using UI.Cosmetic;
using UnityEngine;

namespace AloftModFramework.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "AloftModFramework/Item")]
    public class AloftModFrameworkItem : ScriptableInventoryItem
    {
        public int ItemId;
    }
}
