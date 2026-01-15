using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformPathTrigger : MonoBehaviour
{
    private List<GameObject> platforms = new List<GameObject>();

    private float appearanceInterval = 1.0f; // 다음 플랫폼이 나타날 때까지의 간격
    private float platformDuration = 1.5f;   // 플랫폼이 완전히 나타나서 유지되는 시간
    private float fadeDuration = 0.5f;       // fade In & Out 지속시간

    private bool isStarted = false;

    private void Start()
    {
        // "Platforms" 자식 오브젝트를 찾아 그 아래의 모든 플랫폼을 리스트에 담습니다.
        Transform container = transform.Find("Platforms");

        foreach (Transform child in container)
        {
            platforms.Add(child.gameObject);
            child.gameObject.SetActive(false); // 시작 시 모두 비활성화
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isStarted)
        {
            Debug.Log("Timing Platform Start!");
            isStarted = true;
            StartCoroutine(SequenceRoutine());
        }
    }

    IEnumerator SequenceRoutine()
    {
        foreach (GameObject platform in platforms)
        {
            // 각 플랫폼마다 독립적인 "나타났다 사라지기" 코루틴을 실행. (병렬 실행)
            StartCoroutine(FadeInAndOut(platform));

            // 다음 플랫폼이 나타나기 전까지 기다립니다.
            yield return new WaitForSeconds(appearanceInterval);
        }

        isStarted = false;
        Debug.Log("Timing Platform End!");
    }

    IEnumerator FadeInAndOut(GameObject platform)
    {
        SpriteRenderer[] renderers = platform.GetComponentsInChildren<SpriteRenderer>();
        CompositeCollider2D compositeCol = platform.GetComponent<CompositeCollider2D>();
        Rigidbody2D rb = platform.GetComponent<Rigidbody2D>();

        if (compositeCol == null) yield break;

        // Fade In (예고 및 충돌 비활성화)
        platform.SetActive(true);
        if (rb != null) rb.simulated = false; // 물리 시뮬레이션 중지 (충돌 안됨)

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.0f, 0.5f, elapsed / fadeDuration); // 0에서 0.5까지 흐릿하게
            SetAlpha(renderers, alpha);
            yield return null;
        }

        // 플레이어 위치 체크 및 밀어올리기
        // 활성화 직전, 플레이어가 겹쳐 있는지 확인
        Collider2D playerCol = Physics2D.OverlapBox(compositeCol.bounds.center, compositeCol.bounds.size, 0, LayerMask.GetMask("Player"));
        if (playerCol != null)
        {
            float targetY = compositeCol.bounds.max.y + (playerCol.bounds.size.y / 2f);
            playerCol.transform.position = new Vector3(playerCol.transform.position.x, targetY, playerCol.transform.position.z);
        }

        // 완전 활성화
        if (rb != null) rb.simulated = true; // 물리 시뮬레이션 시작 (이제 밟을 수 있음)
        SetAlpha(renderers, 1.0f); // 완전히 불투명하게

        yield return new WaitForSeconds(platformDuration);

        // Fade Out (사라지기)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1.0f, 0.0f, elapsed / fadeDuration);
            SetAlpha(renderers, alpha);
            yield return null;
        }

        platform.SetActive(false);
    }

    private void SetAlpha(SpriteRenderer[] srs, float alpha)
    {
        foreach (var sr in srs)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}