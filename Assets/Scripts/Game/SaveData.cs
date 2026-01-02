using UnityEngine;
using System.IO;
using Unity.VisualScripting;

// 게임의 세이브데이터 파일을 관리하는 스크립트

[System.Serializable]
public class SaveDataModel
{
    public Vector2 spawnPos;
    public string sceneName;
}

public class SaveData
{
    // 파일이 저장될 경로
    string path = Path.Combine(Application.persistentDataPath, "saveData.json");

    public void Save(SaveDataModel data)
    {
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        Debug.Log("저장완료! path : " + path);
    }

    public SaveDataModel Read()
    {
        if (!File.Exists(path))
        {
            Debug.Log("세이브가 없습니다!");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveDataModel data = JsonUtility.FromJson<SaveDataModel>(json);

        Debug.Log("세이브 불러오기 성공!");

        return data;
    }

    public void UpdateScene(string newScene)
    {
        SaveDataModel data = Read();

        if (data != null)
        {
            data.sceneName = newScene;
            Save(data);
        }
    }

    public void UpdateSpawnPos(Vector2 newPos)
    {
        SaveDataModel data = Read();

        if (data != null)
        {
            data.spawnPos = newPos;
            Save(data);
        }
    }

}

