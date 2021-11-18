using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortionItem : CountableItem , IUsableItem
{
    public PortionItem(CountableItemData _Data, int a_Count = 1) : base(_Data, a_Count) { }

    public bool Use()
    {
        Count--;
        return true;
    }

    protected override CountableItem Clone(int a_Count)
    { 
        return new PortionItem(_CountableData as PortionItemData, a_Count);
    }
}
