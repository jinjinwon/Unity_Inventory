using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CountableItem : Item
{
    public CountableItemData _CountableData { get; private set; }
    public int Count { get; protected set; }                        // 현재 개수
    public int MaxCount => _CountableData.MaxCount;                 // 최대 개수
    public bool IsMax => Count >= _CountableData.MaxCount;          // 최대개수를 넘어가는가?
    public bool IsEmpty => Count <= 0;                              // 0보다 작은가?

    // 생성자 초기화 리스트
    public CountableItem(CountableItemData _Data, int a_Count = 1) : base(_Data)
    {
        _CountableData = _Data;
        SetCount(a_Count);
    }

    // 개수 지정 및 범위 제한
    public void SetCount(int a_Count)
    {
        Count = Mathf.Clamp(a_Count, 0, MaxCount);
    }

    // 개수 추가 및 최대 개수 달성 시 초과량 반환
    public int AddCountAndGetExcess(int a_Count)
    {
        int a_NextCount = Count + a_Count;
        SetCount(a_NextCount);

        return (a_NextCount > MaxCount) ? (a_NextCount - MaxCount) : 0;
    }

    // 개수를 나누어 복제
    public CountableItem SeperateAndClone(int a_Count)
    {
        // 수량이 1개라면
        if (Count <= 1)
            return null;

        if (a_Count > Count - 1)
            a_Count = Count - 1;

        Count -= a_Count;
        return Clone(a_Count);
    }

    protected abstract CountableItem Clone(int a_Count);
}
