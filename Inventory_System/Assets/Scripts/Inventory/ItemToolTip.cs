using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTip : MonoBehaviour
{
    [SerializeField] Text m_Title_Txt = null;
    [SerializeField] Text m_Content_Txt = null;

    RectTransform m_RT = null;
    CanvasScaler m_CanvasScaler = null;

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    void Awake()
    {
        Init();
        Hide();
    }

    void Init()
    {
        this.gameObject.TryGetComponent(out m_RT);
        m_RT.pivot = new Vector2(0.0f, 1.0f);
        m_CanvasScaler = GetComponentInParent<CanvasScaler>();

        DisableAllChildrenRaycastTarget(transform);
    }

    // 모든 자식들 레이케스트 타겟 해제
    void DisableAllChildrenRaycastTarget(Transform a_Tr)
    {
        // 레이케스트 타겟 해제
        a_Tr.TryGetComponent(out Graphic a_GR);
        if (a_GR != null)
            a_GR.raycastTarget = false;

        // 자식이 없다면 종료
        int a_ChildCount = a_Tr.childCount;
        if (a_ChildCount == 0) return;

        for(int i = 0; i <a_ChildCount; i++)
        {
            DisableAllChildrenRaycastTarget(a_Tr.GetChild(i));
        }
    }

    // 아이템 정보 등록
    public void SetItemInfo(ItemData a_Data)
    {
        m_Title_Txt.text = a_Data.Name;
        m_Content_Txt.text = a_Data.Tooltip;
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

        m_RT.position = new Vector2(pos.x + (slotWidth / 2) , pos.y + slotHeight);
    }
}
