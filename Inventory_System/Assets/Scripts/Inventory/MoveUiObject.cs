using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// UI 오브젝트의 움직임을 담당

public class MoveUiObject : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] Transform m_Tr = null;

    Vector2 m_BeginPoint = Vector2.zero;
    Vector2 m_MoveBegin = Vector2.zero;

    void Start()
    {
        // 지정하지 않은 경우 부모로 초기화
        if (m_Tr == null)
            m_Tr = this.transform.parent;
    }

    // 위치를 기억시킨다.
    void IPointerDownHandler.OnPointerDown(PointerEventData _Data)
    {
        m_BeginPoint = m_Tr.position;
        m_MoveBegin = _Data.position;
    }

    // 기억한 위치로부터 OffSet값 만큼 더하여 이동시킨다.
    void IDragHandler.OnDrag(PointerEventData _Data)
    {
        m_Tr.position = m_BeginPoint + (_Data.position - m_MoveBegin);
    }
}
