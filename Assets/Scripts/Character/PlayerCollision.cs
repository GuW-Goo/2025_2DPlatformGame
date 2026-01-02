using MyGameEnums;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.PlayerSettings;

// 캐릭터의 충돌감지 스크립트

public class PlayerCollision : MonoBehaviour
{
    SaveData saveData;
    PlayerStatus status;    
    CharacterMove move;


    void Start()
    {
        saveData = new SaveData();
        status = GetComponent<PlayerStatus>();
        move = GetComponent<CharacterMove>();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 플랫폼 위에 머물 시 플랫폼 중앙으로 스폰포인트를 갱신합니다.
        if (collision.gameObject.CompareTag( TagName.Platform.GetTag()) )
        {
            // 모든 충돌 지점을 검사합니다.
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 위에서 아래로 밟았을 때만 로직 실행
                if (contact.normal.y > 0.9f)
                {
                    // 실제로 발에 닿은 '그 콜라이더'의 영역(Bounds) 정보를 가져옵니다.
                    Bounds platformBounds = collision.collider.bounds;

                    // 해당 콜라이더의 중앙 좌표를 추출해서 스폰포인트로 변경합니다.
                    Vector2 centerPos = new Vector2(platformBounds.center.x, platformBounds.max.y + 0.8f);

                    status.spawnPos = centerPos;
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject obj = collision.gameObject;

        if (obj.CompareTag(TagName.Monster.GetTag())
            || obj.CompareTag(TagName.Mace.GetTag()))
        {
            // 생명 1 감소
            status.TakeDamage(1);
            // 캐릭터 넉백
            move.Knockback(collision.transform.position);

            Debug.Log("Collosion Tag : " + obj.tag);
        }
        else if (obj.CompareTag( TagName.Spike.GetTag() ))
        {
            // 생명 1 감소
            status.TakeDamage(1);

            // 플레이어의 체력이 남아있다면 스폰포인트로 이동
            if (status.currentHealth > 0)
            {
                gameObject.transform.position = status.spawnPos;
            }
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;
        
        // 낙사구간 충돌
        if (obj.CompareTag(TagName.DeadZone.GetTag()))
        {
            // 생명 1 감소
            status.TakeDamage(1);

            // 플레이어의 체력이 남아있다면 스폰포인트로 이동
            if (status.currentHealth > 0)
            {
                gameObject.transform.position = status.spawnPos;
            }
        }
        // 힐오브젝트 충돌
        else if (obj.CompareTag(TagName.Heal.GetTag()))
        {
            if (status.currentHealth == status.maxHealth)
                return;

            // 태그변경 (힐이 2번 되는 것을 방지)
            obj.tag = TagName.Untagged.GetTag();

            // 생명을 최대치로 회복
            status.Heal(status.maxHealth);

            // 회복오브젝트 제거
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject obj = collision.gameObject;

        // 스폰포인트 깃발과 충돌, E를 눌렀을 때
        if (obj.CompareTag(TagName.SpawnPoint.GetTag()) && Input.GetKeyDown(KeyCode.E))
        {
            // 충돌한 깃발의 위치를 가져옵니다.
            Vector2 flagCenterPos = obj.transform.position;

            // 발이 땅에 묻히지 않도록 약간의 높이 보정(+0.2f)만 해줍니다.
            Vector2 spawnPointPos = new Vector2(flagCenterPos.x, flagCenterPos.y + 0.2f);

            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            SaveDataModel data = new SaveDataModel
            {
                spawnPos = spawnPointPos,
                sceneName = currentScene
            };

            // 기기에 스폰 저장
            saveData.Save(data);

            // 플레이어 상태 업데이트
            status.spawnSceneName = currentScene;
            status.Heal(status.maxHealth);
        }
    }
}
