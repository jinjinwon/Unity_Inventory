using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public enum MonType
{
    Skeleton,
    Alien
}

public class MonsterCtrl : MonoBehaviour
{
    //------------------------ HP바 표시
    float Curhp = 100;
    float Maxhp = 100;
    public Image imgHpbar;

    //인스펙터뷰에 표시할 애니메이션 클래스 변수
    public Anim anim;

    //몬스터의 현재 상태 정보를 저장할 Enum 변수
    public AnimState MonState = AnimState.idle;

    //public MonType m_MonType = MonType.Skeleton;
    Animation m_RefAnimation = null; //Skeleton
    Animator  m_RefAnimator = null;  //Alien
    AnimatorStateInfo animaterStateInfo;

    //----MonsterAI
    [HideInInspector] public GameObject m_AggroTarget = null;
    int m_AggroTgID = -1;                  //이 몬스터가 공격해야할 캐럭터의 고유번호
    Vector3 m_MoveDir = Vector3.zero;      //수평 진행 노멀 방향 벡터
    Vector3 m_CacVLen = Vector3.zero;      //주인공을 향하는 벡터
    float  a_CacDist  = 0.0f;              //거리 계산용 변수
    float  traceDist  = 7.0f;              //추적 거리
    float  attackDist = 1.8f;              //공격 거리
    Quaternion a_TargetRot;                //회전 계산용 변수
    float m_RotSpeed = 7.0f;               //초당 회전 속도
    float m_NowStep = 0.0f;                //이동 계산용 변수
    Vector3 a_MoveNextStep = Vector3.zero; //이동 계산용 변수
    float m_MoveVelocity = 2.0f;           //평면 초당 이동 속도...

    //--------죽는 연출
    Vector3 m_DieDir = Vector3.zero;
    float m_DieDur = 0.0f;
    float m_DieTimer = 0.0f;

    //-------------데미지 칼라 관련 변수
    SkinnedMeshRenderer m_SMR = null;
    SkinnedMeshRenderer[] m_SMRList = null;
    MeshRenderer[] m_MeshList = null;          //장착 무기
    float a_Ratio = 0.0f;
    Color a_CalcColor;

    Shader  g_DefTexShader = null;
    Shader  g_WeaponTexShader = null;
    Color   g_DefColor;

    //---- Navigation
    protected NavMeshAgent nvAgent;    //using UnityEngine.AI;
    protected NavMeshPath movePath;

    protected Vector3 m_PathEndPos = Vector3.zero;
    int m_CurPathIndex = 1;
    protected double m_MoveDurTime = 0.0;     //목표점까지 도착하는데 걸리는 시간
    protected double m_AddTimeCount = 0.0;    //누적시간 카운트 
    float m_MoveTick = 0.0f;

    void Start()
    {
        m_RefAnimation = GetComponentInChildren<Animation>();
        m_RefAnimator = GetComponentInChildren<Animator>();

        movePath = new NavMeshPath();
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        nvAgent.updateRotation = false;

        FindDefShader();
    }

    void Update()
    {
        if (MonState == AnimState.die)
            return;

        m_MoveTick = m_MoveTick - Time.deltaTime;
        if (m_MoveTick < 0.0f)
            m_MoveTick = 0.0f;

        MonStateUpdate();
        MonActionUpdate();
    }

    //일정한 간격으로 몬스터의 행동 상태를 체크하고 monsterState 값 변경
    void MonStateUpdate()
    {
        //어그로 타겟이 있을 경우
        if (m_AggroTarget != null)
        {
            m_CacVLen = m_AggroTarget.transform.position - this.transform.position;

            m_CacVLen.y = 0.0f;
            m_MoveDir = m_CacVLen.normalized;
            a_CacDist = m_CacVLen.magnitude;

            //--- 직선거리가 아니고 길찾기로 이동중 이면
            if (2 < movePath.corners.Length) traceDist = 14.0f;
            //일반 이동일 때
            else traceDist = 7.0f;

            //공격거리 범위 이내로 들어왔는지 확인
            if (a_CacDist <= attackDist) MonState = AnimState.attack;
            //추적거리 범위 이내로 들어왔는지 확인
            else if (a_CacDist <= traceDist) MonState = AnimState.trace;
            //몬스터의 상태를 idle 모드로 설정
            else
            {
                MonState = AnimState.idle;   //몬스터의 상태를 idle 모드로 설정
                m_AggroTarget = null;
                m_AggroTgID = -1;
            }
        }
        else
        {
            GameObject[] a_players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < a_players.Length; i++)
            {
                m_CacVLen = a_players[i].transform.position - this.transform.position;
                m_CacVLen.y = 0.0f;
                m_MoveDir = m_CacVLen.normalized;
                a_CacDist = m_CacVLen.magnitude;

                //--- 직선거리가 아니고 길찾기로 이동중 이면
                if (2 < movePath.corners.Length) traceDist = 14.0f;
                //일반 이동일 때
                else traceDist = 7.0f;

                //공격거리 범위 이내로 들어왔는지 확인
                if (a_CacDist <= attackDist)
                {
                    MonState = AnimState.attack;
                    m_AggroTarget = a_players[i].gameObject;
                    break;
                }
                //추적거리 범위 이내로 들어왔는지 확인
                else if (a_CacDist <= traceDist)
                {
                    MonState = AnimState.trace;
                    m_AggroTarget = a_players[i].gameObject;
                    break;
                }
            }
        }
    }

    void MonActionUpdate()
    {
        //공격상태 일 때
        if (MonState == AnimState.attack) 
        {
            //아직 공격 애니메이션 중이라면...
            if (m_AggroTarget == null)
            {
                MySetAnim(anim.Idle.name, 0.13f); //애니메이션 적용
                return;
            }

            if (0.0001f < m_MoveDir.magnitude)
            {
                a_TargetRot = Quaternion.LookRotation(m_MoveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation,a_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            MySetAnim(anim.Attack1.name, 0.12f); 
            ClearMsPickPath(); //이동 즉시 취소
        }
        else if (MonState == AnimState.trace)
        {
            if (m_AggroTarget == null)
            {
                MySetAnim(anim.Idle.name, 0.13f);
                return;
            }

            //아직 공격 애니메이션 중이라면
            if (IsAttackAnim() == true) return;

            //---------------- 네비게이션 메시를 이용한 이동 방법            
            if (m_MoveTick <= 0.0f)
            {
                //주기적으로 피킹
                float a_PathLen = 0.0f;
                if (MyNavCalcPath(this.transform.position,m_AggroTarget.transform.position, ref a_PathLen) == true)
                {
                    m_MoveDurTime = a_PathLen / m_MoveVelocity; //도착하는데 걸리는 시간
                    m_AddTimeCount = 0.0;
                }
                m_MoveTick = 0.2f;
            }
            MoveToPath();
        }
        else if (MonState == AnimState.idle) MySetAnim(anim.Idle.name, 0.13f);
    }

    public void MySetAnim(string newAnim, float CrossTime = 0.0f)
    {
        if (m_RefAnimation != null)
        {
            if (0.0f < CrossTime) m_RefAnimation.CrossFade(newAnim, CrossTime);
            else m_RefAnimation.Play(newAnim);
        }

        if (m_RefAnimator != null)
        {
            if (newAnim == anim.Move.name)
            {
                if (m_RefAnimator.GetBool("IsAttack") == true) m_RefAnimator.SetBool("IsAttack", false);
                if (m_RefAnimator.GetBool("IsRun") == false) m_RefAnimator.SetBool("IsRun", true);
            }
            else if (newAnim == anim.Idle.name)
            {
                if (m_RefAnimator.GetBool("IsAttack") == true) m_RefAnimator.SetBool("IsAttack", false);
                if (m_RefAnimator.GetBool("IsRun") == true) m_RefAnimator.SetBool("IsRun", false);
            }
            else if (newAnim == anim.Attack1.name)
            {
                if (m_RefAnimator.GetBool("IsAttack") == false) m_RefAnimator.SetBool("IsAttack", true);
            }
            if (newAnim == anim.Die.name)
            {
                //CrossFade상태에서는 이것보다는 위의 if (m_CurAniState == newAnim) 더 정확하다.
                animaterStateInfo = m_RefAnimator.GetCurrentAnimatorStateInfo(0);  
                if (animaterStateInfo.IsName(anim.Die.name) == false) m_RefAnimator.CrossFade(anim.Die.name, CrossTime);
            }
        }
    }

    //공격애니메이션 상태 체크 함수
    float a_CacRate = 0.0f;
    float a_NormalTime = 0.0f;
    public bool IsAttackAnim()
    {
        if (m_RefAnimation != null)
        {
            if (m_RefAnimation.IsPlaying(anim.Attack1.name) == true)
            {
                a_NormalTime = m_RefAnimation[anim.Attack1.name].time / m_RefAnimation[anim.Attack1.name].length;

                //소수점 한동작이 몇프로 진행되었는지 계산 변수
                a_CacRate = a_NormalTime - (float)((int)a_NormalTime);

                //공격 애니메이션 끝부분이 아닐 때(공격애니메이션 중이라는 뜻)
                if (a_CacRate < 0.95f) return true;
            }
        }
        return false;
    }

    public void TakeDamage(GameObject a_Attacker, float a_Damage = 10.0f, int a_AttackNum = 1)
    {
        if (Curhp <= 0.0f) return;
        Curhp -= a_Damage;
        if (Curhp < 0.0f) Curhp = 0.0f;

        imgHpbar.fillAmount = (float)Curhp / (float)Maxhp;

        GameMgr.Inst.SpawnDamageTxt((int)a_Damage, this.transform);

        if (Curhp <= 0)  //사망 처리
        {
            gameObject.tag = "Untagged";
            StopAllCoroutines();
            MonState = AnimState.die;
            MySetAnim(anim.Die.name, 0.1f);
            m_DieDur = 2.0f;
            m_DieTimer = 2.0f;
            m_DieDir = this.transform.position - a_Attacker.transform.position;
            m_DieDir.y = 0.0f;
            m_DieDir.Normalize();

            // 몬스터에 추가된 Collider 다시 활성화
            gameObject.GetComponentInChildren<BoxCollider>().enabled = true;

            FindDefShader();
            StartCoroutine("DieDirect"); //죽는 연출
            StartCoroutine("PushObjectPool");
        }
    }


    // 몬스터 재활용
    IEnumerator PushObjectPool()
    {
        yield return new WaitForSeconds(3.0f);

        // 각종 변수 초기화
        Curhp = 100;
        gameObject.tag = "Enemy";
        imgHpbar.fillAmount = (float)Curhp / (float)Maxhp;
        MonState = AnimState.idle;
        MySetAnim(anim.Idle.name);
        
        // 쉐이더 교체
        for (int i = 0; i < m_SMRList.Length; i++)
        {
            if (g_DefTexShader != null) m_SMRList[i].material.shader = g_DefTexShader;
            m_SMRList[i].material.SetColor("_Color", a_CalcColor);
        }

        //------------무기
        if (m_MeshList != null)
        {
            for (int i = 0; i < m_MeshList.Length; i++)
            {
                if (g_WeaponTexShader != null) m_MeshList[i].material.shader = g_WeaponTexShader;
                m_MeshList[i].material.SetColor("_Color", a_CalcColor);
            }
        }
        // 몬스터에 추가된 Collider 다시 활성화
        gameObject.GetComponentInChildren<BoxCollider>().enabled = true;
        // 몬스터를 비활성화
        gameObject.SetActive(false);
    }


    void FindDefShader()
    {
        if (m_SMR == null)
        {
            m_SMRList = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            m_MeshList = gameObject.GetComponentsInChildren<MeshRenderer>();
            m_SMR = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

            if (m_SMR != null) g_DefTexShader = m_SMR.material.shader;
            if (0 < m_MeshList.Length) g_WeaponTexShader = m_MeshList[0].material.shader;
        }
    }

    //몬스터 죽는 연출 
    Transform a_Canvas = null;
    IEnumerator DieDirect()
    {
        while (true)
        {
            a_Ratio = m_DieTimer / m_DieDur;
            a_Ratio = Mathf.Min(a_Ratio, 1f);
            a_CalcColor = new Color(1.0f, 1.0f, 1.0f, a_Ratio);

            // 뒤로 밀리게...
            if (0.9f < a_Ratio && 0.0f < m_DieDir.magnitude) transform.position = transform.position + m_DieDir * (((a_Ratio * 0.38f)) * Time.deltaTime);
            if (a_Ratio < 0.83f)
            {
                if (a_Canvas == null) a_Canvas = transform.Find("Canvas");
                if (a_Canvas != null) a_Canvas.gameObject.SetActive(false);
            }

            for (int i = 0; i < m_SMRList.Length; i++)
            {
                if (GameMgr.Inst.g_VertexLitShader != null && m_SMRList[i].material.shader != GameMgr.Inst.g_VertexLitShader) m_SMRList[i].material.shader = GameMgr.Inst.g_VertexLitShader;
                m_SMRList[i].material.SetColor("_Color", a_CalcColor);
            }

            //------------무기
            if (m_MeshList != null)
            {
                for (int i = 0; i < m_MeshList.Length; i++)
                {
                    if (GameMgr.Inst.g_VertexLitShader != null && m_MeshList[i].material.shader != GameMgr.Inst.g_VertexLitShader) m_MeshList[i].material.shader = GameMgr.Inst.g_VertexLitShader;
                    m_MeshList[i].material.SetColor("_Color", a_CalcColor);
                }
            }
            yield return null;
        }
    }

    //애니메이션 이벤트 함수로 호출
    Vector3 a_DistVec = Vector3.zero;
    float   a_CacLen = 0.0f;
    public void Event_AttDamage(string Type)
    {
        if (m_AggroTarget == null) return;

        a_DistVec = m_AggroTarget.transform.position - transform.position;
        a_CacLen = a_DistVec.magnitude;
        a_DistVec.y = 0.0f;

        //공격각도 안에 없는 경우
        if (Vector3.Dot(transform.forward, a_DistVec.normalized) < 0.0f) return;
        //공격 범위 밖에 있는 경우
        if ((attackDist + 1.7f) < a_CacLen) return;

        if (m_RefAnimator != null) m_AggroTarget.GetComponent<Hero_Ctrl>().TakeDamage(null, 5);
        else m_AggroTarget.GetComponent<Hero_Ctrl>().TakeDamage(null, 10);
    }

    //경로 탐색 함수
    Vector3 a_VecLen = Vector3.zero;
    public bool MyNavCalcPath(Vector3 a_StartPos, Vector3 a_TargetPos,ref float a_PathLen)
    { 
        //--- 피킹이 발생된 상황이므로 초기화 하고 계산한다.
        movePath.ClearCorners();                            //경로 모두 제거 
        m_CurPathIndex = 1;                                 //진행 인덱스 초기화 
        m_PathEndPos = transform.position;

        if (nvAgent == null || nvAgent.enabled == false) return false;
        if (NavMesh.CalculatePath(a_StartPos, a_TargetPos, -1, movePath) == false) return false;
        if (movePath.corners.Length < 2) return false;

        for (int i = 1; i < movePath.corners.Length; ++i)
        {
            a_VecLen = movePath.corners[i] - movePath.corners[i - 1];
            a_PathLen = a_PathLen + a_VecLen.magnitude;
        }

        if (a_PathLen <= 0.0f) return false;

        //-- 주인공이 마지막 위치에 도착했을 때 정확한 방향을 바라보게 하고 싶은 경우 때문에 계산해 놓는다.
        m_PathEndPos = movePath.corners[(movePath.corners.Length - 1)];

        return true;
    }

    //--- MoveToPath 관련 변수들...
    private bool a_isSucessed = true;
    private Vector3 a_CurCPos = Vector3.zero;
    private Vector3 a_CacDestV = Vector3.zero;
    private Vector3 a_TargetDir;
    private float a_CacSpeed = 0.0f;
    private float a_NowStep = 0.0f;
    private Vector3 a_Velocity = Vector3.zero;
    private Vector3 a_vTowardNom = Vector3.zero;
    private int a_OldPathCount = 0;
    public bool MoveToPath(float overSpeed = 1.0f)
    {
        a_isSucessed = true;

        if (movePath == null) movePath = new NavMeshPath();

        a_OldPathCount = m_CurPathIndex;

        //최소 m_CurPathIndex = 1 보다 큰 경우에는 캐릭터를 이동시켜 준다
        if (m_CurPathIndex < movePath.corners.Length)
        {
            a_CurCPos = this.transform.position;
            a_CacDestV = movePath.corners[m_CurPathIndex];
            a_CurCPos.y = a_CacDestV.y;
            a_TargetDir = a_CacDestV - a_CurCPos;
            a_TargetDir.y = 0.0f;
            a_TargetDir.Normalize();

            a_CacSpeed = m_MoveVelocity;
            a_CacSpeed = a_CacSpeed * overSpeed;

            a_NowStep = a_CacSpeed * Time.deltaTime; //이번에 이동했을 때 이 안으로만 들어와도 무조건 도착한 것으로 본다.

            a_Velocity = a_CacSpeed * a_TargetDir;
            a_Velocity.y = 0.0f;
            nvAgent.velocity = a_Velocity;

            //중간점에 도착한 것으로 본다
            if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep)
            {
                movePath.corners[m_CurPathIndex] = this.transform.position;
                m_CurPathIndex = m_CurPathIndex + 1;
            }

            //목표점에 도착한 것으로 판정한다.
            m_AddTimeCount = m_AddTimeCount + Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount) m_CurPathIndex = movePath.corners.Length;
        }

        //목적지에 아직 도착 하지 않은 경우 매 프레임
        if (m_CurPathIndex < movePath.corners.Length) 
        {
            //-------------캐릭터 회전 / 애니메이션 방향 조정
            a_vTowardNom = movePath.corners[m_CurPathIndex] - this.transform.position;
            a_vTowardNom.y = 0.0f;
            a_vTowardNom.Normalize();

            //로테이션에서는 모두 들어가야 한다.
            if (0.0001f < a_vTowardNom.magnitude)  
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(a_vTowardNom);
                transform.rotation = Quaternion.Slerp(transform.rotation,a_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            MySetAnim(anim.Move.name, 0.12f);
        }
        //최종 목적지에 도착한 경우 매 프레임
        else
        {
            //최종 목적지에 도착한 경우 한번 발생시키기 위한 부분
            if (a_OldPathCount < movePath.corners.Length) MySetAnim(anim.Idle.name, 0.13f);
            a_isSucessed = false; //아직 목적지에 도착하지 않았다면 다시 잡아 줄 것이기 때문에... 
        }
        return a_isSucessed;
    }

    //마우스 피킹이동 취소 함수
    void ClearMsPickPath() 
    {
        //----피킹을 위한 동기화 부분
        m_PathEndPos = transform.position;

        //경로 모두 제거 
        if (0 < movePath.corners.Length) movePath.ClearCorners();

        //진행 인덱스 초기화
        m_CurPathIndex = 1;       
    }
}
