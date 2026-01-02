using UnityEngine;
using MyGameEnums;

public class SpawnPoint : MonoBehaviour
{

    [SerializeField] private Sprite activeFlagSprite;   // 활성화된 깃발 이미지

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag( TagName.Player.GetTag() ) && Input.GetKeyDown(KeyCode.E) )
        {
            SpriteRenderer flagSr = GetComponentInChildren<SpriteRenderer>();
            if (flagSr != null && activeFlagSprite != null)
            {
                flagSr.sprite = activeFlagSprite; // 새로운 이미지로 교체
                Debug.Log($"{this.gameObject.name}의 이미지가 활성화 상태로 변경되었습니다.");
            }

            // 다시 충돌해도 로직이 돌지 않게 태그변경
            this.gameObject.tag = TagName.Untagged.GetTag();
        }
    }
}
