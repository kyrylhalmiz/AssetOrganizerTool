using UnityEngine;

namespace Editor.AssetsOrganizer.Model
{
    public enum ItemCategory
    {
        None,
        Weapon,
        Armor,
        Consumable,
        Quest,
        Misc
    }

    [CreateAssetMenu(
        fileName = "GameItem_",
        menuName = "Models/Tools/Item Config",
        order = 0)]
    public class GameItemConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField]
        private string itemId = System.Guid.NewGuid().ToString();

        [SerializeField]
        private string displayName;

        [SerializeField, TextArea]
        private string description;

        [Header("Gameplay")]
        [SerializeField]
        private ItemCategory category = ItemCategory.None;

        [SerializeField, Min(0)]
        private int price;

        [SerializeField]
        private bool isUnique;

        [Header("Visuals")]
        [SerializeField]
        private Sprite icon;
        
        public string ItemId => itemId; 

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public ItemCategory Category
        {
            get => category;
            set => category = value;
        }

        public int Price
        {
            get => price;
            set => price = Mathf.Max(0, value);
        }

        public bool IsUnique
        {
            get => isUnique;
            set => isUnique = value;
        }

        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                itemId = System.Guid.NewGuid().ToString();
            }

            price = Mathf.Max(0, price);
        }
#endif
    }
}
