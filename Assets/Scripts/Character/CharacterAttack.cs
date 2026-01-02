using UnityEngine;
using MyGameEnums;
using System.Collections;

// 플레이어의 공격을 감지하는 스크립트

public class CharacterAttack : MonoBehaviour
{
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private Collider2D frontAttackRange;
    [SerializeField] private Collider2D downAttackRange;

    [SerializeField] private GameObject sparkPrefab;        // 공격 파티클 프리팹

    [Header("Combat Settings")]
    [SerializeField] private float recoilForce = 11.0f; // 반동 힘

    // 참조변수들
    SpriteRenderer swordSR;
    PlayerStatus status;
    Transform swordPivot;
    GameObject sword;
    GameObject effect;
    SwordEffectPos effectScript;
    Rigidbody2D rigidbody;

    bool isHitStop = false;

    private void Awake()
    {
        // 참조변수 초기화
        status = GetComponent<PlayerStatus>();
        swordPivot = transform.Find("SwordPos");

        frontAttackRange.enabled = false;
        downAttackRange.enabled = false;

        // 검의 상태 초기화
        sword = Instantiate(swordPrefab, swordPivot);
        swordSR = sword.GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody2D>();

        effect = sword.transform.Find("SwordEffect").gameObject;
        effectScript = effect.GetComponent<SwordEffectPos>();

        SetSwordVisuals(false);
        effect.SetActive(false);

        status.OnPlayerDeathStart.AddListener(HideSword);
    }

    void Update()
    {
        if(!status.isAttacking)
        {
            SetSwordFacing();
        }

        InputDetection();
    }

    // 검이 어떻게 보여질지 설정
    private void SetSwordVisuals(bool isAttacking)
    {
        if(isAttacking)
        {
            // 검을 등 뒤에 꽂는 느낌
            swordSR.flipX = true;
            swordSR.flipY = false;
        }
        else
        {
            //검을 뽑은 느낌
            swordSR.flipX = false;
            swordSR.flipY = true;
        }
    }

    // 검이 바라보는 방향
    private void SetSwordFacing()
    {
        // 검이 향하고 있을 방향
        swordPivot.localScale = Vector3.one;
        swordPivot.localRotation = Quaternion.identity;
    }

    // 공격종료 후 검의 상태 리셋
    private void ResetAttactState()
    {
        swordPivot.localRotation = Quaternion.identity;
        sword.transform.localPosition = Vector3.zero;
        frontAttackRange.enabled = false;
        downAttackRange.enabled = false;
        effect.SetActive(false);
        SetSwordVisuals(false);
    }

    // 입력(키보드) 감지
    private void InputDetection()
    {
        if(!status.canAttack)
        {
            return;
        }

        // 아래방향키와 S키를 동시에 눌렀을때, 공중에 있다면 하단공격 발동
        if (Input.GetKeyDown(KeyCode.S))
        {
            bool isDown;
            if (!status.onGrounded && Input.GetKey(KeyCode.DownArrow))
            {
                // 하단공격
                Debug.Log("DownAttack");

                isDown = true;
                StartCoroutine(SwingSword(-60.0f, -170.0f, isDown));
            }
            else
            {
                // 전방공격
                Debug.Log("Attack");

                isDown = false;
                StartCoroutine(SwingSword(60.0f, -90.0f, isDown));
            }
        }
    }

    // startAngle 검을 휘두를 시작 각도, endAngle, 끝 각도
    private IEnumerator SwingSword(float startAngle, float endAngle, bool isDown)
    {
        status.canAttack = false;
        status.isAttacking = true;

        float elapsed = 0.0f;   // 경과한 시간
        float duration = 0.1f;  // 휘두르기 지속시간
        float cooldown = 0.3f;  // 기본공격 쿨타임

        // 공격상태 설정
        SetSwordVisuals(true);
        PrepareAttack(isDown);

        while (elapsed < duration)
        {
            // 경과시간 저장
            elapsed += Time.deltaTime;

            // 진행도
            float progress = elapsed / duration;

            // 회전시키기 Lerp로 부드럽게 회전
            float currentAngle = Mathf.Lerp(startAngle, endAngle, progress);

            swordPivot.localRotation = Quaternion.Euler(0, 0, currentAngle);

            yield return null;
        }

        // 공격종료, 기본상태로 되돌림
        ResetAttactState();
        status.isAttacking = false;

        yield return new WaitForSeconds(cooldown);
        status.canAttack = true;
    }

    private void PrepareAttack(bool isDown)
    {
        // 공격판정의 콜라이더 및 이펙트 설정
        if(isDown)
        {
            downAttackRange.enabled = true;
            effectScript.SetAttackDirection(isDown);
        }
        else
        {
            frontAttackRange.enabled = true;
            effectScript.SetAttackDirection(isDown);
        }

        effect.SetActive(true);
        sword.transform.localPosition = new Vector3(0.75f, 0.3f, 0.0f);
    }

    // 데미지를 주는 로직
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        // 몬스터, 철퇴, 가시 충돌
        if( obj.CompareTag( TagName.Monster.GetTag() ) 
            || obj.CompareTag( TagName.Mace.GetTag() ) 
            || obj.CompareTag( TagName.Spike.GetTag() ))
        {
            Debug.Log("Player Attack");

            // 히트스탑 (타격 성공시 정지) 코루틴
            if (!isHitStop)
            {
                StartCoroutine(HitStop(0.15f));
            }

            // 파티클 스파크 생성
            Vector3 contactPoint = collision.ClosestPoint(collision.transform.position);
            GameObject spark = Instantiate(sparkPrefab, contactPoint, Quaternion.identity);
            Destroy(spark, 0.5f); // 0.5초 뒤 자동 삭제

            // 땅위에 있지 않을 때만 튀어오르기 
            if (!status.onGrounded)
            {
                status.canDash = true;
                status.canDoubleJump = true;

                // 위로 튕겨오르기
                rigidbody.linearVelocity = new Vector2(rigidbody.linearVelocity.x, 0);
                rigidbody.AddForce(Vector2.up * recoilForce, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator HitStop(float duration)
    {
        isHitStop = true;

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;

        isHitStop = false;
    }

    private void HideSword()
    {
        StopAllCoroutines();

        sword.SetActive(false);
    }
}
