using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PopupConfirm : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject boxPanel;
    [SerializeField] private Button[] buttons; // 0: 아니오(좌), 1: 예(우)

    [Header("애니메이션")]
    public float speed = 0.15f;
    private int selectedIndex = 0;

    private void Start()
    {
        boxPanel.SetActive(false);
    }

    public void Open()
    {
        StopAllCoroutines();
        boxPanel.SetActive(true);
        selectedIndex = 0; // 시작 시 '아니오' 선택
        UpdateSelection();
        StartCoroutine(ScaleBox(Vector3.one));
    }

    public void Navigate(int direction)
    {
        // 0과 1 사이를 이동
        int nextIndex = selectedIndex + direction;

        // 인덱스 범위 제한 (0~1)
        if (nextIndex < 0) nextIndex = 0;
        if (nextIndex >= buttons.Length) nextIndex = buttons.Length - 1;

        if (selectedIndex != nextIndex)
        {
            selectedIndex = nextIndex;
            UpdateSelection();
        }
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            // 선택된 버튼은 1.3배, 나머지는 1.0배
            Vector3 targetScale = (i == selectedIndex) ? Vector3.one * 1.3f : Vector3.one;
            buttons[i].transform.localScale = targetScale;

            if (i == selectedIndex)
                EventSystem.current.SetSelectedGameObject(buttons[i].gameObject);
        }
    }

    public void ConfirmSelection()
    {
        buttons[selectedIndex].onClick.Invoke();
    }

    private IEnumerator ScaleBox(Vector3 targetSize, System.Action onComplete = null)
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
        onComplete?.Invoke();
    }

    public void Close()
    {
        StartCoroutine(ScaleBox(Vector3.zero, () => boxPanel.SetActive(false)));
    }
}