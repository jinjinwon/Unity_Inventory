using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType
{
    Armor,
    Helmat,
    Shoes,
    Gloves,
    Sword,
    Ring,
    Necklace
}

[CreateAssetMenu(fileName = "Item_Weapon_", menuName = "Inventory System/Item Data/Weapon", order = 3)]
public class WeaponItemData : ItemData
{
    public int Atk => _Atk;
    public int AtkSp => _AtkSp;
    public int Def => _Def;
    public int HP => _HP;
    public int MP => _Mp;
    public int Critical => _Critical;
    public int CriticalDmg => _CriticalDmg;
    public WeaponType WeaponType => _WeaponType;

    [SerializeField] int _Atk;
    [SerializeField] int _AtkSp;
    [SerializeField] int _Def;
    [SerializeField] int _HP;
    [SerializeField] int _Mp;
    [SerializeField] int _Critical;
    [SerializeField] int _CriticalDmg;
    [SerializeField] WeaponType _WeaponType;

    public override Item CreateItem()
    {
        return new WeaponItem(this);
    }
}
