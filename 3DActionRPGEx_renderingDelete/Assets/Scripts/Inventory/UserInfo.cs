using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleJSON;
using UnityEngine.Networking;

public class UserInfo : MonoBehaviour
{
    [SerializeField] Inventory m_Inventory;

    [Header("UserInfo UI")]
    [SerializeField] Text m_UserLevelID_Txt = null;
    [SerializeField] Button m_UserOff_Btn = null;
    [SerializeField] Text m_UserStat_Txt = null;
    [SerializeField] Image m_User_Img = null;

    [Header("Equipment Slot")]
    [Tooltip("0 = 무기 1 = 방어구 2 = 투구 3 = 장갑 4 = 신발 5 = 반지 6 = 목걸이")]
    [SerializeField] SlotUI[] m_UsSlotGroup = null;

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

    Item[] m_UsItems;
    List<SlotUI> m_UsSlotList = new List<SlotUI>();

    [Header("Item External Elements")]
    [SerializeField] InventoryPopupUI m_ItemPopup = null;
    [SerializeField] ItemToolTip m_ItemToolTip = null;
    [SerializeField] inventoryTester _inventoryTester = null;

    string m_SvStrJson = "";
    string SaveUserItemUrl = "";

    bool IsValidIndex(int a_Index) => a_Index >= 0 && a_Index < m_UsSlotGroup.Length;
    bool HasItem(int a_Index) => IsValidIndex(a_Index) && m_UsItems[a_Index] != null;
    bool IsOverUI() => EventSystem.current.IsPointerOverGameObject();
    public void RemoveItem(int a_Index) => m_UsSlotList[a_Index].RemoveItem();
    public void SetItemIcon(int a_Index, Sprite a_Icon, GradeType a_GradeType = GradeType.Null) => m_UsSlotList[a_Index].SetItem(a_Icon,a_GradeType);

    void Awake()
    {
        Init();
        InitSetting();
    }

    void Init()
    {
        if (gameObject.GetComponent<GraphicRaycaster>() == false)
            m_Gr.gameObject.AddComponent<GraphicRaycaster>();
        else
            m_Gr = GetComponent<GraphicRaycaster>();

        m_Data = new PointerEventData(EventSystem.current);
        m_RayList = new List<RaycastResult>(10);
        m_UsItems = new Item[m_UsSlotGroup.Length];
        SaveUserItemUrl = "http://jinone12.dothome.co.kr/SaveUserItem.php";
    }

    void InitSetting()
    {
        for (int i = 0; i < m_UsSlotGroup.Length; i++)
        {
            m_UsSlotGroup[i].SetSlotIndex(i);
            m_UsSlotGroup[i].SetItemAccessibleState(true);
            m_UsSlotList.Add(m_UsSlotGroup[i]);
        }
    }

    void Start()
    {
        if (m_UserOff_Btn != null) m_UserOff_Btn.onClick.AddListener(() => gameObject.SetActive(false));
        gameObject.SetActive(false);
    }

    void Update()
    {
        m_Data.position = Input.mousePosition;

        OnPointerEnterAndExit();
        OnPointerDown();
        OnPointerDrag();
        OnPointerUp();
        if(m_ItemPopup.m_InventoryPopupUI.activeSelf == false)ShowOrHideItemTooltip();
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
        if (Input.GetMouseButtonDown(0))
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
        else if (Input.GetMouseButtonDown(1))
        {
            SlotUI a_SlotUI = RaycastAndGetFristComponet();

            if (a_SlotUI == null || !a_SlotUI.HasItem || !a_SlotUI.IsAccessible) return;

            string a_ItemName = GetItemName(a_SlotUI.Index);

            if(a_ItemName != "" || a_ItemName != null)
                m_ItemPopup.OpenPopup(() => ItemRelease(a_SlotUI.Index), a_ItemName, (int)PopOption.Release);
            else
                return;
        }
    }

    void OnPointerDrag()
    {
        if (m_BeginDragSlot == null)
            return;

        if (Input.GetMouseButton(0))
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
            else if (a_PrevSlot != a_CurSlot)
            {
                OnPrevExit(a_PrevSlot);
                OnCurrentEnter(a_CurSlot);
            }
        }
    }

    // 함수 종료 처리
    void EndDrag()
    {
        SlotUI a_EndDragSlot = RaycastAndGetFristComponet();

        // 아이템 버리기
        if (IsOverUI() == false)
        {
            int a_Index = m_BeginDragSlot.Index;
            string a_ItemName = GetItemName(a_Index);

            m_ItemPopup.OpenPopup(() => Remove(a_Index), a_ItemName, (int)PopOption.Confirmation);
        }
    }

    // 하이라이트 이미지 OnOff
    void OnCurrentEnter(SlotUI a_Slot)
    {
        a_Slot.Highlight(true);
    }

    void OnPrevExit(SlotUI a_Slot)
    {
        a_Slot.Highlight(false);
    }

    // 0 = 무기 1 = 방어구 2 = 투구 3 = 장갑 4 = 신발 5 = 반지 6 = 목걸이
    public void Equipment(WeaponItemData a_WeaponData, int a_Index)
    {
        int a_FindIndex_A = FindWeaponItemSlotIndex(a_WeaponData);
        Item m_Item_B = m_Inventory.m_Items[a_Index];

        Debug.Log(a_FindIndex_A);

        // 아이템이 이미 존재한다면
        if (m_UsSlotGroup[a_FindIndex_A].HasItem)
        {
            Swap(a_FindIndex_A, a_Index);
        }
        else
        {
            m_UsItems[a_FindIndex_A] = m_Item_B;
            m_Inventory.Remove(a_Index);
            m_Inventory.UpdateSlot(a_Index);
            UpdateSlot(a_FindIndex_A);
        }
    }

    // 0 = 무기 1 = 방어구 2 = 투구 3 = 장갑 4 = 신발 5 = 반지 6 = 목걸이
    public void LoadEquipment(WeaponItemData a_WeaponData, int a_Index)
    {
        int a_FindIndex_A = FindWeaponItemSlotIndex(a_WeaponData);

        m_UsItems[a_FindIndex_A] = _inventoryTester._itemDataArray[a_Index].CreateItem();
        UpdateSlot(a_FindIndex_A);
    }

    // 장착 -> 인벤토리
    void ItemRelease(int a_Index)
    {
        Item a_Item = m_UsItems[a_Index];
        int a_FindIndex = m_Inventory.FindEmptySlotIndex();

        m_Inventory.m_Items[a_FindIndex] = a_Item;

        m_Inventory.UpdateSlot(a_FindIndex);
        Remove(a_Index);
        SlotSave();
    }

    // 앞에서부터 타입이 맞는 슬롯 검색
    public int FindWeaponItemSlotIndex(WeaponItemData a_WeaponData)
    {
        if (a_WeaponData == null)
            return -1;

        switch(a_WeaponData.WeaponType)
        {
            case WeaponType.Sword: return 0;
            case WeaponType.Armor: return 1;
            case WeaponType.Helmat: return 2;
            case WeaponType.Gloves: return 3;
            case WeaponType.Shoes: return 4;
            case WeaponType.Ring: return 5;
            case WeaponType.Necklace: return 6;
            default: return -1;
        }
    }

    public void Swap(int a_Index_A, int a_Index_B)
    {
        if (!IsValidIndex(a_Index_A)) return;
        if (!m_Inventory.IsValidIndex(a_Index_B)) return;

        Item a_Item_A = m_UsItems[a_Index_A];
        Item a_Item_B = m_Inventory.m_Items[a_Index_B];

        m_UsItems[a_Index_A] = a_Item_B;
        m_Inventory.m_Items[a_Index_B] = a_Item_A;

        // 슬롯 갱신
        UpdateSlot(a_Index_A);
        m_Inventory.UpdateSlot(a_Index_B);
    }

    // 해당 슬롯의 정보 갱신
    public void UpdateSlot(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;             // 잘못된 인덱스

        Item a_Item = m_UsItems[a_Index];

        // 아이템이 슬롯에 존재하는 경우
        if (a_Item != null)
        {
            // 아이콘 등록
            SetItemIcon(a_Index, a_Item.Data.Sprite, a_Item.Data.Grade);
        }
        // 빈 슬롯
        else
        {
            RemoveIcon(a_Index);
        }

        if (gameObject.activeSelf == true) SlotSave();
    }

    // 아이템 아이콘 제거
    void RemoveIcon(int a_Index)
    {
        RemoveItem(a_Index);
    }

    // 해당 슬롯의 아이템 정보 리턴
    ItemData GetItemData(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return null;            // 잘못된 인덱스
        if (m_UsItems[a_Index] == null) return null;        // 빈 슬롯

        return m_UsItems[a_Index].Data;
    }

    // 해당 슬롯의 아이템 이름 리턴
    string GetItemName(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return "";               // 잘못된 인덱스
        if (m_UsItems[a_Index] == null) return "";           // 빈 슬롯

        return m_UsItems[a_Index].Data.Name;
    }

    void Remove(int a_Index)
    {
        if (!IsValidIndex(a_Index)) return;

        m_UsItems[a_Index] = null;
        RemoveItem(a_Index);
    }

    // 툴팁 활성화,비활성화
    void ShowOrHideItemTooltip()
    {
        // 마우스가 아이템 아이콘 위에 있다면
        bool a_IsValid = m_PointerOverSlot != null && m_PointerOverSlot.HasItem && m_PointerOverSlot.IsAccessible && (m_PointerOverSlot != m_BeginDragSlot);

        if (a_IsValid && m_ItemPopup.m_InventoryPopupUI.activeSelf == false)
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
        m_ItemToolTip.SetItemInfo(GetItemData(a_Slot.Index));

        // 툴팁 위치 조정
        m_ItemToolTip.SetRectPosition(a_Slot.SlotRect);
    }

    public void SlotSave()
    {
        // 아이템 종류
        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray();//배열이 필요할때
        for (int i = 0; i < m_UsItems.Length; i++)
        {
            if (m_UsItems[i] != null) jArray.Add(m_UsItems[i].Data.ItemIndex);
            else jArray.Add(-1);
            a_MkJSON.Add("User_Item", jArray);//배열을 넣음            
            m_SvStrJson = a_MkJSON.ToString();
        }
        StartCoroutine(SaveUserItem());
    }

    IEnumerator SaveUserItem()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        a_Form.AddField("User_Item", m_SvStrJson, System.Text.Encoding.UTF8);


        UnityWebRequest a_WWW = UnityWebRequest.Post(SaveUserItemUrl, a_Form);
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
