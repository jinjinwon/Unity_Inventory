using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using SimpleJSON;


// 스킬 포인트 , 스킬 레벨 적용 , 타입에 맞는 스킬 생성 담당

public class SkillBook : MonoBehaviour
{
    [Header("SkillBook UI")]
    [SerializeField] Text m_SkillBookName_Txt = null;
    [SerializeField] Text m_SP_Txt = null;
    [SerializeField] Transform m_SkContent = null;
    [SerializeField] Button m_OK_Btn = null;
    [SerializeField] Button m_Cancel_Btn = null;
    [SerializeField] GameObject m_SkillSlot = null;
    [SerializeField] SkillSet m_SkillSet = null;
    [SerializeField] CharType m_CharType = CharType.Swordman;
    [SerializeField] InventoryPopupUI m_PopUI = null;
    [SerializeField] QuickSlotGroup m_QuickObj = null;

    Skill[] m_Skills = null;
    [HideInInspector] public List<SkillSlot> m_SSList = new List<SkillSlot>();


    GraphicRaycaster m_Gr;
    PointerEventData m_Data;
    List<RaycastResult> m_RayList;

    string m_SkBookName = "";
    string UpdateSkillPointUrl = "";
    string m_SvStrJson = "";
    public static int _SP = 0;

    public void SpTxtUpdate() => m_SP_Txt.text = $"SP {_SP.ToString()}";
    public int SetSkillPoint(int index)
    {
        _SP += index;

        if (_SP <= 0)
        {
            _SP = 0;
            GlobalValue.g_Sp = _SP;
            return _SP;
        }

        if (_SP >= (GlobalValue.g_Level * 3))
        {
            _SP = (GlobalValue.g_Level * 3);
            GlobalValue.g_Sp = _SP;
            return _SP;
        }

        GlobalValue.g_Sp = _SP;
        return _SP;
    }

    void Awake()
    {
        Init();
        InitSlot(m_CharType);
    }

    void Init()
    {
        m_CharType = GlobalValue.g_CharType;
        _SP = GlobalValue.g_Sp;

        if ((int)m_CharType < (int)CharType.Swordman || (int)m_CharType > (int)CharType.Wizard) return;

        if(m_CharType == CharType.Swordman)
        {
            m_SkBookName = "전사의 스킬북";
        }
        else if(m_CharType == CharType.Archer)
        {
            m_SkBookName = "궁수의 스킬북";
        }
        else if(m_CharType == CharType.Wizard)
        {
            m_SkBookName = "마법사의 스킬북";
        }

        if (gameObject.GetComponent<GraphicRaycaster>() == false)
            m_Gr.gameObject.AddComponent<GraphicRaycaster>();
        else
            m_Gr = GetComponent<GraphicRaycaster>();

        m_Data = new PointerEventData(EventSystem.current);
        m_RayList = new List<RaycastResult>(10);

        //SetSkillPoint(3);

        m_SkillBookName_Txt.text = m_SkBookName;
        SpTxtUpdate();
    }

    void ButtonSetting()
    {
        if (m_OK_Btn != null)
            m_OK_Btn.onClick.AddListener(OnClickOk);

        if (m_Cancel_Btn != null)
            m_Cancel_Btn.onClick.AddListener(OnClickCancel);
    }

    void Start()
    {
        ButtonSetting();
        UpdateSkillPointUrl = "http://jinone12.dothome.co.kr/UpdateSkill.php";
        gameObject.SetActive(false);
    }

    void InitSlot(CharType _CharType)
    {
        m_Skills = null;

        // 타입 구분
        if(_CharType == CharType.Swordman)
        {
            m_Skills = m_SkillSet._SwordMan_SkillSet;
        }
        else if (_CharType == CharType.Archer)
        {
            m_Skills = m_SkillSet._Archer_SkillSet;
        }
        else if (_CharType == CharType.Wizard)
        {
            m_Skills = m_SkillSet._Wizard_SkillSet;
        }

        // 슬롯 생성
        for(int i = 0; i < m_Skills.Length; i++)
        {
            GameObject a_Go = Instantiate(m_SkillSlot);
            a_Go.TryGetComponent<SkillSlot>(out var a_SkillSlot);
            a_SkillSlot.SetSprite(m_Skills[i].Sprite);                                       // 이미지 지정
            a_SkillSlot.SetName(m_Skills[i].SkName);                                         // 이름 지정
            a_SkillSlot.SetGetLevel(m_Skills[i].AcquisitionLevel);                           // 습득 레벨
            a_SkillSlot.SetExplanation(m_Skills[i].Explanation,m_Skills[i].SkillType);       // 스킬 정보
            a_SkillSlot.SetPos(m_SkContent);                                                 // 생성 위치         
            a_SkillSlot.SetIndex(i);
            m_SSList.Add(a_SkillSlot);
        }
    }

    void Update()
    {
        m_Data.position = Input.mousePosition;
        OnPointerDown();
    }

    // 스킬 배우기 및 정보 동기화
    void OnClickOk()
    {
        for (int i = 0; i < m_SSList.Count; i++)
        {
            // 올리려고 하는 스킬 찾기
            if(m_SSList[i].m_SkillOn == true)
            {
                m_SSList[i].LevelSetting(m_SSList[i].m_SkillCount);
            }
        }

        JSONObject a_MkJSON = new JSONObject();
        JSONArray jArray = new JSONArray();//배열이 필요할때
        for (int i = 0; i < m_SSList.Count; i++)
        {
            jArray.Add(m_SSList[i].m_SkillLevel);
            a_MkJSON.Add("SkillList", jArray);//배열을 넣음
            m_SvStrJson = a_MkJSON.ToString();
        }
        StartCoroutine(UpdateSkillPoint());
    }

    // 스킬 배우기 취소 및 팝업 창 종료
    void OnClickCancel()
    {
        for(int i = 0; i < m_SSList.Count; i++)
        {
            if(m_SSList[i].m_SkillOn == true)
            {
                SetSkillPoint(m_SSList[i].m_SkillCount);
                m_SSList[i].m_SkillCount = 0;
                m_SSList[i].m_SkillOn = false;
                m_SSList[i].SetCount();
            }
        }
        SpTxtUpdate();
        gameObject.SetActive(false);
    }


    T RaycastAndGetFristComponet<T>() where T : Component
    {
        // 리스트 초기화
        m_RayList.Clear();

        // 포인터 위치로부터 Raycast 발생, 결과는 m_RayList 담긴다
        m_Gr.Raycast(m_Data, m_RayList);

        // 아무것도 없다면 리턴으로 null을 반환
        if (m_RayList.Count == 0)
            return null;

        // 있다면 SlotUI를 반환
        return m_RayList[0].gameObject.GetComponent<T>();
    }

    void OnPointerDown()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SkillSlot a_SlotUI = RaycastAndGetFristComponet<SkillSlot>();

            if (a_SlotUI == null) return;

            int a_Level = a_SlotUI.m_SkillLevel;
            string a_SkillName = GetSkillData(a_SlotUI.Index);

            if (a_Level >= 1) m_PopUI.OpenPopup(() => AddSkill(a_SlotUI.Index), GetSkillData(a_SlotUI.Index), 3);
        }
    }

    // 퀵 슬롯에 스킬 추가
    void AddSkill(int a_Index = 0)
    {
        int a_FindIndex = m_QuickObj.FindEmptySlotIndex(0, 1);
        if (a_FindIndex == -1) return;

        for(int i = 0; i < m_QuickObj.m_QuickSkills.Length; i++)
        {
            // 중복 스킬 처리
            if (m_QuickObj.m_QuickSkills[i] == m_Skills[a_Index]) return;
        }

        for(int i = 0; i < m_Skills.Length; i++)
        {
            if (m_Skills[i].SkillIndex == a_Index) m_QuickObj.m_QuickSkills[a_FindIndex] = m_Skills[a_Index];
        }
        m_QuickObj.UpdateSkillSlot(a_FindIndex);
        m_QuickObj.SlotSave();
    }

    int a_FindIndex = 0;
    public void AddLoadSkill(int a_Index = 0 , int a_SlotIndex = -1)
    {
        if (a_SlotIndex == -1) a_FindIndex = m_QuickObj.FindEmptySlotIndex(0, 1);
        else a_FindIndex = a_SlotIndex;

        if (a_FindIndex == -1) return;

        for (int i = 0; i < m_Skills.Length; i++)
        {
            if (m_Skills[i].SkillIndex == a_Index)
            {
                m_QuickObj.m_QuickSkills[a_FindIndex] = m_Skills[i];
                m_QuickObj.UpdateSkillSlot(a_FindIndex);
            }
        }      
    }

    // 해당 슬롯의 아이템 정보 리턴
    public string GetSkillData(int a_Index)
    {
        if (m_Skills[a_Index] == null) return null;      // 빈 슬롯
        return m_Skills[a_Index].SkName;
    }

    IEnumerator UpdateSkillPoint()
    {
        if (GlobalValue.g_Unique_ID == "")
            yield break;

        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", GlobalValue.g_Unique_ID, System.Text.Encoding.UTF8);
        a_Form.AddField("SkillPoint", GlobalValue.g_Sp);
        a_Form.AddField("Skill_list", m_SvStrJson, System.Text.Encoding.UTF8);

        UnityWebRequest a_WWW = UnityWebRequest.Post(UpdateSkillPointUrl, a_Form);
        yield return a_WWW.SendWebRequest();

        if (a_WWW.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string a_ReStr = enc.GetString(a_WWW.downloadHandler.data);

            if (a_ReStr.Contains("UpdateSuccess") == true)
                SpTxtUpdate();
        }
        else Debug.Log(a_WWW.error);
    }
}
