using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] Transform[]  m_Points = null;              // 몬스터가 출현할 위치를 담을 배열
    [SerializeField] GameObject[] m_MonsterPrefab;              // 몬스터 프리팹
    List<GameObject> m_MonsterPool = new List<GameObject>();

    float m_CreateTime = 2.0f;                                  // 몬스터를 발생시킬 주기
    int m_MaxMonster = 4;                                       // 몬스터의 최대 발생 개수

    int a_RanValue = 0;
    void Start()
    {
        for (int a_ii = 0; a_ii < m_MaxMonster; a_ii++)
        {
            a_RanValue = Random.Range(0, m_MonsterPrefab.Length);
            // 몬스터 프리팹 생성
            GameObject a_Monster = Instantiate(m_MonsterPrefab[a_RanValue]);
            // 생성한 몬스터의 이름 설정
            a_Monster.name = "Monster_" + a_ii.ToString();
            // 생성한 몬스터를 비활성화
            a_Monster.SetActive(false);
            // 생성한 몬스터 위치 지정
            a_Monster.transform.SetParent(m_Points[a_ii].transform, false);
            // 생성한 몬스터를 오브젝트 풀에 추가
            m_MonsterPool.Add(a_Monster);
        }

        if (m_Points.Length > 0)
        {
            // 몬스터 생성 코루틴 함수 호출
            StartCoroutine(this.CreateMonster());
        }
    }

    int a_Idx = 0;
    float a_RanPos = 0.0f;
    IEnumerator CreateMonster()
    {
        while (!GameMgr.Inst.TestDie)
        {
            // 몬스터의 생성 주기 시간만큼 대기
            yield return new WaitForSeconds(m_CreateTime);

            // 플레이어가 사망하였을 때 코루틴을 종료해 다음 루틴을 진행하지 않음
            if (GameMgr.Inst.TestDie)
                yield break;

            // 오브젝트 풀에 처음부터 끝까지 순회
            foreach (GameObject a_Monster in m_MonsterPool)
            {
                // 비활성화 여부로 사용가능한 몬스터 생성
                if (a_Monster.activeSelf == false)
                {
                    a_RanPos = Random.Range(-10.0f, 10.0f);
                    // 몬스터의 출현위치 선정
                    a_Monster.transform.position = m_Points[a_Idx].position + new Vector3(a_RanPos,0.0f, a_RanPos);
                    a_Idx++;

                    if (a_Idx == m_Points.Length) a_Idx = 0;

                    // 몬스터를 활성화
                    a_Monster.SetActive(true);
                    // 오브젝트 풀에서 몬스터 프리팹을 하나를 활성화한 후 foreach루프를 빠져나간다.
                    break;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
