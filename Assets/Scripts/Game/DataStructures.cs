using System;
using System.Collections.Generic;
using UnityEngine;

// 세이브 데이터 모델 (위치 + 시간 정보)
[Serializable]
public class SaveDataModel
{
    public Vector2 spawnPos;
    public string sceneName;
    public int currentStageIndex = 0;

    public float[] stageClearTimes = new float[5]; // Stage 1~5
    public float tempElapsedTime = 0f;      // 중도 저장용 시간
}

// 개별 플레이 기록 (랭킹용)
[Serializable]
public class PlayRecord
{
    public string date;
    public float totalTime;
    public float[] detailTimes;

    public PlayRecord(float total, float[] details)
    {
        date = DateTime.Now.ToString("yyyy-MM-dd");
        totalTime = total;
        detailTimes = (float[])details.Clone();
    }
}

// 전체 랭킹 데이터
[Serializable]
public class RankingData
{
    public List<PlayRecord> records = new List<PlayRecord>();
}