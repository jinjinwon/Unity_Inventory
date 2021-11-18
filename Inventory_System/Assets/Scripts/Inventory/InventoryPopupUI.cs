using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryPopupUI : MonoBehaviour
{
    [HideInInspector] public GameObject m_InventoryPopupUI;

    [Header("Confirmation")]
    // 아이템 버리기
    [SerializeField] GameObject m_ConfirmationPopupObj = null;
    [SerializeField] Text m_ConfirmationItemName_Txt = null;
    [SerializeField] Text m_Confirmation_Txt = null;
    [SerializeField] Button m_ConfirmationOk_Btn = null;
    [SerializeField] Button m_ConfirmationCancel_Btn = null;

    [Header("Mount")]
    // 아이템 장착
    [SerializeField] GameObject m_MountPopupObj = null;
    [SerializeField] Text m_MountItemName_Txt = null;
    [SerializeField] Text m_Mount_Txt = null;
    [SerializeField] Button m_MountOk_Btn = null;
    [SerializeField] Button m_MountCencel_Btn = null;

    [Header("Release")]
    // 아이템 해제
    [SerializeField] GameObject m_ReleasePopupObj = null;
    [SerializeField] Text m_ReleaseItemName_Txt = null;
    [SerializeField] Text m_Release_Txt = null;
    [SerializeField] Button m_ReleaseOk_Btn = null;
    [SerializeField] Button m_ReleaseCencel_Btn = null;

    [Header("AddSkill")]
    [SerializeField] GameObject m_AddSkillPopupObj = null;
    [SerializeField] Text m_AddSkillName_Txt = null;
    [SerializeField] Text m_AddSkill_Txt = null;
    [SerializeField] Button m_AddSkillOk_Btn = null;
    [SerializeField] Button m_AddSkillCencel_Btn = null;


    // 확인 버튼 이벤트
    event Action m_OnConfirmationOk;
    event Action m_MountOk;
    event Action m_ReleaseOk;
    event Action m_AddSkillOk;

    void Awake()
    {
        // 확인 취소 팝업
        // 버리기
        m_ConfirmationOk_Btn.onClick.AddListener(HidePanel);
        m_ConfirmationOk_Btn.onClick.AddListener(HideConfirmationPopup);
        m_ConfirmationOk_Btn.onClick.AddListener(() => m_OnConfirmationOk?.Invoke());

        m_ConfirmationCancel_Btn.onClick.AddListener(HidePanel);
        m_ConfirmationCancel_Btn.onClick.AddListener(HideConfirmationPopup);

        // 장착
        m_MountOk_Btn.onClick.AddListener(HidePanel);
        m_MountOk_Btn.onClick.AddListener(HideMountPopup);
        m_MountOk_Btn.onClick.AddListener(() => m_MountOk?.Invoke());

        m_MountCencel_Btn.onClick.AddListener(HidePanel);
        m_MountCencel_Btn.onClick.AddListener(HideMountPopup);

        // 해제
        m_ReleaseOk_Btn.onClick.AddListener(HidePanel);
        m_ReleaseOk_Btn.onClick.AddListener(HideReleasePopup);
        m_ReleaseOk_Btn.onClick.AddListener(() => m_ReleaseOk?.Invoke());

        m_ReleaseCencel_Btn.onClick.AddListener(HidePanel);
        m_ReleaseCencel_Btn.onClick.AddListener(HideReleasePopup);

        // 스킬 추가
        m_AddSkillOk_Btn.onClick.AddListener(HidePanel);
        m_AddSkillOk_Btn.onClick.AddListener(HideAddSkillPopup);
        m_AddSkillOk_Btn.onClick.AddListener(() => m_AddSkillOk?.Invoke());

        m_AddSkillCencel_Btn.onClick.AddListener(HidePanel);
        m_AddSkillCencel_Btn.onClick.AddListener(HideAddSkillPopup);

        HidePanel();
        HideConfirmationPopup();
        HideMountPopup();
        HideReleasePopup();
        HideAddSkillPopup();

        m_InventoryPopupUI = this.gameObject;
    }

    // 온 오프
    void ShowPanel() => gameObject.SetActive(true);
    void HidePanel() => gameObject.SetActive(false);

    // 버리기 이벤트
    void HideConfirmationPopup() => m_ConfirmationPopupObj.SetActive(false);
    void SetConfirmationOkEvent(Action a_Handler) => m_OnConfirmationOk = a_Handler;

    // 장착 이벤트
    void HideMountPopup() => m_MountPopupObj.SetActive(false);
    void SetMountOkEvent(Action a_Handler) => m_MountOk = a_Handler;

    // 해제 이벤트
    void HideReleasePopup() => m_ReleasePopupObj.SetActive(false);
    void SetReleaseOkEvent(Action a_Handler) => m_ReleaseOk = a_Handler;

    // 스킬 추가 이벤트
    void HideAddSkillPopup() => m_AddSkillPopupObj.SetActive(false);
    void SetAddSkillOkEvent(Action a_Handler) => m_AddSkillOk = a_Handler;

    void ShowPopup(string a_ItemName,int a_Popup = 0)
    {
        // 버리기 팝업
        if (a_Popup == 0)
        {
            m_ConfirmationItemName_Txt.text = a_ItemName;
            HideMountPopup();
            HideReleasePopup();
            HideAddSkillPopup();
            m_ConfirmationPopupObj.SetActive(true);
        }
        // 장착 팝업
        else if(a_Popup == 1)
        {
            m_MountItemName_Txt.text = a_ItemName;
            HideConfirmationPopup();
            HideReleasePopup();
            HideAddSkillPopup();
            m_MountPopupObj.SetActive(true);
        }
        else if(a_Popup == 2)
        {
            m_ReleaseItemName_Txt.text = a_ItemName;
            HideConfirmationPopup();
            HideMountPopup();
            HideAddSkillPopup();
            m_ReleasePopupObj.SetActive(true);
        }
        else if(a_Popup == 3)
        {
            m_AddSkillName_Txt.text = a_ItemName;
            HideConfirmationPopup();
            HideReleasePopup();
            HideMountPopup();
            m_AddSkillPopupObj.SetActive(true);
        }
    }

    public void OpenPopup(Action a_Callback, string a_ItemName, int a_Popup = 0)
    {
        // 버리기 팝업
        if(a_Popup == 0)
        {
            ShowPanel();
            ShowPopup(a_ItemName, a_Popup);
            m_OnConfirmationOk = a_Callback;
        }
        // 장착 팝업
        else if(a_Popup == 1)
        {
            ShowPanel();
            ShowPopup(a_ItemName, a_Popup);
            m_MountOk = a_Callback;
        }
        // 해제 팝업
        else if(a_Popup == 2)
        {
            ShowPanel();
            ShowPopup(a_ItemName, a_Popup);
            m_ReleaseOk = a_Callback;
        }
        // 퀵슬롯 추가 팝업
        else if(a_Popup == 3)
        {
            ShowPanel();
            ShowPopup(a_ItemName, a_Popup);
            m_AddSkillOk = a_Callback;
        }
    }
}
