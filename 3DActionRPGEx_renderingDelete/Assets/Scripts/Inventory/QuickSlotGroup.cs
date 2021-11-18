using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using SimpleJSON;

public class QuickSlotGroup : MonoBehaviour
{
    [SerializeField] Inventory m_Inventory;

    [Header("Drag and Drop")]
    GraphicRaycaster m_Gr;
    PointerEventData m_Data;
    List<RaycastResult> m_RayList;

    SlotUI m_BeginDragSlot;                                      // Drag를 시작한 슬롯
    QuickSkillSlot m_BeginDragSkillSlot;                         // Drag를 시작한 스킬 슬롯
    Transform m_BeginDragIconTransform;                          // 슬롯의 아이콘 트랜스폼
    Transform m_BeginSkillDragIconTransform;                     // 슬롯의 아이콘 트랜스폼

    Vector3 m_BeginSkillDragIconPoint;                           // 드래그 시작 시 슬롯의 위치
    Vector3 m_BeginSkillDragCursorPoint;                         // 드래그 시작 시 커서의 위치

    Vector3 m_BeginDragIconPoint;                                // 드래그 시작 시 슬롯의 위치
    Vector3 m_BeginDragCursorPoint;                              // 드래그 시작 시 커서의 위치
    int m_BeginDragSlotIndex;

    SlotUI m_PointerOverSlot;                                    // 현재 포인터가 위치한 슬롯

    [Header("QuickSlotItem")]
    List<SlotUI> m_QuickSlotList = new List<SlotUI>();
    [SerializeField] SlotUI[] m_QuickGroup = null;
    [HideInInspector] public Item[] m_QuickItems;
    [SerializeField] InventoryPopupUI m_ItemPopup = null;
    [SerializeField] ItemUseable m_ItemUseable = null;

    // 스킬 퀵 슬롯
    [Header("QuickSlotSkill")]    
    [SerializeField] QuickSkillSlot[] m_QuickSkillGroup = null;
    [HideInInspector] public Skill[] m_QuickSkills = new Skill[4];
    List<QuickSkillSlot> m_QuickSkillList = new List<QuickSkillSlot>();

    [SerializeField] Hero_Ctrl m_RefHero = null;
    bool m_RefSkillOn = false;
    float m_Delay = 0.0f;

    string m_SvStrJson_1 = "";
    string m_SvStrJson_2 = "";
    string m_SvStrJson_3 = "";
    string SaveQuickItemUrl = "";

    bool IsValidIndex(int a_Index) => a_Index >= 0 && a_Index < m_QuickGroup.Length;
    public void SetSkillIcon(int a_Index, Sprite a_Icon) => m_QuickSkillList[a_Index].SetSkill(a_Icon);              // 스킬 아이콘 등록
    public void RemoveSkill(int a_Index) => m_QuickSkillList[a_Index].RemoveItem();                                  // 아이템 제거
    public void SetItemIcon(int a_Index, Sprite a_Icon, GradeType a_GradeType = GradeType.Null) => m_QuickSlotList[a_Index].SetItem(a_Icon,a_GradeType);                 // 아이템 아이콘 등록
    public void SetItemCountText(int a_Index, int a_Count) => m_QuickSlotList[a_Index].SetItemCount(a_Count);        // 아이템 개수 텍스트에 등록
    public void HideItemCountText(int a_Index) => m_QuickSlotList[a_Index].SetItemCount(1);                          // 아이템 개수 텍스트 비 활성화
    public void RemoveItem(int a_Index) => m_QuickSlotList[a_Index].RemoveItem();                                    // 아이템 제거
    bool IsOverUI() => EventSystem.current.IsPointerOverGameObject();

    void Awake()
    {
        Init();
        InitSetting();
    }

    void Update()
    {
        m_Data.position = Input.mousePosition;

        // 스킬 딜레이
        if(m_RefSkillOn == true)
        {
            m_Delay -= Time.deltaTime;

            if(m_Delay <= 0.0f)
            {
                m_RefSkillOn = false;
                m_Delay = 0.0f;
                m_RefHero.Event_SkillDamage(a_RefActiveSkill);
            }
        }

        // 아이템 관련
        OnPointerEnterAndExit();
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();

        // 스킬 관련
        OnSPointerDown();
        OnSPointerDrag();
        OnSPointerUp();

        // 퀵 슬롯 키
        InputQuickSlotKey();
    }

    void Init()
    {
        if (gameObject.GetComponent<GraphicRaycaster>() == false)
            m_Gr.gameObject.AddComponent<GraphicRaycaster>();
        else
            m_Gr = GetComponent<GraphicRaycaster>();

        m_Data = new PointerEventData(EventSystem.current);
        m_RayList = new List<RaycastResult>(10);
        m_QuickItems = new Item[m_QuickGroup.Length];
        m_QuickSkills = new Skill[m_QuickSkillGroup.Length];    // 4개 생성
    }

    void InitSetting()
    {
        for(int i = 0; i < m_QuickGroup.Length; i++)
        {
            m_QuickGroup[i].SetSlotIndex(i);
            m_QuickGroup[i].SetItemAccessibleState(true);
            m_QuickSlotList.Add(m_QuickGroup[i]);
        }

        for(int i = 0; i < m_QuickSkillGroup.Length; i++)
        {
            m_QuickSkillGroup[i].SetSlotIndex(i);
            m_QuickSkillList.Add(m_QuickSkillGroup[i]);
        }

        SaveQuickItemUrl = "http://jinone12.dothome.co.kr/SaveQuickItem.php";
    }

    // 첫번째 슬롯의 정보를 가져온다.
    T RaycastAndGetFristComponet<T>() where T : Component
    {
        // 리스트 초기화
        m_RayList.Clear();

        // 포인터 위치로부터 Raycast 발생, 결과는 m_RayList 담긴다
        m_Gr.Raycast(m_Data, m_RayList);

        // 아무것도 없다면 리턴으로 null을 반환
        if (m_RayList.Count == 0)
            return null;

        // 있다면 SlotUI를 반환
        return m_RayList[0].gameObject.GetComponent<T>();
    }

    void InputQuickSlotKey()
    {
        var a_KeyCode = Input.inputString;

        if (a_KeyCode == "" || a_KeyCode == null) return;
        if (!string.IsNullOrEmpty(a_KeyCode))
        {
            switch (a_KeyCode.ToUpper())
            {
                case "1":
                    TryUseItem(0);
                    break;
                case "2":
                    TryUseItem(1);
                    break;
                case "3":
                    TryUseItem(2);
                    break;
                case "4":
                    TryUseItem(3);
                    break;
                case "Z":
                    TryUseSkill(0);
                    break;
                case "X":
                    TryUseSkill(1);
                    break;
                case "C":
                    TryUseSkill(2);
                    break;
                case "V":
                    TryUseSkill(3);
                    break;
                default:
                    break;
            }
        }
    }

    #region 아이템 관련 드래그 드롭
    void OnPointerDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_BeginDragSlot = RaycastAndGetFristComponet<SlotUI>();

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
        else if (Input.GetMouseButtonDown(1))
        {
            SlotUI a_SlotUI = RaycastAndGetFristComponet<SlotUI>();

            if (a_SlotUI == null || !a_SlotUI.HasItem || !a_SlotUI.IsAccessible) return;

            int a_Count = GetCurrentCount(a_SlotUI.Index);
            string a_ItemName = GetItemName(a_SlotUI.Index);

            if (a_Count > 1)
            {
                m_ItemUseable.OpenItemUsePop(() => TryUseItem(a_SlotUI.Index), GetItemData(a_SlotUI.Index));
                m_ItemUseable.ActionSetting_b(() => TryInvenSlotAddItem(a_SlotUI.Index));
                m_ItemUseable.SetRectPosition(a_SlotUI.SlotRect);
            }
        }
    }

    void OnPointerDrag()
    {
        if (m_BeginDragSlot == null)
            return;

        if (Input.GetMouseButton(0))
            // 위치 이동
            m_BeginDragIconTransform.position = m_BeginDragIconPoint + (Input.mousePosition - m_BeginDragCursorPoint);
    }

    void OnPointerUp()
    {
        if (Input.GetMouseButtonUp(0))
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
        SlotUI a_EndDragSlot = RaycastAndGetFristComponet<SlotUI>();

        if (a_EndDragSlot != null && a_EndDragSlot.IsAccessible)
        {
            TrySwapItems(m_BeginDragSlot, a_EndDragSlot);

            return;
        }

        // 아이템 버리기
        if (IsOverUI() == false)
        {
            int a_Index = m_BeginDragSlot.Index;
            string a_ItemName = GetItemName(a_Index);
            int a_Count = GetCurrentCount(a_Index);

            // 재료,포션 아이템인 경우
            if (a_Count > 1)
                a_ItemName += " x" + a_Count;

             m_ItemPopup.OpenPopup(() => Remove(a_Index), a_ItemName, (int)PopOption.Confirmation);
        }
    }
    #endregion

    #region 스킬 관련 드래그 드롭
    void OnSPointerDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_BeginDragSkillSlot = RaycastAndGetFristComponet<QuickSkillSlot>();

            // 아이템을 갖고있는 슬롯만
            if (m_BeginDragSkillSlot != null && m_BeginDragSkillSlot.HasSkill)
            {
                // 위치 기억, 참조 등록
                m_BeginSkillDragIconTransform = m_BeginDragSkillSlot.IconRect.transform;
                m_BeginSkillDragIconPoint = m_BeginSkillDragIconTransform.position;
                m_BeginSkillDragCursorPoint = Input.mousePosition;

                // 맨 위에 보이기
                m_BeginDragSlotIndex = m_BeginDragSkillSlot.transform.GetSiblingIndex();
                m_BeginDragSkillSlot.transform.SetAsLastSibling();

            }
            else
                m_BeginDragSkillSlot = null;
        }
    }

    void OnSPointerDrag()
    {
        if (m_BeginDragSkillSlot == null)
            return;

        if (Input.GetMouseButton(0))
            // 위치 이동
            m_BeginSkillDragIconTransform.position = m_BeginSkillDragIconPoint + (Input.mousePosition - m_BeginSkillDragCursorPoint);
    }

    void OnSPointerUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (m_BeginDragSkillSlot == null)
                return;

            // 위치 복원
            m_BeginSkillDragIconTransform.position = m_BeginSkillDragIconPoint;

            // UI 순서 복원
            m_BeginDragSkillSlot.transform.SetSiblingIndex(m_BeginDragSlotIndex);

            // Drag 완료
            EndSDrag();

            // 참조 제거
            m_BeginDragSkillSlot = null;
            m_BeginSkillDragIconTransform = null;
        }
    }

    // 함수 종료 처리
    void EndSDrag()
    {
        QuickSkillSlot a_EndDragSlot = RaycastAndGetFristComponet<QuickSkillSlot>();

        if (a_EndDragSlot != null)
        {
            TrySwapSkill(m_BeginDragSkillSlot, a_EndDragSlot);
            return;
        }

        // 아이템 버리기
        if (IsOverUI() == false)
        {
            int a_Index = m_BeginDragSkillSlot.Index;
            SkillRemove(a_Index);
        }
    }
    #endregion

    // 퀵 슬롯 -> 인벤토리
    public void TryInvenSlotAddItem(int a_Index, int a_Count = 0)
    {
        Item m_Item_A = m_QuickItems[a_Index];
        int a_QuickIndex_A = -1;
        int a_QuickIndex_B = -1;

        if (m_Item_A is CountableItem a_CI_A)
        {

            a_QuickIndex_A = m_Inventory.FindCountableItemSlotIndex(a_CI_A._CountableData, 0);
            a_QuickIndex_B = m_Inventory.FindEmptySlotIndex();

            if (a_QuickIndex_A != -1)
            {
                Item m_Item_B = m_Inventory.m_Items[a_QuickIndex_A];

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

                    m_Inventory.UpdateSlot(a_QuickIndex_A);
                    UpdateSlot(a_Index);
                }
            }
            else
            {
                m_Inventory.m_Items[a_QuickIndex_B] = m_Item_A;
                m_Inventory.UpdateSlot(a_QuickIndex_B);
                Remove(a_Index);
            }
        }
    }

    // 아이템 추가
    public int Add(ItemData a_ItemData, int a_Count = 1, int a_SlotIndex = -1)
    {
        int a_Index;
        // 재료, 포션
        if (a_ItemData is CountableItemData a_CiData)
        {
            bool a_FindNextCountable = true;
            a_Index = -1;

            while (a_Count > 0)
            {
                // 이미 아이템이 존재한다면
                if (a_FindNextCountable && a_SlotIndex == -1)
                {
                    // 여유가 없을땐 -1 리턴
                    a_Index = FindCountableItemSlotIndex(a_CiData, a_Index + 1);

                    // 개수 여유가 없다면 빈 슬롯 검사
                    if (a_Index == -1) a_FindNextCountable = false;
                    else
                    {
                        CountableItem a_CI = m_QuickItems[a_Index] as CountableItem;
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
                        m_QuickItems[a_Index] = a_CI;

                        // 남은 개수 계산
                        a_Count = (a_Count > a_CiData.MaxCount) ? (a_Count - a_CiData.MaxCount) : 0;

                        UpdateSlot(a_Index);
                    }
                }
            }
        }
        return a_Count;
    }

    // 앞에서부터 개수가 여유 있는 아이템 슬롯 탐색
    public int FindCountableItemSlotIndex(CountableItemData a_CI_Data, int a_StartIndex = 0)
    {
        if (a_CI_Data == null || a_StartIndex < 0 || a_StartIndex > m_QuickItems.Length)
            return -1;

        // 검사하려는 인덱스부터 시작
        for (int i = a_StartIndex; i < m_QuickGroup.Length; i++)
        {
            // 아이템이 없다면 넘어간다.
            var a_CurData = m_QuickItems[i];
            if (a_CurData == null)
                continue;

            // 아이템 종류 일치 및 개수 여유
            if (a_CurData.Data == a_CI_Data && a_CurData is CountableItem a_CI)
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

    // 앞에서부터 비어있는 슬롯 인덱스 탐색
    public int FindEmptySlotIndex(int a_StartIndex = 0, int a_Type = 0)
    {
        // 아이템
        if (a_Type == 0)
        {
            for (int i = a_StartIndex; i < m_QuickGroup.Length; i++)
            {
                if (m_QuickItems[i] == null)
                    return i;
            }
            return -1;
        }
        // 스킬
        else if(a_Type == 1)
        {
            for(int i = a_StartIndex; i < m_QuickSkillGroup.Length; i++)
            {
                if (m_QuickSkills[i] == null) return i;
            }
        }
        return -1;
    }

    // 아이템 사용
    void TryUseItem(int a_Index)
    {
        Use(a_Index);
    }

    void TryUseSkill(int a_Index)
    {
        SkillUse(a_Index);
    }

    // 아이템 사용
    public void Use(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;
        if (m_QuickItems[a_Index] == null) return;

        if (m_QuickItems[a_Index] is IUsableItem a_uItem)
        {
            // 아이템 사용
            bool a_Succeeded = a_uItem.Use();

            if (a_Succeeded)
                UpdateSlot(a_Index);
        }

        SlotSave();
    }

    ActiveSkill a_RefActiveSkill = null;
    // 스킬 사용
    public void SkillUse(int a_Index)
    {
        if (m_QuickSkills[a_Index] == null) return;
        if (m_QuickSkills[a_Index] is IUsableSkill a_uSkill)
        {
            // 스킬 사용
            bool a_Succeeded = a_uSkill.Skill();

            if(a_Succeeded)
            {
                if (m_QuickSkills[a_Index] is ActiveSkill a_ActiveSkill)
                {
                    // 이펙트 연출
                    a_ActiveSkill.UseSkill();
                    // 실제 데미지 계산 부분
                    m_RefHero.SkillOn();
                    m_RefHero.SetColliderCenter(a_ActiveSkill.SkCollider_Center_X, a_ActiveSkill.SkCollider_Center_Y, a_ActiveSkill.SkCollider_Center_Z);
                    m_RefHero.SetColliderSize(a_ActiveSkill.SkCollider_Size_X, a_ActiveSkill.SkCollider_Size_Y, a_ActiveSkill.SkCollider_Size_Z);
                    // SkillOff()를 어디서 처리할 것인가
                    m_RefSkillOn = m_RefHero.TrySkill();
                    m_Delay = a_ActiveSkill.AttackDelay;
                    a_RefActiveSkill = a_ActiveSkill;
                }
                else if (m_QuickSkills[a_Index] is BuffSkill a_BuffSkill)
                    a_BuffSkill.UseSkill();                
            }                
        }

        
    }

    // 슬롯에 포인터가 올라가는 경우와 빠져나오는 경우
    void OnPointerEnterAndExit()
    {
        // 이전 프레임의 슬롯
        SlotUI a_PrevSlot = m_PointerOverSlot;

        // 현재 프레임의 슬롯
        m_PointerOverSlot = RaycastAndGetFristComponet<SlotUI>();
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
            else if (a_PrevSlot != a_CurSlot)
            {
                OnPrevExit(a_PrevSlot);
                OnCurrentEnter(a_CurSlot);
            }
        }
    }

    // 장비 아이템 교환
    void TrySwapItems(SlotUI a_TempSlot_A, SlotUI a_TempSlot_B)
    {
        if (a_TempSlot_A == a_TempSlot_B)
            return;

        a_TempSlot_A.SwapOrMove(a_TempSlot_B);
        Swap(a_TempSlot_A.Index, a_TempSlot_B.Index);
    }

    void Swap(int a_Index_A, int a_Index_B)
    {
        if (!IsValidIndex(a_Index_A)) return;
        if (!IsValidIndex(a_Index_B)) return;

        Item a_Item_A = m_QuickItems[a_Index_A];
        Item a_Item_B = m_QuickItems[a_Index_B];

        // 재료이거나 포션이며 같은 아이템일 경우
        if (a_Item_A != null && a_Item_B != null &&
           a_Item_A.Data == a_Item_B.Data &&
           a_Item_A is CountableItem a_CI_A && a_Item_B is CountableItem a_CI_B)
        {

            int a_MaxCount = a_CI_B.MaxCount;
            int a_Sum = a_CI_A.Count + a_CI_B.Count;

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
        }
        // 슬롯 교체
        else
        {
            m_QuickItems[a_Index_A] = a_Item_B;
            m_QuickItems[a_Index_B] = a_Item_A;
        }

        // 슬롯 갱신
        UpdateSlot(a_Index_A, a_Index_B);
        SlotSave();
    }

    // 해당 슬롯의 정보 갱신
    public void UpdateSlot(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;             // 잘못된 인덱스

        Item a_Item = m_QuickItems[a_Index];

        // 아이템이 슬롯에 존재하는 경우
        if (a_Item != null)
        {
            // 아이콘 등록
            SetItemIcon(a_Index, a_Item.Data.Sprite,a_Item.Data.Grade);

            // 재료 , 포션 아이템
            if (a_Item is CountableItem a_CI)
            {
                // 수량이 0이라면 
                if (a_CI.IsEmpty)
                {
                    m_QuickItems[a_Index] = null;
                    RemoveIcon(a_Index);
                    return;
                }
                else SetItemCountText(a_Index, a_CI.Count);
            }
            // 장비 아이템
            else HideItemCountText(a_Index);
        }
        // 빈 슬롯
        else RemoveIcon(a_Index);
    }

    void Remove(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;

        m_QuickItems[a_Index] = null;
        RemoveItem(a_Index);
        SlotSave();
    }

    void UpdateSlot(int a_Temp_A, int a_Temp_B)
    {
        UpdateSlot(a_Temp_A);
        UpdateSlot(a_Temp_B);
    }

    void UpdateAllSlot(int a_Capacity)
    {
        for (int i = 0; i < a_Capacity; i++)
            UpdateSlot(i);
    }

    // 아이템 아이콘 제거
    void RemoveIcon(int a_Index)
    {
        RemoveItem(a_Index);
        HideItemCountText(a_Index);
        SlotSave();
    }

    void OnCurrentEnter(SlotUI a_Slot)
    {
        a_Slot.Highlight(true);
    }

    void OnPrevExit(SlotUI a_Slot)
    {
        a_Slot.Highlight(false);
    }

    // 해당 슬롯의 현재 아이템 개수 리턴
    public int GetCurrentCount(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return -1;      // 잘못된 인덱스
        if (m_QuickItems[a_Index] == null) return 0;     // 빈 슬롯

        CountableItem m_CI = m_QuickItems[a_Index] as CountableItem;

        // 장비 아이템인 경우
        if (m_CI == null)
            return 1;

        return m_CI.Count;
    }

    // 해당 슬롯의 아이템 정보 리턴
    public ItemData GetItemData(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return null;        // 잘못된 인덱스
        if (m_QuickItems[a_Index] == null) return null;      // 빈 슬롯

        return m_QuickItems[a_Index].Data;
    }

    // 해당 슬롯의 아이템 이름 리턴
    public string GetItemName(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return "";        // 잘못된 인덱스
        if (m_QuickItems[a_Index] == null) return "";      // 빈 슬롯

        return m_QuickItems[a_Index].Data.Name;
    }

    // 스킬 위치 교환
    void TrySwapSkill(QuickSkillSlot a_TempSlot_A, QuickSkillSlot a_TempSlot_B)
    {
        if (a_TempSlot_A == a_TempSlot_B)
            return;

        a_TempSlot_A.SwapOrMove(a_TempSlot_B);
        SkillSwap(a_TempSlot_A.Index, a_TempSlot_B.Index);
    }

    void SkillSwap(int a_Index_A, int a_Index_B)
    {
        if (!IsValidIndex(a_Index_A)) return;
        if (!IsValidIndex(a_Index_B)) return;

        Skill a_Skill_A = m_QuickSkills[a_Index_A];
        Skill a_Skill_B = m_QuickSkills[a_Index_B];

        m_QuickSkills[a_Index_A] = a_Skill_B;
        m_QuickSkills[a_Index_B] = a_Skill_A;

        // 슬롯 갱신
        UpdateSkillSlot(a_Index_A, a_Index_B);
        SlotSave();
    }

    // 해당 스킬 슬롯의 정보 갱신
    public void UpdateSkillSlot(int a_Index)
    {
        Skill a_Skill = m_QuickSkills[a_Index];

        // 아이템이 슬롯에 존재하는 경우
        if (a_Skill != null)
        {
            // 아이콘 등록
            SetSkillIcon(a_Index, a_Skill.Sprite);
        }
        // 빈 슬롯
        else
        {
            RemoveSkillIcon(a_Index);
        }
    }

    void UpdateSkillSlot(int a_Index_A, int a_Index_B)
    {
        UpdateSkillSlot(a_Index_A);
        UpdateSkillSlot(a_Index_B);
    }

    void SkillRemove(int a_Index)
    {
        m_QuickSkills[a_Index] = null;
        RemoveSkill(a_Index);
        SlotSave();
    }

    // 아이템 아이콘 제거
    void RemoveSkillIcon(int a_Index)
    {
        RemoveSkill(a_Index);
        SlotSave();
    }

    public void SlotSave()
    {      
        // 아이템 종류
        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray();//배열이 필요할때
        for (int i = 0; i < m_QuickItems.Length; i++)
        {
            if (m_QuickItems[i] != null) jArray.Add(m_QuickItems[i].Data.ItemIndex);
            else jArray.Add(-1);
            a_MkJSON.Add("Quick_Item", jArray);//배열을 넣음            
            m_SvStrJson_1 = a_MkJSON.ToString();
        }

        // 초기화
        a_MkJSON = new JSONObject();
        jArray = new JSONArray();

        // 아이템 수량
        for (int i = 0; i < m_QuickItems.Length; i++)
        {
            if (m_QuickItems[i] != null) jArray.Add(GetCurrentCount(i));
            else jArray.Add(-1);
            a_MkJSON.Add("Quick_Item_Num", jArray);
            m_SvStrJson_2 = a_MkJSON.ToString();
        }

        // 초기화
        a_MkJSON = new JSONObject();
        jArray = new JSONArray();

        // 스킬 종류
        for (int i = 0; i < m_QuickSkills.Length; i++)
        {
            if (m_QuickSkills[i] != null) jArray.Add(m_QuickSkills[i].SkillIndex);
            else jArray.Add(-1);
            a_MkJSON.Add("Quick_Skill",jArray);
            m_SvStrJson_3 = a_MkJSON.ToString();
        }
        StartCoroutine(SaveQuickItem());
    }


    IEnumerator SaveQuickItem()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        a_Form.AddField("Quick_Item", m_SvStrJson_1, System.Text.Encoding.UTF8);
        a_Form.AddField("Quick_Item_Num", m_SvStrJson_2, System.Text.Encoding.UTF8);
        a_Form.AddField("Quick_Skill", m_SvStrJson_3, System.Text.Encoding.UTF8);


        UnityWebRequest a_WWW = UnityWebRequest.Post(SaveQuickItemUrl, a_Form);
        yield return a_WWW.SendWebRequest();

        if (a_WWW.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_WWW.downloadHandler.data);

            if (a_ReStr.Contains("UpdateSuccess") == true)
                Debug.Log("Success"); ;
        }
        else Debug.Log(a_WWW.error);
    }
}
