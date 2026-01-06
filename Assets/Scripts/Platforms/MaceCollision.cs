using UnityEngine;
using MyGameEnums;

public class MaceCollision : MonoBehaviour, IAttackable
{
    private PhysicsMace parentScript;

    void Start()
    {
        parentScript = GetComponentInParent<PhysicsMace>();
    }

    public void OnHit(Vector2 attackerPoint)
    {
        // 공격방향 계산 ( 플레이어의 반대방향으로 튕겨나가게 )
        bool isRight = (transform.position.x - attackerPoint.x) < 0;

        // 부모의 함수 호출
        parentScript.ApplySwingForce(350.0f, isRight);
    }

}
