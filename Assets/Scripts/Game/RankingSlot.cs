using UnityEngine;
using TMPro;

public class RankingSlot : MonoBehaviour
{
    public TextMeshProUGUI rankText;      // 'rank'
    public TextMeshProUGUI dateText;      // 'Date'
    public TextMeshProUGUI totalText;     // 'total'
    public TextMeshProUGUI[] stageTexts;  // 'Stage1' ~ 'Stage5'를 배열로 연결

    public void SetData(int rank, PlayRecord record)
    {
        rankText.text = (rank == -1) ? "10위 밖" : $"{rank}위";
        dateText.text = record.date;
        totalText.text = $"total : {record.totalTime:F2}s";

        for (int i = 0; i < stageTexts.Length; i++)
        {
            if (i < record.detailTimes.Length)
            {
                stageTexts[i].text = $"S{i + 1} : {record.detailTimes[i]:F2}s";
            }
        }
    }
}