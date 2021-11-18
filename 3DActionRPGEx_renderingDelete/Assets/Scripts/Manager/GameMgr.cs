using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using SimpleJSON;

enum JoyStickType
{
    Fixed = 0,                  
    Flexible = 1,               
    FlexibleOnOff = 2           
}

public class GameMgr : MonoBehaviour
{
    //싱글턴 패턴을 위한 인스턴스 변수 선언
    public static GameMgr Inst = null;
    [HideInInspector] public static Hero_Ctrl m_refHero = null;

    JoyStickType m_JoyStickType = JoyStickType.Fixed;

    //-----------Fixed JoyStick 처리 부분
    public GameObject m_JoySBackObj = null;
    public Image m_JoyStickImg = null;
    float m_Radius = 0.0f;
    Vector3 m_OrignPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;

    //-----------------피킹 이동 관련 변수
    Ray a_MousePos;
    RaycastHit hitInfo;
    private LayerMask m_layerMask = -1;

    public GameObject m_CursorMark = null;
    Vector3 a_CacVLen = Vector3.zero;

    [Header("--- Shader ---")]
    public Shader g_AddTexShader = null;                //주인공 데미지 연출용(빨간색으로 변했다 돌아올 때)
    public Shader g_VertexLitShader = null;             //몬스터 사망시 투명하게 사라지게하기 용

    [Header("--- DamageText ---")]
    //----------------- 머리위에 데미지 띄우기용 변수 선언
    public Transform m_Damage_Canvas = null;
    public GameObject m_DamagePrefab = null;
    RectTransform CanvasRect;
    Vector2 screenPos = Vector2.zero;
    Vector2 WdScPos = Vector2.zero;

    //-------------- DamageTxt 카메라 반대편에 있을 때 컬링하기 위한 변수들...
    GameObject[] m_DmgTxtList = null;
    Vector3 a_CacWdPos = Vector3.zero;
    Vector3 a_CacTgVec = Vector3.zero;

    public Button m_Attack_Btn = null;
    public Button m_Skill_Btn = null;

    //-------------- 스킬 쿨 타임 적용
    private Text    m_Skill_Cool_Label = null;
    private Image   m_Skell_Cool_Mask = null;
    private Button  m_SkillUIBtn = null;
    [HideInInspector] public float m_Skill_Cooltime = 0.0f;
    float m_SkillCoolLen = 7.0f;

    [Header("Button Active Object")]
    [SerializeField] GameObject m_Inventory = null;
    [SerializeField] GameObject m_UserInfo = null;
    [SerializeField] GameObject m_SkillBook = null;
    [SerializeField] GameObject m_Shop = null;

    [Header("Button")]
    [SerializeField] Button m_Shop_Btn = null;
    [SerializeField] Button m_Inven_Btn = null;
    [SerializeField] Button m_MyInfo_Btn = null;
    [SerializeField] Button m_SkillBook_Btn = null;
    [SerializeField] Button m_AutoHunting_Btn = null;

    [SerializeField] GameObject m_AutoHunting_Obj = null;
    [SerializeField] SkillBook m_SkillBook_Obj = null;
    [SerializeField] Inventory m_Inventory_Obj = null;
    [SerializeField] inventoryTester m_InvenTester = null;
    [SerializeField] QuickSlotGroup m_QuickSlotGroup_Obj = null;
    [SerializeField] UserInfo m_UserInfo_Obj = null;

    [HideInInspector] public bool m_AutoHunt = false;

    public bool TestDie = false;
    string JsonLoadUrl;

    void Awake()
    {
        Inst = this;
        JsonLoadUrl = "http://jinone12.dothome.co.kr/JsonLoadUrl.php";
    }

    void ButtonSetting()
    {
        //------ Skill Button 처리 코드
        m_Skill_Cooltime = 0.0f;

        if (m_Skill_Btn != null)
        {
            m_Skill_Btn.onClick.AddListener(() =>
            {
                if (m_refHero != null)
                    m_refHero.SkillOrder("RainArrow", ref m_SkillCoolLen, ref m_Skill_Cooltime);
            });

            m_Skill_Cool_Label = m_Skill_Btn.transform.GetComponentInChildren<Text>(true);
            m_Skell_Cool_Mask = m_Skill_Btn.transform.Find("SkillCoolMask").GetComponent<Image>();

            m_SkillUIBtn = m_Skill_Btn.GetComponent<Button>();
        }

        //------ Attack Button 처리 코드
        if (m_Attack_Btn != null)
            m_Attack_Btn.onClick.AddListener(() =>
            {
                if (m_refHero != null)
                    m_refHero.AttackOrder();
            });

        if (m_Shop_Btn != null) m_Shop_Btn.onClick.AddListener(() => m_Shop.SetActive(true));
        if (m_Shop_Btn != null) m_Shop_Btn.onClick.AddListener(() => m_Inventory.SetActive(true));
        if (m_Inven_Btn != null) m_Inven_Btn.onClick.AddListener(() => m_Inventory.SetActive(true));
        if (m_Inven_Btn != null) m_Inven_Btn.onClick.AddListener(() => m_UserInfo.SetActive(true));
        if (m_MyInfo_Btn != null) m_MyInfo_Btn.onClick.AddListener(() => m_UserInfo.SetActive(true));
        if (m_SkillBook_Btn != null) m_SkillBook_Btn.onClick.AddListener(() => m_SkillBook.SetActive(true));
        if (m_AutoHunting_Btn != null) m_AutoHunting_Btn.onClick.AddListener(() => m_AutoHunt = !m_AutoHunt);
        if (m_AutoHunting_Btn != null) m_AutoHunting_Btn.onClick.AddListener(() => m_AutoHunting_Obj.SetActive(m_AutoHunt));
    }

    void Start()
    {
        m_layerMask = 1 << LayerMask.NameToLayer("MyTerrain");
        m_layerMask |= 1 << LayerMask.NameToLayer("MyUnit");

        //-----------Fixed JoyStick 처리 부분
        if (m_JoySBackObj != null && m_JoyStickImg != null
            && m_JoySBackObj.activeSelf == true)
        {
            m_JoyStickType = JoyStickType.Fixed;

            Vector3[] v = new Vector3[4];
            m_JoySBackObj.GetComponent<RectTransform>().GetWorldCorners(v);
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OrignPos = m_JoyStickImg.transform.position;

            //스크립트로만 대기하고자 할 때
            EventTrigger trigger = m_JoySBackObj.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) => { 
                            OnDragJoyStick((PointerEventData)data); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) => { 
                           OnEndDragJoyStick((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }

        ButtonSetting();
        StartCoroutine(LoadCo(GlobalValue.g_Unique_ID));
    }

    void Update()
    {
        Sill_Cooltime(ref m_Skill_Cooltime, ref m_Skill_Cool_Label,
                                            ref m_Skell_Cool_Mask, m_SkillCoolLen);

        //-----------------피킹 이동 부분 
        if (Input.GetMouseButtonDown(0))
        if (IsPointerOverUIObject() == false)
        {
            a_MousePos = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(a_MousePos, out hitInfo, Mathf.Infinity, m_layerMask.value))
            {
                if (m_refHero != null)
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("MyUnit")) //몬스터 피킹
                    {     //몬스터 픽킹일 때 
                        m_refHero.MousePicking(hitInfo.point, hitInfo.collider.gameObject);

                        if (m_CursorMark != null)
                            m_CursorMark.SetActive(false);
                    }
                    else  //지형 바닥 픽킹일 때 
                    {
                        m_refHero.MousePicking(hitInfo.point);

                        if (m_CursorMark != null)
                        {
                            m_CursorMark.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + 0.01f, hitInfo.point.z);
                            m_CursorMark.SetActive(true);
                        }
                    }
                }
            }
        }

        //---클릭마크 끄기
        if (m_CursorMark != null && m_CursorMark.activeSelf == true)
        {
            if (m_refHero != null) //아직 죽지 않았을 때 
            {
                a_CacVLen = m_refHero.transform.position - 
                            m_CursorMark.transform.position;
                a_CacVLen.y = 0.0f;
                if (a_CacVLen.magnitude < 1.0f)
                    m_CursorMark.SetActive(false);
            }
        }
    }

    //-----------Fixed JoyStick 처리 부분
    void OnDragJoyStick(PointerEventData _data)
    {
        if (m_JoyStickImg == null)
            return;

        m_JsCacVec = Input.mousePosition - m_OrignPos;
        m_JsCacVec.z = 0.0f;
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        //조이스틱 백그라운드를 벗어나지 못하게 막는 부분
        if (m_Radius < m_JsCacDist)
        {
            m_JoyStickImg.transform.position =
                                    m_OrignPos + m_Axis * m_Radius;
        }
        else
        {
            m_JoyStickImg.transform.position =
                                    m_OrignPos + m_Axis * m_JsCacDist;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        //캐릭터 이동 처리
        if (m_refHero != null)
            m_refHero.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

    void OnEndDragJoyStick(PointerEventData _data)
    {
        if (m_JoyStickImg == null)
            return;

        m_Axis = Vector3.zero;
        m_JoyStickImg.transform.position = m_OrignPos;

        m_JsCacDist = 0.0f;

        //캐릭터 정지 처리
        if (m_refHero != null)
            m_refHero.SetJoyStickMv(0.0f, m_Axis);
    }

    PointerEventData a_EDCurPos; // using UnityEngine.EventSystems;
    public bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
    {
        a_EDCurPos = new PointerEventData(EventSystem.current);

        // 유니티 에디터가 아닌 휴대폰으로 실행했을 때 사용되는 코드
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
 
            //using System.Collections.Generic;
            List<RaycastResult> results = new List<RaycastResult>();
            for (int i = 0; i < Input.touchCount; ++i)
            {
                a_EDCurPos.position = Input.GetTouch(i).position;  //new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                results.Clear();
                EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
            }
 
            return false;
// 에디터로 실행했을 때 사용되는 코드
#else
        a_EDCurPos.position = Input.mousePosition;
        //using System.Collections.Generic;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return results.Count > 0;
#endif
    }

    Vector3 a_StCacPos = Vector3.zero;
    public void SpawnDamageTxt(int dmg, Transform txtTr, int a_ColorIdx = 0)
    {
        if (m_DamagePrefab != null && m_Damage_Canvas != null)
        {
            GameObject m_DamageObj = (GameObject)Instantiate(m_DamagePrefab);
            a_StCacPos = new Vector3(txtTr.position.x, 
                                     txtTr.position.y + 2.65f, txtTr.position.z);

            m_DamageObj.transform.SetParent(m_Damage_Canvas, false);
            DamageText a_DamageTx = m_DamageObj.GetComponent<DamageText>();
            a_DamageTx.m_BaseWdPos = a_StCacPos;
            a_DamageTx.m_DamageVal = (int)dmg;

            //초기 위치 잡아 주기 , World 좌표를 UGUI 좌표로 환산해 주는 코드
            CanvasRect = m_Damage_Canvas.GetComponent<RectTransform>();
            screenPos = Camera.main.WorldToViewportPoint(a_StCacPos);
            WdScPos.x = ((screenPos.x * CanvasRect.sizeDelta.x) - 
                                        (CanvasRect.sizeDelta.x * Random.Range(-0.5f, 0.5f)));
            WdScPos.y = ((screenPos.y * CanvasRect.sizeDelta.y) - 
                                        (CanvasRect.sizeDelta.y * Random.Range(-0.5f, 0.5f)));
            m_DamageObj.GetComponent<RectTransform>().anchoredPosition = WdScPos;

            //주인공 일때 데미지 택스트 색 바꾸기...
            if (a_ColorIdx == 1) 
            {
                Outline a_Outline = m_DamageObj.GetComponentInChildren<Outline>();
                a_Outline.effectColor = new Color32(255, 255, 255, 0);
                a_Outline.enabled = false;

                Text a_RefText = m_DamageObj.GetComponentInChildren<Text>();
                a_RefText.color = new Color32(255, 255, 230, 255);
            }
        }
    }

    void Sill_Cooltime(ref float Cool_float, ref Text Cool_Label,ref Image Cool_Sprite, float Max_Cool)
    {
        if (0.0f < Cool_float)
        {
            Cool_float -= Time.deltaTime;
            Cool_Label.text = ((int)Cool_float).ToString();
            Cool_Sprite.fillAmount = Cool_float / Max_Cool;

            if (m_SkillUIBtn != null)
                m_SkillUIBtn.enabled = false;
        }
        else
        {
            Cool_float = 0.0f;
            Cool_Sprite.fillAmount = 0.0f;
            Cool_Label.text = "";

            if (m_SkillUIBtn != null)
                m_SkillUIBtn.enabled = true;
        }
    }

    int a_ItemIndex = -1;       // 인벤토리 아이템 위치
    int a_ItemNum = -1;         // 인베노리 아이템 수량
    IEnumerator LoadCo(string a_Id_Str)
    {
        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", a_Id_Str, System.Text.Encoding.UTF8);

        UnityWebRequest a_WWW = UnityWebRequest.Post(JsonLoadUrl, a_Form);
        yield return a_WWW.SendWebRequest();

        if (a_WWW.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_WWW.downloadHandler.data);

            var N = JSON.Parse(sz);

            // 스킬북 정보
            if (N["skill_list"] != null)
            {
                string m_StrJson = N["skill_list"];
                if (m_StrJson != "" && m_StrJson.Contains("SkillList") == true)
                {
                    var a_N = JSON.Parse(m_StrJson);
                    for (int ii = 0; ii < a_N["SkillList"].Count; ii++)
                    {
                        int a_CrLevel = a_N["SkillList"][ii].AsInt;
                        if (ii < m_SkillBook_Obj.m_SSList.Count)
                        {
                            m_SkillBook_Obj.m_SSList[ii].m_SkillLevel = a_CrLevel;
                        }
                    }
                }
            }
            // 인벤토리 아이템
            if(N["item_list"] != null && N["item_Numlist"] != null)
            {
                string m_StrJson_1 = N["item_list"];
                string m_StrJson_2 = N["item_Numlist"];
               
                var a_N = JSON.Parse(m_StrJson_1);
                var a_NN = JSON.Parse(m_StrJson_2);

                for(int i = 0; i < a_N["ItemList"].Count; i++)
                {
                    a_ItemIndex = a_N["ItemList"][i].AsInt;
                    a_ItemNum = a_NN["ItemNumList"][i].AsInt;

                    if (FindItemType(a_ItemIndex) != null) m_Inventory_Obj.Add(FindItemType(a_ItemIndex), a_ItemNum, i);
                    else continue;
                }
            }
            // 퀵 슬롯 아이템
            if (N["quick_item"] != null && N["quick_item_num"] != null)
            {
                string m_StrJson_1 = N["quick_item"];
                string m_StrJson_2 = N["quick_item_num"];

                var a_N = JSON.Parse(m_StrJson_1);
                var a_NN = JSON.Parse(m_StrJson_2);

                for (int i = 0; i < a_N["Quick_Item"].Count; i++)
                {
                    a_ItemIndex = a_N["Quick_Item"][i].AsInt;
                    a_ItemNum = a_NN["Quick_Item_Num"][i].AsInt;

                    if (FindItemType(a_ItemIndex) != null) m_QuickSlotGroup_Obj.Add(FindItemType(a_ItemIndex), a_ItemNum, i);
                    else continue;
                }
            }
            // 퀵슬롯 스킬
            if (N["quick_skill"] != null)
            {
                string m_StrJson = N["quick_skill"];
                if (m_StrJson != "" && m_StrJson.Contains("Quick_Skill") == true)
                {
                    var a_N = JSON.Parse(m_StrJson);
                    for (int ii = 0; ii < a_N["Quick_Skill"].Count; ii++)
                    {
                        int a_SkillIndex = a_N["Quick_Skill"][ii].AsInt;
                         m_SkillBook_Obj.AddLoadSkill(a_SkillIndex,ii);
                    }
                }
            }
            // 유저 장착 아이템
            if(N["user_item"] != null)
            {
                string m_StrJson = N["user_item"];
                if (m_StrJson != "" && m_StrJson.Contains("User_Item") == true)
                {
                    var a_N = JSON.Parse(m_StrJson);
                    for (int ii = 0; ii < a_N["User_Item"].Count; ii++)
                    {
                        int a_SkillIndex = a_N["User_Item"][ii].AsInt;
                        m_Inventory_Obj.m_InventoryUI.ItemEquipment(a_SkillIndex,1);
                    }
                }
            }
            // 인벤토리 슬롯 개수
            if (N["Slot_Num"] != null)
            {
                if (N["Slot_Num"] != null)
                {
                    if (N["Slot_Num"].AsInt == 0) m_Inventory_Obj.AddSlot(32);
                    else m_Inventory_Obj.AddSlot(N["Slot_Num"].AsInt);
                }
            }
        }
        else Debug.Log("error");
    }

    ItemData FindItemType(int a_Index)
    {
        for(int i = 0; i < m_InvenTester._itemDataArray.Length; i++)
        {
            if (m_InvenTester._itemDataArray[i].ItemIndex == a_Index) return m_InvenTester._itemDataArray[i];
            if (a_Index == -1) return null;
        }
        return null;
    }
}
