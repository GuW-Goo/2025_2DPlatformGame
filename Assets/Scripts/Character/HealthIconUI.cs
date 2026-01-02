using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthIconUI : MonoBehaviour
{

    [SerializeReference] private GameObject healthIcon;     // 생명이 있을 때 아이콘
    [SerializeReference] private GameObject HitHealthIcon;  // 생명이 없을 때 아이콘
    [SerializeReference] private Transform iconsTransform;  // 아이콘을 그리는 위치

    private List<GameObject> icons = new List<GameObject>();

    public void UpdateIcon(int currentHP, int maxHP)
    {
        // 체력 값이 0~maxHP 범위를 벗어나지 않도록 보정
        int clampedHP = Mathf.Clamp(currentHP, 0, maxHP);

        // 기존 아이콘 제거
        foreach (GameObject icon in icons)
        {
            if (icon != null) Destroy(icon);
        }
        icons.Clear();

        // 새 변수 값만큼 아이콘 생성 (살아있는 하트)
        for (int i = 0; i < clampedHP; i++)
        {
            GameObject icon = Instantiate(healthIcon, iconsTransform);
            icons.Add(icon);
        }

        // 잃은 체력 표시
        for (int i = 0; i < maxHP - clampedHP; i++)
        {
            GameObject icon = Instantiate(HitHealthIcon, iconsTransform);
            icons.Add(icon);
        }

    }
}
