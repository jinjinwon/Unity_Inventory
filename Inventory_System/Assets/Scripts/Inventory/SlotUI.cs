using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 슬롯의 외부적인 요소를 담당

public class SlotUI : MonoBehaviour
{
    [Header("Slot UI")]
    [SerializeField] Image m_IconBack_Img = null;
    [SerializeField] Image m_Icon_Img = null;                       // 아이템 아이콘 이미지
    [SerializeField] Text m_Count_Txt = null;                       // 아이템 수량
    [SerializeField] Image m_HighLight_Img = null;                  // 아이템 하이라이트 이미지
    [SerializeField] float m_HightLightAlpha = 0.5f;                // 하이라이트 이미지 알파 값
    [SerializeField] float m_HighlightFadeDuration = 0.2f;          // 하이라이트 이미지 활성화 시간
    [SerializeField] Sprite[] m_GradeSprite = null;                 // 등급별 이미지                 0=노말 1=레어 2=유니크 3=전설 4=유일
    float m_CurHightLightAlpha = 0f;                                // 현재 하이라이트 알파 값

    InventoryUI m_InventoryUI = null;
    RectTransform m_SlotRect = null;
    RectTransform m_IconRect = null;
    RectTransform m_HighLightRect = null;

    GameObject m_IconGo = null;
    GameObject m_TextGo = null;
    GameObject m_HighLightGo = null;

    Image m_Slot_Img = null;

    bool m_IsAccessibleSlot = true;                                 // 슬롯 접근가능 여부
    bool m_IsAccessibleItem = true;                                 // 아이템 접근가능 여부

    public int Index { get; private set; }
    public bool HasItem => m_Icon_Img.sprite != null;
    public bool IsAccessible => m_IsAccessibleSlot && m_IsAccessibleItem;
    public RectTransform SlotRect => m_SlotRect;
    public RectTransform IconRect => m_IconRect;

    void ShowIcon() => m_IconGo.SetActive(true);
    void HideIcon() => m_IconGo.SetActive(false);

    void ShowText() => m_TextGo.SetActive(true);
    void HideText() => m_TextGo.SetActive(false);

    public void SetSlotIndex(int index) => Index = index;

    // 비활성화된 슬롯의 색상
    static readonly Color m_InaccessibleSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    // 비활성화된 아이콘 색상
    static readonly Color m_InaccessibleIconColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    Color _OnColor = new Color(1, 1, 1, 1);             // 아이템이 존재할 때
    Color _OffColor = new Color(0, 0, 0, 1);            // 아이템이 존재하지 않을 때

    void Awake()
    {
        InitComponents();
        InitValues();
    }

    // 컴포넌트 초기화
    void InitComponents()
    {
        m_InventoryUI = GetComponentInParent<InventoryUI>();

        m_SlotRect = GetComponent<RectTransform>();
        m_IconRect = m_Icon_Img.rectTransform;
        m_HighLightRect = m_HighLight_Img.rectTransform;

        m_IconGo = m_IconRect.gameObject;
        m_TextGo = m_Count_Txt.gameObject;
        m_HighLightGo = m_HighLight_Img.gameObject;

        m_Slot_Img = GetComponent<Image>();
    }

    float a_Padding = 1.0f;
    // 변수 값 초기화
    void InitValues()
    {
        m_IconRect.pivot = new Vector2(0.5f, 0.5f);         // 중앙
        m_IconRect.anchorMin = Vector2.zero;
        m_IconRect.anchorMax = Vector2.one;

        m_IconRect.offsetMin = Vector2.one * (a_Padding);
        m_IconRect.offsetMax = Vector2.one * (-a_Padding);

        m_HighLightRect.pivot = m_IconRect.pivot;
        m_HighLightRect.anchorMin = m_IconRect.anchorMin;
        m_HighLightRect.anchorMax = m_IconRect.anchorMax;
        m_HighLightRect.offsetMin = m_IconRect.offsetMin;
        m_HighLightRect.offsetMax = m_IconRect.offsetMax;

        m_Icon_Img.raycastTarget = false;
        m_HighLight_Img.raycastTarget = false;

        HideIcon();
        m_HighLightGo.SetActive(false);
    }

    // 슬롯 활성화 비활성화 설정
    public void SetSlotAccessibleState(bool a_Value)
    {
        if (m_IsAccessibleSlot == a_Value)
            return;

        if (a_Value == true)
            m_Slot_Img.color = Color.black;
        else
        {
            m_Slot_Img.color = m_InaccessibleSlotColor;
            HideIcon();
            HideText();
        }
        m_IsAccessibleSlot = a_Value;
    }

    // 아이템 활성화 비활성화 설정
    public void SetItemAccessibleState(bool a_Value)
    {
        if (m_IsAccessibleItem == a_Value)
            return;

        if (a_Value == true)
        {
            m_Icon_Img.color = Color.white;
            m_Count_Txt.color = Color.white;
        }
        else
        {
            m_Icon_Img.color = m_InaccessibleIconColor;
            m_Count_Txt.color = m_InaccessibleIconColor;
        }
        m_IsAccessibleItem = a_Value;
    }

    // 아이템 이동
    public void SwapOrMove(SlotUI a_SlotUI)
    {
        if (a_SlotUI == null) return;
        if (a_SlotUI == this) return;
        if (!this.IsAccessible) return;
        if (!a_SlotUI.IsAccessible) return;

        Sprite m_TempSpr = m_Icon_Img.sprite;

        // 대상에 아이템이 있는 경우
        if (a_SlotUI.HasItem)
            SetItem(a_SlotUI.m_Icon_Img.sprite);
        // 대상에 아이템이 없는 경우
        else
            RemoveItem();

        a_SlotUI.SetItem(m_TempSpr);
    }

    // 슬롯에 아이템 등록
    public void SetItem(Sprite a_ItemSpr, GradeType a_GradeType = GradeType.Null)
    {
        if (a_ItemSpr != null)
        {
            m_Icon_Img.sprite = a_ItemSpr;
            m_IconBack_Img.sprite = SetGradeSpr(a_GradeType);
            m_IconBack_Img.color = _OnColor;
            ShowIcon();
        }
        else
            RemoveItem();
    }

    public Sprite SetGradeSpr(GradeType a_GradeType)
    {
        if (a_GradeType == GradeType.Normal) return m_GradeSprite[0];
        else if (a_GradeType == GradeType.Rare) return m_GradeSprite[1];
        else if (a_GradeType == GradeType.Unique) return m_GradeSprite[2];
        else if (a_GradeType == GradeType.Legend) return m_GradeSprite[3];
        else if (a_GradeType == GradeType.Only) return m_GradeSprite[4];
        return null;
    }

    // 아이템 제거
    public void RemoveItem()
    {
        m_Icon_Img.sprite = null;
        m_IconBack_Img.color = _OffColor;
        HideIcon();
        HideText();
    }

    // 아이템 개수
    public void SetItemCount(int a_Count)
    {
        // 아이템이 있고 수량이 1개보다 많다면
        if (HasItem && a_Count > 1)
            ShowText();
        else
            HideText();

        m_Count_Txt.text = a_Count.ToString();
    }

    // 슬롯 하이라이트 표시
    public void Highlight(bool a_Show)
    {
        // 접근이 불가능한 슬롯이라면
        if (m_IsAccessibleSlot == false) return;

        if (a_Show)
            StartCoroutine(nameof(HighlightFadeInRoutine));
        else
            StartCoroutine(nameof(HighlightFadeOutRoutine));
    }

    public void SetHighlightOnTop(bool a_Value)
    {
        if (a_Value)
            m_HighLightRect.SetAsLastSibling();
        else
            m_HighLightRect.SetAsFirstSibling();
    }

    // 하이라이트 값 증가
    IEnumerator HighlightFadeInRoutine()
    {
        StopCoroutine(nameof(HighlightFadeOutRoutine));
        m_HighLightGo.SetActive(true);

        float a_Unit = m_HightLightAlpha / m_HighlightFadeDuration;

        for(int a_Nullable = 0; m_CurHightLightAlpha <= m_HightLightAlpha; m_CurHightLightAlpha += a_Unit * Time.deltaTime)
        {
            m_HighLight_Img.color = new Color(m_HighLight_Img.color.r, m_HighLight_Img.color.g, m_HighLight_Img.color.b, m_CurHightLightAlpha);
            yield return null;
        }
    }

    // 하이라이트 값 감소
    IEnumerator HighlightFadeOutRoutine()
    {
        StopCoroutine(nameof(HighlightFadeInRoutine));

        float a_Unit = m_HightLightAlpha / m_HighlightFadeDuration;

        for (int a_Nullable = 0; m_CurHightLightAlpha >= 0.0f; m_CurHightLightAlpha -= a_Unit * Time.deltaTime)
        {
            m_HighLight_Img.color = new Color(m_HighLight_Img.color.r, m_HighLight_Img.color.g, m_HighLight_Img.color.b, m_CurHightLightAlpha);
            yield return null;
        }
        m_HighLightGo.SetActive(false);
    }
}
