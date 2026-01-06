using MyGameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatus : MonoBehaviour
{
    [Header("UI & Effect References")]
    [SerializeField] private HealthIconUI healthIconUI;
    [SerializeField] private GameObject effectPrefab; // 사망 이펙트 프리팹

    [Header("Events")]
    public UnityEvent OnPlayerDied = new UnityEvent();
    public UnityEvent OnPlayerDeathStart = new UnityEvent();

    [Header("Player Stats")]
    [SerializeField] public int maxHealth = 3;
    [SerializeField] public float moveSpeed = 8.5f;
    [SerializeField] public float jumpPower = 630.0f;
    [SerializeField] public float dashPower = 30.0f;
    [SerializeField] public float dashDuration = 0.185f;

    [Header("State Flags")]
    public bool canDoubleJump = false;
    public bool canDash = false;
    public bool canAttack = true;
    public bool onGrounded = false;
    public bool isMoving = false;
    public bool isAttacking = false;
    public bool isDashing = false;
    public bool wasKnockedback = false;
    public bool isKnockedback = false;

    // 내부 관리 변수
    public int currentHealth;
    private SpriteRenderer sr;
    private bool isInvincible = false;
    private float invincibleDuration = 1.5f;
    private float flickerSpeed = 0.1f;

    public Vector2 spawnPos;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (healthIconUI != null)
            healthIconUI.UpdateIcon(currentHealth, maxHealth);
    }

    // 데미지 처리 함수
    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        Debug.Log($"Damaged. CurrentHP: {currentHealth}");

        // UI 업데이트
        if (healthIconUI != null)
            healthIconUI.UpdateIcon(currentHealth, maxHealth);

        // 사망 처리
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
        else
        {
            // 살았으면 무적 코루틴 실행
            StartCoroutine(StartInvincible());
        }

    }

    // 사망 처리 코루틴
    private IEnumerator Die()
    {
        Debug.Log("Die 코루틴 시작");

        currentHealth = 0;
        OnPlayerDeathStart.Invoke();

        // 시각적 요소 끄기
        if (sr != null) sr.enabled = false;

        // 물리적 충돌 끄기 (죽은 상태에서 적에게 밀리거나 또 맞지 않도록)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 중력 영향 없애기
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false; // 물리 연산 중단

        // 이펙트 생성
        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Debug.Log("이펙트 생성됨");
        }
        else
        {
            Debug.LogWarning("Effect Prefab이 비어있습니다!");
        }

        // 시간 정지 상태에서도 작동하도록 Realtime 사용
        yield return new WaitForSecondsRealtime(0.5f);

        Debug.Log("Die 코루틴 종료 - 이벤트 발생");
        OnPlayerDied.Invoke();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (healthIconUI != null)
            healthIconUI.UpdateIcon(currentHealth, maxHealth);
    }

    // 무적처리
    private IEnumerator StartInvincible()
    {
        isInvincible = true;
        float elapsed = 0.0f;

        // 피격 시 캐릭터 깜빡임 연출
        Color originalColor = sr.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);

        while (elapsed < invincibleDuration)
        {
            sr.color = transparentColor;
            yield return new WaitForSeconds(flickerSpeed);
            sr.color = originalColor;
            yield return new WaitForSeconds(flickerSpeed);
            elapsed += flickerSpeed * 2;
        }

        sr.color = originalColor;
        isInvincible = false;
    }
}