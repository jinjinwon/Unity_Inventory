using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUseable : MonoBehaviour
{
    [HideInInspector] public GameObject m_Itemuseable;

    [SerializeField] Text m_Title_Txt = null;
    [SerializeField] Button m_Use_Btn = null;
    [SerializeField] Button m_Quick_Btn = null;
    [SerializeField] Button m_Inven_Btn = null;

    RectTransform m_RT = null;
    CanvasScaler m_CanvasScaler = null;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    [HideInInspector] public event Action m_UseItemOk;
    [HideInInspector] public event Action m_QuickOk;
    [HideInInspector] public event Action m_InvenOk;

    void Awake()
    {
        Init();
        InitComponent();
        Hide();
    }

    void Init()
    {
        m_Itemuseable = this.gameObject;
        this.gameObject.TryGetComponent(out m_RT);
        m_RT.pivot = new Vector2(0.0f, 1.0f);
        m_CanvasScaler = GetComponentInParent<CanvasScaler>();
    }

    void InitComponent()
    {
        m_Use_Btn.onClick.AddListener(Hide);
        m_Use_Btn.onClick.AddListener(() => m_UseItemOk?.Invoke());

        m_Quick_Btn.onClick.AddListener(Hide);
        m_Quick_Btn.onClick.AddListener(() => m_QuickOk?.Invoke());

        m_Inven_Btn.onClick.AddListener(Hide);
        m_Inven_Btn.onClick.AddListener(() => m_InvenOk?.Invoke());
    }

    // 아이템 정보 등록
    public void SetItemInfo(ItemData a_Data)
    {
        m_Title_Txt.text = a_Data.Name;
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

        // 툴팁의 크기
        float width = m_RT.rect.width * ratio;
        float height = m_RT.rect.height * ratio;

        // 우측, 하단이 잘렸는지 여부
        bool rightTruncated = pos.x + width > Screen.width;
        bool bottomTruncated = pos.y - height < 0f;

        ref bool R = ref rightTruncated;
        ref bool B = ref bottomTruncated;

        // 오른쪽만 잘림 => 슬롯의 Left Bottom 방향으로 표시
        if (R && !B)
        {
            m_RT.position = new Vector2(pos.x - width - slotWidth, pos.y);
        }
        // 아래쪽만 잘림 => 슬롯의 Right Top 방향으로 표시
        else if (!R && B)
        {
            m_RT.position = new Vector2(pos.x, pos.y + height + slotHeight);
        }
        // 모두 잘림 => 슬롯의 Left Top 방향으로 표시
        else if (R && B)
        {
            m_RT.position = new Vector2(pos.x - width - slotWidth, pos.y + height + slotHeight);
        }
        // 잘리지 않음 => 슬롯의 Right Bottom 방향으로 표시
        // Do Nothing
    }

    public void OpenItemUsePop(Action a_Callback = null, ItemData a_IData = null)
    {
        Show();
        SetItemInfo(a_IData);
        m_UseItemOk = a_Callback;
    }

    public void ActionSetting_a(Action a_Callback)
    {
        m_QuickOk = a_Callback;
        m_Inven_Btn.gameObject.SetActive(false);
        m_Quick_Btn.gameObject.SetActive(true);
    }

    public void ActionSetting_b(Action a_Callback)
    {
        m_InvenOk = a_Callback;
        m_Quick_Btn.gameObject.SetActive(false);
        m_Inven_Btn.gameObject.SetActive(true);
    }
}
