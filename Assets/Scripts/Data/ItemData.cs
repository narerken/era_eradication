using UnityEngine;

namespace Emberfall.Data
{
    public enum ItemKind { EmberShard, Heart }

    [CreateAssetMenu(fileName = "ItemData", menuName = "Emberfall/Item Data")]
    public class ItemData : ScriptableObject
    {
        public ItemKind kind = ItemKind.EmberShard;
        public int scoreValue = 50;
        public int healAmount = 1;
    }
}
