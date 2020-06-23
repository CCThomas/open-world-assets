using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attire", menuName = "Interactable/Item/Attire/Attire")]
public class Attire : AbstractItem
{
    AttireCategory attireCategory;
    List<TraitModifier> traitModifiers; // Stealth & Damage Protection
}
