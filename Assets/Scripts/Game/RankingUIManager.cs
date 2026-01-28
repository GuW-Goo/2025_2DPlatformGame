using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RankingUIManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform contentParent; // 여기에 Vertical Layout Group이 있어야 합니다.
    public RankingSlot myRecordSlot;

    void Start()
    {
        if (RankingManager.Instance != null)
        {
            RankingManager.Instance.RegisterClearRecord();
        }

        DisplayRanking();
    }

    public void DisplayRanking()
    {
        // 기존 슬롯 제거 (내 기록 슬롯 제외)
        foreach (Transform child in contentParent)
        {
            if (child.gameObject != myRecordSlot.gameObject)
                Destroy(child.gameObject);
        }

        // 데이터 준비
        RankingData data = RankingManager.Instance.rankingData;
        SaveDataModel currentSave = GameManager.Instance.saveData.Read();
        float currentTotal = currentSave.stageClearTimes.Sum();
        PlayRecord currentRecord = new PlayRecord(currentTotal, currentSave.stageClearTimes);

        // 상위 10위 생성 
        for (int i = 0; i < data.records.Count; i++)
        {
            GameObject go = Instantiate(slotPrefab, contentParent);
            RankingSlot slot = go.GetComponent<RankingSlot>();
            slot.SetData(i + 1, data.records[i]);
        }

        // 내 순위 찾기
        int myRank = -1;
        for (int i = 0; i < data.records.Count; i++)
        {
            if (data.records[i].date == currentRecord.date &&
                Mathf.Approximately(data.records[i].totalTime, currentRecord.totalTime))
            {
                myRank = i + 1;
                break;
            }
        }

        // 내 기록 슬롯 업데이트
        myRecordSlot.SetData(myRank, currentRecord);

        // 세이브 초기화
        SaveData saveData = new SaveData();
        saveData.Clear();
    }
}