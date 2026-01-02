using UnityEngine;
using TMPro;
using System.Collections;
using MyGameEnums;

public class SignPost : MonoBehaviour
{
    [Header("UI 자동 할당")]
    private GameObject boxPanel;

    [Header("애니메이션 설정")]
    public float speed = 0.2f;
    public Vector3 maxScale = Vector3.one;

    private bool isOpen = false;
    private Coroutine currentAnim;

    private void Awake()
    {
        // 자식인 Canvas를 찾기
        Canvas childCanvas = GetComponentInChildren<Canvas>();
        if (childCanvas != null)
        {
            // Panel을 찾습니다.
            if (childCanvas.transform.childCount > 0)
            {
                boxPanel = childCanvas.transform.GetChild(0).gameObject;
            }
        }
    }

    private void Start()
    {
        if (boxPanel != null)
        {
            // 패널의 사이즈를 줄이고 숨기기
            boxPanel.transform.localScale = Vector3.zero;
            boxPanel.SetActive(false);
        }
    }

    // 표지판 상태를 변경하는 애니메이션
    private void ChangeState()
    {
        // 씬 전환시(오브젝트 파괴시) return
        if (!gameObject.activeInHierarchy) return;

        isOpen = !isOpen;
        if (currentAnim != null) StopCoroutine(currentAnim);

        if (isOpen)
        {
            boxPanel.SetActive(true);
            currentAnim = StartCoroutine(ScaleBox(maxScale));
        }
        else
        {
            currentAnim = StartCoroutine(ScaleBox(Vector3.zero, () => boxPanel.SetActive(false)));
        }
    }

    // Panel박스가 커지는 애니메이션 코루틴
    private IEnumerator ScaleBox(Vector3 targetSize, System.Action onEnd = null)
    {
        Vector3 startSize = boxPanel.transform.localScale;
        float time = 0f;

        while (time < speed)
        {
            time += Time.unscaledDeltaTime;
            boxPanel.transform.localScale = Vector3.Lerp(startSize, targetSize, time / speed);
            yield return null;
        }

        boxPanel.transform.localScale = targetSize;
        if (onEnd != null) onEnd();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(TagName.Player.GetTag()))
        {
            if (!isOpen) ChangeState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(TagName.Player.GetTag()))
        {
            if (isOpen) ChangeState();
        }
    }
}