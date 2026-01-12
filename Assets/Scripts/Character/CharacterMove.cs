using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static PlayerStatus;

// 캐릭터의 이동을 담당하는 스크립트

public class CharacterMove : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    SearchOnPlatform onPlatform;
    PlayerStatus status;

    // 캐릭터의 진행 방향
    float moveDirectionX = 0f;

    // 코요테타임 (점프 조작감 개선)
    float coyoteTimeDuration = 0.1f;
    float coyoteTimeCounter;

    // 공중저항계수
    float airResistance = 3.0f; // 넉백상태로 공중에있을 때 감속할 힘

    // Start is called before the first frame update
    void Start()
    {
        // 플레이어의 스테이터스
        status = GetComponent<PlayerStatus>();

        // 플레이어 발에 붙어있는 오브젝트 -> 플레이어가 땅에 있는지 확인하는 용도
        onPlatform = GetComponentInChildren<SearchOnPlatform>();

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 플레이어가 땅에 붙어있는지 확인하는 이벤트리스너
        onPlatform.OnPlatform.AddListener(OnGround);
        onPlatform.ExitPlatform.AddListener(ExitGround);
}

    // 점프씹힘 방지를 위해 FixedUpdate로 처리
    private void FixedUpdate()
    {
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        // 코요테 타임 카운터 계산
        if (status.onGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration; // 땅에 있으면 게이지 충전
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // 공중이면 시간 감소
        }

        // 대쉬 중 이동조작 막기
        if(!status.isDashing)
        {
            // Update()에서 Move()에 필요한 변수만 저장 -> FixedUpdate()에서 Move()호출
            if (Input.GetKey(KeyCode.RightArrow))
            {
                status.isMoving = true;
                moveDirectionX = 1.0f;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                status.isMoving = true;
                moveDirectionX = -1.0f;
            }
            else
            {
                status.isMoving = false;
                moveDirectionX = 0f;
            }
        }
        

        Jump();
        Dash();
    }

    void Move()
    {
        // 대쉬, 넉백 중일때 방향전환을 제한
        if (status.isDashing || status.isKnockedback)
        {
            return;
        }

        animator.SetBool("isMove", status.isMoving);

        // 바라보는 방향 수정
        if (!status.isAttacking)
        {
            // 공격중일때는 바라보는 방향이 바뀌지 않도록 수정
            if (moveDirectionX > 0)
            {
                transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else if (moveDirectionX < 0)
            {
                transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }
        }
        

        if (status.isMoving)
        {
            status.wasKnockedback = false;  // 키를 누르는 순간 넉백 관성 해제
            rb.linearVelocity = new Vector2(moveDirectionX * status.moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            //  땅 위라면 즉시 정지 (미끄럼 방지)
            if (status.onGrounded)
            {
                rb.linearVelocity = new Vector2(0.0f, rb.linearVelocity.y);
            }
            // 공중인데 넉백 상태가 아니라면 즉시 정지 (공중 조작감 향상)
            else if (!status.wasKnockedback)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            // 넉백 상태로 공중에 있을 때 -> 서서히 속도를 줄임 (감속)
            else
            {
                // 현재 x축 속도를 0으로 부드럽게 보간(Lerp)합니다.
                float dampedX = Mathf.Lerp(rb.linearVelocity.x, 0f, Time.deltaTime * airResistance);

                rb.linearVelocity = new Vector2(dampedX, rb.linearVelocity.y);

                // 속도가 거의 0에 가까워지면 넉백 관성 상태를 끕니다.
                if (Mathf.Abs(dampedX) < 0.1f)
                {
                    status.wasKnockedback = false;
                }
            }
        }
    }

    void Jump()
    {
        // 넉백상태일 때 점프하지 못하도록 막음
        if (status.isKnockedback)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            // coyoteTimeCounter를 체크합니다.
            if (coyoteTimeCounter > 0f)
            {
                status.onGrounded = false;
                coyoteTimeCounter = 0f; // 점프를 한 번 하면 코요테 타임 즉시 종료
                status.canDoubleJump = true;

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // y가속도 초기화
                rb.AddForce(Vector2.up * status.jumpPower);

                animator.SetBool("isJump", true);
                Debug.Log("Coyote Jump!");
            }
            else if (status.canDoubleJump)
            {
                status.canDoubleJump = false;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * status.jumpPower);
                animator.Play("PlayerJump", -1, 0f);
            }
        }

    }

    void Dash()
    {
        // 넉백 중일 때 대쉬하지 못하도록 막음
        if (status.isKnockedback)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.D))
        {
            if (status.canDash && !status.isDashing)
            {
                Debug.Log("Dash");
                StartCoroutine(StartDash());    // 대쉬 코루틴 시작
            }
        }
    }

    // 대쉬 코루틴
    private IEnumerator StartDash()
    {
        status.canDash = false;
        status.isDashing = true;
        moveDirectionX = 0.0f;

        animator.SetBool("isFalling", true);

        // 캐릭터가 바라보는 방향으로 대쉬
        float dashDirection = transform.localScale.x > 0 ? 1.0f : -1.0f;

        // 캐릭터가 받고있는 중력을 저장하고 대쉬중일 때 중력영향을 0으로 설정
        float defaultGravity = rb.gravityScale;
        rb.gravityScale = 0.0f;

        // 대쉬 진행
        rb.linearVelocity = new Vector2(dashDirection * status.dashPower, 0.0f);

        // 대쉬 지속시간만큼 기다림
        yield return new WaitForSeconds(status.dashDuration);

        // 관성 초기화
        rb.linearVelocity = Vector2.zero;

        // 대쉬 직후 코요테 점프를 방지함
        coyoteTimeCounter = 0;

        // 캐릭터를 기본상태로 되돌림
        rb.gravityScale = defaultGravity;
        status.isDashing = false;
        animator.SetBool("isFalling", false);

        if (status.onGrounded)
        {
            status.canDash = true;
        }
    }

    public void Knockback(Vector2 damageSourcePos)
    {
        // 넉백당할 방향을 저장
        float direction = (transform.position.x > damageSourcePos.x) ? 1.0f : -1.0f;  // 충돌한 오브젝트 기준 반대방향으로 밀려남

        // 넉백의 지속시간
        float knockbackDuration = 0.2f;

        // 캐릭터에 가해지는 넉백 파워
        float knockbackX = 10.0f;
        float knockbackY = 8.0f;

        status.wasKnockedback = true;

        Vector2 knockback = new Vector2(knockbackX * direction, knockbackY);

        Rigidbody2D playerRigidbody = gameObject.GetComponent<Rigidbody2D>();

        // 캐릭터에 넉백 파워 가함
        playerRigidbody.linearVelocity = Vector2.zero;
        playerRigidbody.AddForce(knockback, ForceMode2D.Impulse);


        // 캐릭터가 조작하지 못하도록 player에 있는 코루틴 진행
        StartCoroutine(KnockbackRoutine(knockbackDuration));
    }

    // 넉백 코루틴
    private IEnumerator KnockbackRoutine(float duration)
    {
        status.isKnockedback = true;

        // 넉백 중일 때 캐릭터의 조작을 막음
        yield return new WaitForSeconds(duration);

        status.isKnockedback = false;
    }

    void OnGround()
    {
        status.onGrounded = true;
        status.wasKnockedback = false;

        // 유틸리티 초기화
        status.canDash = true;
        status.canDoubleJump = true;

        animator.SetBool("isJump", false);
        animator.SetBool("isFalling", false);
        Debug.Log("Move.OnGround()");
    }


    void ExitGround()
    {
        status.onGrounded = false;

        animator.SetBool("isFalling", true);
        Debug.Log("Move.ExitGround()");
    }



}
