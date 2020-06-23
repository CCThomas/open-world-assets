using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ranged Weapon", menuName = "Interactable/Item/Weapon/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    float capacity;
    float reloadInSeconds;
    float radius;
    float height;
    AreaOfEffect areaOfEffect;
    List<Projectile> supportedProjectiles;
}

public enum AreaOfEffect
{
    Cone, Cylinder, Line,
}
