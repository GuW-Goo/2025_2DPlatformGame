using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyGameEnums;

// 게임매니저 (게임오버 등을 감지함)

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    static public GameManager Instance;

    PlayerStatus playerStatus;  // 플레이어의 상태를 알려줌

    public SaveData saveData { get; private set; }  // 세이브 데이터 관리
    private SaveDataModel pendingLoadData = null;   // 이어하기 시 임시로 저장할 변수

    public bool isPaused = false;
    
    private void Awake()
    {
        // 싱글톤 처리
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // save를 Awake에서 미리 생성
            saveData = new SaveData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    private void Update()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 현재 씬이 메인메뉴, 게임오버씬이면 return
        if (currentSceneName == SceneName.MainMenuScene.GetScene() ||
        currentSceneName == SceneName.GameOverScene.GetScene())
        {
            return;
        }

        if (!isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            // 옵션씬 열기
            OpenOption();
        }
        else if (isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            // 옵션씬 닫기
            CloseOption();
        }
        
    }

    // 씬 전환시 실행하는 함수
    public void InitSceneAfterLoading()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 메인메뉴, 게임오버, 랭킹 씬이 아닐 때만 플레이어 생성
        if (currentSceneName != SceneName.MainMenuScene.GetScene() &&
            currentSceneName != SceneName.GameOverScene.GetScene() &&
            currentSceneName != SceneName.RankingScene.GetScene() )
        {
            SpawnPlayer();

            // 기존 데이터를 불러오기
            SaveDataModel data = saveData.Read();

            // 만약 data가 null이면 새로 생성
            if(data == null)
            {
                data = new SaveDataModel();
            }

            // 위치와 씬 이름만 갱신 (stageClearTimes는 보존)
            data.spawnPos = playerStatus.transform.position;
            data.sceneName = currentSceneName;

            saveData.Save(data);
        }
    }

    public void SpawnPlayer()
    {
        // 씬에 이미 플레이어가 있는지 확인
        playerStatus = FindAnyObjectByType<PlayerStatus>();

        // 플레이어가 아예 없을 때만 새로 소환
        if (playerStatus == null)
        {
            GameObject go = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity);
            playerStatus = go.GetComponent<PlayerStatus>();
            RegisterPlayer(playerStatus);

            // 새로 소환했을 때만 위치 결정 로직 실행
            SetPlayerPosition();
        }
        // 이미 플레이어가 있다면 아무것도 하지 않습니다.

        // 카메라 연결은 항상 확인 (씬이 바뀌었을 수 있으므로)
        PlayerCamera cam = Camera.main.GetComponent<PlayerCamera>();
        if (cam != null) cam.player = playerStatus.gameObject;
    }

    // 위치 설정 로직
    private void SetPlayerPosition()
    {
        if (pendingLoadData != null) // 이어하기 시
        {
            playerStatus.transform.position = pendingLoadData.spawnPos;
            playerStatus.spawnPos = pendingLoadData.spawnPos;
            if(TimeManager.Instance != null)
            {
                TimeManager.Instance.SetCurrentTime(pendingLoadData.tempElapsedTime);
            }
            pendingLoadData = null;
        }
        else    // 처음부터 시작 시
        {
            // 새 게임 위치 (StartPoint 찾기)
            GameObject startPoint = GameObject.Find("StartPoint");
            Vector3 targetPos = (startPoint != null) ? startPoint.transform.position : Vector3.zero;

            playerStatus.transform.position = targetPos;
            playerStatus.spawnPos = targetPos;

            if(TimeManager.Instance != null)
            {
                TimeManager.Instance.SetCurrentTime(0.0f);
            }
        }
    }

    // 이어하기, 다시하기
    public bool ContinueGame()
    {
        // saveData 자체가 null인지 확인
        if (saveData == null) saveData = new SaveData();

        SaveDataModel data = saveData.Read();

        // 데이터 유효성 검사
        if (data == null || string.IsNullOrEmpty(data.sceneName))
        {
            Debug.LogWarning("불러올 세이브 데이터가 유효하지 않습니다.");
            return false;
        }

        pendingLoadData = data;

        // SceneTransitionManager 인스턴스 체크 (에러 방지)
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ChangeScene(data.sceneName);
            return true;
        }
        else
        {
            Debug.LogError("SceneTransitionManager가 씬에 존재하지 않습니다!");
            return false;
        }
    }

    // 새 게임 시작
    public void StartNewGame()
    {
        if (saveData == null)
        {
            saveData = new SaveData();
            Debug.Log("saveData 객체가 null이어서 새로 생성했습니다.");
        }

        saveData.Clear();
        pendingLoadData = null;
        SceneTransitionManager.Instance.ChangeScene( SceneName.TutorialStage.GetScene() );
    }

    // Stage1부터 시작 (튜토리얼 스킵)
    public void StartFromStage1()
    {
        if (saveData == null)
        {
            saveData = new SaveData();
        }
        saveData.Clear();

        // Stage1으로 씬 전환
        SceneTransitionManager.Instance.ChangeScene( SceneName.StageScene1.GetScene() );
    }

    // 플레이어 생성시 (씬 전환시) 플레이어가 매니저에게 알림
    public void RegisterPlayer(PlayerStatus player)
    {
        playerStatus = player;

        playerStatus.OnPlayerDied.RemoveListener(GameOver);
        playerStatus.OnPlayerDied.AddListener(GameOver);
        
    }

    public void OpenOption()
    {
        isPaused = true;
        SceneManager.LoadScene(SceneName.OptionScene.GetScene(), LoadSceneMode.Additive);
        Time.timeScale = 0.0f;
    }

    public void CloseOption()
    {
        isPaused = false;
        Time.timeScale = 1.0f;
        SceneManager.UnloadSceneAsync(SceneName.OptionScene.GetScene());
    }

    // 게임오버 처리
    public void GameOver()
    {
        Debug.Log("GameOver");
        Time.timeScale = 1.0f;
        SceneTransitionManager.Instance.ChangeScene(SceneName.GameOverScene.GetScene());
    }

}
