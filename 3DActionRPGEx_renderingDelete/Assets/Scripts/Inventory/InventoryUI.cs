using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

// UI의 상호작용과 레이캐스트를 담당

// 장점
// 슬롯 개수만큼의 오버헤드는 더이상 발생하지 않는다.
// 드래그 앤 드롭 관련 모든 이벤트의 중앙 관리가 가능해진다.        

enum FilterOption
{
    All,
    Weapon,
    Portion
}

enum PopOption
{
    Confirmation,
    Mount,
    Release
}

public class InventoryUI : MonoBehaviour
{
    Inventory m_Inventory;
    [SerializeField] QuickSlotGroup m_QuickSlotGroup = null;
    [SerializeField] UserInfo m_UserInfo = null;

    [Header("Slot UI")]
    [SerializeField] RectTransform m_Content = null;        // 슬롯이 생성 될 위치
    [SerializeField] GameObject m_SlotPrefab = null;        // 슬롯의 프리팹
    List<SlotUI> m_SlotList = new List<SlotUI>();           // 슬롯 리스트
    [SerializeField] int m_HorizontalSlotCount = 0;         // 슬롯 가로 개수
    [SerializeField] int m_VerticalSlotCount = 0;           // 슬롯 세로 개수
    [SerializeField] float m_SlotMargin = 8.0f;             // 슬롯의 상하좌우 여백
    [SerializeField] float m_ContentAreaPadding = 20f;      // 인벤토리 영역의 내부 여백
    [SerializeField] float m_SlotSize = 50f;                // 슬롯의 크기

    [SerializeField] Text m_CurSlotCount_Txt = null;                  // 현재 슬롯 개수 텍스트
    [SerializeField] Button m_AddSlot_Btn = null;                     // 슬롯 추가 버튼

    [Header("Drag and Drop")]
    GraphicRaycaster m_Gr;
    PointerEventData m_Data;
    List<RaycastResult> m_RayList;

    SlotUI m_BeginDragSlot;                                 // Drag를 시작한 슬롯
    Transform m_BeginDragIconTransform;                     // 슬롯의 아이콘 트랜스폼

    Vector3 m_BeginDragIconPoint;                           // 드래그 시작 시 슬롯의 위치
    Vector3 m_BeginDragCursorPoint;                         // 드래그 시작 시 커서의 위치
    int m_BeginDragSlotIndex;

    SlotUI m_PointerOverSlot;                               // 현재 포인터가 위치한 슬롯
    [SerializeField] ItemToolTip m_ItemToolTip = null;
    [SerializeField] ItemUseable m_ItemUseable = null;
    bool m_ShowTooltip = true;
    [SerializeField] InventoryPopupUI m_ItemPopup = null;
    [SerializeField] inventoryTester m_InvenTester = null;
    bool m_ShowPopup = true;

    [Header("Button")]
    [SerializeField] Button m_Trim_Btn = null;
    [SerializeField] Button m_Sort_Btn = null;
    [SerializeField] Button m_Exit_Btn = null;
    
    [Header("Filter Toggles")]
    [SerializeField] Toggle m_ToggleFilter_All = null;
    [SerializeField] Toggle m_ToggleFilter_Weapon = null;
    [SerializeField] Toggle m_ToggleFilter_Portion = null;
    FilterOption m_CurFilterOption = FilterOption.All;

    public void SetItemIcon(int a_Index, Sprite a_Icon, GradeType a_GradeType = GradeType.Null) => m_SlotList[a_Index].SetItem(a_Icon, a_GradeType);                 // 아이템 아이콘 등록
    public void SetItemCountText(int a_Index, int a_Count) => m_SlotList[a_Index].SetItemCount(a_Count);        // 아이템 개수 텍스트에 등록
    public void HideItemCountText(int a_Index) => m_SlotList[a_Index].SetItemCount(1);                          // 아이템 개수 텍스트 비 활성화
    public void RemoveItem(int a_Index) => m_SlotList[a_Index].RemoveItem();                                    // 아이템 제거
    bool IsOverUI() => EventSystem.current.IsPointerOverGameObject();                                           // 커서가 UI에 위치하고있는지 확인

    void Awake()
    {
        Init();
        InitSlot();
        InitSetButton();
        InitSetToggle();
    }

    void InitSetButton()
    {
        m_Trim_Btn.onClick.AddListener(() => m_Inventory.TrimAll());
        m_Sort_Btn.onClick.AddListener(() => m_Inventory.SortAll());
        m_Exit_Btn.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void Init()
    {
        if (gameObject.GetComponent<GraphicRaycaster>() == false)
            m_Gr.gameObject.AddComponent<GraphicRaycaster>();
        else
            m_Gr = GetComponent<GraphicRaycaster>();

        m_Data = new PointerEventData(EventSystem.current);
        m_RayList = new List<RaycastResult>(10);      
    }

    void InitSetToggle()
    {
        m_ToggleFilter_All.onValueChanged.AddListener(a_Flag => UpdateFilter(a_Flag, FilterOption.All));
        m_ToggleFilter_Weapon.onValueChanged.AddListener(a_Flag => UpdateFilter(a_Flag, FilterOption.Weapon));
        m_ToggleFilter_Portion.onValueChanged.AddListener(a_Flag => UpdateFilter(a_Flag, FilterOption.Portion));

        void UpdateFilter(bool a_Flag, FilterOption a_Option)
        {
            if (a_Flag)
            {
                m_CurFilterOption = a_Option;
                UpdateAllSlotFilters();
            }
        }
    }

    void InitSlot()
    {
        m_SlotPrefab.TryGetComponent(out RectTransform a_SlotRect);
        m_SlotPrefab.TryGetComponent(out SlotUI a_SlotUI_1);

        if (a_SlotRect == null) return;
        if (a_SlotUI_1 == null) return;

        a_SlotRect.sizeDelta = new Vector2(m_SlotSize, m_SlotSize); 

        m_SlotPrefab.SetActive(false);

        Vector2 a_BeginPos = new Vector2(m_ContentAreaPadding, -m_ContentAreaPadding);
        Vector2 a_CurPos = a_BeginPos;

        m_SlotList = new List<SlotUI>(m_VerticalSlotCount * m_HorizontalSlotCount);

        // 슬롯 생성
        for (int i = 0; i < m_VerticalSlotCount; i++)
        {
            for(int j = 0; j < m_HorizontalSlotCount; j++)
            {
                int a_SlotIndex = (m_HorizontalSlotCount * i) + j;

                var a_SlotRT = CloneSlot();
                a_SlotRT.pivot = new Vector2(0.0f, 1.0f);
                a_SlotRT.anchoredPosition = a_CurPos;
                a_SlotRT.gameObject.SetActive(true);
                a_SlotRT.gameObject.name = $"Slot {a_SlotIndex}";

                var a_SlotUI_2 = a_SlotRT.GetComponent<SlotUI>();
                a_SlotUI_2.SetSlotIndex(a_SlotIndex);
                m_SlotList.Add(a_SlotUI_2);

                a_CurPos.x += (m_SlotMargin + m_SlotSize);
            }
            a_CurPos.x = a_BeginPos.x;
            a_CurPos.y -= (m_SlotMargin + m_SlotSize);
        }
    }

    RectTransform CloneSlot()
    {
        GameObject a_SlotGo = Instantiate(m_SlotPrefab);
        RectTransform a_RT = a_SlotGo.GetComponent<RectTransform>();
        a_RT.SetParent(m_Content,false);

        return a_RT;
    }

    void Start()
    {
        m_AddSlot_Btn.onClick.AddListener(() => { m_Inventory.AddSlot(); });
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (m_Data == null)
            return;

        m_Data.position = Input.mousePosition;

        OnPointerEnterAndExit();
        if (m_ShowTooltip) ShowOrHideItemTooltip();
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();
    }

    public void UpdateAllSlotFilters()
    {
        int a_Capacity = m_Inventory.m_Capacity;

        for(int i = 0; i < a_Capacity; i++)
        {
            ItemData a_IData = m_Inventory.GetItemData(i);
            UpdateSlotFilterState(i,a_IData);
        }
    }

    public void UpdateSlotFilterState(int a_Index,ItemData a_IData)
    {
        bool a_IsFiltered = true;

        // null일 경우 타입 검사 없이 필터 활성화
        if (a_IData != null)
            switch (m_CurFilterOption)
            {
                case FilterOption.Weapon:
                    a_IsFiltered = (a_IData is WeaponItemData);
                    break;

                case FilterOption.Portion:
                    a_IsFiltered = (a_IData is PortionItemData);
                    break;              
            }
        m_SlotList[a_Index].SetItemAccessibleState(a_IsFiltered);
    }

    public void UpdateSlotCountUI(int a_Capacity, int a_MaxCapacity)
    {
        m_CurSlotCount_Txt.text = $"{a_Capacity.ToString()} / {a_MaxCapacity.ToString()}";
    }

    public void SetInventoryReference(Inventory a_Inventory)
    {
        m_Inventory = a_Inventory;
    }

    // 첫번째 슬롯의 정보를 가져온다.
    SlotUI RaycastAndGetFristComponet()
    {
        // 리스트 초기화
        m_RayList.Clear();

        // 포인터 위치로부터 Raycast 발생, 결과는 m_RayList 담긴다
        m_Gr.Raycast(m_Data, m_RayList);

        // 아무것도 없다면 리턴으로 null을 반환
        if (m_RayList.Count == 0)
            return null;

        if (m_RayList[0].gameObject.GetComponent<SlotUI>() == false)
            return null;

        // 있다면 SlotUI를 반환
        return m_RayList[0].gameObject.GetComponent<SlotUI>();
    }

    void OnPointerDown()
    {
        if(Input.GetMouseButtonDown(0))
        {
            m_BeginDragSlot = RaycastAndGetFristComponet();

            // 아이템을 갖고있는 슬롯만
            if (m_BeginDragSlot != null && m_BeginDragSlot.HasItem && m_BeginDragSlot.IsAccessible)
            {
                // 위치 기억, 참조 등록
                m_BeginDragIconTransform = m_BeginDragSlot.IconRect.transform;
                m_BeginDragIconPoint = m_BeginDragIconTransform.position;
                m_BeginDragCursorPoint = Input.mousePosition;

                // 맨 위에 보이기
                m_BeginDragSlotIndex = m_BeginDragSlot.transform.GetSiblingIndex();
                m_BeginDragSlot.transform.SetAsLastSibling();

                // 해당 슬롯의 하이라이트 이미지를 아이콘보다 뒤에 배치시키기
                m_BeginDragSlot.SetHighlightOnTop(false);
            }
            else
                m_BeginDragSlot = null;
        }
        else if(Input.GetMouseButtonDown(1))
        {
            SlotUI a_SlotUI = RaycastAndGetFristComponet();

            if (a_SlotUI == null || !a_SlotUI.HasItem || !a_SlotUI.IsAccessible) return;

            int a_Count = m_Inventory.GetCurrentCount(a_SlotUI.Index);
            string a_ItemName = m_Inventory.GetItemName(a_SlotUI.Index);
            ItemData a_ItemData = m_Inventory.GetItemData(a_SlotUI.Index);

            if (a_Count == 1 && m_ShowPopup == true && a_ItemData is CountableItemData a_PortionData == false)
                m_ItemPopup.OpenPopup(() => TryUseItem(a_SlotUI.Index), a_ItemName, (int)PopOption.Mount);
            else if (a_Count >= 1 && m_ShowPopup == true)
            {
                m_ItemUseable.OpenItemUsePop(() => TryUseItem(a_SlotUI.Index), m_Inventory.GetItemData(a_SlotUI.Index));
                m_ItemUseable.ActionSetting_a(() => TryQuickSlotAddItem(a_SlotUI.Index));
                m_ItemUseable.SetRectPosition(a_SlotUI.SlotRect);
            }
        }
    }

    void OnPointerDrag()
    {
        if (m_BeginDragSlot == null)
            return;

        if(Input.GetMouseButton(0))
            // 위치 이동
            m_BeginDragIconTransform.position = m_BeginDragIconPoint + (Input.mousePosition - m_BeginDragCursorPoint);

        // m_BeginDragCursorPoint -> 처음 마우스 클릭한 위치
        // Input.MouserPosition -> 이동중인 위치
    }

    void OnPointerUp()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if (m_BeginDragSlot == null)
                return;

            // 위치 복원
            m_BeginDragIconTransform.position = m_BeginDragIconPoint;

            // UI 순서 복원
            m_BeginDragSlot.transform.SetSiblingIndex(m_BeginDragSlotIndex);

            // Drag 완료
            EndDrag();

            // 해당 슬롯의 하이라이트 이미지를 아이콘보다 앞에 배치시키기
            m_BeginDragSlot.SetHighlightOnTop(true);

            // 참조 제거
            m_BeginDragSlot = null;
            m_BeginDragIconTransform = null;
        }
    }

    // 함수 종료 처리
    void EndDrag()
    {
        SlotUI a_EndDragSlot = RaycastAndGetFristComponet();

        if (a_EndDragSlot != null && a_EndDragSlot.IsAccessible)
        {
            TrySwapItems(m_BeginDragSlot, a_EndDragSlot);

            UpdateToolTipUI(a_EndDragSlot);
            return;
        }

        // 아이템 버리기
        if (IsOverUI() == false)
        {
            int a_Index = m_BeginDragSlot.Index;
            string a_ItemName = m_Inventory.GetItemName(a_Index);
            int a_Count = m_Inventory.GetCurrentCount(a_Index);

            // 재료,포션 아이템인 경우
            if (a_Count > 1)
                a_ItemName += " x" + a_Count;

            if (m_ShowPopup == true)
                m_ItemPopup.OpenPopup(() => TryRemoveItem(a_Index), a_ItemName, (int)PopOption.Confirmation);
            else
                TryRemoveItem(a_Index);
        }
    }

    // 인벤토리 -> 퀵 슬롯
    public void TryQuickSlotAddItem(int a_Index, int a_Count = 0)
    {
        Item m_Item_A = m_Inventory.m_Items[a_Index];
        int a_QuickIndex_A = -1;
        int a_QuickIndex_B = -1;

        // 재료,포션 아이템이라면
        if (m_Item_A is CountableItem a_CI_A)
        {

            a_QuickIndex_A = m_QuickSlotGroup.FindCountableItemSlotIndex(a_CI_A._CountableData, 0);
            a_QuickIndex_B = m_QuickSlotGroup.FindEmptySlotIndex(0);

            if (a_QuickIndex_A != -1)
            {
                Item m_Item_B = m_QuickSlotGroup.m_QuickItems[a_QuickIndex_A];

                if (m_Item_B is CountableItem a_CI_B)
                {
                    // 99
                    int a_MaxCount = a_CI_B.MaxCount;
                    int a_Sum = a_CI_B.Count + a_CI_A.Count;

                    if (a_Sum <= a_MaxCount)
                    {
                        a_CI_A.SetCount(0);
                        a_CI_B.SetCount(a_Sum);
                    }
                    else
                    {
                        a_CI_A.SetCount(a_Sum - a_MaxCount);
                        a_CI_B.SetCount(a_MaxCount);
                    }
                    m_QuickSlotGroup.UpdateSlot(a_QuickIndex_A);
                    m_Inventory.UpdateSlot(a_Index);
                }
            }
            else
            {
                if (a_QuickIndex_B == -1)
                    return;

                m_QuickSlotGroup.m_QuickItems[a_QuickIndex_B] = m_Item_A;
                m_QuickSlotGroup.UpdateSlot(a_QuickIndex_B);
                m_Inventory.Remove(a_Index);
            }

            m_Inventory.SlotSave();
            m_QuickSlotGroup.SlotSave();
        }
    }

    // 인벤토리 -> 장착
    public void ItemEquipment(int a_Index , int a_LoadIndex = 0)
    {
        Item a_Item = null;
        ItemData a_ItemData = null;
        if (a_LoadIndex == 0)
        {
            a_Item = m_Inventory.m_Items[a_Index];

            if (a_Item is WeaponItem a_CI_A && a_Item != null)
            {
                m_UserInfo.Equipment(a_CI_A._WeaponData, a_Index);
            }
            else
                return;
        }
        else if(a_LoadIndex == 1)
        {
            for (int i = 0; i < m_InvenTester._itemDataArray.Length; i++)
            {
                if (m_InvenTester._itemDataArray[i].ItemIndex == a_Index)
                {
                    a_ItemData = m_InvenTester._itemDataArray[i];

                    if (a_ItemData is WeaponItemData a_Items)
                    {
                        WeaponItem a_CI_A = a_Items.CreateItem() as WeaponItem;
                        m_UserInfo.LoadEquipment(a_CI_A._WeaponData, i);
                        return;
                    }
                    else return;
                }
            }
        }
    }

    public void UserInfoAdd(int a_Index , int a_LoadIndex = 0)
    {

    }

    // 장비 아이템 교환
    void TrySwapItems(SlotUI a_TempSlot_A, SlotUI a_TempSlot_B)
    {
        if (a_TempSlot_A == a_TempSlot_B)
            return;

        a_TempSlot_A.SwapOrMove(a_TempSlot_B);
        m_Inventory.Swap(a_TempSlot_A.Index, a_TempSlot_B.Index);
    }

    // 아이템 제거
    void TryRemoveItem(int a_Index)
    {
        m_Inventory.Remove(a_Index);
    }

    // 아이템 사용
    public void TryUseItem(int a_Index)
    {
        m_Inventory.Use(a_Index);
    }

    // 슬롯에 포인터가 올라가는 경우와 빠져나오는 경우
    void OnPointerEnterAndExit()
    {
        // 이전 프레임의 슬롯
        SlotUI a_PrevSlot = m_PointerOverSlot;

        // 현재 프레임의 슬롯
        m_PointerOverSlot = RaycastAndGetFristComponet();
        SlotUI a_CurSlot = m_PointerOverSlot;

        if (a_PrevSlot == null)
        {
            // 들어올 때
            if (a_CurSlot != null)
                OnCurrentEnter(a_CurSlot);
        }
        else
        {
            // 나갈 때
            if (a_CurSlot == null)
                OnPrevExit(a_PrevSlot);
            else if(a_PrevSlot != a_CurSlot)
            {
                OnPrevExit(a_PrevSlot);
                OnCurrentEnter(a_CurSlot);
            }
        }
    }

    void OnCurrentEnter(SlotUI a_Slot)
    {
        a_Slot.Highlight(true);
    }

    void OnPrevExit(SlotUI a_Slot)
    {
        a_Slot.Highlight(false);
    }

    // 접근 가능한 슬롯 범위 설정
    public void SetAccessibleSlotRange(int a_AccessibleSlotCount)
    {
        for (int i = 0; i < m_SlotList.Count; i++)
        {
            m_SlotList[i].SetSlotAccessibleState(i < a_AccessibleSlotCount);
        }
    }

    // 툴팁 활성화,비활성화
    void ShowOrHideItemTooltip()
    {
        // 마우스가 아이템 아이콘 위에 있다면
        bool a_IsValid = m_PointerOverSlot != null && m_PointerOverSlot.HasItem && m_PointerOverSlot.IsAccessible && (m_PointerOverSlot != m_BeginDragSlot);

        if (a_IsValid && m_ItemUseable.m_Itemuseable.activeSelf == false)
        {
            UpdateToolTipUI(m_PointerOverSlot);
            m_ItemToolTip.Show();
        }
        else
            m_ItemToolTip.Hide();
    }

    // 툴팁 UI의 슬롯 데이터 갱신
    void UpdateToolTipUI(SlotUI a_Slot)
    {
        if (m_ItemToolTip == null)
            return;

        // 툴팁 정보 갱신
        m_ItemToolTip.SetItemInfo(m_Inventory.GetItemData(a_Slot.Index));

        // 툴팁 위치 조정
        m_ItemToolTip.SetRectPosition(a_Slot.SlotRect);
    }
}
