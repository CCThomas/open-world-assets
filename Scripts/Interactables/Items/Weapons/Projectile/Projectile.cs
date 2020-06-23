using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon Category", menuName = "Interactable/Item/Weapon/Projectile/Projectile")]
public class Projectile : AbstractItem
{
    public Effect effect;
    public ProjectileCategory projectileCategory;
}