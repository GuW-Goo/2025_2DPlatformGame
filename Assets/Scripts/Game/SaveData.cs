using UnityEngine;
using System.IO;

public class SaveData
{
    private string path;

    public SaveData()
    {
        path = Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    public void Save(SaveDataModel data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Data 저장 완료 Path : " + path);
    }

    public SaveDataModel Read()
    {
        if (!File.Exists(path)) return new SaveDataModel();
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveDataModel>(json);
    }

    public void Clear()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}