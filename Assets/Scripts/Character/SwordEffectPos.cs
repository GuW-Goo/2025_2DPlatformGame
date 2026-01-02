using UnityEngine;

// Sword Prefab에 있는 자식인스턴스 SwordEffect를 조작하기 위한 스크립트

public class SwordEffectPos : MonoBehaviour
{
    private bool isDown = false;
    private float effectSize = 0.1f;
    private Transform playerTransform;

    void Start()
    {
        // 캐릭터 본체(최상위 부모) 참조
        playerTransform = transform.root;
    }

    public void SetAttackDirection(bool _isDown)
    {
        isDown = _isDown;
    }



    void LateUpdate()
    {
        if (transform.parent == null) return;

        // 방향 동기화
        // 캐릭터 본체의 scale.x를 체크
        float lookDir = playerTransform.localScale.x > 0 ? 1.0f : -1.0f;
        transform.localScale = new Vector3(effectSize, effectSize, effectSize);

        // 회전 동기화
        if (isDown == true)
        {
            // 하단공격시 왼쪽을 본 상태면 위를 공격하기 때문에 동기화 해줌
            transform.rotation = Quaternion.Euler(0, 0, -90.0f * lookDir);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        
    }

}