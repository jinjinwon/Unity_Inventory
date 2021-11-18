using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SimpleJSON;
using UnityEngine.Networking;

// 인벤토리 내부 동작 담당

public class Inventory : MonoBehaviour
{
    // 아이템 수용 한도
    public int m_Capacity { get; private set; }

    [SerializeField] int m_InitalCapacity = 32;                     // 초기 수용 한도
    [SerializeField] int m_MaxCapacity = 64;                        // 최대 수용 한도
    public InventoryUI m_InventoryUI;                               // 연결할 인벤토리 UI
    [HideInInspector] public Item[] m_Items;                        // 아이템 목록
    HashSet<int> m_IndexSetForUpdate = new HashSet<int>();          // 업데이트 할 인덱스 목록
    string m_SvStrJson_1 = "";
    string m_SvStrJson_2 = "";
    string SaveItemUrl = "";

    static Dictionary<Type, int> m_SortWeightDict = new Dictionary<Type, int>
    {
        {typeof(PortionItemData), 10000 },
        {typeof(WeaponItemData), 20000 },
        {typeof(DefaultItemData),30000 },
    };

    // 중첩 클래스 사용
    class ItemComparer : IComparer<Item>
    {
        public int Compare(Item A, Item B) => (A.Data.ID + m_SortWeightDict[A.Data.GetType()]) - (B.Data.ID + m_SortWeightDict[B.Data.GetType()]);
    }

    static ItemComparer m_ItemComparer = new ItemComparer();

    // 인벤토리
    public bool IsValidIndex(int a_Index) => a_Index >= 0 && a_Index < m_Capacity;                      // 인덱스가 수용 범위에 있는지 확인
    public bool HasItem(int a_Index) => IsValidIndex(a_Index) && m_Items[a_Index] != null;              // 해당 슬롯이 아이템을 갖고 있는가?
    public bool IsCountableItem(int a_Index) => HasItem(a_Index) && m_Items[a_Index] != null;           // 해당 슬롯이 셀 수 있는 아이템 인가?

    void Awake()
    {
        InitSetting();
    }

    void Start()
    {
        UpdateAccessibleStatesAll();
        m_InventoryUI.UpdateSlotCountUI(m_Capacity, m_MaxCapacity);
        Invoke("UpdateAllSlot", 0.1f);
    }

    public void SlotSave()
    {
        // 아이템 종류
        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray();//배열이 필요할때
        for (int i = 0; i < m_Items.Length; i++)
        {
            if (m_Items[i] != null) jArray.Add(m_Items[i].Data.ItemIndex);
            else jArray.Add(-1);
            a_MkJSON.Add("ItemList", jArray);//배열을 넣음            
            m_SvStrJson_1 = a_MkJSON.ToString();
        }

        // 초기화
        a_MkJSON = new JSONObject();
        jArray = new JSONArray();

        // 아이템 수량
        for (int i = 0; i < m_Items.Length; i++)
        {
            if (m_Items[i] != null) jArray.Add(GetCurrentCount(i));
            else jArray.Add(-1);
            a_MkJSON.Add("ItemNumList", jArray);
            m_SvStrJson_2 = a_MkJSON.ToString();
        }
        StartCoroutine(SaveItem());
    }

    // 초기 세팅
    void InitSetting()
    {
        m_Items = new Item[m_MaxCapacity];
        SaveItemUrl = "http://jinone12.dothome.co.kr/SaveItem.php";
        m_InventoryUI.SetInventoryReference(this);
    }

    #region 인벤토리
    // 앞에서부터 비어있는 슬롯 인덱스 탐색
    public int FindEmptySlotIndex(int a_StartIndex = 0)
    {
        for(int i = a_StartIndex; i < m_Capacity; i++)
        {
            if (m_Items[i] == null)
                return i;
        }
        return -1;
    }

    // 앞에서부터 개수가 여유 있는 아이템 슬롯 탐색
    public int FindCountableItemSlotIndex(CountableItemData a_CI_Data , int a_StartIndex = 0)
    {
        if (a_CI_Data == null || a_StartIndex < 0 || a_StartIndex > m_Capacity)
            return -1;


        // 검사하려는 인덱스부터 시작
        for (int i = a_StartIndex; i < m_Capacity; i++)
        {
            // 아이템이 없다면 넘어간다.
            var a_CurData = m_Items[i];
            if (a_CurData == null)
                continue;

            // 아이템 종류 일치 및 개수 여유
            if(a_CurData.Data == a_CI_Data && a_CurData is CountableItem a_CI)
            {
                // 여유가 있다면 = fales
                // 여유가 없다면 = true
                if (a_CI.IsMax == false)
                {
                    return i;
                }             
            }
        }
        return -1;
    }

    // 모든 슬롯 접근 가능 여부 업데이트
    public void UpdateAccessibleStatesAll()
    {
        m_InventoryUI.SetAccessibleSlotRange(m_Capacity);
    }

    // 슬롯 추가
    public void AddSlot(int a_Index = 0)
    {
        if (m_Capacity >= m_MaxCapacity)
        {
            m_Capacity = m_MaxCapacity;
            return;
        }

        if (a_Index == 0) m_Capacity += 4;
        else m_Capacity = a_Index;
        m_InventoryUI.SetAccessibleSlotRange(m_Capacity);
        m_InventoryUI.UpdateSlotCountUI(m_Capacity, m_MaxCapacity);
        SlotSave();
    }

    // 해당 슬롯의 현재 아이템 개수 리턴
    public int GetCurrentCount(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return -1;      // 잘못된 인덱스
        if (m_Items[a_Index] == null) return 0;     // 빈 슬롯

        CountableItem m_CI = m_Items[a_Index] as CountableItem;

        // 장비 아이템인 경우
        if (m_CI == null)
            return 1;

        return m_CI.Count;
    }

    // 해당 슬롯의 아이템 정보 리턴
    public ItemData GetItemData(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return null;        // 잘못된 인덱스
        if (m_Items[a_Index] == null) return null;      // 빈 슬롯

        return m_Items[a_Index].Data;
    }

    // 해당 슬롯의 아이템 이름 리턴
    public string GetItemName(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return "";        // 잘못된 인덱스
        if (m_Items[a_Index] == null) return "";      // 빈 슬롯

        return m_Items[a_Index].Data.Name;
    }

    // 해당 슬롯의 정보 갱신
    public void UpdateSlot(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;             // 잘못된 인덱스

        Item a_Item = m_Items[a_Index];

        // 아이템이 슬롯에 존재하는 경우
        if(a_Item != null)
        {
            // 아이콘 등록
            m_InventoryUI.SetItemIcon(a_Index, a_Item.Data.Sprite,a_Item.Data.Grade);

            // 재료 , 포션 아이템
            if(a_Item is CountableItem a_CI)
            {
                // 수량이 0이라면 
                if(a_CI.IsEmpty)
                {
                    m_Items[a_Index] = null;
                    RemoveIcon(a_Index);
                    return;
                } 
                else m_InventoryUI.SetItemCountText(a_Index, a_CI.Count);
            }
            // 장비 아이템 
            else m_InventoryUI.HideItemCountText(a_Index);

            // 슬롯 필터 상태 업데이트
            m_InventoryUI.UpdateSlotFilterState(a_Index, a_Item.Data);
            SlotSave();
        }
        // 빈 슬롯
        else RemoveIcon(a_Index);
        SlotSave();
    }

    void UpdateSlot(int a_Temp_A, int a_Temp_B)
    {
        UpdateSlot(a_Temp_A);
        UpdateSlot(a_Temp_B);
    }

    public void UpdateAllSlot()
    {
        for(int i = 0; i < m_Capacity; i++)
            UpdateSlot(i);
    }

    // 아이템 제거
    public void Remove(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;

        m_Items[a_Index] = null;
        m_InventoryUI.RemoveItem(a_Index);
        SlotSave();
    }

    // 아이템 아이콘 제거
    void RemoveIcon(int a_Index)
    {
        m_InventoryUI.RemoveItem(a_Index);
        m_InventoryUI.HideItemCountText(a_Index);
        SlotSave();
    }

    public void Swap(int a_Index_A, int a_Index_B)
    {
        if (!IsValidIndex(a_Index_A)) return;
        if (!IsValidIndex(a_Index_B)) return;

        Item a_Item_A = m_Items[a_Index_A];
        Item a_Item_B = m_Items[a_Index_B];

        // 재료이거나 포션이며 같은 아이템일 경우
        if (a_Item_A != null && a_Item_B != null &&
           a_Item_A.Data == a_Item_B.Data && 
           a_Item_A is CountableItem a_CI_A && a_Item_B is CountableItem a_CI_B)
        {
            int a_MaxCount = a_CI_B.MaxCount;
            int a_Sum = a_CI_A.Count + a_CI_B.Count;

            if(a_Sum <= a_MaxCount)
            {
                a_CI_A.SetCount(0);
                a_CI_B.SetCount(a_Sum);
            }
            else
            {
                a_CI_A.SetCount(a_Sum - a_MaxCount);
                a_CI_B.SetCount(a_MaxCount);
            }
        }
        // 장비 아이템인 경우
        else
        {
            m_Items[a_Index_A] = a_Item_B;
            m_Items[a_Index_B] = a_Item_A;
        }

        // 슬롯 갱신
        UpdateSlot(a_Index_A, a_Index_B);
        SlotSave();
    }

    // 아이템 추가
    public int Add(ItemData a_ItemData, int a_Count = 1 , int a_SlotIndex = -1)
    {
        int a_Index;

        // 재료, 포션
        if(a_ItemData is CountableItemData a_CiData)
        {
            bool a_FindNextCountable = true;
            a_Index = -1;

            while(a_Count > 0)
            {
                // 이미 아이템이 존재한다면
                if(a_FindNextCountable && a_SlotIndex == -1)
                {
                    // 여유가 없을땐 -1 리턴
                    a_Index = FindCountableItemSlotIndex(a_CiData, a_Index + 1);

                    // 개수 여유가 없다면 빈 슬롯 검사
                    if (a_Index == -1) a_FindNextCountable = false;
                    else
                    {
                        CountableItem a_CI = m_Items[a_Index] as CountableItem;
                        a_Count = a_CI.AddCountAndGetExcess(a_Count);
                        UpdateSlot(a_Index);
                    }
                }
                // 빈 슬롯 탐색
                else
                {
                    a_Index = FindEmptySlotIndex(a_Index + 1);
                    if (a_SlotIndex != -1) a_Index = a_SlotIndex;

                    // 빈 슬롯조차 없는 경우 종료
                    if (a_Index == -1) break;
                    else
                    {
                        // 새 아이템 생성
                        CountableItem a_CI = a_CiData.CreateItem() as CountableItem;
                        a_CI.SetCount(a_Count);

                        // 슬롯에 추가
                        m_Items[a_Index] = a_CI;

                        // 남은 개수 계산
                        a_Count = (a_Count > a_CiData.MaxCount) ? (a_Count - a_CiData.MaxCount) : 0;

                        UpdateSlot(a_Index);
                    }
                }
            }
        }
        // 장비 아이템
        else
        {
            // 1개만 넣는 경우
            if(a_Count == 1)
            {
                a_Index = FindEmptySlotIndex();
                if (a_SlotIndex != -1) a_Index = a_SlotIndex;

                if (a_Index != -1)
                {
                    // 아이템을 생성하여 슬롯에 추가
                    m_Items[a_Index] = a_ItemData.CreateItem();
                    a_Count = 0;

                    UpdateSlot(a_Index);
                }
            }

            // 2개 이상의 아이템을 넣는 경우
            a_Index = -1;
            for(int Nullable =0; a_Count > 0; a_Count--)
            {
                // 아이템을 넣고난 다음에 인덱스를 검사
                a_Index = FindEmptySlotIndex(a_Index + 1);
                if (a_SlotIndex != -1) a_Index = a_SlotIndex;

                // 다 넣지 못한 경우는 루프 종료
                if (a_Index == -1) break;

                // 아이템을 생성하여 슬롯에 추가
                m_Items[a_Index] = a_ItemData.CreateItem();

                UpdateSlot(a_Index);
            }
        }
        SlotSave();
        return a_Count;
    }

    // 아이템 사용
    public void Use(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;
        if (m_Items[a_Index] == null) return;

        if(m_Items[a_Index] is IUsableItem a_uItem)
        {
            // 아이템 사용
            bool a_Succeeded = a_uItem.Use();

            if (a_Succeeded)
                UpdateSlot(a_Index);
        }
        else if(m_Items[a_Index] is IUsableItem == false)
        {
            m_InventoryUI.ItemEquipment(a_Index);
        }

        SlotSave();
    }

    // 빈 슬롯 앞에서부터 채우기
    public void TrimAll()
    {
        #region 빠른 배열 빈공간 채우기 알고리즘
        // 가장 빠른 배열 빈공간 채우기 알고리즘

        // i 커서와 j 커서
        // i 커서 : 가장 앞에 있는 빈칸을 찾는 커서
        // j 커서 : i 커서 위치에서부터 뒤로 이동하며 기존재 아이템을 찾는 커서

        // i커서가 빈칸을 찾으면 j 커서는 i+1 위치부터 탐색
        // j커서가 아이템을 찾으면 아이템을 옮기고, i 커서는 i+1 위치로 이동
        // j커서가 Capacity에 도달하면 루프 즉시 종료
        #endregion

        m_IndexSetForUpdate.Clear();

        int i = -1;
        while (m_Items[++i] != null);
        int j = i;

        while(true)
        {
            while (++j < m_Capacity && m_Items[j] == null) ;

            if (j == m_Capacity) break;

            m_IndexSetForUpdate.Add(i);
            m_IndexSetForUpdate.Add(j);

            m_Items[i] = m_Items[j];
            m_Items[j] = null;
            i++;
        }

        foreach(var a_Index in m_IndexSetForUpdate)
        {
            UpdateSlot(a_Index);
        }

        SlotSave();
    }

    // 빈 칸 없이 채우고 정렬
    public void SortAll()
    {
        // 우선 빈 칸 없이 채워준다.
        int i = -1;
        while (m_Items[++i] != null);
        int j = i;

        while (true)
        {
            while (++j < m_Capacity && m_Items[j] == null) ;

            if (j == m_Capacity) break;

            m_IndexSetForUpdate.Add(i);
            m_IndexSetForUpdate.Add(j);

            m_Items[i] = m_Items[j];
            m_Items[j] = null;
            i++;
        }

        Array.Sort(m_Items, 0, i, m_ItemComparer);

        UpdateAllSlot();
        m_InventoryUI.UpdateAllSlotFilters();
        SlotSave();
    }
    #endregion

    IEnumerator SaveItem()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        a_Form.AddField("ItemList", m_SvStrJson_1, System.Text.Encoding.UTF8);
        a_Form.AddField("ItemNumlist", m_SvStrJson_2, System.Text.Encoding.UTF8);
        a_Form.AddField("Slot_Num", m_Capacity);

        UnityWebRequest a_WWW = UnityWebRequest.Post(SaveItemUrl, a_Form);
        yield return a_WWW.SendWebRequest();

        if (a_WWW.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_WWW.downloadHandler.data);

            if (a_ReStr.Contains("UpdateSuccess") == true)
                Debug.Log("Success");;
        }
        else Debug.Log(a_WWW.error);
    }
}
