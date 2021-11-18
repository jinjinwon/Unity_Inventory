using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSkillSlot : MonoBehaviour
{
    Skill m_QuickSkill = null;
    [SerializeField] Image m_Icon_Img = null;

    RectTransform m_SlotRect = null;
    RectTransform m_IconRect = null;

    public RectTransform SlotRect => m_SlotRect;
    public RectTransform IconRect => m_IconRect;

    public int Index { get; private set; }
    public bool HasSkill => m_Icon_Img.sprite != null;
    void Hide() => m_Icon_Img.gameObject.SetActive(false);
    void Show() => m_Icon_Img.gameObject.SetActive(true);
    public void SetSlotIndex(int index) => Index = index;


    void Awake()
    {
        InitComponent();
    }

    void Start()
    {
        Hide();
    }

    void InitComponent()
    {
        m_SlotRect = GetComponent<RectTransform>();
        m_IconRect = m_Icon_Img.rectTransform;
    }

    // 슬롯에 아이템 등록
    public void SetSkill(Sprite a_ItemSpr)
    {
        if (a_ItemSpr != null)
        {
            m_Icon_Img.sprite = a_ItemSpr;
            Show();
        }
        else
            RemoveItem();
    }

    // 아이템 제거
    public void RemoveItem()
    {
        m_Icon_Img.sprite = null;
        m_QuickSkill = null;
        Hide();
    }

    public void SetSkill(Skill _Skill = null) => m_QuickSkill = _Skill;

    // 아이템 이동
    public void SwapOrMove(QuickSkillSlot a_SlotUI)
    {
        if (a_SlotUI == null) return;
        if (a_SlotUI == this) return;

        Sprite m_TempSpr = m_Icon_Img.sprite;

        // 대상에 아이템이 있는 경우
        if (a_SlotUI.HasSkill)
            SetSkill(a_SlotUI.m_Icon_Img.sprite);
        // 대상에 아이템이 없는 경우
        else
            RemoveItem();

        a_SlotUI.SetSkill(m_TempSpr);
    }
}
