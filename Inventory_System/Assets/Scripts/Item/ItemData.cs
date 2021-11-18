using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Default,
    Portion,
    Weapon
}

public enum GradeType
{
    Null,
    Normal,
    Rare,
    Unique,
    Legend,
    Only
}

public abstract class ItemData : ScriptableObject
{
    public int ID => _ID;
    public string Name => _Name;
    public string Tooltip => _Tooltip;
    public Sprite Sprite => _IconSprite;
    public GameObject DropItem => _DropItemPrefab;
    public int Price => _Price;
    public ItemType ItemType => _ItemType;
    public GradeType Grade => _Grade;
    public int ItemIndex => _ItemIndex;

    [SerializeField] int _ID;
    [SerializeField] string _Name;
    [SerializeField] string _Tooltip;
    [SerializeField] Sprite _IconSprite;
    [SerializeField] GameObject _DropItemPrefab;
    [SerializeField] int _Price;
    [SerializeField] ItemType _ItemType;
    [SerializeField] GradeType _Grade;
    [SerializeField] int _ItemIndex;

    public abstract Item CreateItem();
}
