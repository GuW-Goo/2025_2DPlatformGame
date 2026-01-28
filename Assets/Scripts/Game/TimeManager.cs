using UnityEngine;

// 각 스테이지당 진행 시간을 저장하는 스크립트
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;
    public float currentStageTime = 0.0f;
    public bool isTimerRunning = false;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentStageTime += Time.deltaTime;
        }
    }

    public void SetCurrentTime(float time)
    {
        currentStageTime = time;
        isTimerRunning = true;
    }

    private void OnApplicationQuit()
    {
        SaveDataModel data = GameManager.Instance.saveData.Read();
    }

    private void OnDisable()
    {
        SaveCurrentProgress();
    }

    // data 세이브
    public void SaveCurrentProgress()
    {
        if (GameManager.Instance != null)
        {
            SaveDataModel data = GameManager.Instance.saveData.Read();
            data.tempElapsedTime = currentStageTime;
            GameManager.Instance.saveData.Save(data);
        }
    }

    // 스테이지 클리어 시 data세이브
    public void CompleteStage(int stageDataIndex)
    {
        isTimerRunning = false;
        SaveDataModel data = GameManager.Instance.saveData.Read();
        data.stageClearTimes[stageDataIndex] = currentStageTime;
        data.tempElapsedTime = 0.0f;
        currentStageTime = 0.0f;
        GameManager.Instance.saveData.Save(data);
    }
}
