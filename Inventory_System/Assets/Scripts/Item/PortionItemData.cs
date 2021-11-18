using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Item_Portion_", menuName = "Inventory System/Item Data/Portion",order = 3)]

public class PortionItemData : CountableItemData
{
    public int HPRegen => _HpRegen;
    public int MPRegen => _MpRegen;
    public int EXP => _EXP;

    [SerializeField] int _HpRegen;
    [SerializeField] int _MpRegen;
    [SerializeField] int _EXP;

    public override Item CreateItem()
    {
        return new PortionItem(this);
    }
}
