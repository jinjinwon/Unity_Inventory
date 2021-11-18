using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharType
{
    Swordman,           // 전사
    Archer,             // 궁수
    Wizard              // 마법사
}

// [CreateAssetMenu(fileName = "Skill_", menuName = "Skill System/Skill Data/Passive", order = 0)]
public abstract class Skill : ScriptableObject
{    
    public int TargetNumbers => _TargetNumbers;
    public int AcquisitionLevel => _AcquisitionLevel;
    public float Damage => _Damage;
    public float ManaCost => _ManaCost;
    public float Range => _Range;
    public string SkName => _SkName;
    public string AnimID => _AnimID;
    public string Explanation => _Explanation;
    public Sprite Sprite => _Sprite;
    public string SkillType => _SkillType;
    public CharType CharType => _CharType;
    public EffectPoolUnit Effect => _Effect;
    public int SkillLevel => _SkillLevel;
    public int SkillIndex => _SkillIndex;

    [Tooltip("애니메이션 ID")][SerializeField] string _AnimID;                       // 애니메이션 ID
    [Tooltip("스킬 이름")][SerializeField] string _SkName;                           // 스킬 이름
    [Tooltip("소모 마나")][SerializeField] float _ManaCost;                          // 소모 마나
    [Tooltip("데미지")][SerializeField] float _Damage;                               // 데미지
    [Tooltip("공격 범위")][SerializeField] float _Range;                             // 범위
    [Tooltip("스킬 이미지")][SerializeField] Sprite _Sprite;                         // 이미지
    [Tooltip("피해를 입힐 수 있는 수")][SerializeField] int _TargetNumbers;           // 피해를 입힐 수 있는 수
    [Tooltip("캐릭터 타입")][SerializeField] CharType _CharType;                     // 캐릭터 타입
    [Tooltip("습득 레벨")][SerializeField] int _AcquisitionLevel;                    // 습득 레벨
    [Tooltip("이펙트")][SerializeField] EffectPoolUnit _Effect;                      // 이펙트
    [Tooltip("스킬 정보")][SerializeField] string _Explanation;                      // 스킬 설명
    [Tooltip("스킬 타입")] [SerializeField] string _SkillType;                       // 스킬 설명
    [Tooltip("스킬 레벨")] [SerializeField] int _SkillLevel;                         // 스킬 레벨
    [Tooltip("스킬 인덱스")] [SerializeField] int _SkillIndex;                      // 스킬 인덱스
}
