using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DayPlayer :  MonoBehaviour , IDamagable
{
    float maxHP;
    [SerializeField]
    float curHP;

    private void Awake() {
        maxHP = DayPhasePlayerManager.Instance.playerMaxHP;
        curHP = maxHP;
    }

    void Update()
    {
        DayPhasePlayerManager.Instance.playerCurHP = curHP;
    }

    public void TakeDamage(float damage)
    {
        //애니메이션 추가
        curHP -= damage;
    }
}