using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : Item
{
    public WeaponItemData _WeaponData { get; private set; }

    // 생성자 초기화 리스트
    public WeaponItem(WeaponItemData _Data) : base(_Data)
    {
        _WeaponData = _Data;
    }
}
