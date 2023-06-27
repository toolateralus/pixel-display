using Newtonsoft.Json;
using Pixel;
using Pixel.Types.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel
{
    [Serializable]
    public class Item
    {
        private readonly Dictionary<string, int> data = new Dictionary<string, int>();
        public ItemType Type { get; private set; }
        public string Name { get; private set; }
        public readonly int slot;
        public Item(string name, ItemType type, int slot, Dictionary<string, int> data)
        {
            Type = type;
            Name = name;
            this.slot = slot;
            this.data = data;
        }
        public int GetProperty(string name)
        {
            if (data.ContainsKey(name))
                return data[name];
            return 0;
        }
        public void SetProperty(string name, int value)
        {
            data[name] = value;
        }
        internal int GetWeaponMeleeDamage()
        {
            if (this.Type != ItemType.Weapon)
                throw new Exception("Weapon damage being read from a non-weapon");

            int base_weapon_damage = JRandom.Int(GetProperty(MinWeaponDamageProperty), GetProperty(MaxWeaponDamageProperty));
            return base_weapon_damage * GetProperty(StrengthValueProperty) + GetProperty(AgilityValueProperty);
        }

        public const string HitSpeedValueProperty = "hit_speed";
        public const string ArmorValueProperty = "armor";
        public const string MinWeaponDamageProperty = "damage_min";
        public const string MaxWeaponDamageProperty = "damage_max";
        public const string StaminaValueProperty = "stamina";
        public const string StrengthValueProperty = "strength";
        public const string AgilityValueProperty = "agility";
        public const string AnimationIDProperty = "anim";
        public const string ModelIDProperty = "model";
        public const string AudioIDProperty = "audio";
    }
    public enum ItemType
    {
        Weapon,
        Armor,
        Jewelry,
    }
    public class Inventory
    {
        [JsonProperty]
        public List<Item> inventory = new();
        public List<Item> equippedInventory = new();
        public void InsertNew(string name, ItemType type, int slot, Dictionary<string, int> itemData) => inventory.Add(new Item(name, type, slot, itemData));

        internal void Add(Item item)
        {
            inventory.Add(item);
        }

        internal (int, int) GetWeaponDamageAndSpeed()
        {
            var weps = inventory.Where(item => item.Type == ItemType.Weapon);
            if(weps.Any())
                return (weps.First().GetWeaponMeleeDamage(), weps.First().GetProperty(Item.HitSpeedValueProperty));
            return (0, 0); 
        }
    }
 
}
