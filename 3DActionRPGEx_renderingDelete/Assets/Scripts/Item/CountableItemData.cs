using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 셀 수 있는 아이템
public abstract class CountableItemData : ItemData
{
    public int MaxCount => _MaxCount; 
    [SerializeField] int _MaxCount = 99;    
}
