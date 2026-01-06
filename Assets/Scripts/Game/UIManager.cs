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

    [Header("팝업 설정")]
    [SerializeField] private PopupNotice errorPopup;
    [SerializeField] private PopupConfirm confirmPopup;

    [SerializeField] private CanvasGroup mainButtonsGroup;

    // 현재 팝업이 열려있는지 확인하는 플래그
    private bool isPopupOpen = false;

    private int selectedButtonIndex = -1;
    private Dictionary<Transform, Coroutine> activeCoroutines = new Dictionary<Transform, Coroutine>();

    void Start()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        foreach (var btn in menuButtons)
        {
            btn.transform.localScale = Vector3.one;
        }

        StartCoroutine(InitFirstButton());
    }

    IEnumerator InitFirstButton()
    {
        yield return null; // 한 프레임 대기
        SetSelectedButton(0);
    }

    void Update()
    {
        if (isPopupOpen)
        {
            // 팝업이 열려있을 때는 팝업 전용 조작
            if (Input.GetKeyDown(KeyCode.LeftArrow)) confirmPopup.Navigate(-1);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) confirmPopup.Navigate(1);

            return;
        }

        // 기존 상/하 조작
        if (Input.GetKeyDown(KeyCode.UpArrow)) Navigate(-1);
        else if (Input.GetKeyDown(KeyCode.DownArrow)) Navigate(1);
    }

    // 인스펙터에서 버튼 클릭 시 연결할 함수들
    public void StartGameScene()
    {
        SaveData saveData = new SaveData();

        if (saveData.Read() != null) // 데이터가 있다면
        {
            SetMainButtonsInteractable(false);
            isPopupOpen = true;
            confirmPopup.Open();
        }
        else
        {
            StartNewGame();
        }

    }

    // 팝업의 "예" 버튼에 연결할 함수
    public void StartNewGame()
    {
        SaveData saveData = new SaveData();
        saveData.Clear();
        isPopupOpen = false;
        GameManager.Instance.StartNewGame();
    }

    // 팝업의 "아니오" 버튼에 연결할 함수
    public void CloseConfirm()
    {
        SetMainButtonsInteractable(true);
        isPopupOpen = false;
        confirmPopup.Close();
        SetSelectedButton(selectedButtonIndex);
    }

    // 버튼들의 상호작용을 한 번에 끄고 켜는 함수
    private void SetMainButtonsInteractable(bool state)
    {
        if (mainButtonsGroup != null)
        {
            mainButtonsGroup.interactable = state;
            mainButtonsGroup.blocksRaycasts = state;
        }
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
            // 옵션씬이 있다면 옵션씬을 언로드 후 메인메뉴 로드
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
        // 이전 선택된 버튼 축소 (이전 인덱스가 유효하고 새 인덱스와 다를 때만)
        if (selectedButtonIndex >= 0 && selectedButtonIndex < menuButtons.Length && selectedButtonIndex != newIndex)
        {
            StartScaleAnimation(menuButtons[selectedButtonIndex].transform, Vector3.one);
        }

        // 인덱스 갱신
        selectedButtonIndex = newIndex;

        // 새 버튼 확대
        if (selectedButtonIndex >= 0)
        {
            EventSystem.current.SetSelectedGameObject(menuButtons[selectedButtonIndex].gameObject);
            StartScaleAnimation(menuButtons[selectedButtonIndex].transform, Vector3.one * scaleFactor);
        }
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