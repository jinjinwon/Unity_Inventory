//#define EconomyType
//UnityEditor에서 사용자 디파인 셋팅
//edit -> Player Settings -> Other Setting -> Scripting Define Symbols

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

//----------------- 이메일형식이 맞는지 확인하는 방법 스크립트
using System.Globalization;
using System.Text.RegularExpressions;
using System;
using UnityEngine.Networking;
//----------------- 이메일형식이 맞는지 확인하는 방법 스크립트

public class Login_Mgr : MonoBehaviour
{
    public string g_Message = "";

    [Header("LoginPanel")]              //이렇게 쓰면 편집창에 태그들이 나온다. 
    [SerializeField] GameObject m_LoginPanelObj;
    [SerializeField] InputField IDInputField;     //Email 로 받을 것임
    [SerializeField] InputField PassInputField;
    [SerializeField] Button m_LoginBtn = null;
    [SerializeField] Button m_CreateAccOpenBtn = null;

    [Header("CreateAccountPanel")]
    [SerializeField] GameObject m_CreateAccPanelObj;
    [SerializeField] InputField New_IDInputField;  //Email 로 받을 것임
    [SerializeField] InputField New_PassInputField;
    [SerializeField] InputField New_NickInputField;
    [SerializeField] Button m_CreateAccountBtn = null;
    [SerializeField] Button m_CancelButton = null;

    private bool invalidEmailType = false;       // 이메일 포맷이 올바른지 체크
    private bool isValidFormat = false;          // 올바른 형식인지 아닌지 체크

    string LoginUrl;
    string CreateUrl;

    void Hide(GameObject _Obj) => _Obj.SetActive(false);
    void Show(GameObject _Obj) => _Obj.SetActive(true);

    // Start is called before the first frame update
    void Start()
    {
        if (m_CreateAccOpenBtn != null)
            m_CreateAccOpenBtn.onClick.AddListener(OpenCreateAccBtn);

        if (m_CancelButton != null)
            m_CancelButton.onClick.AddListener(CreateCancelBtn);

        if (m_CreateAccountBtn != null)
            m_CreateAccountBtn.onClick.AddListener(CreateAccountBtn);

        if (m_LoginBtn != null)
            m_LoginBtn.onClick.AddListener(LoginBtn);

        LoginUrl = "http://jinone12.dothome.co.kr/Login.php";
        CreateUrl = "http://jinone12.dothome.co.kr/CreateAccount.php";
    }

    public void OpenCreateAccBtn()
    {
        if (m_LoginPanelObj != null)
            Hide(m_LoginPanelObj);

        if (m_CreateAccPanelObj != null)
            Show(m_CreateAccPanelObj);
    }

    public void CreateCancelBtn()
    {
        if (m_LoginPanelObj != null)
            Show(m_LoginPanelObj);

        if (m_CreateAccPanelObj != null)
            Hide(m_CreateAccPanelObj);
    }

    public void CreateAccountBtn() //계정 생성 요청 함수
    {
        string a_IdStr = New_IDInputField.text;
        string a_PwStr = New_PassInputField.text;
        string a_NickStr = New_NickInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "" || a_NickStr.Trim() == "")
        {
            g_Message = "ID, PW, 별명 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 15))  //3~15
        {
            g_Message = "ID는 3글자 이상 15글자 이하로 작성해 주세요.";
            return;
        }

        if (!(3 <= a_PwStr.Length && a_PwStr.Length < 15))  //6~15
        {
            g_Message = "비밀번호는 3글자 이상 15글자 이하로 작성해 주세요.";
            return;
        }

        if (!(2 <= a_NickStr.Length && a_NickStr.Length < 15))  //2~15
        {
            g_Message = "별명은 2글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }
        StartCoroutine(CreateCo(a_IdStr, a_PwStr, a_NickStr));
    }

    IEnumerator CreateCo(params string[] a_Str)
    {
        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", a_Str[0], System.Text.Encoding.UTF8);
        a_Form.AddField("Input_pass", a_Str[1]);
        a_Form.AddField("Input_nick", a_Str[2]);


        UnityWebRequest a_WWW = UnityWebRequest.Post(CreateUrl, a_Form);
        yield return a_WWW.SendWebRequest();

        if (a_WWW.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_WWW.downloadHandler.data);
            g_Message = sz;

            if (sz.Contains("Create Success.") == true) g_Message = "계정을 생성하였습니다.";
            if (sz.Contains("ID Does Exist.") == true) g_Message = "중복된 ID가 존재합니다.";
            if (sz.Contains("Nick Does Exist.") == true) g_Message = "중복된 닉네임이 존재합니다.";
        }
        else Debug.Log("error");

        CreateCancelBtn();
    }

    void OnGUI()
    {
        if (g_Message != "")
        {
            GUI.Label(new Rect(20, 15, 1500, 100),
                "<color=Yellow><size=28>" + g_Message + "</size></color>");
        }
    }

    public void LoginBtn()
    {
        string a_IdStr = IDInputField.text;
        string a_PwStr = PassInputField.text;

        if (a_IdStr.Trim() == "" || a_PwStr.Trim() == "")
        {
            g_Message = "ID, PW 빈칸 없이 입력해 주셔야 합니다.";
            return;
        }

        if (!(3 <= a_IdStr.Length && a_IdStr.Length < 15))  //3~15
        {
            g_Message = "ID는 3글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }
        if (!(3 <= a_PwStr.Length && a_PwStr.Length < 15))  //6~15
        {
            g_Message = "비밀번호는 3글자 이상 20글자 이하로 작성해 주세요.";
            return;
        }
        StartCoroutine(LoginCo(a_IdStr, a_PwStr));
    }

    IEnumerator LoginCo(string a_Id_Str, string a_Pw_Str)
    {
        WWWForm a_Form = new WWWForm();
        a_Form.AddField("Input_user", a_Id_Str, System.Text.Encoding.UTF8);
        a_Form.AddField("Input_pass", a_Pw_Str);

        UnityWebRequest a_WWW = UnityWebRequest.Post(LoginUrl, a_Form);
        yield return a_WWW.SendWebRequest();

        if (a_WWW.error == null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            string sz = enc.GetString(a_WWW.downloadHandler.data);

            if (sz.Contains("ID Does Exist.") == true) g_Message = "아이디가 존재하지 않습니다.";
            if (sz.Contains("Pass does not Match.") == true) g_Message = "비밀번호가 일치하지 않습니다.";

            if (sz.Contains("Login-Success") == false)
                yield break;

            GlobalValue.g_Unique_ID = a_Id_Str;

            // JSON 파싱
            if (sz.Contains("nick_name") == false)
                yield break;

            var N = JSON.Parse(sz);

            if (N["nick_name"] != null)
                GlobalValue.g_NickName = N["nick_name"];

            if (N["SkillPoint"] != null)
            {
                if (N["SkillPoint"].AsInt == 0)
                    GlobalValue.g_Sp = 3;
                else
                    GlobalValue.g_Sp = N["SkillPoint"].AsInt;
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("FAE_Demo1");
        }
        else Debug.Log("error");
    }
}