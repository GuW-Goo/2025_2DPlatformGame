using UnityEngine;

public class CreateChains : MonoBehaviour
{
    
    void Awake()
    {
        AlignChains();
    }

    void AlignChains()
    {
        float distance = 0.5f; // 체인 간격
        for (int i = 0; i < transform.childCount; i++)
        {
            // 아래쪽(Vector3.down) 방향으로 쭉 나열
            transform.GetChild(i).localPosition = Vector3.down * (distance * i);
        }
    }
}
