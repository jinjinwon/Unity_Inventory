using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_Default_", menuName = "Inventory System/Item Data/Default", order = 3)]

public class DefaultItemData : CountableItemData
{
    public override Item CreateItem()
    {
        return new DefaultItem(this);
    }
}
