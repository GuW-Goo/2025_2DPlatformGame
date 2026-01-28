using UnityEngine;
using System.IO;
using System.Linq;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;
    private string path;
    public RankingData rankingData = new RankingData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        path = Path.Combine(Application.persistentDataPath, "ranking.json");
        LoadRanking();
    }

    public void RegisterClearRecord()
    {
        SaveDataModel currentSave = GameManager.Instance.saveData.Read();

        // 시간이 0이면 비정상 기록으로 간주
        if (currentSave == null || currentSave.stageClearTimes.Sum() <= 0) return;

        float total = currentSave.stageClearTimes.Sum();
        PlayRecord newRecord = new PlayRecord(total, currentSave.stageClearTimes);

        // 리스트에 동일한 기록이 있는지 검사 (중복데이터 방지)
        bool alreadyRegistered = rankingData.records.Any(r =>
        r.date == newRecord.date &&
        Mathf.Approximately(r.totalTime, newRecord.totalTime));

        if (alreadyRegistered)
        {
            Debug.Log("이미 등록된 기록입니다. 중복 추가를 건너뜁니다.");
            return;
        }

        // 새로운 기록일 경우 리스트에 추가
        rankingData.records.Add(newRecord);

        // 랭킹데이터 중 상위 10개 뽑아오기
        rankingData.records = rankingData.records
            .OrderBy(r => r.totalTime)
            .Take(10)
            .ToList();

        SaveRanking();
    }

    private void LoadRanking()
    {
        // 파일 읽어오기
        if (File.Exists(path))
        {
            // 파일이 있으면 읽어서 메모리에 덮어씀
            string json = File.ReadAllText(path);
            rankingData = JsonUtility.FromJson<RankingData>(json);

            // 만약 파일이 깨져서 rankingData가 null이면 초기화
            if (rankingData == null) rankingData = new RankingData();
        }
        else
        {
            // 파일이 없으면 새 리스트 생성
            rankingData = new RankingData();
        }
    }

    private void SaveRanking()
    {
        // 파일 저장하기
        string json = JsonUtility.ToJson(rankingData, true);
        File.WriteAllText(path, json);
    }
}
