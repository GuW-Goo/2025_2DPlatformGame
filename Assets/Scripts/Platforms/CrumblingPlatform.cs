using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblingPlatform : MonoBehaviour
{
    [Header("붕괴 설정")]
    [SerializeField] private float fallDelay = 0.7f;
    [SerializeField] private float respawnTime = 3.0f;
    [SerializeField] private int fragmentsPerTile = 6;

    [Header("조각 색상 및 밝기")]
    [Range(0.5f, 2.0f)]
    [SerializeField] private float fragmentBrightness = 1.5f; // 조각이 너무 진하면 이 값을 높이세요
    [SerializeField] private Color grassColor = new Color(0.4f, 0.8f, 0.4f); // 풀 색상
    [SerializeField] private Color soilColor = new Color(0.7f, 0.5f, 0.4f);  // 흙 색상

    private CompositeCollider2D compositeCol;
    private Rigidbody2D rb;
    private SpriteRenderer[] childRenderers;
    private Vector3[] childOriginalLocalPositions; // 자식들의 원래 위치 저장
    private bool isCrumbling = false;

    void Awake()
    {
        compositeCol = GetComponent<CompositeCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        childRenderers = GetComponentsInChildren<SpriteRenderer>();

        // 자식들의 초기 로컬 위치를 저장
        childOriginalLocalPositions = new Vector3[childRenderers.Length];
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childOriginalLocalPositions[i] = childRenderers[i].transform.localPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCrumbling)
        {
            if (collision.contacts[0].normal.y < -0.5f)
            {
                StartCoroutine(CrumbleRoutine());
            }
        }
    }

    IEnumerator CrumbleRoutine()
    {
        isCrumbling = true;

        // 콜라이더는 두고 이미지만 흔들기
        float elapsed = 0;
        while (elapsed < fallDelay)
        {
            float shakeStrength = 0.05f;
            for (int i = 0; i < childRenderers.Length; i++)
            {
                Vector3 randomOffset = (Vector3)Random.insideUnitCircle * shakeStrength;
                childRenderers[i].transform.localPosition = childOriginalLocalPositions[i] + randomOffset;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 자식 위치 복구 후 비활성화
        for (int i = 0; i < childRenderers.Length; i++)
        {
            childRenderers[i].transform.localPosition = childOriginalLocalPositions[i];
            childRenderers[i].enabled = false;
        }

        // 콜라이더 및 물리 시뮬레이션 정지
        compositeCol.enabled = false;
        if (rb != null) rb.simulated = false; // 물리 엔진에서 해당 오브젝트를 제외함

        // 조각 생성
        SpawnSmartFragments();

        yield return new WaitForSeconds(respawnTime);

        // 복구
        compositeCol.enabled = true;
        if (rb != null) rb.simulated = true;
        foreach (var sr in childRenderers) sr.enabled = true;
        isCrumbling = false;
    }

    void SpawnSmartFragments()
    {
        foreach (var sr in childRenderers)
        {
            for (int i = 0; i < fragmentsPerTile; i++)
            {
                GameObject frag = new GameObject("Debris");
                bool isGrass = (i < fragmentsPerTile / 2);

                Vector3 spawnPos = sr.transform.position;
                spawnPos.y += isGrass ? Random.Range(0.1f, 0.3f) : Random.Range(-0.3f, -0.1f);
                spawnPos.x += Random.Range(-0.4f, 0.4f);

                frag.transform.position = spawnPos;
                frag.transform.localScale = Vector3.one * Random.Range(0.1f, 0.2f);

                SpriteRenderer fragSR = frag.AddComponent<SpriteRenderer>();
                fragSR.sprite = sr.sprite;

                // 밝기(fragmentBrightness)를 곱해 색상을 보정
                Color targetColor = isGrass ? grassColor : soilColor;
                fragSR.color = targetColor * fragmentBrightness;
                fragSR.sortingOrder = sr.sortingOrder + 1;

                Rigidbody2D fragRB = frag.AddComponent<Rigidbody2D>();
                fragRB.gravityScale = 1.5f;
                Vector2 force = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 6f));
                fragRB.AddForce(force, ForceMode2D.Impulse);
                fragRB.AddTorque(Random.Range(-500f, 500f));

                Destroy(frag, 1.2f);
            }
        }
    }
}