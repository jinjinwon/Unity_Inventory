using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkill_", menuName = "Skill System/Skill Data/Active", order = 1)]
public class ActiveSkill : Skill, IUsableSkill
{
    public float AttackDelay => _AttackDelay;
    public int AttackNum => _AttackNum;
    public int SkCollider_Center_X => _SkCollider_Center_X;
    public int SkCollider_Center_Y => _SkCollider_Center_Y;
    public int SkCollider_Center_Z => _SkCollider_Center_Z;
    public int SkCollider_Size_X => _SkCollider_Size_X;
    public int SkCollider_Size_Y => _SkCollider_Size_Y;
    public int SkCollider_Size_Z => _SkCollider_Size_Z;

    [Tooltip("스킬 딜레이")][SerializeField] float _AttackDelay;
    [Tooltip("피해를 입히는 횟수")] [SerializeField] int _AttackNum;
    [Tooltip("스킬 콜라이더 Center X")] [SerializeField] int _SkCollider_Center_X;   // 스킬 콜라이더 Center X
    [Tooltip("스킬 콜라이더 Center Y")] [SerializeField] int _SkCollider_Center_Y;   // 스킬 콜라이더 Center Y
    [Tooltip("스킬 콜라이더 Center Z")] [SerializeField] int _SkCollider_Center_Z;   // 스킬 콜라이더 Center Z
    [Tooltip("스킬 콜라이더 Size X")] [SerializeField] int _SkCollider_Size_X;   // 스킬 콜라이더 Size X
    [Tooltip("스킬 콜라이더 Size Y")] [SerializeField] int _SkCollider_Size_Y;   // 스킬 콜라이더 Size Y
    [Tooltip("스킬 콜라이더 Size Z")] [SerializeField] int _SkCollider_Size_Z;   // 스킬 콜라이더 Size Z

    public bool Skill()
    {
        return true;
    }

    public void UseSkill()
    {
        Debug.Log("엑티브 스킬사용!");
    }
}
