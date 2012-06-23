using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD.Items
{
    /// <summary>
    /// Instead of using a strictly object-oriented methodology, we're using the entity-component model for items.
    /// This allows a given item to have several different properties - consider a bladed wand that could be used
    /// as both a magic focus and a weapon; or some antidotal plant leaves that could either be eaten as food or
    /// used as a crafting material.
    /// </summary>
    public enum ItemComponentTypes
    {
        Light,
        Armor,
        Weapon,
        Food,
        Drink,
        Wand,
        Furniture,
        Key,
        Container,
        Vehicle,
        Reagent,
        Material,
        Socketable
    }
}
