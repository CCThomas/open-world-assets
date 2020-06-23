//using System;
//using System.Collections.Generic;

//public class Playground
//{
//    public static Weapon sword()
//    {
//        Weapon weapon = new Weapon();
//        weapon.damage = 6;
//        weapon.cost = 10;
//        weapon.weight = 2;
//        WeaponCategory category = new WeaponCategory();
//        category.name = "Sword";
//        weapon.category = category;
//        WeaponDamageType weaponDamageType = new WeaponDamageType();
//        weaponDamageType.name = "Piercing";
//        weapon.weaponDamageType = weaponDamageType;
//        WeaponProperty weaponProperty1 = new WeaponProperty();
//        weaponProperty1.name = "Finesse";
//        WeaponProperty weaponProperty2 = new WeaponProperty();
//        weaponProperty2.name = "Light";
//        weapon.weaponProperty = new List<WeaponProperty>()
//        {
//            weaponProperty1,weaponProperty2
//        };
//        return weapon;
//    }

//    public static RangedWeapon bow()
//    {
//        RangedWeapon weapon = new RangedWeapon();
//        weapon.damage = 1;
//        weapon.cost = 25;
//        weapon.weight = 2;
//        WeaponCategory category = new WeaponCategory();
//        category.name = "Bow";
//        weapon.category = category;
//        WeaponDamageType weaponDamageType = new WeaponDamageType();
//        weaponDamageType.name = "Bludgeoning";
//        weapon.weaponDamageType = weaponDamageType;
//        WeaponProperty weaponProperty1 = new WeaponProperty();
//        weaponProperty1.name = "Two-Handed";
//        weapon.weaponProperty = new List<WeaponProperty>()
//        {
//            weaponProperty1
//        };
//        weapon.supportedAmmunation = new List<Projectile>()
//        {
//            arrow()
//        };
//        return weapon;
//    }

//    public static Projectile arrow()
//    {
//        Projectile projectile = new Projectile();
//        projectile.damage = 6;
//        projectile.cost = 0.1f;
//        projectile.weight = 0.1f;
//        ProjectileCategory projectileCategory = new ProjectileCategory();
//        projectileCategory.name = "Arrow";
//        projectile.projectileCategory = projectileCategory;
//        return projectile;
//    }
//}
