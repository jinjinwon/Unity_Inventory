using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInfo : MonoBehaviour
{
    [SerializeField] ItemData[] m_PortionData = null;
    [SerializeField] ItemData[] m_EquipmentData = null;
    [SerializeField] GameObject m_ShopSlot = null;
    [SerializeField] Transform m_Content_Tr = null;
    [SerializeField] Toggle m_ToggleFilter_Weapon = null;
    [SerializeField] Toggle m_ToggleFilter_Portion = null;
    [SerializeField] Button m_Exit_Btn = null;

    int a_Index = 0;
    int a_Type = 0;         // 0 = Portion , 1 = Equipment
    FilterOption m_CurFilterOption = FilterOption.Portion;
    List<ShopSlot> m_PortionList = new List<ShopSlot>();
    List<ShopSlot> m_EquipmentList = new List<ShopSlot>();
    
    void Awake()
    {
        InitSetting();
        InitSetToggle();
    }

    void InitSetting()
    {
        for(int i = 0; i < m_PortionData.Length; i++)
        {
            GameObject a_Go = Instantiate(m_ShopSlot);

            if(a_Go.TryGetComponent(out ShopSlot a_ShopSlot) == true)
            {
                a_ShopSlot.SetItem(m_PortionData[i].Sprite,m_PortionData[i].Grade);
                a_ShopSlot.SetItemData(m_PortionData[i]);
                a_ShopSlot.SetIndex(a_Index);
                a_ShopSlot.transform.SetParent(m_Content_Tr,false);
                a_ShopSlot.Hide();
                a_Index++;
                m_PortionList.Add(a_ShopSlot);
            }
        }

        for(int i = 0; i < m_EquipmentData.Length; i++)
        {
            GameObject a_Go = Instantiate(m_ShopSlot);

            if (a_Go.TryGetComponent(out ShopSlot a_ShopSlot) == true)
            {
                a_ShopSlot.SetItem(m_EquipmentData[i].Sprite, m_EquipmentData[i].Grade);
                a_ShopSlot.SetItemData(m_EquipmentData[i]);
                a_ShopSlot.SetIndex(a_Index);
                a_ShopSlot.transform.SetParent(m_Content_Tr, false);
                a_ShopSlot.Hide();
                a_Index++;
                m_EquipmentList.Add(a_ShopSlot);
            }
        }
    }

    void InitSetToggle()
    {
        m_ToggleFilter_Portion.onValueChanged.AddListener(a_Flag => UpdateFilter(a_Flag, 0));
        m_ToggleFilter_Weapon.onValueChanged.AddListener(a_Flag => UpdateFilter(a_Flag, 1));

        void UpdateFilter(bool a_Flag, int a_Option)
        {
            if (a_Flag)
            {
                a_Type = a_Option;
                UpdateAllSlotFilters(a_Type);
            }
        }
    }

    public void UpdateAllSlotFilters(int a_Type)
    {

        if(a_Type == 0)
        {
            for (int i = 0; i < m_PortionList.Count; i++)
            {
                UpdateSlotFilterState(a_Type, m_PortionList[i].m_ItemData);
            }
        }
        else if(a_Type == 1)
        {
            for (int i = 0; i < m_EquipmentList.Count; i++)
            {
                UpdateSlotFilterState(a_Type, m_EquipmentList[i].m_ItemData);
            }
        }
    }

    public void UpdateSlotFilterState(int a_Type, ItemData a_IData)
    {
        if (a_IData == null) return;
        if(a_Type == 0)
        {
            for (int i = 0; i < m_PortionList.Count; i++)
                m_PortionList[i].Show();

            for (int i = 0; i < m_EquipmentList.Count; i++)
                m_EquipmentList[i].Hide();
        }
        else if (a_Type == 1)
        {
            for (int i = 0; i < m_EquipmentList.Count; i++)
                m_EquipmentList[i].Show();

            for (int i = 0; i < m_PortionList.Count; i++)
                m_PortionList[i].Hide();
        }
    }

    void FindType(int a_Type = 0)
    {
        if(a_Type == 0)
        {
            for(int i = 0; i < m_PortionList.Count; i++)
            {
                m_PortionList[i].Show();
            }

            for(int i = 0; i < m_EquipmentList.Count; i++)
            {
                m_EquipmentList[i].Hide();
            }
        }
        else if (a_Type == 1)
        {
            for (int i = 0; i < m_EquipmentList.Count; i++)
            {
                m_EquipmentList[i].Show();
            }

            for (int i = 0; i < m_PortionList.Count; i++)
            {
                m_PortionList[i].Hide();
            }
        }
    }

    void Start()
    {
        if (m_Exit_Btn != null) m_Exit_Btn.onClick.AddListener(() => gameObject.SetActive(false));

        FindType();
        gameObject.SetActive(false);
    }
}
