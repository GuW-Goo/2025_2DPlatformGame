using MyGameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("버튼 배열")]
    public Button[] menuButtons;

    [Header("애니메이션 설정")]
    public float scaleFactor = 1.1f;
    public float animationSpeed = 0.1f;

    private int selectedButtonIndex = 0;
    private Dictionary<Transform, Coroutine> activeCoroutines = new Dictionary<Transform, Coroutine>();

    PopupNotice errorPopup;

    void Start()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        foreach (var btn in menuButtons)
        {
            btn.transform.localScale = Vector3.one;
        }

        SetSelectedButton(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) Navigate(1);
    }

    // 인스펙터에서 버튼 클릭 시 연결할 함수들
    public void StartGameScene()
    {
        GameManager.Instance.StartNewGame();
    }

    public void ContinueGameScene()
    {
        bool isSuccess = GameManager.Instance.ContinueGame();

        // 세이브 데이터가 없다면
        if(!isSuccess)
        {
            if(errorPopup != null)
            {
                errorPopup.ShowNotice();
            }
        }
    }

    public void LoadMainMenu()
    {
        // 로드된 씬 중에 옵션씬 찾기
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == SceneName.OptionScene.GetScene())
            {
                UnloadOption();
                break;
            }
        }

        SceneTransitionManager.Instance.ChangeScene(SceneName.MainMenuScene.GetScene());
    }

    public void UnloadOption()
    {
        GameManager.Instance.CloseOption();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    // 위/아래 입력에 따라 다음에 선택될 버튼의 번호(Index)를 계산합니다.
    private void Navigate(int direction)
    {
        // 현재 번호에서 방향(1 또는 -1)을 더해 새 번호를 만듭니다.
        int newIndex = selectedButtonIndex + direction;

        // 리스트의 맨 위에서 위를 누르면 맨 아래로, 맨 아래에서 아래를 누르면 맨 위로 순환시킵니다.
        if (newIndex < 0) newIndex = menuButtons.Length - 1;
        else if (newIndex >= menuButtons.Length) newIndex = 0;

        // 계산된 번호를 실제 선택 상태로 적용합니다.
        SetSelectedButton(newIndex);
    }

    // 선택된 버튼을 바꾸고 애니메이션을 명령합니다.
    private void SetSelectedButton(int newIndex)
    {
        // 현재 선택된 버튼이 배열안에 있다면
        if (selectedButtonIndex < menuButtons.Length)
            // 기존 버튼 축소
            StartScaleAnimation(menuButtons[selectedButtonIndex].transform, Vector3.one);

        // 인덱스 갱신
        selectedButtonIndex = newIndex;

        // 유니티 시스템상에서 이 버튼이 '선택됨' 상태임을 알림
        EventSystem.current.SetSelectedGameObject(menuButtons[selectedButtonIndex].gameObject);

        // 새로 선택된 버튼은 설정한 비율(scaleFactor)만큼 키웁니다.
        StartScaleAnimation(menuButtons[selectedButtonIndex].transform, Vector3.one * scaleFactor);
    }

    // 코루틴이 중복 실행되지 않도록 관리하는 함수입니다.
    private void StartScaleAnimation(Transform targetTransform, Vector3 targetScale)
    {
        // Dictionary에서 이 버튼(targetTransform)이 이미 애니메이션 중인지 확인합니다.
        if (activeCoroutines.TryGetValue(targetTransform, out Coroutine existing))
        {
            // 이미 움직이고 있다면, 이전 애니메이션을 강제로 멈춰서 떨림 현상을 방지합니다.
            StopCoroutine(existing);
            activeCoroutines.Remove(targetTransform);
        }

        // 새로운 크기 변경 애니메이션을 시작하고, 이를 Dictionary에 기록해둡니다.
        activeCoroutines.Add(targetTransform, StartCoroutine(ScaleButtonCoroutine(targetTransform, targetScale)));
    }

    // 시간에 따라 부드럽게 크기를 변경합니다.
    private IEnumerator ScaleButtonCoroutine(Transform targetTransform, Vector3 targetScale)
    {
        Vector3 initialScale = targetTransform.localScale; // 시작 시점의 크기
        float elapsedTime = 0f; // 경과 시간

        while (elapsedTime < animationSpeed)
        {
            // Lerp를 사용하여 시작 크기에서 목표 크기로 부드럽게 변화시킵니다.
            targetTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / animationSpeed);

            // Time.unscaledDeltaTime을 사용해 게임이 일시정지(Time.scale=0) 상태여도 UI는 움직이게 합니다.
            elapsedTime += Time.unscaledDeltaTime;
            yield return null; // 한 프레임 대기
        }

        // 오차를 없애기 위해 마지막에 정확한 목표 크기로 설정합니다.
        targetTransform.localScale = targetScale;

        // 애니메이션이 끝났으므로 기록(Dictionary)에서 제거합니다.
        activeCoroutines.Remove(targetTransform);
    }
}