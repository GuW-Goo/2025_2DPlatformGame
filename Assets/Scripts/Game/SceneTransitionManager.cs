using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

// 씬 전환시 페이드아웃 스크립트
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI 요소")]
    public CanvasGroup fadeGroup; // 페이드용 검은 이미지의 CanvasGroup
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        // 화면 어둡게 만들기 (Fade Out)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        // 실제 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 씬 로드 후 플레이어생성
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitSceneAfterLoading();
        }

        // 화면 밝게 만들기 (Fade In)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
    }
}