using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;  //SimpleJSON을 사용하기 위해 네임스페이스를 추가

public class LobbyMgr : MonoBehaviour
{
    public Text MyInfo_Text = null;
    public Text Ranking_Text = null;
    public Text MessageText;

    public Button m_GoStoreBtn = null;
    public Button m_GameStartBtn = null;
    public Button m_LogOutBtn = null;
    public Button RestRk_Btn;

    void Start()
    {
        Ranking_Text.text = "";

        if (m_GoStoreBtn != null)
            m_GoStoreBtn.onClick.AddListener(() =>
            {

            });

        if (m_GameStartBtn != null)
            m_GameStartBtn.onClick.AddListener(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("scLobby");
            });

        if (m_LogOutBtn != null)
            m_LogOutBtn.onClick.AddListener(() =>
            {

            });
    }
}
