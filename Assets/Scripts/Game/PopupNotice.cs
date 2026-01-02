using UnityEngine;
using System.Collections;

public class PopupNotice : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private GameObject boxPanel; // 인스펙터에서 할당하거나 Awake에서 찾기

    [Header("애니메이션 설정")]
    public float speed = 0.2f;          // 커지는 속도
    public float displayDuration = 2.0f; // 몇 초 동안 보여줄지
    public Vector3 maxScale = Vector3.one;

    private Coroutine currentAnim;

    private void Awake()
    {
        if (boxPanel == null)
            boxPanel = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        boxPanel.transform.localScale = Vector3.zero;
        boxPanel.SetActive(false);
    }

    // 외부(UIManager)에서 호출할 함수
    public void ShowNotice()
    {
        if (currentAnim != null) StopCoroutine(currentAnim);
        currentAnim = StartCoroutine(NoticeSequence());
    }

    // 나타남 -> 대기 -> 사라짐 시퀀스
    private IEnumerator NoticeSequence()
    {
        // 패널 활성화 및 나타나기
        boxPanel.SetActive(true);
        yield return StartCoroutine(ScaleBox(maxScale));

        // 지정된 시간만큼 대기 (Time.unscaledDeltaTime 사용으로 일시정지 무관하게 작동)
        yield return new WaitForSecondsRealtime(displayDuration);

        // 사라지기 애니메이션
        yield return StartCoroutine(ScaleBox(Vector3.zero));

        // 비활성화
        boxPanel.SetActive(false);
    }

    private IEnumerator ScaleBox(Vector3 targetSize)
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
    }
}