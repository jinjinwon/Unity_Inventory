using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterEvent : MonoBehaviour
{
    MonsterCtrl m_RefMonCS;

    void Start() 
    {
        m_RefMonCS = transform.parent.GetComponent<MonsterCtrl>();
    }

    void Event_AttDamage(string Type)
    {
        m_RefMonCS.Event_AttDamage(Type);
    }
}
