using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultItem : CountableItem , IUsableItem
{
    public DefaultItem(CountableItemData _Data, int a_Count = 1) : base(_Data, a_Count) { }

    public bool Use()
    {
        Count--;
        return true;
    }

    protected override CountableItem Clone(int a_Count)
    {
        return new DefaultItem(_CountableData as DefaultItemData, a_Count);
    }
}
