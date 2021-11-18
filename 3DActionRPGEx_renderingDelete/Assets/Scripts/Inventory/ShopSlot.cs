using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] Image m_IconImg = null;
    [SerializeField] Image m_IconBackImg = null;
    [SerializeField] Text m_SlotName_Txt = null;
    [SerializeField] Text m_SlotToolTip_Txt = null;
    [SerializeField] Button m_Buy_Btn = null;
    [SerializeField] Text m_SlotPrice_Txt = null;
    [SerializeField] Sprite[] m_GradeSpr = null;
    RectTransform m_MyTr = null;
    [HideInInspector] public ItemData m_ItemData = null;
    ItemBuy m_ItemBuy = null;
    int m_Index = 0;
    bool m_Bool = false;

    // 슬롯에 아이템 등록
    public void SetItem(Sprite a_ItemSpr, GradeType a_GradeType = GradeType.Null)
    {
        if (a_ItemSpr != null)
        {
            m_IconImg.sprite = a_ItemSpr;
            m_IconBackImg.sprite = SetGradeSpr(a_GradeType);
        }      
    }

    public void SetItemData(ItemData a_ItemData)
    {
        m_ItemData = a_ItemData;
        m_SlotName_Txt.text = m_ItemData.Name;
        m_SlotToolTip_Txt.text = m_ItemData.Tooltip;
        m_SlotPrice_Txt.text = $"{m_ItemData.Price.ToString()} $";
    }

    public void SetIndex(int a_Index = 0)
    {
        m_Index = a_Index;
    }

    public Sprite SetGradeSpr(GradeType a_GradeType)
    {
        if (a_GradeType == GradeType.Normal) return m_GradeSpr[0];
        else if (a_GradeType == GradeType.Rare) return m_GradeSpr[1];
        else if (a_GradeType == GradeType.Unique) return m_GradeSpr[2];
        else if (a_GradeType == GradeType.Legend) return m_GradeSpr[3];
        else if (a_GradeType == GradeType.Only) return m_GradeSpr[4];
        return null;
    }

    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);

    void Awake()
    {
        m_ItemBuy = FindObjectOfType<ItemBuy>();
        m_MyTr = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (m_Buy_Btn != null) m_Buy_Btn.onClick.AddListener(() => m_Bool = true);
    }

    void Update()
    {
        if (m_Bool == false) return;

        if (m_Bool == true)
        {
            m_ItemBuy.Show();
            m_ItemBuy.SetRectPosition(m_MyTr);
            m_ItemBuy.SetItemInfo(m_ItemData);
            m_Bool = false;
        }
    }
}
