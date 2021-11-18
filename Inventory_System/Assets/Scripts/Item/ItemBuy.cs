using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemBuy : MonoBehaviour
{
    [SerializeField] Inventory m_Inventory;
    [HideInInspector] public GameObject m_ItemBuy;
    [SerializeField] Text m_ItemName_Txt = null;
    [SerializeField] InputField m_InputField = null;
    [SerializeField] Button m_Buy_Btn = null;
    [SerializeField] Button m_Cancel_Btn = null;

    int a_DefaultNumber = 1;
    ItemData m_ItemData = null;

    RectTransform m_RT = null;
    CanvasScaler m_CanvasScaler = null;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    void Awake()
    {
        Init();
        InitSetting();
        Hide();
    }

    void Init()
    {
        m_ItemBuy = gameObject;
        gameObject.TryGetComponent(out m_RT);
        m_RT.pivot = new Vector2(0.0f, 1.0f);
        m_CanvasScaler = GetComponentInParent<CanvasScaler>();
    }

    void InitSetting()
    {
        m_Buy_Btn.onClick.AddListener(Hide);
        m_Buy_Btn.onClick.AddListener(() => ShopItemBuy());
        m_Cancel_Btn.onClick.AddListener(Hide);
    }

    // 아이템 정보 등록
    public void SetItemInfo(ItemData a_Data)
    {
        m_ItemData = a_Data;
        m_ItemName_Txt.text = a_Data.Name;
    }

    // 위치 조정
    public void SetRectPosition(RectTransform a_Rt)
    {
        if (m_CanvasScaler == null)
            return;

        // 캔버스 스케일러에 따른 해상도 대응
        float wRatio = Screen.width / m_CanvasScaler.referenceResolution.x;
        float hRatio = Screen.height / m_CanvasScaler.referenceResolution.y;
        float ratio = wRatio * (1f - m_CanvasScaler.matchWidthOrHeight) + hRatio * (m_CanvasScaler.matchWidthOrHeight);

        float slotWidth = a_Rt.rect.width * ratio;
        float slotHeight = a_Rt.rect.height * ratio;

        // 툴팁 초기 위치(슬롯 우하단) 설정
        m_RT.position = a_Rt.position + new Vector3(slotWidth, -slotHeight);
        Vector2 pos = m_RT.position;

        m_RT.position = new Vector2((pos.x / 2) + (slotWidth / 2), pos.y + (slotHeight * 2f));
    }

    int a_Capecity = 1;
    // 수량 입력받고 구매
    void ShopItemBuy()
    {
        if (m_InputField.text == "") m_Inventory.Add(m_ItemData);
        else
        {
            a_Capecity = int.Parse(m_InputField.text);
            m_Inventory.Add(m_ItemData, a_Capecity);
        }
        m_InputField.text = "";
        m_Inventory.UpdateAllSlot();
    }
}
