using UnityEngine;

public class PhysicsMace : MonoBehaviour
{
    private Rigidbody2D rb;

    public float customAngularDamping = 0.8f;
    public float gravityScale = 1.2f;
    public float maxSpeed = 500f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 초기 설정
        rb.gravityScale = gravityScale;
        rb.angularDamping = customAngularDamping;

        // 처음에는 정지상태
        rb.angularVelocity = 0.0f;
    }

    // 철퇴에 물리력을 가하는 함수
    public void ApplySwingForce(float force, bool isRight)
    {
        // 기존의 운동량을 삭제
        rb.angularVelocity = 0.0f;

        // 원형운동이므로 AddRorque(회전력)을 사용
        // 시계방향은 음수, 반시계방향은 양수
        float direction = isRight ? -1.0f : 1.0f;

        // 속도를 가함
        rb.AddTorque(direction * force, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        // 묵직한 느낌과 안정성을 내기 위해 맥스 스피드를 고정
        if (Mathf.Abs(rb.angularVelocity) > maxSpeed)
        {
            rb.angularVelocity = Mathf.Sign(rb.angularVelocity) * maxSpeed;
        }

        // 방향 전환 시점(속도가 느려지는 정점) 감지
        // 회전 속도가 매우 낮아지는 구간을 '무중력 타이밍'으로 판단합니다.
        float currentSpeed = Mathf.Abs(rb.angularVelocity);

        // 특정 속도 이하로 떨어질 때부터 서서히 물리값이 변하기 시작함
        float threshold = 300.0f;
        if (rb.angularVelocity != 0 && currentSpeed < threshold)
        {
            // 속도가 0에 가까워질수록 t는 0에 가까워짐
            float t = currentSpeed / threshold;

            // 중력과 댐핑을 선형 보간(Lerp)하여 서서히 변화시킴
            // 속도가 낮을수록 gravityScale * 0.3f에 가까워지고, 빠를수록 원본값에 가까워짐
            rb.gravityScale = Mathf.Lerp(gravityScale * 0.3f, gravityScale, t);
            rb.angularDamping = Mathf.Lerp(customAngularDamping * 3.0f, customAngularDamping, t);
        }
        else
        {
            // 속도가 붙으면(하강 시작 시) 다시 정상 물리 적용
            rb.gravityScale = gravityScale;
            rb.angularDamping = customAngularDamping;
        }

        // 자연스러운 멈춤 속도가 매우 낮아지면 0으로 고정
        if (rb.angularVelocity != 0 && Mathf.Abs(rb.angularVelocity) < 0.5f)
        {
            rb.angularVelocity = 0f;
        }
    }
}
