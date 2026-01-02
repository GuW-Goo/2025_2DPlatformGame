using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrapPlatform : MonoBehaviour
{
    [SerializeField] GameObject SpikePrefab;
    [SerializeField] int SpikeCount = 2;    // 튀어나올 Spike 갯수

    List<GameObject> spikeList = new List<GameObject>();

    private float spikeOut = 1.5f;  // Spike가 튀어나와있을 시간
    private float spikeIn = 1.5f;   // Spike가 들어가있을 시간

    private void Start()
    {
        // Spike 생성 로컬좌표 x : 0 ~ i 로 지정됨
        for(int i = 0; i < SpikeCount; i++)
        {
            GameObject newSpike = Instantiate(SpikePrefab, this.transform);
            newSpike.transform.localPosition = new Vector2(i + 0.5f, 0);
            spikeList.Add(newSpike);
        }

        StartCoroutine(TrapRoutine());
    }
    
    IEnumerator TrapRoutine()
    {
        while (true)
        {
            // Spike 튀어나오기
            StartCoroutine(SpikeInOut(0.95f));
            yield return new WaitForSeconds(spikeOut);

            // Spike 들어가기
            StartCoroutine(SpikeInOut(0.0f));
            yield return new WaitForSeconds(spikeIn);
            
        }
    }

    IEnumerator SpikeInOut(float yPos)
    {
        // Spike가 튀어나올때는 즉시 활성화
        if (yPos > 0.0f)
        {
            SetSpikesActive(true);
        }

        float elapsed = 0;
        float duration = 0.1f; // 이동하는 데 걸리는 시간

        List<Vector2> spikePos = new List<Vector2>();
        foreach (GameObject s in spikeList)
        {
            spikePos.Add(s.transform.localPosition);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            for (int i = 0; i < spikePos.Count; i++)
            {
                Vector2 startPos = spikePos[i];
                Vector2 endPos = new Vector2(startPos.x, yPos);

                spikeList[i].transform.localPosition = Vector2.Lerp(startPos, endPos, progress);
            }

            yield return null;
        }


        // 가시가 완전히 들어간 후에는 비활성화
        if (yPos <= 0.0f)
        {
            SetSpikesActive(false);
        }

    }

    void SetSpikesActive(bool active)
    {
        foreach (GameObject s in spikeList)
        {
            // 가시 오브젝트의 Collider2D를 찾아 활성화/비활성화
            Collider2D col = s.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = active;
            }
        }
    }
}
