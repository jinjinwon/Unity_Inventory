using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffSkill_", menuName = "Skill System/Skill Data/Buff", order = 2)]
public class BuffSkill : Skill, IUsableSkill
{
    public float BuffTime => _BuffTime;

    [SerializeField] float _BuffTime;

    public bool Skill()
    {
        return true;
    }

    public void UseSkill()
    {
        Debug.Log("버프 스킬사용!");
    }
}
