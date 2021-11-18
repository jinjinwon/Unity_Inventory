using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Transform m_CameraTr = null;

    void Start()
    {
        m_CameraTr = Camera.main.transform;
    }

    void LateUpdate()
    {
        this.transform.forward = m_CameraTr.forward;  //빌보드
    }
}
