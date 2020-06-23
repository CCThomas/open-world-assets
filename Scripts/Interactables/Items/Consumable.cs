using System.Collections.Generic;
using UnityEngine;

// Books, Drinks, Food, Potions, ect...
[CreateAssetMenu(fileName = "New Book", menuName = "Interactable/Item/Book")]
public class Consumable : AbstractItem
{
    List<TraitModifier> modifiers;
}
