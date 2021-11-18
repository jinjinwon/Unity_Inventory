using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlot : MonoBehaviour
{
    SkillBook _SkillBook = null;

    [SerializeField] Image m_Slot_Img = null;
    [SerializeField] Text m_SlotName_Txt = null;
    [SerializeField] Text m_SlotGetLevel_Txt = null;
    [SerializeField] Text m_SlotExplanation_Txt = null;
    [SerializeField] Text m_SlotLevel_Txt = null;
    [SerializeField] Text m_SlotCount_Txt = null;
    [SerializeField] Button m_Up_Btn = null;
    [SerializeField] Button m_Down_Btn = null;

    public int m_SkillCount = 0;
    public bool m_SkillOn = false;
    [HideInInspector] public int m_SkillLevel = 0;


    public int Index { get; private set; }
    public void SetSprite(Sprite _Spr) => m_Slot_Img.sprite = _Spr;
    public void SetName(string _Str) => m_SlotName_Txt.text = $"{_Str}";
    public void SetGetLevel(int _Level) => m_SlotGetLevel_Txt.text = $"필요레벨 {_Level.ToString()}";
    public void SetPos(Transform _Tr) => gameObject.transform.SetParent(_Tr);
    public void SetExplanation(string _Str,string _Type) => m_SlotExplanation_Txt.text = $"<{_Type}>\n{_Str}";
    public void SetIndex(int _Index) => Index = _Index;
    void SetLevel(int _Level) => m_SlotLevel_Txt.text = $"Lv {_Level.ToString()}";
    public void GetSetSP(int _Count) => _SkillBook.SetSkillPoint(_Count);

    public void SetCount()
    {
        m_SkillOn = true;

        if (m_SkillCount <= 0)
        {
            m_SkillCount = 0;
            m_SkillOn = false;
        }
        if (m_SkillCount >= GlobalValue.g_Level * 3) m_SkillCount = GlobalValue.g_Level * 3;

        m_SlotCount_Txt.text = $"스킬 투자 SP [{m_SkillCount.ToString()}]";
    }

    void Awake()
    {
        Init();
    }

    void Init()
    {
        _SkillBook = FindObjectOfType<SkillBook>();
    }
    
    void Start()
    {
        SettingButton();
        SetLevel(m_SkillLevel);
    }

    void SettingButton()
    {
        if (m_Up_Btn != null)
            m_Up_Btn.onClick.AddListener(() =>
            {
                if (SkillBook._SP == 0) return;

                GetSetSP(-1);
                m_SkillCount++;
                SetCount();
                _SkillBook.SpTxtUpdate();
            });

        if (m_Down_Btn != null)
            m_Down_Btn.onClick.AddListener(() =>
            {
                if (m_SkillCount == 0) return;

                GetSetSP(1);
                m_SkillCount--;
                SetCount();
                _SkillBook.SpTxtUpdate();
            });
    }

    public void LevelSetting(int _Upgrade)
    {
        m_SkillLevel += _Upgrade;
        m_SkillCount = 0;

        // 스킬 업그레이드 구현 내용
        SetCount();
        SetLevel(m_SkillLevel);
    }
}
