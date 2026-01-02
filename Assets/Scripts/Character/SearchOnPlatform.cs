using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MyGameEnums;

// 플레이어가 땅 위에 있는지 확인하는 스크립트
// 이벤트로 알림

public class SearchOnPlatform : MonoBehaviour
{
    // 플레이어 캐릭터의 발 아래에 있는 박스가 플랫폼과 충돌했는지 확인하는 이벤트
    public UnityEvent OnPlatform = new UnityEvent();
    public UnityEvent ExitPlatform = new UnityEvent();

    // 현재 접촉 중인 플랫폼 개수 카운트
    private int contactCount = 0;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag( TagName.Platform.GetTag() )
            || collision.gameObject.CompareTag( TagName.CrumblePlatform.GetTag() ))
        {
            contactCount++;
            if (contactCount == 1) // 첫 번째 발을 들였을 때만 이벤트 발생
            {
                OnPlatform.Invoke();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag( TagName.Platform.GetTag() ) 
            || collision.gameObject.CompareTag( TagName.CrumblePlatform.GetTag() ))
        {
            contactCount--;
            if (contactCount <= 0) // 모든 플랫폼에서 발이 떨어졌을 때만 이벤트 발생
            {
                contactCount = 0;
                ExitPlatform.Invoke();
            }
        }
    }

}
