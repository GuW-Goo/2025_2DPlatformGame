using MyGameEnums;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// 공중에 있는 몬스터 스크립트

public class FlyingMonster : MonoBehaviour, IAttackable
{
    // 몬스터의 왕복지점 2개
    [SerializeField] private Transform patrolPointA;
    [SerializeField] private Transform patrolPointB;

    [SerializeField] private GameObject smokeEffect;

    Animator animator;
    GameObject smoke;

    // 몬스터 -> 플레이어 공격 시
    public UnityEvent AttackPlayer = new UnityEvent();

    // 몬스터의 스테이터스
    private float respawnTime = 3.0f;   // 몬스터의 리스폰 대기시간
    private float moveSpeed = 3.0f;     // 몬스터의 이동 속도

    // 몬스터가 목표지점에 도착했다고 판단하는 허용오차
    private float arrivalThreshold = 0.1f;

    // 몬스터의 목표지점
    private Vector3 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();

        // 초기 목표를 A로 설정
        targetPosition = patrolPointA.position;
    }

    private void FixedUpdate()
    {
        // 목표 지점을 향해 이동
        MoveToTarget();

        // 목표지점 도달시 새 목표 지정
        CheckArrivalNSetTarget();
    }

    // 타겟으로 이동하는 함수
    private void MoveToTarget()
    {
        // 목표로 향하는 방향벡터 설정
        Vector2 direction = (targetPosition - transform.position).normalized;

        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

        // 몬스터 이동
        rigidbody.linearVelocity = direction * moveSpeed;
    }

    // 목표지점 도달 확인 및 타겟변경 함수
    private void CheckArrivalNSetTarget()
    {
        // 목표지점까지의 거리를 계산
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 목표 근처에 도달 시 목표지점 변경
        if (distanceToTarget < arrivalThreshold)
        {
            // target이 A였다면 B로 변경
            if (targetPosition == patrolPointA.position)
            {
                targetPosition = patrolPointB.position;
            }
            // target이 B였다면 A로 변경
            else
            {
                targetPosition = patrolPointA.position;
            }
        }
    }

    public void OnHit(Vector2 attackerPoint)
    {
        Dead();
    }

    // 몬스터 사망 처리 후 리스폰
    private void Dead()
    {
        // 리스폰 대기 코루틴 시작
        StartCoroutine(WaitRespawnTime());
    }

    // 리스폰 대기 코루틴
    IEnumerator WaitRespawnTime()
    {
        // 충돌 비활성화
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        this.enabled = false; // FixedUpdate 등 로직 정지

        // 애니메이션 재생
        animator.SetBool("Hit", true);
        yield return new WaitForSeconds(0.5f);
        CreateSmokeAnim();

        // 몬스터 숨기기
        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(respawnTime);

        // 위치 초기화 (B 지점에서 다시 시작)
        transform.position = patrolPointB.position;
        targetPosition = patrolPointA.position;

        // 몬스터 다시 나타내기
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
        this.enabled = true; // 로직 재시작

        CreateSmokeAnim();
        animator.SetBool("Hit", false);

        Debug.Log("Flying Monster Respawn");
    }

    // 연기애니메이션 생성
    private void CreateSmokeAnim()
    {
        GameObject smoke = Instantiate(smokeEffect, transform.position, Quaternion.identity);
        Destroy(smoke, 0.2f);
    }

    // 몬스터 경로를 드로우
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 pointA = patrolPointA.position;
        Vector3 pointB = patrolPointB.position;

        Gizmos.DrawLine(pointA, pointB);

        Gizmos.DrawSphere(pointA, 0.1f);
        Gizmos.DrawSphere(pointB, 0.1f);
    }
}
