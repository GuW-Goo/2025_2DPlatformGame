using UnityEngine;

// 플레이어를 따라다니는 메인카메라 스크립트

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] public GameObject player;

    void LateUpdate()
    {
        // player가 비어있다면(null), 씬에서 플레이어를 찾아봅니다.
        if (player == null)
        {
            // PlayerStatus 스크립트가 붙은 오브젝트를 찾습니다.
            PlayerStatus target = Object.FindAnyObjectByType<PlayerStatus>();

            if (target != null)
            {
                player = target.gameObject;
            }
            else
            {
                // 아직 플레이어가 소환 전이라면 이번 프레임은 쉽니다.
                return;
            }
        }

        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1.8f, -10.0f);
    }
}
