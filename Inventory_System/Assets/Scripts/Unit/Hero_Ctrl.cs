using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System;

public enum CtrlMode
{
    Immediate,
    S_Lock,     //스킬 예약
    AS_Lock     //공격, 스킬 예약
}

public class Hero_Ctrl : MonoBehaviour
{
    //------------------------ HP바 표시
    [HideInInspector] public float Curhp = 100;
    [HideInInspector] public float Maxhp = 100;
    public Image imgHpbar;

    float m_MoveVelocity = 5.0f;                        //평면 초당 이동 속도...

    //---------- 키보드 입력값 변수 선언
    float h = 0, v = 0;
    Vector3 MoveNextStep;                               //보폭을 계산해 주기 위한 변수
    Vector3 MoveHStep;
    Vector3 MoveVStep;

    float a_CalcRotY = 0.0f;
    float rotSpeed = 150.0f;                            //초당 150도 회전하라는 속도

    //------ JoyStick 이동 처리 변수
    private float m_JoyMvLen = 0.0f;
    private Vector3 m_JoyMvDir = Vector3.zero;

    //------ Picking 관련 변수 
    private Vector3 m_MoveDir = Vector3.zero;            //평면 진행 방향
    protected float m_RotSpeed = 7.0f;                   //초당 회전 속도

    private bool m_isPickMvOnOff = false;                //피킹 이동 OnOff
    private Vector3 m_TargetPos = Vector3.zero;          //최종 목표 위치
    private double m_MoveDurTime = 0.0;                  //목표점까지 도착하는데 걸리는 시간
    private double m_AddTimeCount = 0.0;                 //누적시간 카운트 
    Vector3 a_StartPos = Vector3.zero;
    Vector3 a_CacLenVec = Vector3.zero;
    Quaternion a_TargetRot;

    public Anim anim;
    Animator m_RefAnimator = null;
    AnimatorStateInfo animaterStateInfo;
    string m_prevState = "";

    //------ 공격시 방향전환용 변수들
    GameObject[] m_EnemyList = null;

    float   m_AttackDist = 1.9f;
    private GameObject m_TargetUnit = null;

    Vector3 a_CacTgVec = Vector3.zero;
    Vector3 a_CacAtDir = Vector3.zero;                  //공격시 방향전환용 변수

    //------ 데미지 계산용 변수들...
    float a_fCacLen = 0.0f;
    int iCount = 0;
    GameObject a_EffObj = null;
    Vector3 a_EffPos = Vector3.zero;

    //-------------데미지 칼라 관련 변수
    private Shader g_DefTexShader = null;
    private Shader g_WeaponTexShader = null;

    private bool AttachColorChange = false;
    private SkinnedMeshRenderer m_SMR = null;
    private SkinnedMeshRenderer[] m_SMRList = null;
    private MeshRenderer[] m_MeshList = null;           //장착 무기
    private float AttachColorStartTime = 0f;
    private float AttachColorTime = 0.2f;
    private float a_Ratio = 0.0f;
    private float a_fCol = 0.0f;
    private float a_DamageColor = 0.73f;
    private Color a_CalcColor;

    //---------------------- 마우스 피킹 이동 예약
    public CtrlMode m_CtrlMode = CtrlMode.S_Lock;
    private float m_RsvPicking = 0.0f;                          //reservation 예약 //피킹 이동 예약
    private Vector3 m_RsvTargetPos = Vector3.zero;              //최종 목표 위치
    private double m_RsvMvDurTime = 0.0;                        //목표점까지 도착하는데 걸리는 시간
    private GameObject m_RsvTgUnit = null;                      //타겟 유닛도 예약해 둔다.

    bool m_AttRotPermit = false;

    //---- Navigation
    protected NavMeshAgent nvAgent;
    protected NavMeshPath movePath;

    protected Vector3 m_PathEndPos = Vector3.zero;
    [HideInInspector] public int m_CurPathIndex = 1;

    // ---- Skill
    public BoxCollider m_SkillCollider = null;
    bool m_SkillOn = false;
    
    public bool SkillOn() => m_SkillCollider.enabled = true;
    public bool SkillOff() => m_SkillCollider.enabled = false;
    public void SetColliderCenter(int _X, int _Y, int _Z)
    {
        if (m_SkillCollider == null || m_SkillCollider.enabled == false) return;
        m_SkillCollider.center = new Vector3(_X, _Y, _Z);
    }
    public void SetColliderSize(int _X, int _Y, int _Z)
    {
        if (m_SkillCollider == null || m_SkillCollider.enabled == false) return;
        m_SkillCollider.size = new Vector3(_X, _Y, _Z);
    }

    void Start()
    {
        //주인공 고유 변수 초기화
        Maxhp = 1000;
        Curhp = Maxhp;

        m_RefAnimator = this.gameObject.GetComponent<Animator>();

        movePath = new NavMeshPath();
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        nvAgent.updateRotation = false;

        GameMgr.m_refHero = this;

        FindDefShader();
        AttachColorTime = 0.1f;     //피격을 짧게 준다.
    }

    bool a_AutoControl = false;
    void Update()
    {
        if (0.0f < m_RsvPicking) m_RsvPicking -= Time.deltaTime;

        AttackRotUpdate();          
        KeyBDMove();
        JoyStickMvUpdate();
        MousePickUpdate();

        DamageColorUpdate();

        if (GameMgr.Inst.m_AutoHunt == true)
        {
            if (m_TargetUnit == null) AutoHunt_FindTarget();
            if (m_TargetUnit != null)
            {
                if (((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen || m_isPickMvOnOff == true))
                {
                    MySetAnim(AnimState.move);
                    m_RsvPicking = 0.5f;
                    return;
                }
                else if(m_RsvPicking <= 0.0f) MousePicking(m_TargetUnit.transform.position, m_TargetUnit);
            }
            a_AutoControl = true;
        }
        else if(GameMgr.Inst.m_AutoHunt == false && a_AutoControl == true)
        {
            m_TargetUnit = null;
            ClearMsPickPath();
            a_AutoControl = false;
        }
    }

    void KeyBDMove()      //키보드 이동
    {
        h = Input.GetAxisRaw("Horizontal"); 
        v = Input.GetAxisRaw("Vertical");   

        if (v < 0.0f)
            v = 0.0f;

        if (0.0f != h || 0.0f != v) //키보드 이동처리
        {
            if (m_CtrlMode == CtrlMode.AS_Lock) if (ISAttack() == true) return;
            else if (m_CtrlMode == CtrlMode.S_Lock) if (ISSkill() == true) return;

            //-------- 일반적인 이동 계산법
            a_CalcRotY = transform.eulerAngles.y;
            a_CalcRotY = a_CalcRotY + (h * rotSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0.0f, a_CalcRotY, 0.0f);

            MoveVStep = transform.forward * v;

            //---------------- 네비게이션 메시를 이용한 이동 방법
            MoveNextStep = MoveVStep;
            MoveNextStep = MoveNextStep.normalized * m_MoveVelocity;
            MoveNextStep.y = 0.0f;
            nvAgent.velocity = MoveNextStep;

            MySetAnim(AnimState.move);
            ClearMsPickPath();
        }
        else
        {
            //키보드 이동중이 아닐 때만 아이들 동작으로 돌아가게 한다.
            if (m_isPickMvOnOff == false && m_JoyMvLen <= 0.0f && ISAttack() == false) MySetAnim(AnimState.idle);
        }
    }

    public void MySetAnim(AnimState newAnim, float CrossTime = 1.0f)
    {
        if (m_RefAnimator == null) return;
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState)) if (m_prevState.ToString() == newAnim.ToString()) return;
        if (!string.IsNullOrEmpty(m_prevState))
        {
            m_RefAnimator.ResetTrigger(m_prevState.ToString());
            m_prevState = null;
        }

        // 모든 애니메이션이 시작할 때 우선 꺼주고
        m_AttRotPermit = false;

        //가운데는 Layer Index, 뒤에 0f는 처음부터 다시시작
        if (0.0f < CrossTime) m_RefAnimator.SetTrigger(newAnim.ToString());
        else m_RefAnimator.Play(newAnim.ToString(), -1, 0f);

        //이전스테이트에 현재스테이트 저장
        m_prevState = newAnim.ToString(); 
    }


    Vector3 a_CacCamVec = Vector3.zero;
    Vector3 a_RightDir = Vector3.zero;
    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        m_JoyMvLen = a_JoyMvLen;
        if (0.0f < a_JoyMvLen)
        {
            //--------카메라가 바라보고 있는 전면을 기준으로 회전 시켜줘야 한다. 
            a_CacCamVec = Camera.main.transform.forward;
            a_CacCamVec.y = 0.0f;
            a_CacCamVec.Normalize();
            m_JoyMvDir = a_CacCamVec * a_JoyMvDir.y;                         //위 아래 조작(카메라가 바라보고 있는 기준으로 위, 아래로 얼만큼 이동시킬 것인지?)
            a_RightDir = Camera.main.transform.right;                        //Vector3.Cross(Vector3.up, a_CacCamVec).normalized;
            m_JoyMvDir = m_JoyMvDir + (a_RightDir * a_JoyMvDir.x);           //좌우 조작(카메라가 바라보고 있는 기준으로 좌, 우로 얼만큼 이동시킬 것인지?)
            m_JoyMvDir.y = 0.0f;
            m_JoyMvDir.Normalize();

            //마우스 피킹 이동 취소
            ClearMsPickPath();
        }
        //공격 애니메이션 중이면 공격 애니메이션이 끝나고 숨쉬기 애니로 돌아가게 한다.
        if (a_JoyMvLen == 0.0f) if (m_isPickMvOnOff == false && ISAttack() == false) MySetAnim(AnimState.idle);
    }

    void JoyStickMvUpdate()
    {
        if (0.0f != h || 0.0f != v) return;

        //--- 조이스틱 코드
        if (0.0f < m_JoyMvLen)
        {
            if (m_CtrlMode == CtrlMode.AS_Lock) if (ISAttack() == true) return;
            else if (m_CtrlMode == CtrlMode.S_Lock) if (ISSkill() == true) return;

            m_MoveDir = m_JoyMvDir;

            float amtToMove = m_MoveVelocity * Time.deltaTime;
            //캐릭터 스프링 회전 
            if (0.0001f < m_JoyMvDir.magnitude)
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(m_JoyMvDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot,Time.deltaTime * m_RotSpeed);
            }
            //캐릭터 스프링 회전   

            //---------------- 네비게이션 메시를 이용한 이동 방법
            MoveNextStep = m_JoyMvDir * m_MoveVelocity;
            MoveNextStep.y = 0.0f;
            nvAgent.velocity = MoveNextStep;

            MySetAnim(AnimState.move);
        }     
    }

    //마우스 좌측버튼 클릭시 호출될 함수
    public void MousePicking(Vector3 a_SetPickVec, GameObject a_PickMon = null)
    {
        a_StartPos = this.transform.position; //출발 위치    

        a_SetPickVec.y = this.transform.position.y; // 최종 목표 위치

        a_CacLenVec = a_SetPickVec - a_StartPos;
        a_CacLenVec.y = 0.0f;

        //------- Picking Enemy 공격 처리 부분
        if (a_PickMon != null)
        {
            a_CacTgVec = a_PickMon.transform.position - transform.position;

            float a_AttDist = m_AttackDist; 
            //지금 공격하려고 하는 몬스터의 어그로 타겟이 내가 아니면...
            if (a_PickMon.GetComponent<MonsterCtrl>().m_AggroTarget == this.gameObject) a_AttDist = m_AttackDist + 1.0f;

            if (a_CacTgVec.magnitude <= a_AttDist)   
            {
                m_TargetUnit = a_PickMon;
                AttackOrder();  //즉시 공격
                return;
            }
        }

        //너무 근거리 피킹은 스킵해 준다.
        if (a_CacLenVec.magnitude < 0.5f) return;

        //---네비게이션 메쉬 길찾기를 이용할 때 코드
        float a_PathLen = 0.0f;
        if (MyNavCalcPath(a_StartPos, a_SetPickVec, ref a_PathLen) == false) return;

        m_TargetPos = a_SetPickVec;                             // 최종 목표 위치
        m_isPickMvOnOff = true;                                 //피킹 이동 OnOff

        m_MoveDir = a_CacLenVec.normalized;
        //---네비게이션 메쉬 길찾기를 이용했을 때 거리 계산법
        m_MoveDurTime = a_PathLen / m_MoveVelocity;             //도착하는데 걸리는 시간
        m_AddTimeCount = 0.0;
        m_TargetUnit = a_PickMon; //타겟 초기화 또는 무효화 

        //공격 중일 때 몬스터를 계속 클릭하면 다음 공격이 예약 되도록...
        //근처에 공격할 몬스터가 있다면....
        if (m_CtrlMode == CtrlMode.AS_Lock)
        {
            if (ISAttack() == true)
            {
                m_RsvPicking = 3.5f;  //지금 이 예약의 유효시간 3.5초 뒤에 무효화 됨
                m_RsvTargetPos = m_TargetPos;
                m_RsvMvDurTime = m_MoveDurTime;
                m_RsvTgUnit = a_PickMon;
                ClearMsPickPath();    //m_isPickMvOnOff = false
                m_MoveDurTime = 0.0f;
            }
        }
        else if (m_CtrlMode == CtrlMode.S_Lock)
        {
            if (ISSkill() == true)
            {
                m_RsvPicking = 3.5f;  //지금 이 예약의 유효시간 3.5초 뒤에 무효화 됨
                m_RsvTargetPos = m_TargetPos;
                m_RsvMvDurTime = m_MoveDurTime;
                m_RsvTgUnit = a_PickMon;
                ClearMsPickPath();    //m_isPickMvOnOff = false
                m_MoveDurTime = 0.0f;
            }
        }
    }

    void MousePickUpdate()  //<--- MousePickMove()
    {
        ////-------------- 마우스 피킹 이동
        if (m_isPickMvOnOff == true)
        {
            //---네비게이션 메쉬 길찾기를 이용할 때 코드
            m_isPickMvOnOff = MoveToPath();                 //도착한 경우 false 리턴함

            //------ 타겟을 향해 피킹 이동 공격
            if (m_TargetUnit != null)
            { 
                // 공격 애니매이션 중이면 가장 가까운 타겟을 자동으로 잡게된다.
                a_CacTgVec = m_TargetUnit.transform.position - this.transform.position;
                if (a_CacTgVec.magnitude <= m_AttackDist) AttackOrder();
            }
        }
    }

    void AutoHunt_FindTarget()
    {
        m_TargetUnit = null;  //우선 타겟 무효화

        if (m_EnemyList != null) Array.Clear(m_EnemyList, 0, m_EnemyList.Length);

        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");

        float a_MinLen = float.MaxValue;
        iCount = m_EnemyList.Length;
        for (int i = 0; i < iCount; ++i)
        {
            a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_CacTgVec.y = 0.0f;

            if (a_CacTgVec.magnitude < a_MinLen)
            {
                a_MinLen = a_CacTgVec.magnitude;
                m_TargetUnit = m_EnemyList[i];
            }
        }
    }

    //마우스 픽킹이동 취소 함수
    void ClearMsPickPath() 
    {
        m_isPickMvOnOff = false;

        //----피킹을 위한 동기화 부분
        m_PathEndPos = transform.position;

        //경로 모두 제거 
        if (0 < movePath.corners.Length) movePath.ClearCorners();

        //진행 인덱스 초기화
        m_CurPathIndex = 1;       

        if (GameMgr.Inst.m_CursorMark != null) GameMgr.Inst.m_CursorMark.SetActive(false);
    }

    public void AttackOrder()
    {
        if (m_prevState == AnimState.idle.ToString() || m_prevState == AnimState.move.ToString())
        {
            if ((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen) return;

            MySetAnim(AnimState.attack);
            ClearMsPickPath();
        }
    }

    public bool ISAttack()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState)) if (m_prevState.ToString() == AnimState.attack.ToString() || m_prevState.ToString() == AnimState.skill.ToString()) return true;
        return false;
    }

    public bool ISSkill()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState)) if (m_prevState.ToString() == AnimState.skill.ToString()) return true;
        return false;
    }

    // 공격 애니메이션 판단 함수
    public void IsAttackFinish(string Type)
    {
        //키보드 이동조작이 있거나, 조이스틱 조작이 있는 경우
        if ((0.0f != h || 0.0f != v) || 0.0f < m_JoyMvLen|| m_isPickMvOnOff == true)
        { 
            MySetAnim(AnimState.move);
            m_RsvPicking = 0.0f;
            return;
        }
        //마우스 피킹 예약이 있었다면...
        else if (0.0f < m_RsvPicking) 
        {
            //--- 네비게이션 메쉬 길찾기를 이용할 때 코드
            m_TargetUnit = m_RsvTgUnit;                     //타겟도 바꾸거나 무효화 시켜 버린다.
            a_StartPos = this.transform.position;           //출발 위치     
            m_TargetPos = m_RsvTargetPos;
            m_TargetPos.y = this.transform.position.y;      // 최종 목표 위치

            float a_PathLen = 0.0f;
            if (MyNavCalcPath(a_StartPos, m_TargetPos, ref a_PathLen) == true)
            {
                m_isPickMvOnOff = true;                     //피킹 이동 OnOff
                a_CacLenVec = m_TargetPos - a_StartPos;
                a_CacLenVec.y = 0.0f;
                m_MoveDir = a_CacLenVec.normalized;
                m_MoveDurTime = a_PathLen / m_MoveVelocity; //도착하는데 걸리는 시간
                m_AddTimeCount = 0.0;
            }
            MySetAnim(AnimState.move);
            return;
        }

        //Skill상태일때는 Skill상태로 끝나야 하고 
        //Attack상태일대는 Attack상태로 끝나야 하고 
        //상태는 Skill인데 Attack애니메이션 끝이 들어온 경우라면 제외시켜버린다.
        if (m_prevState.ToString() == AnimState.skill.ToString() && Type == AnimState.attack.ToString()) return;
        if (m_prevState.ToString() == AnimState.attack.ToString() && Type == AnimState.skill.ToString()) return;


        //공격 거리 안에 타겟이 있느냐?
        if (IsTargetEnemyActive(0.2f) == true)
        {
            //공격 애니메이션으로 끝난 경우 자동 공격 애니메이션을 하게 하고 싶은 경우
            if (m_prevState.ToString() == AnimState.attack.ToString())
            {
                //공격으로 끝났으면 공격 애니메이션을 취소하고 공격을 다시 시도한다.
                if (!string.IsNullOrEmpty(m_prevState) && m_RefAnimator != null) m_RefAnimator.ResetTrigger(m_prevState.ToString());
                //강제 어택이 들어가도록...
                m_prevState = null;
            }
            MySetAnim(AnimState.attack);  //다시 공격 애니
            ClearMsPickPath();
        }
        else MySetAnim(AnimState.idle);
    }

    //공격애니메이션 중일 때 타겟을 향해 회전하게 하는 함수
    float a_CacRotSpeed = 0.0f;
    float a_CacRate = 0.0f;
    public void AttackRotUpdate()
    {
        //마우스 피킹을 시도했고 이동 중이면 타겟을 다시 잡지 않는다.
        if (m_isPickMvOnOff == true) return;

        //보간 때문에 정확히 정밀한 공격 애니메이션만 하고 있을 때만...
        if (ISAttack() == false || IsAttAniData() == false) return;

        FindEnemyTarget();

        a_CacRotSpeed = m_RotSpeed * 3.0f;           //초당 회전 속도

        //타겟이 살아있고 공격 거리 안쪽에 있을 때만 회전 필요
        if (IsTargetEnemyActive(1.0f) == true)
        {
            //--- 회전 허용 여부 판단 코드
            a_CacRate = animaterStateInfo.normalizedTime - (float)((int)animaterStateInfo.normalizedTime);

            //내가 공격 애니메이션 진행 중인 상태에서 몬스터가 내 공격거리 안쪽으로 들어온 경우
            if (a_CacRate <= 0.3f) m_AttRotPermit = true;
            else if (m_AttRotPermit == false) return;

            a_CacTgVec = m_TargetUnit.transform.position - transform.position;
            a_CacTgVec.y = 0.0f;

            //캐릭터 스프링 회전   
            a_CacAtDir = a_CacTgVec.normalized;
            if (0.0001f < a_CacAtDir.magnitude)
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(a_CacAtDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * a_CacRotSpeed);
            }
        }
    }

    bool IsTargetEnemyActive(float a_ExtLen = 0.0f)
    {
        //타겟이 존재한다고 하더라도...
        if (m_TargetUnit != null)
        {
            if (m_TargetUnit.activeSelf == false)
            {
                m_TargetUnit = null;
                return false;
            }

            //isDie 죽어 있어도
            MonsterCtrl a_Unit = m_TargetUnit.GetComponent<MonsterCtrl>();
            if (a_Unit.MonState == AnimState.die)
            {
                m_TargetUnit = null;
                return false;
            }

            a_CacTgVec = m_TargetUnit.transform.position - transform.position;
            a_CacTgVec.y = 0.0f;
            //공격거리 바깥쪽에 있을 경우도 타겟을 무효화 해 버린다.
            if (m_AttackDist + a_ExtLen < a_CacTgVec.magnitude) return false;
            return true;
        }
        return false;
    }

    public bool IsAttAniData()
    {
        if (m_RefAnimator != null)
        {
            //첫번째 레이어
            animaterStateInfo = m_RefAnimator.GetCurrentAnimatorStateInfo(0);  
            if (animaterStateInfo.IsName(anim.Attack1.name) == true || animaterStateInfo.IsName(anim.Skill1.name) == true) return true;
        }
        return false;
    }

    void Event_AttDamage(string Type)
    {
        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");
        iCount = m_EnemyList.Length;

        //공격 애니메이션이면...
        if (Type == AnimState.attack.ToString())
        {
            //---------주변 모든 몬스터를 찾아서 데이지를 준다.(범위공격)
            for (int i = 0; i < iCount; ++i)
            {
                a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
                a_fCacLen = a_CacTgVec.magnitude;
                a_CacTgVec.y = 0.0f;

                //공격각도 안에 있는 경우
                //70도 정도 //-0.7f) //-45도를 넘는 범위에 있다는 뜻
                if (Vector3.Dot(transform.forward, a_CacTgVec.normalized) < 0.45f) continue;

                //공격 범위 밖에 있는 경우
                if (m_AttackDist + 0.1f < a_fCacLen) continue;

                a_EffObj = EffectPool.Inst.GetEffectObj("FX_Hit_01",Vector3.zero, Quaternion.identity);
                a_EffPos = m_EnemyList[i].transform.position;
                a_EffPos.y += 1.1f; 
                a_EffObj.transform.position = a_EffPos + (-a_CacTgVec.normalized * 1.13f);
                a_EffObj.transform.LookAt(a_EffPos + (a_CacTgVec.normalized * 2.0f));
                m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject);
            }
        }
        else if (Type == AnimState.skill.ToString())
        {
            a_EffObj = EffectPool.Inst.GetEffectObj("FX_AttackCritical_01",Vector3.zero, Quaternion.identity);
            a_EffPos = transform.position;
            a_EffPos.y = a_EffPos.y + 1.0f;
            a_EffObj.transform.position = a_EffPos + (transform.forward * 2.3f);
            a_EffObj.transform.LookAt(a_EffPos + (-transform.forward * 2.0f));  

            //---------주변 모든 몬스터를 찾아서 데이지를 준다.(범위공격)
            for (int i = 0; i < iCount; ++i)
            {
                a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
                a_CacTgVec.y = 0.0f;

                //공격 범위 밖에 있는 경우
                if (m_AttackDist + 0.1f < a_CacTgVec.magnitude) continue;

                a_EffObj = EffectPool.Inst.GetEffectObj("FX_Hit_01",
                                        Vector3.zero, Quaternion.identity);
                a_EffPos = m_EnemyList[i].transform.position;
                a_EffPos.y += 1.1f; //1.0f;
                a_EffObj.transform.position = a_EffPos + (-a_CacTgVec.normalized * 1.13f); //0.7f);
                a_EffObj.transform.LookAt(a_EffPos + (a_CacTgVec.normalized * 2.0f));
                m_EnemyList[i].GetComponent<MonsterCtrl>().TakeDamage(this.gameObject, 50);
            }
        }
    }

    public bool TrySkill() => m_SkillOn = true;

    public void Event_SkillDamage(ActiveSkill _ActiveSkill)
    {
        if (m_SkillEnemyList.Count <= 0) return;
        if (m_SkillOn == false) return;

        for (int i = 0; i < m_SkillEnemyList.Count; i++)
        {
            for(int j = 1; j < _ActiveSkill.AttackNum; j++)
            {
                // 나중에 유저 공격력 * _ActiveSkill.Damage 수치 조절이 필요
                m_SkillEnemyList[i].TakeDamage(this.gameObject,_ActiveSkill.Damage,_ActiveSkill.AttackNum);
                Debug.Log(_ActiveSkill.SkName);
            }
        }
        m_SkillOn = false;
    }

    public void TakeDamage(GameObject a_Attacker = null, float a_Damage = 10.0f)
    {
        if (Curhp <= 0.0f) return;
        Curhp -= a_Damage;
        if (Curhp < 0.0f) Curhp = 0.0f;

        SetAttachColor();

        imgHpbar.fillAmount = (float)Curhp / (float)Maxhp;

        GameMgr.Inst.SpawnDamageTxt((int)a_Damage, this.transform, 1);

        if (Curhp <= 0) GameMgr.Inst.TestDie = true;
    }

    protected virtual void SetAttachColor()
    {
        AttachColorChange = true;
        AttachColorStartTime = Time.time;
    }

    private void FindDefShader()
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

    protected virtual void DamageColorUpdate()
    {
        FindDefShader();

        if (this.gameObject.activeSelf == false) return;
        if (AttachColorChange == false) return;


        a_Ratio = (Time.time - AttachColorStartTime) / AttachColorTime;
        a_Ratio = Mathf.Min(a_Ratio, 1f);
        a_fCol = a_DamageColor; 
        a_CalcColor = new Color(a_fCol, a_fCol, a_fCol);  

        if (a_Ratio >= 1f)
        {
            for (int i = 0; i < m_SMRList.Length; i++)
            {
                if (g_DefTexShader != null) m_SMRList[i].material.shader = g_DefTexShader;
            }

            //------------무기
            if (m_MeshList != null)
            {
                for (int i = 0; i < m_MeshList.Length; i++)
                {
                    if (g_WeaponTexShader != null) m_MeshList[i].material.shader = g_WeaponTexShader;
                }
            }
            AttachColorChange = false;
        }
        else
        {
            for (int i = 0; i < m_SMRList.Length; i++)
            {
                if (GameMgr.Inst.g_AddTexShader != null && m_SMRList[i].material.shader != GameMgr.Inst.g_AddTexShader) m_SMRList[i].material.shader = GameMgr.Inst.g_AddTexShader;
                m_SMRList[i].material.SetColor("_AddColor", a_CalcColor);
            }

            //------------무기
            if (m_MeshList != null)
            {
                for (int i = 0; i < m_MeshList.Length; i++)
                {
                    if (GameMgr.Inst.g_AddTexShader != null && m_MeshList[i].material.shader != GameMgr.Inst.g_AddTexShader) m_MeshList[i].material.shader = GameMgr.Inst.g_AddTexShader;
                    m_MeshList[i].material.SetColor("_AddColor", a_CalcColor);
                }
            }
        }
    }

    public void SkillOrder(string Type, ref float Cooltime, ref float CoolType)
    {
        if (m_prevState != AnimState.skill.ToString())
        {
            MySetAnim(AnimState.skill);

            ClearMsPickPath();

            Cooltime = 7.0f;
            CoolType = Cooltime;     //쿨타임 적용
        }

    }

    void FindEnemyTarget()
    {
        //타겟의 교체는 공격거리보다는 조금 더 여유를 두고 바꾸게 한다.
        if (IsTargetEnemyActive(0.5f) == true) return;
        //if (m_EnemyList.Length != 0) Array.Clear(m_EnemyList, 0, m_EnemyList.Length);

        m_EnemyList = GameObject.FindGameObjectsWithTag("Enemy");

        float a_MinLen = float.MaxValue;
        iCount = m_EnemyList.Length;
        m_TargetUnit = null;  //우선 타겟 무효화
        for (int i = 0; i < iCount; ++i)
        {
            a_CacTgVec = m_EnemyList[i].transform.position - transform.position;
            a_CacTgVec.y = 0.0f;

            //공격거리 안쪽에 있을 경우만 타겟으로 잡는다.
            if (a_CacTgVec.magnitude <= m_AttackDist)
            {
                if (a_CacTgVec.magnitude < a_MinLen)
                {
                    a_MinLen = a_CacTgVec.magnitude;
                    m_TargetUnit = m_EnemyList[i];
                }
            }
        }
    }

    //경로 탐색 함수
    Vector3 a_VecLen = Vector3.zero;
    public bool MyNavCalcPath(Vector3 a_StartPos, Vector3 a_TargetPos, ref float a_PathLen)
    {
        //--- 피킹이 발생된 상황이므로 초기화 하고 계산한다.
        movePath.ClearCorners();                    //경로 모두 제거 
        m_CurPathIndex = 1;                     //진행 인덱스 초기화 
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

        //-- 주인공이 마지막 위치에 도착했을 때 정확한 방향을 
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
        if (m_CurPathIndex < movePath.corners.Length)               //최소 m_CurPathIndex = 1 보다 큰 경우에는 캐릭터를 이동시켜 준다.
        {
            a_CurCPos = this.transform.position;
            a_CacDestV = movePath.corners[m_CurPathIndex];
            a_CurCPos.y = a_CacDestV.y;                             //높이 오차가 있어서 도착 판정을 못하는 경우가 있다. 
            a_TargetDir = a_CacDestV - a_CurCPos;
            a_TargetDir.y = 0.0f;
            a_TargetDir.Normalize();

            a_CacSpeed = m_MoveVelocity;
            a_CacSpeed = a_CacSpeed * overSpeed;

            a_NowStep = a_CacSpeed * Time.deltaTime;                //이번에 이동했을 때 이 안으로만 들어와도 무조건 도착한 것으로 본다.

            a_Velocity = a_CacSpeed * a_TargetDir;
            a_Velocity.y = 0.0f;
            nvAgent.velocity = a_Velocity;                          //이동 처리...

            if ((a_CacDestV - a_CurCPos).magnitude <= a_NowStep)    //중간점에 도착한 것으로 본다.  여기서 a_CurCPos == Old Position의미
            {
                movePath.corners[m_CurPathIndex] = this.transform.position;
                m_CurPathIndex = m_CurPathIndex + 1;
            }

            m_AddTimeCount = m_AddTimeCount + Time.deltaTime;
            if (m_MoveDurTime <= m_AddTimeCount)                     //목표점에 도착한 것으로 판정한다.
            {
                m_CurPathIndex = movePath.corners.Length;
            }
        }
        if (m_CurPathIndex < movePath.corners.Length)
        {
            //-------------캐릭터 회전 / 애니메이션 방향 조정
            a_vTowardNom = movePath.corners[m_CurPathIndex] - this.transform.position;
            a_vTowardNom.y = 0.0f;
            a_vTowardNom.Normalize();        

            if (0.0001f < a_vTowardNom.magnitude)
            {
                Quaternion a_TargetRot = Quaternion.LookRotation(a_vTowardNom);
                transform.rotation = Quaternion.Slerp(transform.rotation, a_TargetRot, Time.deltaTime * m_RotSpeed);
            }
            MySetAnim(AnimState.move);
        }
        else
        {
            if (a_OldPathCount < movePath.corners.Length) //최종 목적지에 도착한 경우 한번 발생시키기 위한 부분
            {
                ClearMsPickPath();
                if (ISAttack() == false) MySetAnim(AnimState.idle);
            }
            a_isSucessed = false; //아직 목적지에 도착하지 않았다면 다시 잡아 줄 것이기 때문에... 
        }
        return a_isSucessed;
    }


    [HideInInspector] public List<MonsterCtrl> m_SkillEnemyList = new List<MonsterCtrl>();
    MonsterCtrl a_Monster = null;
    // 스킬 데미지 처리 부분
    void OnTriggerEnter(Collider other)
    {
        if (m_SkillCollider.enabled == false) return;
        if (other.CompareTag("Enemy") == false) return;
        if(other.gameObject.TryGetComponent(out a_Monster)) m_SkillEnemyList.Add(a_Monster);    
    }

    void OnTriggerExit(Collider other)
    {
        if (m_SkillCollider.enabled == false) return;
        if (other.CompareTag("Enemy") == false) return;
        if (m_SkillEnemyList.Count <= 0) return;
        if (other.gameObject.TryGetComponent(out MonsterCtrl a_OutMonster)) m_SkillEnemyList.Remove(a_OutMonster);
    }
}
