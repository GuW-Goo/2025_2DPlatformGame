using UnityEngine;
using MyGameEnums;

public class MaceCollision : MonoBehaviour
{
    private PhysicsMace parentScript;
    private Rigidbody2D parentRB;

    void Start()
    {
        parentScript = GetComponentInParent<PhysicsMace>();
        parentRB = GetComponentInParent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( collision.CompareTag( TagName.Sword.GetTag() ))
        {
            // 플레이어의 좌표를 가져옴
            GameObject player = GameObject.FindGameObjectWithTag( TagName.Player.GetTag() );

            // 공격방향 계산 ( 플레이어의 반대방향으로 튕겨나가게 )
            bool isRight = (transform.position.x - player.transform.position.x) < 0;

            // 부모의 함수 호출
            parentScript.ApplySwingForce(350.0f, isRight);
        }
    }
}
