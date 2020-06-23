using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Interactable/Item/Weapon/Weapon")]
public class Weapon : AbstractItem
{
    Effect[] effect; // Damage
    Effect[] secondaryEffects; // Used if player holds attack action / Versatile Weapons
    WeaponCategory weaponCategory;
    WeaponProperty weaponProperty;
}
